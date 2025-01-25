using System.Globalization;
using Devantler.FluxCLI;
using Devantler.KubernetesProvisioner.GitOps.Core;
using Devantler.KubernetesProvisioner.Resources.Native;
using IdentityModel;
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
public class FluxProvisioner(string? context = default) : IGitOpsProvisioner
{
  /// <inheritdoc/>
  public string? Context { get; set; } = context;

  /// <inheritdoc/>
  public async Task PushManifestsAsync(Uri registryUri, string manifestsDirectory, string? userName = null, string? password = null, CancellationToken cancellationToken = default)
  {
    long currentTimeEpoch = DateTime.Now.ToEpochTime();
    string revision = currentTimeEpoch.ToString(CultureInfo.InvariantCulture);

    await PushArtifactAsync(registryUri, manifestsDirectory, revision, cancellationToken);
  }

  /// <summary>
  /// Install Flux on the Kubernetes cluster.
  /// </summary>
  /// <param name="ociSourceUrl"></param>
  /// <param name="kustomizationDirectory"></param>
  /// <param name="insecure"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  public async Task BootstrapAsync(Uri ociSourceUrl, string kustomizationDirectory, bool insecure = false, CancellationToken cancellationToken = default)
  {
    await InstallAsync(cancellationToken).ConfigureAwait(false);
    await CreateOCISourceAsync(ociSourceUrl, insecure: insecure, cancellationToken: cancellationToken).ConfigureAwait(false);
    await CreateKustomizationAsync(kustomizationDirectory, cancellationToken).ConfigureAwait(false);
  }

  /// <inheritdoc/>
  public async Task ReconcileAsync(string timeout = "5m", CancellationToken cancellationToken = default)
  {
    using var kubernetesResourceProvisioner = new KubernetesResourceProvisioner(Context);
    await ReconcileOCISourceAsync(timeout: timeout, cancellationToken: cancellationToken).ConfigureAwait(false);
    var kustomizations = await kubernetesResourceProvisioner.ListNamespacedCustomObjectAsync<V1CustomResourceDefinitionList>(
      "kustomize.toolkit.fluxcd.io",
      "v1", "flux-system",
      "kustomizations", cancellationToken: cancellationToken).ConfigureAwait(false);

    //TODO: Reconcile all kustomizations, where dependency lists are crawled and reconciled in the correct order.
    // Cycle detection is required to prevent infinite loops.
    // Multiple threads should be used to reconcile multiple independent kustomizations in parallel.
    // For each kustomization, traverse the dependency graph, until an independent kustomization is found. Reconcile it in a separate thread, and mark it as in progres as it starts, and as reconciled when done. Repeat until all kustomizations are reconciled.
    // The traversal should skip kustomizations that are already in progress or reconciled, and it should detect kustomizations that have no dependencies in relation to the status of the other kustomizations.
    var reconciledKustomizations = new List<string>();
    foreach (var kustomization in kustomizations.Items)
    {
      string? kustomizationAsString = kustomization.ToString();
      Console.WriteLine(kustomizationAsString);
    }
  }

  /// <summary>
  /// Uninstall Flux from the Kubernetes cluster.
  /// </summary>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  public async Task UninstallAsync(CancellationToken cancellationToken = default)
  {
    var args = new List<string> { "uninstall", "--silent" };
    args.AddIfNotNull("--context", Context);
    var (exitCode, output) = await FluxCLI.Flux.RunAsync([.. args], cancellationToken: cancellationToken).ConfigureAwait(false);
    if (exitCode != 0 || output.Contains("connection refused", StringComparison.OrdinalIgnoreCase))
    {
      throw new FluxException($"Failed to uninstall flux");
    }
  }

  /// <summary>
  /// Push an artifact to a Flux registry.
  /// </summary>
  /// <param name="registryUri"></param>
  /// <param name="manifestsDirectory"></param>
  /// <param name="revision"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  /// <exception cref="FluxException"></exception>
  public static async Task PushArtifactAsync(Uri registryUri, string manifestsDirectory, string revision, CancellationToken cancellationToken)
  {
    // Push artifact
    var (exitCode, _) = await FluxCLI.Flux.RunAsync([
      "push",
      "artifact",
      $"{registryUri}:{revision}",
      "--path", manifestsDirectory,
      "--source", registryUri.ToString(),
      "--revision", revision],
      cancellationToken: cancellationToken
    );
    if (exitCode != 0)
    {
      throw new FluxException($"Failed to push artifact");
    }

    // Tag artifact
    (exitCode, _) = await FluxCLI.Flux.RunAsync([
        "tag",
        "artifact",
        $"{registryUri}:{revision}",
        "--tag", "latest"
      ], cancellationToken: cancellationToken
    );
    if (exitCode != 0)
    {
      throw new FluxException($"Failed to tag artifact");
    }
  }

  /// <summary>
  /// Install Flux on the Kubernetes cluster.
  /// </summary>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  /// <exception cref="FluxException"></exception>
  public async Task InstallAsync(CancellationToken cancellationToken)
  {
    var args = new List<string> { "install", };
    args.AddIfNotNull("--context", Context);
    var (exitCode, _) = await FluxCLI.Flux.RunAsync([.. args], cancellationToken: cancellationToken).ConfigureAwait(false);
    if (exitCode != 0)
    {
      throw new FluxException($"Failed to install flux");
    }
  }

  /// <summary>
  /// Create an OCI source.
  /// </summary>
  /// <param name="url"></param>
  /// <param name="name"></param>
  /// <param name="namespace"></param>
  /// <param name="tag"></param>
  /// <param name="interval"></param>
  /// <param name="insecure"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  public async Task CreateOCISourceAsync(Uri url,
    string name = "flux-system",
    string @namespace = "flux-system",
    string tag = "latest",
    string interval = "10m",
    bool insecure = false,
    CancellationToken cancellationToken = default
  )
  {
    var args = new List<string>
    {
      "create", "source", "oci", name,
      "--url", url.ToString(),
      "--insecure", insecure.ToString(),
      "--tag", tag,
      "--interval", interval,
      "--namespace", @namespace
    };
    args.AddIfNotNull("--context", Context);

    var (exitCode, _) = await FluxCLI.Flux.RunAsync([.. args], cancellationToken: cancellationToken);
    if (exitCode != 0)
    {
      throw new FluxException($"Failed to create OCI source");
    }
  }

  /// <summary>
  /// Create a Kustomization.
  /// </summary>
  /// <param name="kustomizationDirectory"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  public async Task CreateKustomizationAsync(string kustomizationDirectory, CancellationToken cancellationToken)
  {
    await FluxCLI.Flux.CreateKustomizationAsync("flux-system", "OCIRepository/flux-system", kustomizationDirectory, Context, wait: false,
      cancellationToken: cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Reconcile an OCI source.
  /// </summary>
  /// <param name="name"></param>
  /// <param name="namespace"></param>
  /// <param name="timeout"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  /// <exception cref="FluxException"></exception>
  public async Task ReconcileOCISourceAsync(string name = "flux-system", string @namespace = "flux-system", string timeout = "5m", CancellationToken cancellationToken = default)
  {
    var args = new List<string> { "reconcile", "source", "oci", name, "--namespace", @namespace, "--timeout", timeout };
    args.AddIfNotNull("--context", Context);
    var (exitCode, _) = await FluxCLI.Flux.RunAsync([.. args], cancellationToken: cancellationToken).ConfigureAwait(false) :
    if (exitCode != 0)
    {
      throw new FluxException($"Failed to reconcile OCI source");
    }
  }
}
