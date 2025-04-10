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
  /// <param name="cancellationToken"></param>
  Task ApplyAsync(string kustomizationDirectory, CancellationToken cancellationToken = default);

  /// <summary>
  /// Wait for the resources to be ready.
  /// </summary>
  /// <param name="timeout"></param>
  /// <param name="cancellationToken"></param>
  Task WaitAsync(string timeout = "5m", CancellationToken cancellationToken = default);
}
