using System.Runtime.InteropServices;
using Devantler.KubernetesProvisioner.Cluster.Kind;

namespace Devantler.KubernetesProvisioner.Deployment.Kubectl.Tests.KubectlProvisionerTests;

/// <summary>
/// Tests for all methods in the <see cref="KubectlProvisioner"/> class.
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
      return;

    // Arrange
    string clusterName = "test-kubectl-cluster";
    string kubeconfig = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".kube", "config");
    string context = "kind-" + clusterName;
    string configPath = Path.Combine(AppContext.BaseDirectory, "assets/kind.yaml");
    string kustomizationDirectoryPath = Path.Combine(AppContext.BaseDirectory, "assets/k8s");
    var kubectlProvisioner = new KubectlProvisioner(kubeconfig, context);
    var cancellationToken = new CancellationToken();

    // Act
    await _kindProvisioner.DeleteAsync(clusterName, cancellationToken);
    await _kindProvisioner.CreateAsync(clusterName, configPath, cancellationToken);
    await kubectlProvisioner.PushAsync(kustomizationDirectoryPath, cancellationToken: cancellationToken);

    // Cleanup
    await _kindProvisioner.DeleteAsync(clusterName, cancellationToken);
  }
}
