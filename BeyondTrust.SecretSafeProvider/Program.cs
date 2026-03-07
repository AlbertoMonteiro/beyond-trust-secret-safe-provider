using System.Net;
using BeyondTrust.SecretSafeProvider;
using BeyondTrust.SecretSafeProvider.Services;
// #if DEBUG
// var builder = WebApplication.CreateBuilder(args);
// #else
var builder = WebApplication.CreateSlimBuilder(args);
// #endif

builder.Logging.ClearProviders();

var certificate = CertificateGenerator.GenerateSelfSignedCertificate("CN=127.0.0.1", "CN=root ca");

builder.WebHost.ConfigureKestrel(x =>
    x.Listen(IPAddress.Loopback, 0, x => x.UseHttps(x =>
    {
        x.ServerCertificate = certificate;

        x.AllowAnyClientCertificate();
    })));

// Add services to the container.
builder.Services.AddGrpc();

var app = builder.Build();

app.Lifetime.ApplicationStarted.Register(() =>
{
    // Write directly to the raw stdout stream to avoid any TextWriter CR/LF translation.
    // go-plugin's bufio.Scanner strips \n but leaves \r, which would corrupt the base64 cert.
    var address = new Uri(app.Urls.First()).GetComponents(UriComponents.HostAndPort, UriFormat.UriEscaped);
    // go-plugin uses base64.RawStdEncoding (no padding), so strip trailing '=' chars.
    var cert = Convert.ToBase64String(certificate.RawData).Replace("\r", "").Replace("\n", "").TrimEnd('=');
    var line = $"1|5|tcp|{address}|grpc|{cert}\n";
    var bytes = System.Text.Encoding.ASCII.GetBytes(line);

    using var stdout = Console.OpenStandardOutput();
    stdout.Write(bytes);
    stdout.Flush();
});

// Configure the HTTP request pipeline.
app.MapGrpcService<Terraform5ProviderService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
