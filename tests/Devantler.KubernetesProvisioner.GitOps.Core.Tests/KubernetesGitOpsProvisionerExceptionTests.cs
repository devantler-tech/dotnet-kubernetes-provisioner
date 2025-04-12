namespace Devantler.KubernetesProvisioner.GitOps.Core.Tests;

/// <summary>
/// Tests for the <see cref="KubernetesGitOpsProvisionerException"/> class.
/// </summary>
internal class KubernetesGitOpsProvisionerExceptionTests
{
  /// <summary>
  /// Tests the default constructor of the <see cref="KubernetesGitOpsProvisionerException"/> class.
  /// </summary>
  [Fact]
  public void KubernetesGitOpsProvisonerExceptionTests_DefaultConstructor_ShouldCreateInstance()
  {
    // Act
    var exception = new KubernetesGitOpsProvisionerException();

    // Assert
    Assert.NotNull(exception);
    _ = Assert.IsType<KubernetesGitOpsProvisionerException>(exception);
  }

  /// <summary>
  /// Tests the constructor of the <see cref="KubernetesGitOpsProvisionerException"/> class with a message.
  /// </summary>
  [Fact]
  public void KubernetesGitOpsProvisonerExceptionTests_ConstructorWithMessage_ShouldSetMessage()
  {
    // Arrange
    string message = "Test message";

    // Act
    var exception = new KubernetesGitOpsProvisionerException(message);

    // Assert
    Assert.NotNull(exception);
    Assert.Equal(message, exception.Message);
  }

  /// <summary>
  /// Tests the constructor of the <see cref="KubernetesGitOpsProvisionerException"/> class with a message and an inner exception.
  /// </summary>
  [Fact]
  public void KubernetesGitOpsProvisonerExceptionTests_ConstructorWithMessageAndInnerException_ShouldSetMessageAndInnerException()
  {
    // Arrange
    string message = "Test message";
    var innerException = new KubernetesGitOpsProvisionerException("Inner exception");

    // Act
    var exception = new KubernetesGitOpsProvisionerException(message, innerException);

    // Assert
    Assert.NotNull(exception);
    Assert.Equal(message, exception.Message);
    Assert.Equal(innerException, exception.InnerException);
  }
}
