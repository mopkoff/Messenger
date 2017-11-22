using System;
using System.Data;
using System.Data.SqlClient;
using Messenger.Model;
using System.Drawing;
using Messenger.Model.Enums;
using System.Transactions;
using System.Threading.Tasks;
using System.Text;
using System.Collections.Generic;

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

                    using (var command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.CommandText =
                            "INSERT INTO [UserInfo] ([UserID]) VALUES " +
                            "(@userId)";

                        command.Parameters.AddWithValue("@userId", user.Id);

                        command.ExecuteNonQuery();
                    }
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

        public void UpdateUserInfo(User user, UserInfo userInfo)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                if (!SqlHelper.DoesFieldValueExist(connection, "Users", "Login", user.Login, SqlDbType.VarChar, user.Login.Length))
                    throw new Exception("User not exist");

                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "Update [UserInfo] " +
                        "Set FirstName = @firstName, Lastname = @lastName , Avatar = @avatar, " +
                        "About = @about, GenderType = @genderType WHERE [UserID] = @userId";

                    command.Parameters.AddWithValue("@userId", user.Id);
                    command.Parameters.AddWithValue("@firstName", userInfo.FirstName);
                    command.Parameters.AddWithValue("@lastName", userInfo.LastName);
                    command.Parameters.AddWithValue("@about", userInfo.About);
                    command.Parameters.AddWithValue("@genderType", (int)userInfo.Gender);
                    var avatar =
                                new SqlParameter("@avatar", SqlDbType.VarBinary, userInfo.GetAvatarAsByteArray().Length)
                                { Value = userInfo.GetAvatarAsByteArray() };
                    command.Parameters.Add(avatar);

                    command.ExecuteNonQuery();
                }
            }
        }

        public UserInfo GetUserInfo(int userId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                if (!SqlHelper.DoesFieldValueExist(connection, "Users", "ID", userId, SqlDbType.Int))
                    throw new Exception("User not exist");
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [FirstName], [LastName], [Avatar], [About], [GenderType] FROM [UserInfo] WHERE [UserID] = @userId";
                    command.Parameters.AddWithValue("@userId", userId);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.HasRows)
                            return null;
                        reader.Read();
                        UserInfo result = new UserInfo
                        {
                            FirstName = reader.IsDBNull(reader.GetOrdinal("FirstName")) ? null : reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.IsDBNull(reader.GetOrdinal("LastName")) ? null : reader.GetString(reader.GetOrdinal("LastName")),
                            About = reader.IsDBNull(reader.GetOrdinal("About")) ? null : reader.GetString(reader.GetOrdinal("About")),
                            Gender = reader.IsDBNull(reader.GetOrdinal("GenderType")) ? 0 : (GenderTypes)reader.GetInt32(reader.GetOrdinal("GenderType"))
                        };
                        result.SetAvatarUsingByteArray(reader.IsDBNull(reader.GetOrdinal("Avatar")) ? null : reader[reader.GetOrdinal("Avatar")] as byte[]);
                        return result;
                    }
                }
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Async version of <see cref="M:DotNetMessenger.DataLayer.SqlServer.UsersRepository.GetUserByUsername(System.String)" />
        /// </summary>
        /// <seealso cref="M:DotNetMessenger.DataLayer.SqlServer.UsersRepository.GetUserByUsername(System.String)" />
        public async Task<User> GetUserByLoginAsync(string login)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                if (!SqlHelper.DoesFieldValueExist(connection, "Users", "login", login, SqlDbType.VarChar))
                    throw new Exception("User not exist");
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [ID], [Login], [Password] FROM [Users] WHERE [Login] = @login";
                    command.Parameters.AddWithValue("@Login", login);

                    var user = new User();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (!reader.HasRows)
                            throw new ArgumentException();
                        await reader.ReadAsync();
                        user.Id = reader.GetInt32(reader.GetOrdinal("ID"));
                        user.Login = login;
                    }
                    return user.Id == 0 ? null : user;
                    
                }
            }     
        }

        public string GetPassword(int userId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                if (!SqlHelper.DoesFieldValueExist(connection, "Users", "ID", userId, SqlDbType.Int))
                    throw new Exception("User not exist");
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [Password] FROM [Users] WHERE [ID] = @userId";
                    command.Parameters.AddWithValue("@userId", userId);

                    string password;
                    using (var reader = command.ExecuteReader())
                    {
                        reader.Read();
                        password = reader.GetString(reader.GetOrdinal("Password"));
                    }
                    return password;
                }
            }
        }

        public User GetUser(int userId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                if (!SqlHelper.DoesFieldValueExist(connection, "Users", "ID", userId, SqlDbType.Int))
                    throw new Exception("User not exist");
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

        public User GetUserByLogin(string login)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                if (!SqlHelper.DoesFieldValueExist(connection, "Users", "login", login, SqlDbType.VarChar))
                    throw new Exception("User not exist");
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [ID], [Login], [Password] FROM [Users] WHERE [Login] = @login";
                    command.Parameters.AddWithValue("@Login", login);

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

        public IEnumerable<int> GetUserIdsByLogins(IEnumerable<string> users)
        {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                using (var command = connection.CreateCommand())
                {
                    StringBuilder stringBuilder = new StringBuilder();

                    string usersQueue = ListToString(users);

                    stringBuilder.Append("SELECT [ID] FROM [Users] WHERE [Login] IN (");
                    int i = 1;
                    foreach (var item in users)
                    {
                        command.Parameters.AddWithValue("@user" + i, item);
                        stringBuilder.Append("@user" + i + ", ");
                        ++i;
                    }
                    stringBuilder.Remove(stringBuilder.Length - 2, 2);
                    stringBuilder.Append(");");
                    command.CommandText = stringBuilder.ToString();
                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.HasRows)
                            yield break;
                        while (reader.Read())
                        {
                            yield return reader.GetInt32(reader.GetOrdinal("ID"));
                        }
                    }
                }
                }
            }
        public IEnumerable<User> GetUsersByPattern(string pattern)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    //command.CommandText = "SELECT [ID] FROM [Users] WHERE [Login] LIKE " + "'%" +pattern + "%'";
                    pattern += '%';
                    pattern = '%' + pattern;
                    command.CommandText = "SELECT [ID] FROM [Users] WHERE [Login] LIKE @pattern";
                    command.Parameters.AddWithValue("@pattern", pattern);
                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.HasRows)
                            yield break;
                        while (reader.Read())
                        {
                            yield return this.GetUser(reader.GetInt32(reader.GetOrdinal("ID")));
                        }
                    }
                }
            }
        }

        private string ListToString(IEnumerable<string> list)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append('(');            
            foreach (var item in list)
            {
                stringBuilder.Append(item);
                stringBuilder.Append(", ");
            }
            stringBuilder.Remove(stringBuilder.Length - 2, 2);
            stringBuilder.Append(')');
            return stringBuilder.ToString();
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
                if (!idExists && SqlHelper.DoesFieldValueExist(connection, "Users", "Login", user.Login, SqlDbType.VarChar,
                        user.Login.Length))
                    throw new Exception("User not exist");
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = idExists ?
                        "UPDATE [Users] SET [Login] = @login, [Password] = @password WHERE [ID] = @userId"
                        : "INSERT INTO [Users] ([Login], [Password]) OUTPUT INSERTED.[ID] VALUES (@login, @password)";

                    if (idExists)
                        command.Parameters.AddWithValue("@userId", user.Id);
                    command.Parameters.AddWithValue("@login", user.Login);
                    command.Parameters.AddWithValue("@password", user.Password);

                    if (idExists)
                        command.ExecuteNonQuery();
                    else
                        user.Id = (int)command.ExecuteScalar();

                    return user;
                }

            }
        }

        public void SetPassword(int userId, string newPassword)
        {
            if (userId == 0)
                return;
            if (string.IsNullOrEmpty(newPassword))
                return;
            string newHash = newPassword.GetHashCode().ToString();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                if (!SqlHelper.DoesFieldValueExist(connection, "Users", "ID", userId, SqlDbType.Int))
                    throw new Exception("User not exist");
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