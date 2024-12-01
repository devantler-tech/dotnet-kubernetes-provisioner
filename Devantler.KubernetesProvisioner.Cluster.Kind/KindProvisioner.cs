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
  public async Task DeprovisionAsync(string clusterName, CancellationToken cancellationToken = default) =>
    await KindCLI.Kind.DeleteClusterAsync(clusterName, cancellationToken).ConfigureAwait(false);

  /// <inheritdoc />
  public async Task<bool> ExistsAsync(string clusterName, CancellationToken cancellationToken = default)
  {
    var clusterNames = await ListAsync(cancellationToken).ConfigureAwait(false);
    return clusterNames.Contains(clusterName);
  }

  /// <inheritdoc />
  public async Task<IEnumerable<string>> ListAsync(CancellationToken cancellationToken = default) =>
    await KindCLI.Kind.GetClustersAsync(cancellationToken).ConfigureAwait(false);

  /// <inheritdoc />
  public async Task ProvisionAsync(string clusterName, string configPath, CancellationToken cancellationToken = default) =>
    await KindCLI.Kind.CreateClusterAsync(clusterName, configPath, cancellationToken).ConfigureAwait(false);

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
        );
      }
    }
    using var kubernetesClient = new Kubernetes(KubernetesClientConfiguration.BuildConfigFromConfigObject(KubernetesClientConfiguration.LoadKubeConfig(), "kind-" + clusterName));
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
