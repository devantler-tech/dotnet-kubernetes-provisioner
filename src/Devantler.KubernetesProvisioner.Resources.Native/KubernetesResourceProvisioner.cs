using k8s;

namespace Devantler.KubernetesProvisioner.Resources.Native;

/// <summary>
/// A class that provisions resources in a Kubernetes cluster.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="KubernetesResourceProvisioner"/> class.
/// </remarks>
/// <param name="context"></param>
public sealed class KubernetesResourceProvisioner(string? context = default) : Kubernetes(BuildConfig(context))
{
  static KubernetesClientConfiguration BuildConfig(string? context)
  {
    var kubeConfig = KubernetesClientConfiguration.LoadKubeConfig();
    var config = KubernetesClientConfiguration.BuildConfigFromConfigObject(kubeConfig, context);
    return config;
  }
}
