using System.Runtime.InteropServices;
using Devantler.KubernetesProvisioner.Cluster.Kind;

namespace Devantler.KubernetesProvisioner.CNI.Cilium.Tests.CiliumProvisionerTests;

/// <summary>
/// Tests for all methods in the <see cref="CiliumProvisioner"/> class.
/// </summary>
[Collection("Cilium")]
internal class AllMethodsTests
{
  readonly KindProvisioner _kindProvisioner = new();
  /// <summary>
  /// Test to verify that flux installs and reconciles kustomizations.
  /// </summary>
  /// <returns></returns>
  [Fact]
  public async Task InstallAsync_InstallsCiliumToKindCluster()
  {
    //TODO: Support MacOS and Windows, when dind is supported in GitHub Actions Runners on those platforms
    if ((RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) && Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true")
      return;

    // Arrange
    string clusterName = "test-cilium-cluster";
    string kubeconfig = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".kube", "config");
    string context = "kind-" + clusterName;
    CiliumProvisioner ciliumProvisioner = new(kubeconfig, context);

    string configPath = Path.Combine(AppContext.BaseDirectory, "assets/kind.yaml");
    var cancellationToken = new CancellationToken();

    // Act
    await _kindProvisioner.DeleteAsync(clusterName, cancellationToken).ConfigureAwait(false);
    await _kindProvisioner.CreateAsync(clusterName, configPath, cancellationToken).ConfigureAwait(false);
    await ciliumProvisioner.InstallAsync(cancellationToken).ConfigureAwait(false);

    // Cleanup
    await _kindProvisioner.DeleteAsync(clusterName, cancellationToken).ConfigureAwait(false);
  }
}
