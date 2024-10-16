namespace Devantler.KubernetesProvisioner.GitOps.Core;

/// <summary>
/// A Kubernetes GitOps provisioner.
/// </summary>
public interface IGitOpsProvisioner
{
  /// <summary>
  /// The Kubernetes context.
  /// </summary>
  public string? Context { get; set; }
  /// <summary>
  /// Install the GitOps tooling on the Kubernetes cluster.
  /// </summary>
  public Task InstallAsync(CancellationToken cancellationToken = default);

  /// <summary>
  /// Uninstall the GitOps tooling from the Kubernetes cluster.
  /// </summary>
  public Task UninstallAsync(CancellationToken cancellationToken = default);

  /// <summary>
  /// Reconcile resources on the Kubernetes cluster.
  /// </summary>
  public Task ReconcileAsync(CancellationToken cancellationToken = default);
}
