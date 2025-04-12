namespace Devantler.KubernetesProvisioner.Deployment.Core;

/// <summary>
/// Represents an exception that is thrown when an error occurs in a Kubernetes deployment tool provisioner.
/// </summary>
[Serializable]
public class KubernetesDeploymentToolProvisionerException : Exception
{
  /// <summary>
  /// Initializes a new instance of the <see cref="KubernetesDeploymentToolProvisionerException"/> class.
  /// </summary>
  public KubernetesDeploymentToolProvisionerException()
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="KubernetesDeploymentToolProvisionerException"/> class with a specified error message.
  /// </summary>
  /// <param name="message"></param>
  public KubernetesDeploymentToolProvisionerException(string? message) : base(message)
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="KubernetesDeploymentToolProvisionerException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
  /// </summary>
  /// <param name="message"></param>
  /// <param name="innerException"></param>
  public KubernetesDeploymentToolProvisionerException(string? message, Exception? innerException) : base(message, innerException)
  {
  }
}
