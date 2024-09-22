namespace Devantler.KubernetesProvisioner.Cluster.Kind.Tests.KindProvisionerTests;

/// <summary>
/// Tests for all methods in the <see cref="KindProvisioner"/> class.
/// </summary>
public class AllMethodsTests
{
  readonly KindProvisioner _kindProvisioner = new();
  /// <summary>
  /// Test to verify that all methods in the <see cref="KindProvisioner"/> class work as expected.
  /// </summary>
  /// <returns></returns>
  [Fact]
  public async Task AllMethods_WithValidParameters_Succeeds()
  {
    // Arrange
    string clusterName = "test-cluster";
    string configPath = Path.Combine(AppContext.BaseDirectory, "assets/kind-config.yaml");

    // Act
    var createClusterException = await Record.ExceptionAsync(async () => await _kindProvisioner.ProvisionAsync(clusterName, configPath, CancellationToken.None).ConfigureAwait(false));
    string[] clusters = await _kindProvisioner.ListAsync(CancellationToken.None);
    var stopClusterException = await Record.ExceptionAsync(async () => await _kindProvisioner.StopAsync(clusterName, CancellationToken.None).ConfigureAwait(false));
    var startClusterException = await Record.ExceptionAsync(async () => await _kindProvisioner.StartAsync(clusterName, CancellationToken.None).ConfigureAwait(false));
    bool clusterExists = await _kindProvisioner.ExistsAsync(clusterName, CancellationToken.None);

    // Assert
    Assert.Null(createClusterException);
    string expectedClusterName = Assert.Single(clusters);
    Assert.Equal(clusterName, expectedClusterName);
    Assert.Null(stopClusterException);
    Assert.Null(startClusterException);
    Assert.True(clusterExists);

    // Cleanup
    await _kindProvisioner.DeprovisionAsync(clusterName, CancellationToken.None);
  }

  /// <summary>
  /// Test to verify that all methods in the <see cref="KindProvisioner"/> class fail as expected.
  /// </summary>
  /// <returns></returns>
  [Fact]
  public async Task WithInvalidParameters_Fails()
  {
    // Arrange
    string clusterName = "test-cluster";
    string configPath = Path.Combine(AppContext.BaseDirectory, "assets/invalid-config.yaml");

    // Act
    var createClusterException = await Record.ExceptionAsync(async () => await _kindProvisioner.ProvisionAsync(clusterName, configPath, CancellationToken.None).ConfigureAwait(false));

    // Assert
    Assert.NotNull(createClusterException);
  }
}
