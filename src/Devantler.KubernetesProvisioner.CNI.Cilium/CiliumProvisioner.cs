using Devantler.Commons.Extensions;
using Devantler.KubernetesProvisioner.Cluster.Core;

namespace Devantler.KubernetesProvisioner.CNI.Cilium;

/// <summary>
/// A Cilium CNI provisioner.
/// </summary>
public class CiliumProvisioner : IKubernetesCNIProvisioner
{
  /// <summary>
  /// Installs Cilium as a CNI.
  /// </summary>
  /// <param name="kubeconfig"></param>
  /// <param name="context"></param>
  /// <param name="cancellationToken"></param>
  public async Task InstallAsync(string? kubeconfig, string? context = null, CancellationToken cancellationToken = default)
  {
    var installArgs = new List<string>
    {
      "install"
    };
    installArgs.AddIfNotNull("--kubeconfig={0}", kubeconfig);
    installArgs.AddIfNotNull("--context={0}", context);

    await CiliumCLI.Cilium.RunAsync([.. installArgs], cancellationToken: cancellationToken).ConfigureAwait(false);
    var waitArgs = new List<string>
    {
      "status",
      "--wait"
    };
    waitArgs.AddIfNotNull("--kubeconfig={0}", kubeconfig);
    waitArgs.AddIfNotNull("--context={0}", context);
    await CiliumCLI.Cilium.RunAsync([.. waitArgs], cancellationToken: cancellationToken).ConfigureAwait(false);
  }
}
