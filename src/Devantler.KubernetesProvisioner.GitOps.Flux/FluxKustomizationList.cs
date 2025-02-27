using Devantler.KubernetesGenerator.Flux.Models.Kustomization;

namespace Devantler.KubernetesProvisioner.GitOps.Flux;

public partial class FluxProvisioner
{
  /// <summary>
  /// Flux Kustomization List
  /// </summary>
  public class FluxKustomizationList
  {
    /// <summary>
    /// Flux Kustomizations.
    /// </summary>
    public List<FluxKustomization> Items { get; set; } = [];
  }
}
