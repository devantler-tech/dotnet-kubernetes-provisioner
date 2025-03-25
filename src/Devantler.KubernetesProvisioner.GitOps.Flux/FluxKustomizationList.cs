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
#pragma warning disable CA2227 // Collection properties should be read only
  public Collection<FluxKustomization> Items { get; set; } = [];
#pragma warning restore CA2227 // Collection properties should be read only
}


