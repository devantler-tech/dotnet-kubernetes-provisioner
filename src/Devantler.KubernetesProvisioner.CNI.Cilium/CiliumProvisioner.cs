using Devantler.Commons.Extensions;
using Devantler.KubernetesProvisioner.CNI.Core;

namespace Devantler.KubernetesProvisioner.CNI.Cilium;

/// <summary>
/// A Cilium CNI provisioner.
/// </summary>
public class CiliumProvisioner(string? kubeconfig = default, string? context = default) : IKubernetesCNIProvisioner
{
  /// <inheritdoc/>
  public string? Kubeconfig { get; set; } = kubeconfig;

  /// <inheritdoc/>
  public string? Context { get; set; } = context;

  /// <summary>
  /// Installs Cilium as a CNI.
  /// </summary>
  /// <param name="cancellationToken"></param>
  public async Task InstallAsync(CancellationToken cancellationToken = default)
  {
    var installArgs = new List<string>
    {
      "install"
    };
    installArgs.AddIfNotNull("--kubeconfig={0}", Kubeconfig);
    installArgs.AddIfNotNull("--context={0}", Context);

    _ = await CiliumCLI.Cilium.RunAsync([.. installArgs], cancellationToken: cancellationToken).ConfigureAwait(false);
    var waitArgs = new List<string>
    {
      "status",
      "--interactive", "false",
      "--wait"
    };
    waitArgs.AddIfNotNull("--kubeconfig={0}", Kubeconfig);
    waitArgs.AddIfNotNull("--context={0}", Context);
    _ = await CiliumCLI.Cilium.RunAsync([.. waitArgs], cancellationToken: cancellationToken).ConfigureAwait(false);
  }
}
