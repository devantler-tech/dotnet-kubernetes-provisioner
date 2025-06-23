namespace DevantlerTech.KubernetesProvisioner.Deployment.Core.Tests;

/// <summary>
/// Tests for the <see cref="KubernetesDeploymentToolProvisionerException"/> class.
/// </summary>
public class KubernetesDeploymentToolProvisionerExceptionTests
{
  /// <summary>
  /// Tests the default constructor of the <see cref="KubernetesDeploymentToolProvisionerException"/> class.
  /// </summary>
  [Fact]
  public void KubernetesDeploymentToolProvisionerExceptionTests_DefaultConstructor_ShouldCreateInstance()
  {
    // Act
    var exception = new KubernetesDeploymentToolProvisionerException();

    // Assert
    Assert.NotNull(exception);
    _ = Assert.IsType<KubernetesDeploymentToolProvisionerException>(exception);
  }

  /// <summary>
  /// Tests the constructor of the <see cref="KubernetesDeploymentToolProvisionerException"/> class with a message.
  /// </summary>
  [Fact]
  public void KubernetesDeploymentToolProvisionerExceptionTests_ConstructorWithMessage_ShouldSetMessage()
  {
    // Arrange
    string message = "Test message";

    // Act
    var exception = new KubernetesDeploymentToolProvisionerException(message);

    // Assert
    Assert.NotNull(exception);
    Assert.Equal(message, exception.Message);
  }

  /// <summary>
  /// Tests the constructor of the <see cref="KubernetesDeploymentToolProvisionerException"/> class with a message and an inner exception.
  /// </summary>
  [Fact]
  public void KubernetesDeploymentToolProvisionerExceptionTests_ConstructorWithMessageAndInnerException_ShouldSetMessageAndInnerException()
  {
    // Arrange
    string message = "Test message";
    var innerException = new KubernetesDeploymentToolProvisionerException("Inner exception");

    // Act
    var exception = new KubernetesDeploymentToolProvisionerException(message, innerException);

    // Assert
    Assert.NotNull(exception);
    Assert.Equal(message, exception.Message);
    Assert.Equal(innerException, exception.InnerException);
  }
}
