namespace Devantler.KubernetesProvisioner.Cluster.Core;

/// <summary>
/// A Kubernetes CNI provisioner.
/// </summary>
public interface IKubernetesCNIProvisioner
{
  /// <summary>
  /// Installs a CNI.
  /// </summary>
  /// <param name="context"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task InstallAsync(string? context = default, CancellationToken cancellationToken = default);
}
