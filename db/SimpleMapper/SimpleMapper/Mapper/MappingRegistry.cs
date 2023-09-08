namespace AdoNet.Mapper;

public class MappingRegistry
{
    internal static MappingRegistry Instance = new();
    private readonly Dictionary<Type, object> _mappings = new();

    public IMapper<TEntity> GetMapper<TEntity>()
    {
        lock (_mappings)
        {
            if (_mappings.TryGetValue(typeof(TEntity), out var mapper))
                return (IMapper<TEntity>)mapper;

            mapper = new SimpleMapper<TEntity>();
            _mappings.Add(typeof(TEntity), mapper);
            return (IMapper<TEntity>)mapper;
        }
    }
}