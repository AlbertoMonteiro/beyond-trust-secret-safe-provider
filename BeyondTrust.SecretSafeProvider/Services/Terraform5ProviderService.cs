using System.Text;
using Google.Protobuf;
using Grpc.Core;

namespace BeyondTrust.SecretSafeProvider.Services;

public class Terraform5ProviderService(ILogger<Terraform5ProviderService> logger) : Provider.ProviderBase
{
    private const string DataSourceName = "beyondtrust_hello";
    private readonly ILogger<Terraform5ProviderService> _logger = logger;

    public override Task<GetProviderSchema.Types.Response> GetSchema(GetProviderSchema.Types.Request request, ServerCallContext context)
    {
        var response = new GetProviderSchema.Types.Response
        {
            Provider = new Schema { Block = new Schema.Types.Block() },
        };

        response.DataSourceSchemas[DataSourceName] = new Schema
        {
            Block = new Schema.Types.Block
            {
                Attributes =
                {
                    new Schema.Types.Attribute
                    {
                        Name = "content",
                        Type = ByteString.CopyFromUtf8("\"string\""),
                        Computed = true,
                        Description = "Fixed content returned by the provider.",
                    }
                }
            }
        };

        return Task.FromResult(response);
    }

    public override Task<PrepareProviderConfig.Types.Response> PrepareProviderConfig(PrepareProviderConfig.Types.Request request, ServerCallContext context)
        => Task.FromResult(new PrepareProviderConfig.Types.Response { PreparedConfig = request.Config });

    public override Task<Configure.Types.Response> Configure(Configure.Types.Request request, ServerCallContext context)
        => Task.FromResult(new Configure.Types.Response());

    public override Task<ValidateDataSourceConfig.Types.Response> ValidateDataSourceConfig(ValidateDataSourceConfig.Types.Request request, ServerCallContext context)
        => Task.FromResult(new ValidateDataSourceConfig.Types.Response());

    public override Task<ValidateResourceTypeConfig.Types.Response> ValidateResourceTypeConfig(ValidateResourceTypeConfig.Types.Request request, ServerCallContext context)
        => Task.FromResult(new ValidateResourceTypeConfig.Types.Response());

    public override Task<UpgradeResourceState.Types.Response> UpgradeResourceState(UpgradeResourceState.Types.Request request, ServerCallContext context)
        => Task.FromResult(new UpgradeResourceState.Types.Response());

    public override Task<ReadResource.Types.Response> ReadResource(ReadResource.Types.Request request, ServerCallContext context)
        => Task.FromResult(new ReadResource.Types.Response { NewState = request.CurrentState });

    public override Task<PlanResourceChange.Types.Response> PlanResourceChange(PlanResourceChange.Types.Request request, ServerCallContext context)
        => Task.FromResult(new PlanResourceChange.Types.Response { PlannedState = request.ProposedNewState });

    public override Task<ApplyResourceChange.Types.Response> ApplyResourceChange(ApplyResourceChange.Types.Request request, ServerCallContext context)
        => Task.FromResult(new ApplyResourceChange.Types.Response { NewState = request.PlannedState });

    public override Task<ImportResourceState.Types.Response> ImportResourceState(ImportResourceState.Types.Request request, ServerCallContext context)
        => Task.FromResult(new ImportResourceState.Types.Response());

    public override Task<ReadDataSource.Types.Response> ReadDataSource(ReadDataSource.Types.Request request, ServerCallContext context)
    {
        _logger.LogInformation("ReadDataSource: {TypeName}", request.TypeName);

        return Task.FromResult(new ReadDataSource.Types.Response
        {
            State = new DynamicValue { Msgpack = ByteString.CopyFrom(MsgpackEncodeStringMap("content", "data from .NET 10")) }
        });
    }

    public override Task<Stop.Types.Response> Stop(Stop.Types.Request request, ServerCallContext context)
        => Task.FromResult(new Stop.Types.Response());

    // Encodes {"key": "value"} as msgpack fixmap with one fixstr→fixstr entry.
    private static byte[] MsgpackEncodeStringMap(string key, string value)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var valueBytes = Encoding.UTF8.GetBytes(value);
        var buf = new byte[1 + 1 + keyBytes.Length + 1 + valueBytes.Length];
        int i = 0;
        buf[i++] = 0x81;                              // fixmap(1)
        buf[i++] = (byte)(0xa0 | keyBytes.Length);   // fixstr(key.len)
        keyBytes.CopyTo(buf, i); i += keyBytes.Length;
        buf[i++] = (byte)(0xa0 | valueBytes.Length); // fixstr(value.len)
        valueBytes.CopyTo(buf, i);
        return buf;
    }
}
