using Devantler.KubernetesProvisioner.Cluster.Core;

namespace Devantler.KubernetesProvisioner.Cluster.K3d;

/// <summary>
/// A Kubernetes cluster provisioner for k3d.
/// </summary>
public class K3dProvisioner : IKubernetesClusterProvisioner
{
  /// <inheritdoc />
  public async Task DeprovisionAsync(string clusterName, CancellationToken cancellationToken) =>
    await K3dCLI.K3d.DeleteClusterAsync(clusterName, cancellationToken).ConfigureAwait(false);

  /// <inheritdoc />
  public async Task<bool> ExistsAsync(string clusterName, CancellationToken cancellationToken) =>
    await K3dCLI.K3d.GetClusterAsync(clusterName, cancellationToken).ConfigureAwait(false);

  /// <inheritdoc />
  public async Task ListAsync(CancellationToken cancellationToken) =>
    await K3dCLI.K3d.ListClustersAsync(cancellationToken).ConfigureAwait(false);


  /// <inheritdoc />
  public Task ProvisionAsync(string clusterName, string configPath, CancellationToken cancellationToken) => throw new NotImplementedException();
}
