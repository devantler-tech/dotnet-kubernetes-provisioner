using Devantler.KubernetesProvisioner.Cluster.Core;

namespace Devantler.KubernetesProvisioner.Cluster.Kind;

/// <summary>
/// A Kubernetes cluster provisioner for Kind.
/// </summary>
public class KindProvisioner : IKubernetesClusterProvisioner
{
  /// <inheritdoc />
  public async Task DeprovisionAsync(string clusterName, CancellationToken cancellationToken) =>
    await KindCLI.Kind.DeleteClusterAsync(clusterName, cancellationToken).ConfigureAwait(false);

  /// <inheritdoc />
  public async Task<bool> ExistsAsync(string clusterName, CancellationToken cancellationToken) =>
    await KindCLI.Kind.GetClusterAsync(clusterName, cancellationToken).ConfigureAwait(false);

  /// <inheritdoc />
  public async Task<string[]> ListAsync(CancellationToken cancellationToken) =>
    await KindCLI.Kind.ListClustersAsync(cancellationToken).ConfigureAwait(false);

  /// <inheritdoc />
  public async Task ProvisionAsync(string clusterName, string configPath, CancellationToken cancellationToken) =>
    await KindCLI.Kind.CreateClusterAsync(clusterName, configPath, cancellationToken).ConfigureAwait(false);

  /// <inheritdoc />
  public async Task StartAsync(string clusterName, CancellationToken cancellationToken) =>
    await KindCLI.Kind.StartClusterAsync(clusterName, cancellationToken).ConfigureAwait(false);

  /// <inheritdoc />
  public async Task StopAsync(string clusterName, CancellationToken cancellationToken) =>
    await KindCLI.Kind.StopClusterAsync(clusterName, cancellationToken).ConfigureAwait(false);
}
