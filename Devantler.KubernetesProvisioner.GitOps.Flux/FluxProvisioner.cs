using Devantler.KubernetesProvisioner.GitOps.Core;
using Devantler.KubernetesProvisioner.Resources.Native;
using k8s;
using k8s.Models;

namespace Devantler.KubernetesProvisioner.GitOps.Flux;

/// <summary>
/// A Kubernetes GitOps provisioner using Flux.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="FluxProvisioner"/> class.
/// </remarks>
/// <param name="context"></param>
public class FluxProvisioner(string? context = default) : IKubernetesGitOpsProvisioner
{

  /// <inheritdoc/>
  public string? Context { get; set; } = context;

  /// <summary>
  /// Install Flux on the Kubernetes cluster.
  /// </summary>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  public async Task InstallAsync(CancellationToken cancellationToken = default) =>
    await FluxCLI.Flux.InstallAsync(Context, cancellationToken).ConfigureAwait(false);

  /// <summary>
  /// Reconcile resources on the Kubernetes cluster.
  /// </summary>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  public async Task ReconcileAsync(CancellationToken cancellationToken = default)
  {
    using var kubernetesResourceProvisioner = new KubernetesResourceProvisioner(Context);
    var kustomizations = await kubernetesResourceProvisioner.CustomObjects.ListNamespacedCustomObjectAsync<V1CustomResourceDefinitionList>("kustomize.toolkit.fluxcd.io", "v1", "flux-system", "kustomizations", cancellationToken: cancellationToken).ConfigureAwait(false);
    foreach (var kustomization in kustomizations.Items)
    {
      await FluxCLI.Flux.ReconcileKustomizationAsync(kustomization.Metadata.Name, Context, withSource: true, cancellationToken: cancellationToken).ConfigureAwait(false);
    }
  }

  /// <summary>
  /// Uninstall Flux from the Kubernetes cluster.
  /// </summary>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  public async Task UninstallAsync(CancellationToken cancellationToken = default) =>
    await FluxCLI.Flux.UninstallAsync(Context, cancellationToken).ConfigureAwait(false);
}
