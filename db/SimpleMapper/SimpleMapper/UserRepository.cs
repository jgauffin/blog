using System.Data;
using AdoNet.Mapper;

namespace AdoNet;

public class UserRepository : IUserRepository
{
    private readonly IDbTransaction _transaction;
    private readonly IMapper<User> _mapper;

    public UserRepository(IDbTransaction transaction)
    {
        _transaction = transaction;
        _mapper = MappingRegistry.Instance.GetMapper<User>();
    }

    public async Task<User> GetById(int id)
    {
        await using var cmd = _transaction.CreateCommand();

        cmd.CommandText = "SELECT * FROM Users WHERE Id = @id";
        cmd.AddParameter("Id", id);
        return await cmd.GetOne<User>();
    }

    public async Task<List<User>> List()
    {
        await using var cmd = _transaction.CreateCommand();

        cmd.CommandText = "SELECT * FROM Users";
        cmd.AddParameter("userId", 10);
        return await cmd.ToList<User>();
    }

    public async Task Create(User entity)
    {
        await using var cmd = _transaction.CreateCommand();
        await _mapper.Insert(cmd, entity);
    }

    public async Task Update(User entity)
    {
        await using var cmd = _transaction.CreateCommand();
        await _mapper.Update(cmd, entity);
    }

    public async Task Delete(User entity)
    {
        await using var cmd = _transaction.CreateCommand();
        await _mapper.Delete(cmd, entity);
    }
}

public interface IUserRepository : ICrudOperations<User>
{
    Task<User> GetById(int id);
    Task<List<User>> List();
}

public interface ICrudOperations<in TEntity>
{
    Task Create(TEntity entity);
    Task Delete(TEntity entity);
    Task Update(TEntity entity);
}