using System.Data;
using System.Data.Common;
using System.Reflection;

namespace AdoNet.Mapper;

public class SimpleMapper<TEntity> : IMapper<TEntity>
{
    private readonly List<MethodInfo> _propertySetMethods = new();
    private readonly Dictionary<string, MethodInfo> _propertyGetMethods = new();
    private bool _firstPass = true;
    private string _insertStatement = "";
    private string _updateStatement = "";
    private MethodInfo _idSetter = null!;
    private MethodInfo _idGetter = null!;
    private string _tableName;

    public SimpleMapper()
    {
        _tableName = typeof(TEntity).Name;
    }

    public string TableName
    {
        get => _tableName;
        set
        {
            _tableName = value;
            CreateCrudMapping();
        }
    }

    public async Task Insert(DbCommand command, TEntity entity)
    {
        if (_insertStatement == "")
        {
            CreateCrudMapping();
        }

        command.CommandText = _insertStatement + ";SELECT cast(SCOPE_IDENTITY() as int)";
        foreach (var kvp in _propertyGetMethods)
        {
            if (kvp.Key == "Id")
            {
                continue;
            }

            command.AddParameter(kvp.Key, kvp.Value.Invoke(entity, null) ?? DBNull.Value);
        }

        var id = await command.ExecuteScalarAsync();
        _idSetter.Invoke(entity, new[] { id });
    }

    public async Task Update(DbCommand command, TEntity entity)
    {
        if (_updateStatement == "")
        {
            CreateCrudMapping();
        }

        command.CommandText = _updateStatement;
        foreach (var kvp in _propertyGetMethods)
        {
            command.AddParameter(kvp.Key, kvp.Value.Invoke(entity, null) ?? DBNull.Value);
        }

        command.AddParameter("Id", _idGetter.Invoke(entity, null) ?? DBNull.Value);
        await command.ExecuteNonQueryAsync();
    }

    public async Task Delete(DbCommand command, TEntity entity)
    {
        if (_updateStatement == "")
        {
            CreateCrudMapping();
        }

        command.CommandText = $"DELETE FROM {TableName} WHERE Id = @Id";
        command.AddParameter("Id", _idGetter.Invoke(entity, null) ?? DBNull.Value);
        await command.ExecuteNonQueryAsync();
    }

    public void Map(IDataRecord record, TEntity entity)
    {
        if (_firstPass)
        {
            _firstPass = false;
            CreateMapping(record);
        }

        for (var i = 0; i < _propertySetMethods.Count; i++)
        {
            var value = record[i];
            var setter = _propertySetMethods[i];
            setter.Invoke(entity, new[] { value });
        }
    }

    private void CreateCrudMapping()
    {
        _propertyGetMethods.Clear();

        // These two are for INSERT statements. Columns and values
        var cols = "";
        var values = "";

        // Column assignment for UPDATE statements.
        var updateLine = "";

        foreach (var property in typeof(TEntity).GetProperties())
        {
            if (property.Name == "Id")
            {
                // We need the Id setter for the auto-incremented key
                _idSetter = property.GetSetMethod()!;

                // And the getter for the UPDATE WHERE clause
                _idGetter = property.GetGetMethod()!;
                continue;
            }

            cols += $"{property.Name}, ";
            values += $"@{property.Name}, ";
            updateLine += $"{property.Name} = @{property.Name}, ";

            // Need all properties for 
            _propertyGetMethods.Add(property.Name, property.GetGetMethod()!);
        }

        _updateStatement = $"UPDATE {_tableName} SET {updateLine} WHERE Id = @Id";
        _insertStatement = $"INSERT INTO {_tableName} ({cols[..^2]}) VALUES({values[..^2]})";
    }

    private void CreateMapping(IDataRecord record)
    {
        var props = typeof(TEntity).GetProperties();
        for (var i = 0; i < record.FieldCount; i++)
        {
            var name = record.GetName(i);
            var prop = props.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (prop == null) continue;

            var setMethod = prop.GetSetMethod() ?? throw new InvalidOperationException(
                $"All properties must have a public getter. Fix {typeof(TEntity).Name}.{name}.");


            _propertySetMethods.Add(setMethod);
        }
    }
}