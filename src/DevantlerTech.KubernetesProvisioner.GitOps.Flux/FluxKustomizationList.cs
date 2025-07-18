#pragma warning disable CA2227 // Collection properties should be read only
using System.Collections.ObjectModel;
using DevantlerTech.KubernetesGenerator.Flux.Models.Kustomization;

namespace DevantlerTech.KubernetesProvisioner.GitOps.Flux;

/// <summary>
/// Flux Kustomization List
/// </summary>
public class FluxKustomizationList
{
  /// <summary>
  /// Flux Kustomizations.
  /// </summary>
  public Collection<FluxKustomization> Items { get; set; } = [];
}
#pragma warning restore CA2227 // Collection properties should be read only

