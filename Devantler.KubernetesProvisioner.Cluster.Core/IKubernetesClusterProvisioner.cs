namespace Devantler.KubernetesProvisioner.Cluster.Core;

/// <summary>
/// A Kubernetes cluster provisioner.
/// </summary>
public interface IKubernetesClusterProvisioner
{
  /// <summary>
  /// Provisions a Kubernetes cluster.
  /// </summary>
  /// <param name="clusterName"></param>
  /// <param name="configPath"></param>
  /// <param name="cancellationToken"></param>
  Task ProvisionAsync(string clusterName, string configPath, CancellationToken cancellationToken);

  /// <summary>
  /// Deprovisions a Kubernetes cluster.
  /// </summary>
  /// <param name="clusterName"></param>
  /// <param name="cancellationToken"></param>
  Task DeprovisionAsync(string clusterName, CancellationToken cancellationToken);

  /// <summary>
  /// Lists all Kubernetes clusters.
  /// </summary>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task ListAsync(CancellationToken cancellationToken);

  /// <summary>
  /// Checks if a Kubernetes cluster exists.
  /// </summary>
  /// <param name="clusterName"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task<bool> ExistsAsync(string clusterName, CancellationToken cancellationToken);
}
