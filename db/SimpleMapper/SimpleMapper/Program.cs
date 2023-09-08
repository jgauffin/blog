using System.Data.SqlClient;
using AdoNet;
using AdoNet.Mapper;

// Just customize the name
MappingRegistry.Instance.GetMapper<User>().TableName = "Users";

await using var connection = new SqlConnection("Server=;Database=AdoNetDemo;Trusted_Connection=True;");
await connection.OpenAsync();
await using var transaction = connection.BeginTransaction();

// Fetch a list of users by baking the command yourself
// Can easily be done with the mapping too.
await using var command = transaction.CreateCommand();
command.CommandText = "SELECT * FROM Users";
var users = await command.ToList<User>();
foreach (var entity in users)
{
    Console.WriteLine($"Id: {entity.Id}");
}


// Try an insert.
var user = new User
{
    FirstName = "Adam",
    LastName = "Petter",
    UserName = "nicklas"
};
await transaction.Insert(user);
Console.WriteLine($"Got id {user.Id} after insert :)");


//var repos = new UserRepository(transaction);
//var users = await repos.List();
