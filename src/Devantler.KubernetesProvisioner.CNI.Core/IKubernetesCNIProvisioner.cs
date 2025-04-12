namespace Devantler.KubernetesProvisioner.CNI.Core;

/// <summary>
/// A Kubernetes CNI provisioner.
/// </summary>
public interface IKubernetesCNIProvisioner
{
  /// <summary>
  /// The path to the kubeconfig file.
  /// </summary>
  string? Kubeconfig { get; set; }

  /// <summary>
  /// The context to use.
  /// </summary>
  string? Context { get; set; }

  /// <summary>
  /// Installs a CNI.
  /// </summary>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task InstallAsync(CancellationToken cancellationToken = default);
}
