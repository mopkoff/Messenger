using System;
using System.Data;
using System.Data.SqlClient;
using Messenger.Model;

namespace Messenger.DataLayer.SqlServer
{
    public class UsersRepository : IUsersRepository
    {
        private readonly string _connectionString;
        public IChatsRepository ChatsRepository { get; set; }


        public UsersRepository(string connectionString, IChatsRepository chatsRepository)
        {
            _connectionString = connectionString;
            ChatsRepository = chatsRepository;
        }

        public UsersRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public User CreateUser(User user)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                if (SqlHelper.DoesFieldValueExist(connection, "Users", "Login", user.Login, SqlDbType.VarChar, user.Login.Length))
                    throw new Exception("Login already in use");
                using (var transaction = connection.BeginTransaction())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.CommandText =
                            "INSERT INTO [Users] ([Login], [Password]) OUTPUT INSERTED.[ID] VALUES (@login, @password)";

                        command.Parameters.AddWithValue("@login", user.Login);
                        command.Parameters.AddWithValue("@password", user.Password);

                        user.Id = (int)command.ExecuteScalar();
                    }
                    /*
                    using (var command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.CommandText =
                            "INSERT INTO [UserInfo] ([UserID]) VALUES " +
                            "(@userId)";

                        command.Parameters.AddWithValue("@userId", user.Id);

                        command.ExecuteNonQuery();
                    }*/
                    transaction.Commit();

                    return user;
                }
            }
        }

        public void DeleteUser(int userId)
        {
            // Default (deleted) user check
            if (userId == 0)
                return;

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                //Check if id exists
                if (!SqlHelper.DoesFieldValueExist(connection, "Users", "ID", userId, SqlDbType.Int))
                    return;
                // Just delete the user, everything else will cascade delete
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM [Users] WHERE [ID] = @userId";
                    command.Parameters.AddWithValue("@userId", userId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public User GetUser(int userId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                if (!SqlHelper.DoesFieldValueExist(connection, "Users", "ID", userId, SqlDbType.Int))
                    return null;
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [ID], [Login], [Password] FROM [Users] WHERE [ID] = @userId";
                    command.Parameters.AddWithValue("@userId", userId);

                    var user = new User();
                    using (var reader = command.ExecuteReader())
                    {
                        reader.Read();
                        user.Id = reader.GetInt32(reader.GetOrdinal("ID"));
                        user.Login = reader.GetString(reader.GetOrdinal("Login"));
                        user.Password = reader.GetString(reader.GetOrdinal("Password"));
                    }
                    return user;
                }
            }
        }
                      
        public User PersistUser(User user)
        {
            if (user == null)
                return null;
            // Check for default user change
            if (user.Id == 0)
                return null;

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var idExists = SqlHelper.DoesFieldValueExist(connection, "Users", "ID", user.Id, SqlDbType.Int);
                // If ID does not exist, but username exists, then we cannot create a new user!
                if (!idExists && SqlHelper.DoesFieldValueExist(connection, "Users", "Username", user.Login, SqlDbType.VarChar,
                        user.Login.Length))
                    return null;
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = idExists ?
                        "UPDATE [Users] SET [Username] = @userName, [Password] = @password WHERE [ID] = @userId"
                        : "INSERT INTO [Users] ([Username], [Password]) OUTPUT INSERTED.[ID] VALUES (@userName, @password)";

                    if (idExists)
                        command.Parameters.AddWithValue("@userId", user.Id);
                    command.Parameters.AddWithValue("@userName", user.Login);
                    command.Parameters.AddWithValue("@password", user.Password);

                    if (idExists)
                        command.ExecuteNonQuery();
                    else
                        user.Id = (int)command.ExecuteScalar();

                    return user;
                }

            }
        }

        public void SetPassword(int userId, string newHash)
        {
            if (userId == 0)
                return;
            if (string.IsNullOrEmpty(newHash))
                return;

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                if (!SqlHelper.DoesFieldValueExist(connection, "Users", "ID", userId, SqlDbType.Int))
                    return;
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "UPDATE [Users] SET [Password] = @password WHERE [ID] = @userId";

                    command.Parameters.AddWithValue("@userId", userId);
                    command.Parameters.AddWithValue("@password", newHash);

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}