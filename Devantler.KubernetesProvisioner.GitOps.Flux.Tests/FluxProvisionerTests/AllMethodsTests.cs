using Devantler.KindCLI;

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
    string configPath = Path.Combine(AppContext.BaseDirectory, "assets/kind-config.yaml");
    var fluxProvisioner = new FluxProvisioner("kind-" + clusterName);
    var cancellationToken = new CancellationToken();

    // Act
    await Kind.DeleteClusterAsync(clusterName, cancellationToken);
    await Kind.CreateClusterAsync(clusterName, configPath, cancellationToken);
    await fluxProvisioner.InstallAsync(cancellationToken);
    await FluxCLI.Flux.CreateOCISourceAsync("podinfo", new Uri("oci://ghcr.io/stefanprodan/manifests/podinfo"), cancellationToken: cancellationToken);
    await FluxCLI.Flux.CreateKustomizationAsync("podinfo", "OCIRepository/podinfo", "", cancellationToken: cancellationToken);
    await fluxProvisioner.ReconcileAsync(cancellationToken);
    await fluxProvisioner.UninstallAsync(cancellationToken);

    // Cleanup
    await Kind.DeleteClusterAsync(clusterName, cancellationToken);
  }
}
