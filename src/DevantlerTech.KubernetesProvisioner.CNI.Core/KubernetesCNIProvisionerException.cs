namespace DevantlerTech.KubernetesProvisioner.CNI.Core;

/// <summary>
/// Represents an exception that is thrown when an error occurs in a Kubernetes CNI provisioner.
/// </summary>
[Serializable]
public class KubernetesCNIProvisionerException : Exception
{
  /// <summary>
  /// Initializes a new instance of the <see cref="KubernetesCNIProvisionerException"/> class.
  /// </summary>
  public KubernetesCNIProvisionerException()
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="KubernetesCNIProvisionerException"/> class with a specified error message.
  /// </summary>
  /// <param name="message"></param>
  public KubernetesCNIProvisionerException(string? message) : base(message)
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="KubernetesCNIProvisionerException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
  /// </summary>
  /// <param name="message"></param>
  /// <param name="innerException"></param>
  public KubernetesCNIProvisionerException(string? message, Exception? innerException) : base(message, innerException)
  {
  }
}
