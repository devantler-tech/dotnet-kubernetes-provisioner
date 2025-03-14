using System.Runtime.InteropServices;

namespace Devantler.KubernetesProvisioner.Cluster.K3d.Tests.K3dProvisionerTests;

/// <summary>
/// Tests for all methods in the <see cref="K3dProvisioner"/> class.
/// </summary>
[Collection("K3d")]
public class AllMethodsTests
{
  readonly K3dProvisioner _k3dProvisioner = new();

  /// <summary>
  /// Test to verify that all methods in the <see cref="K3dProvisioner"/> class work as expected.
  /// </summary>
  /// <returns></returns>
  [Fact]
  public async Task AllMethods_WithValidParameters_Succeeds()
  {
    //TODO: Support MacOS and Windows, when dind is supported in GitHub Actions Runners on those platforms
    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
      return;
    }

    // Arrange
    string clusterName = "test-k3d-cluster";
    string configPath = Path.Combine(AppContext.BaseDirectory, "assets/k3d.yaml");

    // Act
    var createClusterException = await Record.ExceptionAsync(async () => await _k3dProvisioner.CreateAsync(clusterName, configPath, CancellationToken.None).ConfigureAwait(false));
    var clusters = await _k3dProvisioner.ListAsync(CancellationToken.None);
    var stopClusterException = await Record.ExceptionAsync(async () => await _k3dProvisioner.StopAsync(clusterName, CancellationToken.None).ConfigureAwait(false));
    var startClusterException = await Record.ExceptionAsync(async () => await _k3dProvisioner.StartAsync(clusterName, CancellationToken.None).ConfigureAwait(false));
    bool clusterExists = await _k3dProvisioner.ExistsAsync(clusterName, CancellationToken.None);

    // Assert
    Assert.Null(createClusterException);
    Assert.Contains(clusterName, clusters);
    Assert.Null(stopClusterException);
    Assert.Null(startClusterException);
    Assert.True(clusterExists);

    // Cleanup
    await _k3dProvisioner.DeleteAsync(clusterName, CancellationToken.None);
  }

  /// <summary>
  /// Test to verify that all methods in the <see cref="K3dProvisioner"/> class fail as expected.
  /// </summary>
  /// <returns></returns>
  [Fact]
  public async Task WithInvalidParameters_Fails()
  {
    // Arrange
    string clusterName = "test-k3d-cluster";
    string configPath = Path.Combine(AppContext.BaseDirectory, "assets/invalid.yaml");

    // Act
    var createClusterException = await Record.ExceptionAsync(async () => await _k3dProvisioner.CreateAsync(clusterName, configPath, CancellationToken.None).ConfigureAwait(false));

    // Assert
    Assert.NotNull(createClusterException);
  }
}
