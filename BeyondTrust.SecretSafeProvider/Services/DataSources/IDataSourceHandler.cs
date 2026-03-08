using BeyondTrust.SecretSafeProvider.Proto;

namespace BeyondTrust.SecretSafeProvider.Services.DataSources;

public interface IDataSourceHandler
{
    string TypeName { get; }
    Schema GetSchema();
    Task<ReadDataSource.Types.Response> ReadAsync(ReadDataSource.Types.Request request);
}
