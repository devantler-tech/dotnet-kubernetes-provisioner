namespace Devantler.KubernetesProvisioner.Cluster.Core.Tests;

/// <summary>
/// Tests for the <see cref="KubernetesClusterProvisionerException"/> class.
/// </summary>
public class KubernetesClusterProvisionerExceptionTests
{
  /// <summary>
  /// Tests the default constructor of the <see cref="KubernetesClusterProvisionerException"/> class.
  /// </summary>
  [Fact]
  public void KubernetesClusterProvisonerExceptionTests_DefaultConstructor_ShouldCreateInstance()
  {
    // Act
    var exception = new KubernetesClusterProvisionerException();

    // Assert
    Assert.NotNull(exception);
    _ = Assert.IsType<KubernetesClusterProvisionerException>(exception);
  }

  /// <summary>
  /// Tests the constructor of the <see cref="KubernetesClusterProvisionerException"/> class with a message.
  /// </summary>
  [Fact]
  public void KubernetesClusterProvisonerExceptionTests_ConstructorWithMessage_ShouldSetMessage()
  {
    // Arrange
    string message = "Test message";

    // Act
    var exception = new KubernetesClusterProvisionerException(message);

    // Assert
    Assert.NotNull(exception);
    Assert.Equal(message, exception.Message);
  }

  /// <summary>
  /// Tests the constructor of the <see cref="KubernetesClusterProvisionerException"/> class with a message and an inner exception.
  /// </summary>
  [Fact]
  public void KubernetesClusterProvisonerExceptionTests_ConstructorWithMessageAndInnerException_ShouldSetMessageAndInnerException()
  {
    // Arrange
    string message = "Test message";
    var innerException = new KubernetesClusterProvisionerException("Inner exception");

    // Act
    var exception = new KubernetesClusterProvisionerException(message, innerException);

    // Assert
    Assert.NotNull(exception);
    Assert.Equal(message, exception.Message);
    Assert.Equal(innerException, exception.InnerException);
  }
}
