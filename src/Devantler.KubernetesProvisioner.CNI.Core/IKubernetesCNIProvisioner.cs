namespace Devantler.KubernetesProvisioner.Cluster.Core;

/// <summary>
/// A Kubernetes CNI provisioner.
/// </summary>
public interface IKubernetesCNIProvisioner
{
  /// <summary>
  /// Installs a CNI.
  /// </summary>
  /// <param name="kubeconfig"></param>
  /// <param name="context"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task InstallAsync(string? kubeconfig = default, string? context = default, CancellationToken cancellationToken = default);
}
