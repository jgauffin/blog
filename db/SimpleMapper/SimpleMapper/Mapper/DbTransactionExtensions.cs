using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace AdoNet.Mapper;

public static class DbTransactionExtensions
{
    public static DbCommand CreateCommand(this IDbTransaction transaction)
    {
        var cmd = transaction.Connection!.CreateCommand();
        cmd.Transaction = transaction;
        return (DbCommand)cmd;
    }

public static async Task Insert<TEntity>(this IDbTransaction transaction, [DisallowNull] TEntity entity)
{
    if (entity == null) throw new ArgumentNullException(nameof(entity));
    var mapper = MappingRegistry.Instance.GetMapper<TEntity>();
    await using var cmd = transaction.CreateCommand();
    await mapper.Insert(cmd, entity);
}

public static async Task Update<TEntity>(this IDbTransaction transaction, [DisallowNull] TEntity entity)
{
    if (entity == null) throw new ArgumentNullException(nameof(entity));
    var mapper = MappingRegistry.Instance.GetMapper<TEntity>();
    await using var cmd = transaction.CreateCommand();
    await mapper.Update(cmd, entity);
}

public static async Task Delete<TEntity>(this IDbTransaction transaction, [DisallowNull] TEntity entity)
{
    if (entity == null) throw new ArgumentNullException(nameof(entity));
    var mapper = MappingRegistry.Instance.GetMapper<TEntity>();
    await using var cmd = transaction.CreateCommand();
    await mapper.Delete(cmd, entity);
}
}