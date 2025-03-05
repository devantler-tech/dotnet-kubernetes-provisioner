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
  /// <param name="context"></param>
  /// <param name="cancellationToken"></param>
  public async Task InstallAsync(string? context = null, CancellationToken cancellationToken = default)
  {
    var args = new List<string>
    {
      "install"
    };
    args.AddIfNotNull("--context", context);

    await CiliumCLI.Cilium.RunAsync([.. args], cancellationToken: cancellationToken).ConfigureAwait(false);
  }
}
