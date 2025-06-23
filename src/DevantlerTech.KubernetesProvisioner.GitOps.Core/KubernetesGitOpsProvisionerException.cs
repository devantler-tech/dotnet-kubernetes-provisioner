namespace DevantlerTech.KubernetesProvisioner.GitOps.Core;

/// <summary>
/// Represents an exception that is thrown when an error occurs in a Kubernetes GitOps provisioner.
/// </summary>
[Serializable]
public class KubernetesGitOpsProvisionerException : Exception
{
  /// <summary>
  /// Initializes a new instance of the <see cref="KubernetesGitOpsProvisionerException"/> class.
  /// </summary>
  public KubernetesGitOpsProvisionerException()
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="KubernetesGitOpsProvisionerException"/> class with a specified error message.
  /// </summary>
  /// <param name="message"></param>
  public KubernetesGitOpsProvisionerException(string? message) : base(message)
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="KubernetesGitOpsProvisionerException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
  /// </summary>
  /// <param name="message"></param>
  /// <param name="innerException"></param>
  public KubernetesGitOpsProvisionerException(string? message, Exception? innerException) : base(message, innerException)
  {
  }
}
