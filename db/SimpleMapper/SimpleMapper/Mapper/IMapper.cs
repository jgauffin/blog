using System.Data;
using System.Data.Common;
using Microsoft.VisualBasic.CompilerServices;

namespace AdoNet.Mapper;

public interface IMapper<in TEntity>
{
    string TableName { get; set; }
    Task Insert(DbCommand command, TEntity entity);
    Task Update(DbCommand command, TEntity entity);
    Task Delete(DbCommand command, TEntity entity);
    void Map(IDataRecord record, TEntity entity);
}