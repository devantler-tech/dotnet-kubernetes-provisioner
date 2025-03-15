using Devantler.K3dCLI;
using Devantler.KubernetesProvisioner.Cluster.Core;

namespace Devantler.KubernetesProvisioner.Cluster.K3d;

/// <summary>
/// A Kubernetes cluster provisioner for K3d.
/// </summary>
public class K3dProvisioner : IKubernetesClusterProvisioner
{
  /// <inheritdoc />
  public async Task DeleteAsync(string clusterName, CancellationToken cancellationToken = default)
  {
    var args = new List<string>
    {
      "cluster",
      "delete",
      clusterName
    };
    var (exitCode, _) = await K3dCLI.K3d.RunAsync([.. args], cancellationToken: cancellationToken).ConfigureAwait(false);
    if (exitCode != 0)
    {
      throw new KubernetesClusterProvisionerException("Failed to delete K3d cluster.");
    }
  }

  /// <inheritdoc />
  public async Task<bool> ExistsAsync(string clusterName, CancellationToken cancellationToken = default)
  {
    var args = new List<string>
    {
      "cluster",
      "get",
      clusterName
    };
    var (exitCode, _) = await K3dCLI.K3d.RunAsync([.. args], CliWrap.CommandResultValidation.None, silent: true, cancellationToken: cancellationToken).ConfigureAwait(false);
    return exitCode == 0;
  }

  /// <inheritdoc />
  public async Task<IEnumerable<string>> ListAsync(CancellationToken cancellationToken = default)
  {
    var args = new List<string> { "cluster", "list" };
    var (exitCode, output) = await K3dCLI.K3d.RunAsync([.. args], silent: true, cancellationToken: cancellationToken).ConfigureAwait(false);
    if (exitCode != 0)
    {
      throw new KubernetesClusterProvisionerException("Failed to list K3d clusters.");
    }
    string[] lines = output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
    string[] clusterLines = [.. lines.Skip(1)];
    string[] clusterNames = [.. clusterLines.Select(line => line.Split(' ', StringSplitOptions.RemoveEmptyEntries).First())];
    return clusterNames;
  }

  /// <inheritdoc />
  public async Task CreateAsync(string clusterName, string configPath, CancellationToken cancellationToken = default)
  {
    var args = new List<string>
    {
      "cluster",
      "create",
      clusterName,
      "--config", configPath
    };
    var (exitCode, _) = await K3dCLI.K3d.RunAsync([.. args], cancellationToken: cancellationToken).ConfigureAwait(false);
    if (exitCode != 0)
    {
      throw new KubernetesClusterProvisionerException("Failed to create K3d cluster.");
    }
  }

  /// <inheritdoc />
  public async Task StartAsync(string clusterName, CancellationToken cancellationToken = default)
  {
    var args = new List<string>
    {
      "cluster",
      "start",
      clusterName
    };
    var (exitCode, _) = await K3dCLI.K3d.RunAsync([.. args], cancellationToken: cancellationToken).ConfigureAwait(false);
    if (exitCode != 0)
    {
      throw new KubernetesClusterProvisionerException($"Failed to start k3d cluster.");
    }
  }

  /// <inheritdoc />
  public async Task StopAsync(string clusterName, CancellationToken cancellationToken = default)
  {
    var args = new List<string>
    {
      "cluster",
      "stop",
      clusterName
    };
    var (exitCode, _) = await K3dCLI.K3d.RunAsync([.. args], cancellationToken: cancellationToken).ConfigureAwait(false);
    if (exitCode != 0)
    {
      throw new KubernetesClusterProvisionerException($"Failed to stop k3d cluster.");
    }
  }
}
