using System.Data;

namespace RMitra.Application.Abstractions;

public interface ISqlConnectionFactory
{
    IDbConnection Create();
}
