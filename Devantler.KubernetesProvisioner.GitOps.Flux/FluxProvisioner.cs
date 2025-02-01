using System.Globalization;
using Devantler.Commons.Extensions;
using Devantler.FluxCLI;
using Devantler.KubernetesProvisioner.GitOps.Core;
using Devantler.KubernetesProvisioner.Resources.Native;
using IdentityModel;
using k8s;
using Medallion.Collections;

namespace Devantler.KubernetesProvisioner.GitOps.Flux;

/// <summary>
/// A Kubernetes GitOps provisioner using Flux.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="FluxProvisioner"/> class.
/// </remarks>
/// <param name="context"></param>
public partial class FluxProvisioner(string? context = default) : IGitOpsProvisioner
{
  /// <inheritdoc/>
  public string? Context { get; set; } = context;

  /// <inheritdoc/>
  public async Task PushManifestsAsync(Uri registryUri, string manifestsDirectory, string? userName = null, string? password = null, CancellationToken cancellationToken = default)
  {
    long currentTimeEpoch = DateTime.Now.ToEpochTime();
    string revision = currentTimeEpoch.ToString(CultureInfo.InvariantCulture);

    await PushArtifactAsync(registryUri, manifestsDirectory, revision, cancellationToken);
    await TagArtifactAsync(registryUri, revision, cancellationToken);
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
    var kustomizationList = await kubernetesResourceProvisioner.ListNamespacedCustomObjectAsync<FluxKustomizationList>(
     "kustomize.toolkit.fluxcd.io",
      "v1", "flux-system",
      "kustomizations", cancellationToken: cancellationToken).ConfigureAwait(false);

    var kustomizationNames = kustomizationList.Items.Select(k => k.Metadata.Name).ToList();
    kustomizationNames = [.. kustomizationNames.StableOrderTopologicallyBy(kn => kustomizationList.Items
      .Where(k => k.Metadata.Name == kn)
      .SelectMany(k => k.Spec?.DependsOn ?? [])
      .Select(d => d.Name)
    )];

    foreach (string kustomizationName in kustomizationNames)
    {
      var args = new List<string>
      {
        "reconcile",
        "kustomization",
        kustomizationName,
        "--namespace", "flux-system",
        "--with-source", "OCIRepository/flux-system",
        "--timeout", timeout
      };
      args.AddIfNotNull("--context={0}", Context);
      var (exitCode, _) = await FluxCLI.Flux.RunAsync([.. args], cancellationToken: cancellationToken).ConfigureAwait(false);
      if (exitCode != 0)
      {
        throw new FluxException($"Failed to reconcile Kustomization");
      }
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
    args.AddIfNotNull("--context={0}", Context);
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
  }

  /// <summary>
  /// Tag an artifact.
  /// </summary>
  /// <param name="registryUri"></param>
  /// <param name="revision"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  /// <exception cref="FluxException"></exception>
  public static async Task TagArtifactAsync(Uri registryUri, string revision, CancellationToken cancellationToken)
  {
    var (exitCode, _) = await FluxCLI.Flux.RunAsync([
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
    args.AddIfNotNull("--context={0}", Context);
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
    args.AddIfNotNull("--context={0}", Context);

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
    var args = new List<string>
    {
      "create",
      "kustomization",
      "flux-system",
      "--source", "OCIRepository/flux-system",
      "--path", kustomizationDirectory,
      "--namespace", "flux-system",
      "--interval", "5m",
      "--prune",
      "--wait"
    };
    args.AddIfNotNull("--context={0}", Context);
    var (exitCode, _) = await FluxCLI.Flux.RunAsync([.. args], cancellationToken: cancellationToken).ConfigureAwait(false);
    if (exitCode != 0)
    {
      throw new FluxException($"Failed to create Kustomization");
    }
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
    args.AddIfNotNull("--context={0}", Context);
    var (exitCode, _) = await FluxCLI.Flux.RunAsync([.. args], cancellationToken: cancellationToken).ConfigureAwait(false);
    if (exitCode != 0)
    {
      throw new FluxException($"Failed to reconcile OCI source");
    }
  }
}
