using System.Runtime.InteropServices;
using Devantler.KubernetesProvisioner.Cluster.Kind;
using k8s;
using k8s.Models;

namespace Devantler.KubernetesProvisioner.Resources.Native.Tests.KubernetesResourceProvisionerTests;

/// <summary>
/// Tests for all methods in the <see cref="KubernetesResourceProvisioner"/> class.
/// </summary>
[Collection("Native")]
public class AllMethodsTests
{
  readonly KindProvisioner _kindProvisioner = new();
  /// <summary>
  /// Test to verify that all methods in the <see cref="KubernetesResourceProvisioner"/> class work as expected.
  /// </summary>
  /// <returns></returns>
  [Fact]
  public async Task AllMethods_WithValidParameters_Succeeds()
  {
    //TODO: Support MacOS and Windows, when dind is supported in GitHub Actions Runners on those platforms
    if ((RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) && Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true")
    {
      return;
    }

    // Arrange
    string clusterName = "test-native-cluster";
    string configPath = Path.Combine(AppContext.BaseDirectory, "assets/kind.yaml");

    // Act
    var createClusterException = await Record.ExceptionAsync(async () =>
    {
      await _kindProvisioner.CreateAsync(clusterName, configPath, CancellationToken.None).ConfigureAwait(false);
    });
    using var kubernetesResourceProvisioner = new KubernetesResourceProvisioner($"kind-{clusterName}");
    var @namespace = new V1Namespace
    {
      ApiVersion = "v1",
      Kind = "Namespace",
      Metadata = new V1ObjectMeta
      {
        Name = "test-namespace"
      }
    };
    var createdNamespaceException = await Record.ExceptionAsync(async () => await kubernetesResourceProvisioner.CreateNamespaceAsync(@namespace).ConfigureAwait(false));
    // Assert
    Assert.Null(createClusterException);
    Assert.Null(createdNamespaceException);

    // Cleanup
    await _kindProvisioner.DeleteAsync(clusterName, CancellationToken.None);
  }
}
