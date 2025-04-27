using CliWrap;
using Devantler.KubernetesProvisioner.Cluster.Core;
using Docker.DotNet;
using Docker.DotNet.Models;
using k8s;
using k8s.Autorest;

namespace Devantler.KubernetesProvisioner.Cluster.Kind;

/// <summary>
/// A Kubernetes cluster provisioner for Kind.
/// </summary>
public class KindProvisioner : IKubernetesClusterProvisioner
{
  readonly DockerClient _dockerClient = new DockerClientConfiguration().CreateClient();

  /// <inheritdoc />
  public async Task DeleteAsync(string clusterName, CancellationToken cancellationToken = default)
  {
    var args = new List<string>
    {
      "delete",
      "cluster",
      "--name", clusterName
    };
    var (exitCode, _) = await KindCLI.Kind.RunAsync([.. args], cancellationToken: cancellationToken).ConfigureAwait(false);
    if (exitCode != 0)
    {
      throw new KubernetesClusterProvisionerException("Failed to delete Kind cluster.");
    }

    var containers = await _dockerClient.Containers.ListContainersAsync(new ContainersListParameters { All = true }, cancellationToken).ConfigureAwait(false);
    var container = containers.FirstOrDefault(c => c.Names.Any(name => name.Equals($"/{clusterName}-cloud-provider-kind", StringComparison.OrdinalIgnoreCase)));
    if (container != null)
    {
      Console.WriteLine($"Deleting {clusterName}-cloud-provider-kind container...");
      _ = await _dockerClient.Containers.StopContainerAsync(
        container.ID,
        new ContainerStopParameters(),
        cancellationToken
      ).ConfigureAwait(false);
    }
  }

  /// <inheritdoc />
  public async Task<bool> ExistsAsync(string clusterName, CancellationToken cancellationToken = default)
  {
    var args = new List<string> { "get", "clusters" };
    var (exitCode, result) = await KindCLI.Kind.RunAsync([.. args], CommandResultValidation.None, silent: true, cancellationToken: cancellationToken).ConfigureAwait(false);
    return exitCode == 0 && result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Contains(clusterName);
  }

  /// <inheritdoc />
  public async Task<IEnumerable<string>> ListAsync(CancellationToken cancellationToken = default)
  {
    var args = new List<string> { "get", "clusters" };
    var (exitCode, result) = await KindCLI.Kind.RunAsync([.. args], silent: true, cancellationToken: cancellationToken).ConfigureAwait(false);
    if (exitCode != 0)
    {
      throw new KubernetesClusterProvisionerException("Failed to list Kind clusters.");
    }
    string[] clusterNames = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
    return clusterNames;
  }

  /// <inheritdoc />
  public async Task CreateAsync(string clusterName, string configPath, CancellationToken cancellationToken = default)
  {
    var args = new List<string>
    {
      "create",
      "cluster",
      "--name", clusterName,
      "--config", configPath
    };
    var (exitCode, _) = await KindCLI.Kind.RunAsync([.. args], cancellationToken: cancellationToken).ConfigureAwait(false);
    if (exitCode != 0)
    {
      throw new KubernetesClusterProvisionerException("Failed to create Kind cluster.");
    }

    Console.WriteLine();

    Console.WriteLine("Creating cloud provider kind container...");
    var containersListParameters = new ContainersListParameters
    {
      All = true
    };
    var containers = await _dockerClient.Containers.ListContainersAsync(containersListParameters, cancellationToken).ConfigureAwait(false);
    var container = containers.FirstOrDefault(c => c.Names.Any(name => name.Equals("/cloud-provider-kind", StringComparison.OrdinalIgnoreCase)));
    if (container != null)
    {
      Console.WriteLine(" ✓ cloud-provider-kind container already running");
      return;
    }
    string cloudControllerManagerImage = $"registry.k8s.io/cloud-provider-kind/cloud-controller-manager";
    string cloudControllerManagerTag = "v0.6.0";
    Console.WriteLine($" • Pulling image {cloudControllerManagerImage}:{cloudControllerManagerTag}");
    var cloudControllerContainerImageParameters = new ImagesCreateParameters
    {
      FromImage = cloudControllerManagerImage,
      Tag = cloudControllerManagerTag
    };
    await _dockerClient.Images.CreateImageAsync(
      cloudControllerContainerImageParameters,
      new AuthConfig(),
      new Progress<JSONMessage>(),
      cancellationToken
    ).ConfigureAwait(false);
    Console.WriteLine($" ✓ Pulled image {cloudControllerManagerImage}:{cloudControllerManagerTag}");
    Console.WriteLine($" • Creating container cloud-provider-kind");
    var cloudControllerContainerParameters = new CreateContainerParameters
    {
      Image = $"{cloudControllerManagerImage}:{cloudControllerManagerTag}",
      Name = "cloud-provider-kind",
      HostConfig = new HostConfig
      {
        AutoRemove = true,
        // TODO: Ensure this is the correct network when using multiple clusters
        NetworkMode = "kind",
        Binds =
        [
          "/var/run/docker.sock:/var/run/docker.sock"
        ]
      }
    };
    if (Environment.OSVersion.Platform is PlatformID.Win32NT or PlatformID.MacOSX)
    {
      cloudControllerContainerParameters.Cmd = ["--enable-lb-port-mapping"];
    }
    var cloudControllerManagerContainerCreateResponse = await _dockerClient.Containers.CreateContainerAsync(cloudControllerContainerParameters, cancellationToken).ConfigureAwait(false);
    Console.WriteLine($" ✓ Created container cloud-provider-kind");
    Console.WriteLine($" • Starting container cloud-provider-kind");
    _ = await _dockerClient.Containers.StartContainerAsync(cloudControllerManagerContainerCreateResponse.ID, new ContainerStartParameters(), cancellationToken).ConfigureAwait(false);
    Console.WriteLine($" ✓ Started container cloud-provider-kind");
  }

  /// <inheritdoc />
  public async Task StartAsync(string clusterName, CancellationToken cancellationToken = default)
  {
    var containersListParameters = new ContainersListParameters
    {
      All = true
    };
    var containers = await _dockerClient.Containers.ListContainersAsync(containersListParameters, cancellationToken).ConfigureAwait(false);
    foreach (var container in containers)
    {
      if (container.Names.Any(name => name.StartsWith($"/{clusterName}", StringComparison.OrdinalIgnoreCase)))
      {
        _ = await _dockerClient.Containers.StartContainerAsync(
          container.ID,
          new ContainerStartParameters(),
          cancellationToken
        ).ConfigureAwait(false);
      }
    }
    using var kubernetesClient = new Kubernetes(KubernetesClientConfiguration.BuildConfigFromConfigObject(
      await KubernetesClientConfiguration.LoadKubeConfigAsync().ConfigureAwait(false), "kind-" + clusterName)
    );
    while (true)
    {
      try
      {
        var namespaceList = await kubernetesClient.ListNamespaceAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        if (namespaceList.Items.Any())
        {
          break;
        }
      }
      catch (HttpRequestException)
      {
        await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
      }
      catch (HttpOperationException)
      {
        await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
      }
    }
  }

  /// <inheritdoc />
  public async Task StopAsync(string clusterName, CancellationToken cancellationToken = default)
  {
    foreach (var container in await _dockerClient.Containers.ListContainersAsync(new ContainersListParameters(), cancellationToken).ConfigureAwait(false))
    {
      if (container.Names.Any(name => name.StartsWith($"/{clusterName}", StringComparison.Ordinal)))
      {
        _ = await _dockerClient.Containers
          .StopContainerAsync(container.ID, new ContainerStopParameters(), cancellationToken)
          .ConfigureAwait(false);
      }
    }
  }
}
