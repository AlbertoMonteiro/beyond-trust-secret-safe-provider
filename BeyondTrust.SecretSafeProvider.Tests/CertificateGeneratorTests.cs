using BeyondTrust.SecretSafeProvider;
using System.Security.Cryptography.X509Certificates;

namespace BeyondTrust.SecretSafeProvider.Tests;

public class CertificateGeneratorTests
{
    private const string SubjectName = "CN=test-provider";
    private const string IssuerName = "CN=test-provider";

    [Test]
    public async Task GenerateSelfSignedCertificate_ReturnsNonNullCertificate()
    {
        // Act
        var cert = CertificateGenerator.GenerateSelfSignedCertificate(SubjectName, IssuerName);

        // Assert
        await Assert.That(cert).IsNotNull();
    }

    [Test]
    public async Task GenerateSelfSignedCertificate_CertificateHasSubjectName()
    {
        // Act
        var cert = CertificateGenerator.GenerateSelfSignedCertificate(SubjectName, IssuerName);

        // Assert
        await Assert.That(cert.Subject).IsEqualTo(SubjectName);
    }

    [Test]
    public async Task GenerateSelfSignedCertificate_CertificateHasPrivateKey()
    {
        // Act
        var cert = CertificateGenerator.GenerateSelfSignedCertificate(SubjectName, IssuerName);

        // Assert
        await Assert.That(cert.HasPrivateKey).IsTrue();
    }

    [Test]
    public async Task GenerateSelfSignedCertificate_CertificateIsCurrentlyValid()
    {
        // Act
        var cert = CertificateGenerator.GenerateSelfSignedCertificate(SubjectName, IssuerName);
        var now = DateTime.UtcNow;

        // Assert
        await Assert.That(cert.NotBefore.ToUniversalTime()).IsLessThanOrEqualTo(now);
        await Assert.That(cert.NotAfter.ToUniversalTime()).IsGreaterThan(now);
    }

    [Test]
    public async Task GenerateSelfSignedCertificate_CertificateExpiresInApproximatelyTwoYears()
    {
        // Arrange
        var expectedExpiry = DateTimeOffset.UtcNow.AddYears(2);

        // Act
        var cert = CertificateGenerator.GenerateSelfSignedCertificate(SubjectName, IssuerName);

        // Assert — allow ±5 minutes tolerance
        await Assert.That(cert.NotAfter.ToUniversalTime())
            .IsGreaterThan(expectedExpiry.AddMinutes(-5).UtcDateTime)
            .And.IsLessThan(expectedExpiry.AddMinutes(5).UtcDateTime);
    }

    [Test]
    public async Task GenerateSelfSignedCertificate_CertificateUsesRsa2048()
    {
        // Act
        var cert = CertificateGenerator.GenerateSelfSignedCertificate(SubjectName, IssuerName);
        using var rsa = cert.GetRSAPublicKey();

        // Assert
        await Assert.That(rsa).IsNotNull();
        await Assert.That(rsa!.KeySize).IsEqualTo(2048);
    }

    [Test]
    public async Task GenerateSelfSignedCertificate_CertificateIsExportableAsDer()
    {
        // Act
        var cert = CertificateGenerator.GenerateSelfSignedCertificate(SubjectName, IssuerName);
        var derBytes = cert.Export(X509ContentType.Cert);

        // Assert
        await Assert.That(derBytes).IsNotNull();
        await Assert.That(derBytes.Length).IsGreaterThan(0);
    }
}
