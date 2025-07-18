using System.Collections.Concurrent;
using System.Globalization;
using DevantlerTech.Commons.Extensions;
using DevantlerTech.Commons.Utils;
using DevantlerTech.KubernetesProvisioner.GitOps.Core;
using DevantlerTech.KubernetesProvisioner.Resources.Native;
using k8s;
using Medallion.Collections;

namespace DevantlerTech.KubernetesProvisioner.GitOps.Flux;

/// <summary>
/// A Kubernetes GitOps provisioner using Flux.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="FluxProvisioner"/> class.
/// </remarks>
/// <param name="registryUri"></param>
/// <param name="registryUserName"></param>
/// <param name="registryPassword"></param>
/// <param name="kubeconfig"></param>
/// <param name="context"></param>
public partial class FluxProvisioner(Uri registryUri, string? registryUserName = default, string? registryPassword = default, string? kubeconfig = default, string? context = default) : IGitOpsProvisioner
{
  /// <inheritdoc/>
  public string? Kubeconfig { get; set; } = kubeconfig;

  /// <inheritdoc/>
  public string? Context { get; set; } = context;

  /// <inheritdoc/>
  public Uri RegistryUri { get; set; } = registryUri;

  /// <inheritdoc/>
  public string? RegistryUserName { get; set; } = registryUserName;

  /// <inheritdoc/>
  public string? RegistryPassword { get; set; } = registryPassword;

  /// <inheritdoc/>
  public async Task PushAsync(string kustomizationDirectory, string timeout = "5m", CancellationToken cancellationToken = default)
  {
    long currentTimeEpoch = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    string revision = currentTimeEpoch.ToString(CultureInfo.InvariantCulture);

    await PushArtifactAsync(RegistryUri, kustomizationDirectory, revision, cancellationToken).ConfigureAwait(false);
    await TagArtifactAsync(RegistryUri, revision, cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Install Flux on the Kubernetes cluster.
  /// </summary>
  /// <param name="ociSourceUrl"></param>
  /// <param name="kustomizationDirectory"></param>
  /// <param name="insecure"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  public async Task InstallAsync(Uri ociSourceUrl, string kustomizationDirectory, bool insecure = false, CancellationToken cancellationToken = default)
  {
    await InstallAsync(cancellationToken).ConfigureAwait(false);
    await CreateOCISourceAsync(ociSourceUrl, insecure: insecure, interval: "30s", cancellationToken: cancellationToken).ConfigureAwait(false);
    await CreateKustomizationAsync(kustomizationDirectory, interval: "1m", cancellationToken).ConfigureAwait(false);
  }
  /// <inheritdoc/>
  public async Task ReconcileAsync(string kustomizationDirectory, string timeout = "5m", CancellationToken cancellationToken = default)
  {
    var args = new List<string>
    {
      "reconcile",
      "source",
      "oci",
      "flux-system",
      "--timeout", timeout
    };
    args.AddIfNotNull("--kubeconfig={0}", Kubeconfig);
    args.AddIfNotNull("--context={0}", Context);
    var (exitCode, _) = await FluxCLI.Flux.RunAsync([.. args], cancellationToken: cancellationToken).ConfigureAwait(false);
    if (exitCode != 0)
    {
      throw new KubernetesGitOpsProvisionerException($"Failed to reconcile OCI source");
    }
    using var kubernetesResourceProvisioner = new KubernetesResourceProvisioner(Kubeconfig, Context);
    var kustomizationList = await kubernetesResourceProvisioner.ListNamespacedCustomObjectAsync<FluxKustomizationList>(
      "kustomize.toolkit.fluxcd.io",
      "v1", "flux-system",
      "kustomizations", cancellationToken: cancellationToken).ConfigureAwait(false);

    var kustomizationTuples = kustomizationList.Items.Select(k => (k.Metadata.Name, (k.Spec?.DependsOn ?? []).Select(d => d.Name))).ToList();
    var kustomizationNames = kustomizationTuples.Select(k => k.Name).ToList();
    kustomizationNames = [.. kustomizationNames.StableOrderTopologicallyBy(kn => kustomizationList.Items
      .Where(k => k.Metadata.Name == kn)
      .SelectMany(k => k.Spec?.DependsOn ?? [])
      .Select(d => d.Name)
    )];
    kustomizationTuples = [.. kustomizationTuples.OrderBy(k => kustomizationNames.IndexOf(k.Name))];

    var reconciledKustomizations = new ConcurrentBag<string>();
    using var semaphore = new SemaphoreSlim(10);
    var tasks = new List<Task>();
    int kustomizationCount = kustomizationTuples.Count;
    foreach (var kustomizationTuple in kustomizationTuples)
    {
      await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

      var task = Task.Run(async () =>
      {
        int dependencyCount = GetDependencyCount(kustomizationTuples, kustomizationTuple) + 1;
        var effectiveTimeout = TimeSpanHelper.ParseDuration(timeout).Multiply(dependencyCount);

        var args = new List<string>
        {
          "reconcile",
          "kustomization",
          kustomizationTuple.Name,
          "--namespace", "flux-system",
          "--with-source",
          "--timeout", $"{effectiveTimeout.TotalSeconds.ToString(CultureInfo.InvariantCulture)}s"
        };
        args.AddIfNotNull("--kubeconfig={0}", Kubeconfig);
        args.AddIfNotNull("--context={0}", Context);
        var startTime = DateTime.UtcNow;
        while (kustomizationTuple.Item2.Any() && !kustomizationTuple.Item2.All(reconciledKustomizations.Contains))
        {
          if (DateTime.UtcNow - startTime > effectiveTimeout)
          {
            _ = semaphore.Release();
            throw new KubernetesGitOpsProvisionerException($"Reconciliation of '{kustomizationTuple.Name}' timed out. Waiting for dependencies: {string.Join(", ", kustomizationTuple.Item2.Select(d => $"'{d}'"))}");
          }
          var elapsed = DateTime.UtcNow - startTime;
          var remaining = effectiveTimeout - elapsed;
          Console.WriteLine(
            "  '{0}' waiting for {1}. Timeout in {2} seconds. ",
            kustomizationTuple.Name,
            string.Join(", ", kustomizationTuple.Item2.Select(d => $"'{d}'")),
            Math.Max(0, remaining.TotalSeconds).ToString("F2", CultureInfo.InvariantCulture)
          );
          await Task.Delay(2500, cancellationToken).ConfigureAwait(false);
        }
        var (exitCode, _) = await FluxCLI.Flux.RunAsync([.. args], cancellationToken: cancellationToken).ConfigureAwait(false);
        if (exitCode != 0)
        {
          _ = semaphore.Release();
          throw new KubernetesGitOpsProvisionerException($"Failed to reconcile Kustomization");
        }
        reconciledKustomizations.Add(kustomizationTuple.Name);
        _ = semaphore.Release();
      }, cancellationToken);

      tasks.Add(task);
    }

    await Task.WhenAll(tasks).ConfigureAwait(false);
  }

  static int GetDependencyCount(List<(string Name, IEnumerable<string>)> kustomizationTuples, (string Name, IEnumerable<string>) kustomizationTuple)
  {
    var visited = new HashSet<string>();
    void Visit(string name)
    {
      if (!visited.Add(name))
        return;
      var kustom = kustomizationTuples.FirstOrDefault(k => k.Name == name);
      foreach (string? dep in kustom.Item2)
        Visit(dep);
    }
    foreach (string? dep in kustomizationTuple.Item2)
      Visit(dep);
    int dependencyCount = visited.Count;
    return dependencyCount;
  }

  /// <summary>
  /// Uninstall Flux from the Kubernetes cluster.
  /// </summary>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  public async Task UninstallAsync(CancellationToken cancellationToken = default)
  {
    var args = new List<string> { "uninstall", "--silent" };
    args.AddIfNotNull("--kubeconfig={0}", Kubeconfig);
    args.AddIfNotNull("--context={0}", Context);
    var (exitCode, output) = await FluxCLI.Flux.RunAsync([.. args], cancellationToken: cancellationToken).ConfigureAwait(false);
    if (exitCode != 0 || output.Contains("connection refused", StringComparison.OrdinalIgnoreCase))
    {
      throw new KubernetesGitOpsProvisionerException($"Failed to uninstall flux");
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
  /// <exception cref="KubernetesGitOpsProvisionerException"></exception>
  public static async Task PushArtifactAsync(Uri registryUri, string manifestsDirectory, string revision, CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(registryUri, nameof(registryUri));
    var (exitCode, _) = await FluxCLI.Flux.RunAsync([
      "push",
      "artifact",
      $"{registryUri}:{revision}",
      "--path", manifestsDirectory,
      "--source", registryUri.ToString(),
      "--revision", revision],
      cancellationToken: cancellationToken
    ).ConfigureAwait(false);
    if (exitCode != 0)
    {
      throw new KubernetesGitOpsProvisionerException($"Failed to push artifact");
    }
  }

  /// <summary>
  /// Tag an artifact.
  /// </summary>
  /// <param name="registryUri"></param>
  /// <param name="revision"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  /// <exception cref="KubernetesGitOpsProvisionerException"></exception>
  public static async Task TagArtifactAsync(Uri registryUri, string revision, CancellationToken cancellationToken = default)
  {
    var (exitCode, _) = await FluxCLI.Flux.RunAsync([
        "tag",
        "artifact",
        $"{registryUri}:{revision}",
        "--tag", "latest"
      ], cancellationToken: cancellationToken
    ).ConfigureAwait(false);
    if (exitCode != 0)
    {
      throw new KubernetesGitOpsProvisionerException($"Failed to tag artifact");
    }
  }

  /// <summary>
  /// Install Flux on the Kubernetes cluster.
  /// </summary>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  /// <exception cref="KubernetesGitOpsProvisionerException"></exception>
  public async Task InstallAsync(CancellationToken cancellationToken = default)
  {
    var args = new List<string> { "install", };
    args.AddIfNotNull("--kubeconfig={0}", Kubeconfig);
    args.AddIfNotNull("--context={0}", Context);
    var (exitCode, _) = await FluxCLI.Flux.RunAsync([.. args], cancellationToken: cancellationToken).ConfigureAwait(false);
    if (exitCode != 0)
    {
      throw new KubernetesGitOpsProvisionerException($"Failed to install flux");
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
    ArgumentNullException.ThrowIfNull(url, nameof(url));
    var args = new List<string>
    {
      "create", "source", "oci", name,
      "--url", url.ToString(),
      "--insecure", insecure.ToString(),
      "--tag", tag,
      "--interval", interval,
      "--namespace", @namespace
    };
    args.AddIfNotNull("--kubeconfig={0}", Kubeconfig);
    args.AddIfNotNull("--context={0}", Context);

    var (exitCode, _) = await FluxCLI.Flux.RunAsync([.. args], cancellationToken: cancellationToken).ConfigureAwait(false);
    if (exitCode != 0)
    {
      throw new KubernetesGitOpsProvisionerException($"Failed to create OCI source");
    }
  }

  /// <summary>
  /// Create a Kustomization.
  /// </summary>
  /// <param name="kustomizationDirectory"></param>
  /// <param name="interval"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  public async Task CreateKustomizationAsync(string kustomizationDirectory, string interval = "5m", CancellationToken cancellationToken = default)
  {
    var args = new List<string>
    {
      "create",
      "kustomization",
      "flux-system",
      "--source", "OCIRepository/flux-system",
      "--path", kustomizationDirectory,
      "--namespace", "flux-system",
      "--interval", interval,
      "--prune"
    };
    args.AddIfNotNull("--kubeconfig={0}", Kubeconfig);
    args.AddIfNotNull("--context={0}", Context);
    var (exitCode, _) = await FluxCLI.Flux.RunAsync([.. args], cancellationToken: cancellationToken).ConfigureAwait(false);
    if (exitCode != 0)
    {
      throw new KubernetesGitOpsProvisionerException($"Failed to create Kustomization");
    }
  }


}
