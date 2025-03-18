namespace Devantler.KubernetesProvisioner.Cluster.Core;

/// <summary>
/// A Kubernetes CNI provisioner.
/// </summary>
public interface IKubernetesCNIProvisioner
{
  /// <summary>
  /// The path to the kubeconfig file.
  /// </summary>
  public string? Kubeconfig { get; set; }

  /// <summary>
  /// The context to use.
  /// </summary>
  public string? Context { get; set; }

  /// <summary>
  /// Installs a CNI.
  /// </summary>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task InstallAsync(CancellationToken cancellationToken = default);
}
