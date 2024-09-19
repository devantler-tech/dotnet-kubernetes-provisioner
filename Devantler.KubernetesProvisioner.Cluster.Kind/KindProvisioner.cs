using Devantler.KubernetesProvisioner.Cluster.Core;

namespace Devantler.KubernetesProvisioner.Cluster.Kind;

public class KindProvisioner : IKubernetesClusterProvisioner
{
  public Task DeprovisionAsync(string clusterName, CancellationToken cancellationToken)
  {

  }
  public Task<bool> ExistsAsync(string clusterName, CancellationToken cancellationToken) => throw new NotImplementedException();
  public Task ListAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
  public Task ProvisionAsync(string clusterName, string configPath, CancellationToken cancellationToken) => throw new NotImplementedException();
}
