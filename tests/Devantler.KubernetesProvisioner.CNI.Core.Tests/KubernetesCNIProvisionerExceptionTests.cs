namespace Devantler.KubernetesProvisioner.CNI.Core.Tests;

/// <summary>
/// Tests for the <see cref="KubernetesCNIProvisionerException"/> class.
/// </summary>
internal class KubernetesCNIProvisionerExceptionTests
{
  /// <summary>
  /// Tests the default constructor of the <see cref="KubernetesCNIProvisionerException"/> class.
  /// </summary>
  [Fact]
  public void KubernetesCNIProvisionerExceptionTests_DefaultConstructor_ShouldCreateInstance()
  {
    // Act
    var exception = new KubernetesCNIProvisionerException();

    // Assert
    Assert.NotNull(exception);
    _ = Assert.IsType<KubernetesCNIProvisionerException>(exception);
  }

  /// <summary>
  /// Tests the constructor of the <see cref="KubernetesCNIProvisionerException"/> class with a message.
  /// </summary>
  [Fact]
  public void KubernetesCNIProvisonerExceptionTests_ConstructorWithMessage_ShouldSetMessage()
  {
    // Arrange
    string message = "Test message";

    // Act
    var exception = new KubernetesCNIProvisionerException(message);

    // Assert
    Assert.NotNull(exception);
    Assert.Equal(message, exception.Message);
  }

  /// <summary>
  /// Tests the constructor of the <see cref="KubernetesCNIProvisionerException"/> class with a message and an inner exception.
  /// </summary>
  [Fact]
  public void KubernetesCNIProvisonerExceptionTests_ConstructorWithMessageAndInnerException_ShouldSetMessageAndInnerException()
  {
    // Arrange
    string message = "Test message";
    var innerException = new KubernetesCNIProvisionerException("Inner exception");

    // Act
    var exception = new KubernetesCNIProvisionerException(message, innerException);

    // Assert
    Assert.NotNull(exception);
    Assert.Equal(message, exception.Message);
    Assert.Equal(innerException, exception.InnerException);
  }
}
