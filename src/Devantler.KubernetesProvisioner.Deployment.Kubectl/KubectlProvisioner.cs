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
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  public async Task ApplyAsync(string kustomizationDirectory, CancellationToken cancellationToken = default)
  {
    var args = new List<string>
    {
      "apply",
      "-k",
      kustomizationDirectory
    };

    args.AddIfNotNull("--kubeconfig={0}", Kubeconfig);
    args.AddIfNotNull("--context={0}", Context);

    await KubectlCLI.Kubectl.RunAsync([.. args], cancellationToken: cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Waits for all pods in all namespaces to be ready.
  /// </summary>
  /// <param name="timeout"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  public Task WaitAsync(string timeout = "5m", CancellationToken cancellationToken = default)
  {
    var args = new List<string>
    {
      "wait",
      "pod",
      "--all",
      "--for=condition=ready",
      "-A",
      $"--timeout={timeout}"
    };
    args.AddIfNotNull("--kubeconfig={0}", Kubeconfig);
    args.AddIfNotNull("--context={0}", Context);
    return KubectlCLI.Kubectl.RunAsync([.. args], cancellationToken: cancellationToken);
  }
}
