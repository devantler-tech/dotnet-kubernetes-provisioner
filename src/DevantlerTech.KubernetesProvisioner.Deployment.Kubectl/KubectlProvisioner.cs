using CliWrap.Exceptions;
using DevantlerTech.Commons.Extensions;
using DevantlerTech.KubernetesProvisioner.Deployment.Core;

namespace DevantlerTech.KubernetesProvisioner.Deployment.Kubectl;

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
  /// Applies a kustomization to the Kubernetes cluster using kubectl with apply-set functionality.
  /// </summary>
  /// <param name="kustomizationDirectory">The directory containing the kustomization to apply.</param>
  /// <param name="timeout">The timeout for the kubectl apply operation (e.g., "5m", "10s").</param>
  /// <param name="cancellationToken">A token to cancel the operation.</param>
  /// <returns>A task representing the asynchronous operation.</returns>
  public async Task PushAsync(string kustomizationDirectory, string timeout = "5m", CancellationToken cancellationToken = default)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(kustomizationDirectory, nameof(kustomizationDirectory));
    ArgumentException.ThrowIfNullOrWhiteSpace(timeout, nameof(timeout));
    
    var args = new List<string>
    {
      "get",
      "crd",
      "applysets.k8s.devantler.tech",
      "--no-headers",
      "--ignore-not-found"
    };
    args.AddIfNotNull("--kubeconfig={0}", Kubeconfig);
    args.AddIfNotNull("--context={0}", Context);
    var (exitCode, result) = await KubectlCLI.Kubectl.RunAsync(
      [.. args],
      silent: true,
      cancellationToken: cancellationToken
    ).ConfigureAwait(false);
    if (exitCode != 0)
    {
      throw new KubernetesDeploymentToolProvisionerException(result);
    }
    if (!result.Contains("applysets.k8s.devantler.tech", StringComparison.Ordinal))
    {
      args =
      [
        "apply",
        "-f", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "k8s", "apply-set-crd.yaml")
      ];
      args.AddIfNotNull("--kubeconfig={0}", Kubeconfig);
      args.AddIfNotNull("--context={0}", Context);
      _ = await KubectlCLI.Kubectl.RunAsync([.. args], cancellationToken: cancellationToken).ConfigureAwait(false);
      await WaitForCRDToBeEstablished(cancellationToken).ConfigureAwait(false);
      args =
      [
        "apply",
        "-f", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "k8s", "apply-set-cr.yaml")
      ];
      args.AddIfNotNull("--kubeconfig={0}", Kubeconfig);
      args.AddIfNotNull("--context={0}", Context);
      _ = await KubectlCLI.Kubectl.RunAsync([.. args], cancellationToken: cancellationToken).ConfigureAwait(false);
    }
    Environment.SetEnvironmentVariable("KUBECTL_APPLYSET", "true");
    args =
    [
      "apply",
      "-k", kustomizationDirectory,
      "--prune",
      "--applyset=applysets.k8s.devantler.tech/ksail",
      $"--timeout={timeout}"
    ];
    args.AddIfNotNull("--kubeconfig={0}", Kubeconfig);
    args.AddIfNotNull("--context={0}", Context);
    (exitCode, result) = await KubectlCLI.Kubectl.RunAsync([.. args], validation: CliWrap.CommandResultValidation.None, cancellationToken: cancellationToken).ConfigureAwait(false);
    if (exitCode != 0 && !result.Contains("error: no objects passed to apply", StringComparison.Ordinal))
    {
      throw new KubernetesDeploymentToolProvisionerException(result);
    }
    Environment.SetEnvironmentVariable("KUBECTL_APPLYSET", null);
  }

  async Task WaitForCRDToBeEstablished(CancellationToken cancellationToken)
  {
    bool crdEstablished = false;
    int retries = 0;
    do
    {
      try
      {
        var args = new List<string>
        {
          "wait",
          "--for=condition=established",
          "--timeout=60s",
          "crd", "applysets.k8s.devantler.tech"
        };
        args.AddIfNotNull("--kubeconfig={0}", Kubeconfig);
        args.AddIfNotNull("--context={0}", Context);
        _ = await KubectlCLI.Kubectl.RunAsync([.. args], cancellationToken: cancellationToken).ConfigureAwait(false);
        crdEstablished = true;
      }
      catch (CommandExecutionException)
      {
        retries++;
        if (retries >= 3)
          throw;
        await Task.Delay(500, cancellationToken).ConfigureAwait(false);
      }
    } while (!crdEstablished);
  }

  /// <inheritdoc/>
  public async Task ReconcileAsync(string kustomizationDirectory, string timeout = "5m", CancellationToken cancellationToken = default)
  {
    var args = new List<string>
    {
      "rollout",
      "status",
      "-k", kustomizationDirectory,
      $"--timeout={timeout}"
    };
    args.AddIfNotNull("--kubeconfig={0}", Kubeconfig);
    args.AddIfNotNull("--context={0}", Context);
    _ = await KubectlCLI.Kubectl.RunAsync([.. args], CliWrap.CommandResultValidation.None, cancellationToken: cancellationToken).ConfigureAwait(false);
  }
}
