using Devantler.KubernetesProvisioner.Deployment.Core;

namespace Devantler.KubernetesProvisioner.GitOps.Core;

/// <summary>
/// A Kubernetes GitOps provisioner.
/// </summary>
public interface IGitOpsProvisioner : IDeploymentToolProvisioner
{
  /// <summary>
  /// The OCI registry URI to push the manifests to.
  /// </summary>
  Uri RegistryUri { get; set; }

  /// <summary>
  /// The OCI registry username.
  /// </summary>
  string? RegistryUserName { get; set; }

  /// <summary>
  /// The OCI registry password.
  /// </summary>
  string? RegistryPassword { get; set; }

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
