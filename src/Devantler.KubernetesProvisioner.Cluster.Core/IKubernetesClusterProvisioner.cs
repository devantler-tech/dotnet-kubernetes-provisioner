namespace Devantler.KubernetesProvisioner.Cluster.Core;

/// <summary>
/// A Kubernetes cluster provisioner.
/// </summary>
public interface IKubernetesClusterProvisioner
{
  /// <summary>
  /// Creates a Kubernetes cluster.
  /// </summary>
  /// <param name="clusterName"></param>
  /// <param name="configPath"></param>
  /// <param name="cancellationToken"></param>
  Task CreateAsync(string clusterName, string configPath, CancellationToken cancellationToken = default);

  /// <summary>
  /// Deletes a Kubernetes cluster.
  /// </summary>
  /// <param name="clusterName"></param>
  /// <param name="cancellationToken"></param>
  Task DeleteAsync(string clusterName, CancellationToken cancellationToken = default);

  /// <summary>
  /// Starts a Kubernetes cluster.
  /// </summary>
  /// <param name="clusterName"></param>
  /// <param name="cancellationToken"></param>
  Task StartAsync(string clusterName, CancellationToken cancellationToken = default);

  /// <summary>
  /// Stops a Kubernetes cluster.
  /// </summary>
  /// <param name="clusterName"></param>
  /// <param name="cancellationToken"></param>
  Task StopAsync(string clusterName, CancellationToken cancellationToken = default);

  /// <summary>
  /// Lists all Kubernetes clusters.
  /// </summary>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task<IEnumerable<string>> ListAsync(CancellationToken cancellationToken = default);

  /// <summary>
  /// Checks if a Kubernetes cluster exists.
  /// </summary>
  /// <param name="clusterName"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task<bool> ExistsAsync(string clusterName, CancellationToken cancellationToken = default);
}
