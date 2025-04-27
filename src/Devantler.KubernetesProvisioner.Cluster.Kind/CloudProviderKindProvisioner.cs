using System.Runtime.InteropServices;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace Devantler.KubernetesProvisioner.Cluster.Kind;

/// <summary>
/// A Kubernetes cluster provisioner for Kinds Cloud Provider, which is a Docker container.
/// </summary>
/// <param name="dockerClient"></param>
public class CloudProviderKindProvisioner(DockerClient dockerClient)
{
  /// <summary>
  /// Creates the cloud provider kind container.
  /// </summary>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  public async Task CreateAsync(CancellationToken cancellationToken)
  {
    var containersListParameters = new ContainersListParameters
    {
      All = true
    };
    var containers = await dockerClient.Containers.ListContainersAsync(containersListParameters, cancellationToken).ConfigureAwait(false);
    var existingContainer = containers.FirstOrDefault(c => c.Names.Any(name => name.Equals("/cloud-provider-kind", StringComparison.OrdinalIgnoreCase)));
    if (existingContainer != null)
    {
      return;
    }

    string cloudControllerManagerImage = $"registry.k8s.io/cloud-provider-kind/cloud-controller-manager";
    string cloudControllerManagerTag = "v0.6.0";
    Console.WriteLine($" • Pulling image {cloudControllerManagerImage}:{cloudControllerManagerTag}");
    var cloudControllerContainerImageParameters = new ImagesCreateParameters
    {
      FromImage = cloudControllerManagerImage,
      Tag = cloudControllerManagerTag
    };
    await dockerClient.Images.CreateImageAsync(
      cloudControllerContainerImageParameters,
      new AuthConfig(),
      new Progress<JSONMessage>(),
      cancellationToken
    ).ConfigureAwait(false);
    Console.WriteLine($" ✓ Pulled image {cloudControllerManagerImage}:{cloudControllerManagerTag}");
    Console.WriteLine($" • Creating container cloud-provider-kind");
    var cloudControllerContainerParameters = new CreateContainerParameters
    {
      Image = $"{cloudControllerManagerImage}:{cloudControllerManagerTag}",
      Name = "cloud-provider-kind",
      HostConfig = new HostConfig
      {
        AutoRemove = true,
        // TODO: Ensure this is the correct network when using multiple clusters
        NetworkMode = "kind",
        Binds =
        [
          "/var/run/docker.sock:/var/run/docker.sock"
        ]
      }
    };
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
    {
      Console.WriteLine(" • Enabling port mapping on Windows/MacOS");
      Console.WriteLine("   See https://github.com/kubernetes-sigs/cloud-provider-kind#enabling-load-balancer-port-mapping");
      cloudControllerContainerParameters.Cmd = ["-enable-lb-port-mapping"];
    }
    var cloudControllerManagerContainerCreateResponse = await dockerClient.Containers.CreateContainerAsync(cloudControllerContainerParameters, cancellationToken).ConfigureAwait(false);
    Console.WriteLine($" ✓ Created container cloud-provider-kind");
    Console.WriteLine($" • Starting container cloud-provider-kind");
    _ = await dockerClient.Containers.StartContainerAsync(cloudControllerManagerContainerCreateResponse.ID, new ContainerStartParameters(), cancellationToken).ConfigureAwait(false);
    Console.WriteLine($" ✓ Started container cloud-provider-kind");
  }

  /// <summary>
  /// Deletes the the cloud provider kind container.
  /// </summary>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  public async Task DeleteAsync(CancellationToken cancellationToken)
  {
    var containersListParameters = new ContainersListParameters
    {
      All = true
    };
    var containers = await dockerClient.Containers.ListContainersAsync(containersListParameters, cancellationToken).ConfigureAwait(false);
    var container = containers.FirstOrDefault(c => c.Names.Any(name => name.Equals("/cloud-provider-kind", StringComparison.OrdinalIgnoreCase)));
    Console.WriteLine("Deleting container \"cloud-provider-kind\"...");
    if (container == null)
    {
      Console.WriteLine("Deleted containers: []");
      return;
    }
    _ = await dockerClient.Containers.StopContainerAsync(
      container.ID,
      new ContainerStopParameters(),
      cancellationToken
    ).ConfigureAwait(false);
    Console.WriteLine("Deleted containers: [\"cloud-provider-kind\"]");
  }
}
