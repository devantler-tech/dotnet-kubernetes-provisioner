using k8s;
using k8s.Models;

namespace Devantler.KubernetesProvisioner.Resource.Native;

/// <summary>
/// A class that provisions resources in a Kubernetes cluster.
/// </summary>
public sealed class KubernetesResourceProvisioner : IDisposable
{
  readonly Kubernetes? _kubernetesClient;

  /// <summary>
  /// Initializes a new instance of the <see cref="KubernetesResourceProvisioner"/> class.
  /// </summary>
  /// <param name="context"></param>
  public KubernetesResourceProvisioner(string context)
  {
    var kubeConfig = KubernetesClientConfiguration.LoadKubeConfigAsync().Result;
    var config = KubernetesClientConfiguration.BuildConfigFromConfigObject(kubeConfig, context);
    _kubernetesClient = new Kubernetes(config);
  }

  /// <summary>
  /// Creates a namespace in the Kubernetes cluster.
  /// </summary>
  /// <param name="namespace"></param>
  /// <returns></returns>
  public async Task CreateNamespaceAsync(V1Namespace @namespace) =>
    await _kubernetesClient.CreateNamespaceAsync(@namespace).ConfigureAwait(false);

  /// <summary>
  /// Creates a secret in the Kubernetes cluster.
  /// </summary>
  /// <param name="secret"></param>
  /// <param name="namespace"></param>
  /// <returns></returns>
  public async Task CreateSecretAsync(V1Secret secret, string @namespace) =>
    await _kubernetesClient.CreateNamespacedSecretAsync(secret, @namespace).ConfigureAwait(false);

  /// <summary>
  /// Disposes the resources used by the <see cref="KubernetesResourceProvisioner"/>.
  /// </summary>
  public void Dispose()
  {
    _kubernetesClient?.Dispose();
    GC.SuppressFinalize(this);
  }
}
