using k8s;

namespace Devantler.KubernetesProvisioner.Resources.Native;

/// <summary>
/// A class that provisions resources in a Kubernetes cluster.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="KubernetesResourceProvisioner"/> class.
/// </remarks>
/// <param name="kubeconfig"></param>
/// <param name="context"></param>
public sealed class KubernetesResourceProvisioner(string? kubeconfig = default, string? context = default) : Kubernetes(BuildConfig(kubeconfig, context))
{
  static KubernetesClientConfiguration BuildConfig(string? kubeconfig, string? context)
  {
    var kubeConfig = KubernetesClientConfiguration.LoadKubeConfig(kubeconfig);
    var config = KubernetesClientConfiguration.BuildConfigFromConfigObject(kubeConfig, context);
    return config;
  }
}
