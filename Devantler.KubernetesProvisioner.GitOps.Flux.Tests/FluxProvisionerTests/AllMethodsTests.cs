using Devantler.KindCLI;
using Devantler.ContainerEngineProvisioner.Docker;
using CliWrap;
using CliWrap.Buffered;
using System.Runtime.InteropServices;

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
    string manifestsDirectoryPath = Path.Combine(AppContext.BaseDirectory, "assets/k8s");
    string kustomizationDirectoryPath = $"clusters/{clusterName}/flux-system";
    var fluxProvisioner = new FluxProvisioner(context);
    var dockerProvisioner = new DockerProvisioner();
    var cancellationToken = new CancellationToken();

    // Act
    await dockerProvisioner.CreateRegistryAsync("ksail-registry", 5555, cancellationToken: cancellationToken);
    await Kind.DeleteClusterAsync(clusterName, cancellationToken);
    await Kind.CreateClusterAsync(clusterName, configPath, cancellationToken);
    await fluxProvisioner.PushManifestsAsync(new Uri($"oci://localhost:5555/{clusterName}"), manifestsDirectoryPath, cancellationToken: cancellationToken);
    var ociUri = new Uri($"oci://host.docker.internal:5555/{clusterName}");
    // Fix for Linux, that doesn't support host.docker.internal
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
    {
      var cmd = Cli.Wrap("ip").WithArguments("addr show docker0");
      var result = await cmd.ExecuteBufferedAsync(cancellationToken);
      string docker0Ip = result.StandardOutput;
      ociUri = new Uri($"oci://{docker0Ip}:5555/{clusterName}");
    }

    await fluxProvisioner.BootstrapAsync(ociUri, kustomizationDirectoryPath, true, cancellationToken: cancellationToken);
    await fluxProvisioner.ReconcileAsync(cancellationToken: cancellationToken);
    await fluxProvisioner.UninstallAsync(cancellationToken);

    // Cleanup
    await Kind.DeleteClusterAsync(clusterName, cancellationToken);
    await dockerProvisioner.DeleteRegistryAsync("ksail-registry", cancellationToken);
  }
}
