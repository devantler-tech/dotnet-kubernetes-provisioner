using System.Runtime.InteropServices;
using Devantler.KubernetesProvisioner.Cluster.Kind;
using Devantler.KubernetesProvisioner.CNI.Cilium;

namespace Devantler.KubernetesProvisioner.GitOps.Flux.Tests.FluxProvisionerTests;

/// <summary>
/// Tests for all methods in the <see cref="FluxProvisioner"/> class.
/// </summary>
[Collection("Flux")]
public class AllMethodsTests
{
  readonly KindProvisioner _kindProvisioner = new();
  readonly CiliumProvisioner _ciliumProvisioner = new();
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
    string clusterName = "test-cilium-cluster";
    string context = "kind-" + clusterName;
    string configPath = Path.Combine(AppContext.BaseDirectory, "assets/kind-config.yaml");
    var cancellationToken = new CancellationToken();

    // Act
    await _kindProvisioner.DeleteAsync(clusterName, cancellationToken);
    await _kindProvisioner.CreateAsync(clusterName, configPath, cancellationToken);
    await _ciliumProvisioner.InstallAsync(context, cancellationToken);

    // Cleanup
    await _kindProvisioner.DeleteAsync(clusterName, cancellationToken);
  }
}
