using CliWrap;
using DevantlerTech.KubernetesProvisioner.Cluster.Core;
using Docker.DotNet;
using Docker.DotNet.Models;
using k8s;
using k8s.Autorest;

namespace DevantlerTech.KubernetesProvisioner.Cluster.Kind;

/// <summary>
/// A Kubernetes cluster provisioner for Kind.
/// </summary>
public class KindProvisioner : IKubernetesClusterProvisioner
{
  readonly DockerClient _dockerClient;
  readonly CloudProviderKindProvisioner _cloudProviderKindProvisioner;

  /// <summary>
  /// Initializes a new instance of the <see cref="KindProvisioner"/> class.
  /// </summary>
  public KindProvisioner()
  {
    using (var dockerClientConfig = new DockerClientConfiguration())
    {
      _dockerClient = dockerClientConfig.CreateClient();
    }
    _cloudProviderKindProvisioner = new CloudProviderKindProvisioner(_dockerClient);
  }

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
    var clusters = await ListAsync(cancellationToken).ConfigureAwait(false);
    if (!clusters.Any())
    {
      await _cloudProviderKindProvisioner.DeleteAsync(cancellationToken).ConfigureAwait(false);
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
    return clusterNames.Where(cluster => !cluster.Contains("No kind clusters found.", StringComparison.Ordinal));
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
    await _cloudProviderKindProvisioner.CreateAsync(cancellationToken).ConfigureAwait(false);
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

    await _cloudProviderKindProvisioner.CreateAsync(cancellationToken).ConfigureAwait(false);
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
    var clusters = await ListAsync(cancellationToken).ConfigureAwait(false);
    if (!clusters.Any())
    {
      await _cloudProviderKindProvisioner.DeleteAsync(cancellationToken).ConfigureAwait(false);
    }
  }
}
