using System.Security.Cryptography.X509Certificates;

namespace BeyondTrust.SecretSafeProvider;

public static class CertificateGenerator
{
    private const int KeyStrength = 2048;

    public static X509Certificate2 GenerateSelfSignedCertificate(string subjectName, string issuerName)
    {
        var certB64 = """
            Code to generate this certificate

            [Subject]
            CN=127.0.0.1

            [Issuer]
            CN=root ca

            [Serial Number]
            VALUE

            [Not Before]
            07/12/2024 21:00:00

            [Not After]
            07/12/2026 21:00:00

            [Thumbprint]
            SOMEThumbprint

            [Nome Alternativo Para o Requerente]
            Nome DNS=localhost
            """;

        var cert = Convert.FromBase64String(certB64);

        return X509CertificateLoader.LoadPkcs12(cert, null);
    }
}
