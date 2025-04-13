namespace Devantler.KubernetesProvisioner.Deployment.Core;

/// <summary>
/// A Kubernetes deployment tool.
/// </summary>
public interface IDeploymentToolProvisioner
{
  /// <summary>
  /// The Kubernetes kubeconfig.
  /// </summary>
  string? Kubeconfig { get; set; }

  /// <summary>
  /// The Kubernetes context.
  /// </summary>
  string? Context { get; set; }

  /// <summary>
  /// Apply a kustomization to the Kubernetes cluster.
  /// </summary>
  /// <param name="kustomizationDirectory"></param>
  /// <param name="timeout"></param>
  /// <param name="cancellationToken"></param>
  Task PushAsync(string kustomizationDirectory, string timeout = "5m", CancellationToken cancellationToken = default);

  /// <summary>
  /// Reconcile resources on the Kubernetes cluster.
  /// </summary>
  Task ReconcileAsync(string kustomizationDirectory, string timeout = "5m", CancellationToken cancellationToken = default);
}
