namespace Devantler.KubernetesProvisioner.Cluster.Core;

/// <summary>
/// Represents an exception that is thrown when an error occurs in a Kubernetes cluster provisioner.
/// </summary>
[Serializable]
public class KubernetesClusterProvisionerException : Exception
{
  /// <summary>
  /// Initializes a new instance of the <see cref="KubernetesClusterProvisionerException"/> class.
  /// </summary>
  public KubernetesClusterProvisionerException()
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="KubernetesClusterProvisionerException"/> class with a specified error message.
  /// </summary>
  /// <param name="message"></param>
  public KubernetesClusterProvisionerException(string? message) : base(message)
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="KubernetesClusterProvisionerException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
  /// </summary>
  /// <param name="message"></param>
  /// <param name="innerException"></param>
  public KubernetesClusterProvisionerException(string? message, Exception? innerException) : base(message, innerException)
  {
  }
}
