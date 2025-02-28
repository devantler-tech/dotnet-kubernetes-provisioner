using System.Collections.ObjectModel;
using Devantler.KubernetesGenerator.Flux.Models.Kustomization;

namespace Devantler.KubernetesProvisioner.GitOps.Flux;


/// <summary>
/// Flux Kustomization List
/// </summary>
public class FluxKustomizationList
{
  /// <summary>
  /// Flux Kustomizations.
  /// </summary>
  public ReadOnlyCollection<FluxKustomization> Items { get; } = new ReadOnlyCollection<FluxKustomization>([]);
}

