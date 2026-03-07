using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using System.Security.Cryptography.X509Certificates;

namespace BeyondTrust.SecretSafeProvider;

public static class CertificateGenerator
{
    public static X509Certificate2 GenerateSelfSignedCertificate(string subjectName, string issuerName)
    {
        var random = new SecureRandom();

        // Generate RSA 2048 key pair — pure BouncyCastle, no OpenSSL
        var keyGen = new RsaKeyPairGenerator();
        keyGen.Init(new RsaKeyGenerationParameters(BigInteger.ValueOf(65537), random, 2048, 80));
        var keyPair = keyGen.GenerateKeyPair();

        var certGen = new X509V3CertificateGenerator();
        certGen.SetSerialNumber(BigInteger.ProbablePrime(120, random));
        certGen.SetSubjectDN(new X509Name(subjectName));
        certGen.SetIssuerDN(new X509Name(issuerName));
        certGen.SetNotBefore(DateTime.UtcNow.AddMinutes(-5));
        certGen.SetNotAfter(DateTime.UtcNow.AddYears(2));
        certGen.SetPublicKey(keyPair.Public);

        certGen.AddExtension(X509Extensions.BasicConstraints, true, new BasicConstraints(false));
        certGen.AddExtension(X509Extensions.KeyUsage, false,
            new KeyUsage(KeyUsage.DigitalSignature | KeyUsage.KeyEncipherment));
        certGen.AddExtension(X509Extensions.ExtendedKeyUsage, false,
            new ExtendedKeyUsage(KeyPurposeID.id_kp_serverAuth));
        certGen.AddExtension(X509Extensions.SubjectAlternativeName, false,
            new GeneralNames([
                new GeneralName(GeneralName.DnsName, "localhost"),
                new GeneralName(GeneralName.IPAddress, "127.0.0.1"),
            ]));

        var cert = certGen.Generate(new Asn1SignatureFactory("SHA256WithRSA", keyPair.Private, random));

        // Pack into PKCS12 so X509Certificate2 gets both cert + private key
        var store = new Pkcs12StoreBuilder().Build();
        var certEntry = new X509CertificateEntry(cert);
        store.SetCertificateEntry("cert", certEntry);
        store.SetKeyEntry("cert", new AsymmetricKeyEntry(keyPair.Private), [certEntry]);

        using var ms = new MemoryStream();
        store.Save(ms, [], random);

        return X509CertificateLoader.LoadPkcs12(ms.ToArray(), null, X509KeyStorageFlags.Exportable);
    }
}
