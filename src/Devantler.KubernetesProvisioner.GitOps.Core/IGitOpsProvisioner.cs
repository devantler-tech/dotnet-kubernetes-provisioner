namespace Devantler.KubernetesProvisioner.GitOps.Core;

/// <summary>
/// A Kubernetes GitOps provisioner.
/// </summary>
public interface IGitOpsProvisioner
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
  /// Push manifests to an OCI registry
  /// </summary>
  /// <param name="registryUri"></param>
  /// <param name="manifestsDirectory"></param>
  /// <param name="userName"></param>
  /// <param name="password"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task PushManifestsAsync(Uri registryUri, string manifestsDirectory, string? userName = default, string? password = default, CancellationToken cancellationToken = default);

  /// <summary>
  /// Bootstrap the GitOps tooling on the Kubernetes cluster.
  /// </summary>
  Task InstallAsync(Uri ociSourceUrl, string kustomizationDirectory, bool insecure = false, CancellationToken cancellationToken = default);

  /// <summary>
  /// Uninstall the GitOps tooling from the Kubernetes cluster.
  /// </summary>
  Task UninstallAsync(CancellationToken cancellationToken = default);

  /// <summary>
  /// Reconcile resources on the Kubernetes cluster.
  /// </summary>
  Task ReconcileAsync(string timeout = "5m", CancellationToken cancellationToken = default);
}
