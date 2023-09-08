using System.Data;
using System.Data.Common;

namespace AdoNet.Mapper;

public static class DbCommandExtensions
{
    public static IDbDataParameter AddParameter(this IDbCommand command, string parameterName, object value)
    {
        var p = command.CreateParameter();
        p.ParameterName = parameterName;
        p.Value = value;
        command.Parameters.Add(p);
        return p;
    }

    public static DbCommand CreateDbCommand(this IDbConnection connection)
    {
        var cmd = connection.CreateCommand();
        return (DbCommand)cmd;
    }

    public static async Task<TEntity> GetOne<TEntity>(this DbCommand command) where TEntity : new()
    {
        var mapper = MappingRegistry.Instance.GetMapper<TEntity>();

        var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            throw new InvalidOperationException("Failed to get entity "  + command.Parameters[0].Value);
        }

        var entity = new TEntity();
        mapper.Map(reader, entity);
        return entity;
    }

    public static async Task<List<TEntity>> ToList<TEntity>(this DbCommand command) where TEntity : new()
    {
        var mapper = MappingRegistry.Instance.GetMapper<TEntity>();

        await using var reader = await command.ExecuteReaderAsync();
        var items = new List<TEntity>();

        while (await reader.ReadAsync())
        {
            var entity = new TEntity();
            mapper.Map(reader, entity);
            items.Add(entity);
        }

        return items;
    }
}