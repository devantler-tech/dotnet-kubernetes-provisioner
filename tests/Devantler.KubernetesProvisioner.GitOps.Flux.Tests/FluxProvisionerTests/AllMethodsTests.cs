using System.Runtime.InteropServices;
using Devantler.ContainerEngineProvisioner.Docker;
using Devantler.KubernetesProvisioner.Cluster.Kind;

namespace Devantler.KubernetesProvisioner.GitOps.Flux.Tests.FluxProvisionerTests;

/// <summary>
/// Tests for all methods in the <see cref="FluxProvisioner"/> class.
/// </summary>
[Collection("Flux")]
public class AllMethodsTests
{
  readonly KindProvisioner _kindProvisioner = new();
  /// <summary>
  /// Test to verify that flux installs and reconciles kustomizations.
  /// </summary>
  /// <returns></returns>
  [Fact]
  public async Task Flux_InstallsAndReconciles_KustomizationsAsync()
  {
    //TODO: Support MacOS and Windows, when dind is supported in GitHub Actions Runners on those platforms
    if ((RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) && Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true")
    {
      return;
    }

    // Arrange
    string clusterName = "test-flux-cluster";
    string kubeconfig = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".kube", "config");
    string context = "kind-" + clusterName;
    string configPath = Path.Combine(AppContext.BaseDirectory, "assets/kind.yaml");
    string manifestsDirectoryPath = Path.Combine(AppContext.BaseDirectory, "assets/k8s");
    string kustomizationDirectoryPath = $"clusters/{clusterName}/flux-system";
    var fluxProvisioner = new FluxProvisioner(new Uri($"oci://localhost:5555/{clusterName}"), kubeconfig: kubeconfig, context: context);
    var dockerProvisioner = new DockerProvisioner();
    var cancellationToken = new CancellationToken();

    // Act
    await dockerProvisioner.CreateRegistryAsync("ksail-registry", 5555, cancellationToken: cancellationToken);
    await _kindProvisioner.DeleteAsync(clusterName, cancellationToken);
    await _kindProvisioner.CreateAsync(clusterName, configPath, cancellationToken);
    await fluxProvisioner.PushAsync(manifestsDirectoryPath, cancellationToken: cancellationToken);
    var ociUri = new Uri($"oci://host.docker.internal:5555/{clusterName}");
    // Fix for Kind on Linux, that doesn't support host.docker.internal via --add-host
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
    {
      ociUri = new Uri($"oci://172.17.0.1:5555/{clusterName}");
    }

    await fluxProvisioner.InstallAsync(ociUri, kustomizationDirectoryPath, true, cancellationToken: cancellationToken);
    await fluxProvisioner.ReconcileAsync(cancellationToken: cancellationToken);
    await fluxProvisioner.UninstallAsync(cancellationToken);

    // Cleanup
    await _kindProvisioner.DeleteAsync(clusterName, cancellationToken);
    await dockerProvisioner.DeleteRegistryAsync("ksail-registry", cancellationToken);
  }
}
