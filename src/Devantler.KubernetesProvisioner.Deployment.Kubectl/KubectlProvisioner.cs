using Devantler.Commons.Extensions;
using Devantler.KubernetesProvisioner.Deployment.Core;

namespace Devantler.KubernetesProvisioner.Deployment.Kubectl;

/// <summary>
/// A Kubernetes deployment tool that uses kubectl to apply a kustomization.
/// </summary>
/// <param name="kubeconfig"></param>
/// <param name="context"></param>
public class KubectlProvisioner(string? kubeconfig = default, string? context = default) : IDeploymentToolProvisioner
{
  /// <inheritdoc/>
  public string? Kubeconfig { get; set; } = kubeconfig;

  /// <inheritdoc/>
  public string? Context { get; set; } = context;

  /// <summary>
  /// Applies a kustomization to the Kubernetes cluster using kubectl.
  /// </summary>
  /// <param name="kustomizationDirectory"></param>
  /// <param name="timeout"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  public async Task PushAsync(string kustomizationDirectory, string timeout = "5m", CancellationToken cancellationToken = default)
  {
    var args = new List<string>
    {
      "apply",
      "-k",
      kustomizationDirectory,
      "-l=provider=ksail",
      $"--timeout={timeout}",
      "--prune",
      "--wait=true",
    };
    args.AddIfNotNull("--kubeconfig={0}", Kubeconfig);
    args.AddIfNotNull("--context={0}", Context);
    var (exitCode, result) = await KubectlCLI.Kubectl.RunAsync([.. args], validation: CliWrap.CommandResultValidation.None, cancellationToken: cancellationToken).ConfigureAwait(false);
    if (exitCode != 0 && !result.Contains("error: no objects passed to apply", StringComparison.Ordinal))
    {
      throw new KubernetesDeploymentToolProvisionerException(result);
    }
    args =
    [
      "label",
      "-k",
      kustomizationDirectory,
      "provider=ksail",
      "--overwrite"
    ];
    args.AddIfNotNull("--kubeconfig={0}", Kubeconfig);
    args.AddIfNotNull("--context={0}", Context);
    _ = await KubectlCLI.Kubectl.RunAsync([.. args], validation: CliWrap.CommandResultValidation.None, cancellationToken: cancellationToken).ConfigureAwait(false);

    args = [
      "rollout",
      "status",
      "-k",
      kustomizationDirectory,
      "-l=provider=ksail",
      $"--timeout={timeout}",
    ];
    args.AddIfNotNull("--kubeconfig={0}", Kubeconfig);
    args.AddIfNotNull("--context={0}", Context);
    _ = await KubectlCLI.Kubectl.RunAsync([.. args], validation: CliWrap.CommandResultValidation.None, cancellationToken: cancellationToken).ConfigureAwait(false);
  }
}
