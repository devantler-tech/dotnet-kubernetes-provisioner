using Devantler.KindCLI;
using Devantler.ContainerEngineProvisioner.Docker;

namespace Devantler.KubernetesProvisioner.GitOps.Flux.Tests.FluxProvisionerTests;

/// <summary>
/// Tests for all methods in the <see cref="FluxProvisioner"/> class.
/// </summary>
[Collection("Flux")]
public class AllMethodsTests
{
  /// <summary>
  /// Test to verify that flux installs and reconciles kustomizations.
  /// </summary>
  /// <returns></returns>
  [Fact]
  public async Task Flux_InstallsAndReconciles_KustomizationsAsync()
  {
    // Arrange
    string clusterName = "test-flux-cluster";
    string context = "kind-" + clusterName;
    string configPath = Path.Combine(AppContext.BaseDirectory, "assets/kind-config.yaml");
    var fluxProvisioner = new FluxProvisioner(context);
    var dockerProvisioner = new DockerProvisioner();
    var cancellationToken = new CancellationToken();

    // Act
    await Kind.DeleteClusterAsync(clusterName, cancellationToken);
    await Kind.CreateClusterAsync(clusterName, configPath, cancellationToken);
    await dockerProvisioner.CreateRegistryAsync("ksail-registry", 5555, cancellationToken: cancellationToken);
    await fluxProvisioner.BootstrapAsync(new Uri("oci://ghcr.io/stefanprodan/manifests/podinfo"), "", cancellationToken);
    string testFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
    await File.WriteAllTextAsync(testFile, "test");
    await fluxProvisioner.PushManifestsAsync(new Uri("oci://localhost:5555/test-manifest"), testFile, cancellationToken: cancellationToken);
    await fluxProvisioner.ReconcileAsync(cancellationToken);
    await fluxProvisioner.UninstallAsync(cancellationToken);

    // Cleanup
    await Kind.DeleteClusterAsync(clusterName, cancellationToken);
    await dockerProvisioner.DeleteRegistryAsync("ksail-registry", cancellationToken);
    File.Delete(testFile);
  }
}
