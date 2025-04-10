using Devantler.Commons.Extensions;
using Devantler.KubernetesProvisioner.Deployment.Core;
using Devantler.KubernetesProvisioner.GitOps.Core;

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
      $"--timeout={timeout}",
      "--wait=true",
    };
    args.AddIfNotNull("--kubeconfig={0}", Kubeconfig);
    args.AddIfNotNull("--context={0}", Context);

    var (exitCode, result) = await KubectlCLI.Kubectl.RunAsync([.. args], validation: CliWrap.CommandResultValidation.None, cancellationToken: cancellationToken).ConfigureAwait(false);
    if (exitCode != 0 && !result.Contains("error: no objects passed to apply", StringComparison.Ordinal)) {
      throw new KubernetesDeploymentToolProvisionerException(result);
    }
    args = [
      "rollout",
      "status",
      "-k",
      kustomizationDirectory,
      $"--timeout={timeout}",
    ];
    args.AddIfNotNull("--kubeconfig={0}", Kubeconfig);
    args.AddIfNotNull("--context={0}", Context);
    await KubectlCLI.Kubectl.RunAsync([.. args], validation: CliWrap.CommandResultValidation.None, cancellationToken: cancellationToken).ConfigureAwait(false);
  }
}
