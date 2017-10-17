using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Transactions;
using Messenger.DataLayer;
using Messenger.Model;
using Messenger.Model.Enums;


namespace Messenger.DataLayer.SqlServer
{
    public class ChatsRepository : IChatsRepository
    {
        private readonly string _connectionString;
        public IUsersRepository UsersRepository { get; set; }

        public ChatsRepository(string connectionString, IUsersRepository usersRepository)
        {
            _connectionString = connectionString;
            UsersRepository = usersRepository;
        }

        public ChatsRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void AddUser(int chatId, int userId)
        {
            // check for default user
            if (userId == 0)
                return;
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    
                    if (SqlHelper.DoesDoubleKeyExist(connection, "Chats", "[ID]", chatId, "[ChatType]",
                        (int)ChatTypes.Dialog))
                        return;
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "INSERT INTO [ChatsUsers]([UserID], [ChatID]) VALUES " +
                                              "(@userId, @chatId)";
                        command.Parameters.AddWithValue("@userId", userId);
                        command.Parameters.AddWithValue("@chatId", chatId);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException) { }
        }

        public void AddUsers(int chatId, IEnumerable<int> newUsers)
        {
            if (newUsers == null)
                return;
            var idList = newUsers as int[] ?? newUsers.ToArray();
            if (idList.Contains(0))
                return;
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    if (SqlHelper.DoesDoubleKeyExist(connection, "Chats", "[ID]", chatId, "[ChatType]",
                        (int)ChatTypes.Dialog))
                        return;
                    using (var transaction = connection.BeginTransaction())
                    {
                        foreach (var userId in idList)
                        {
                            using (var command = connection.CreateCommand())
                            {
                                command.Transaction = transaction;
                                command.CommandText = "INSERT INTO [ChatsUsers]([UserID], [ChatID]) VALUES " +
                                                                             "(@userId, @chatId)";
                                command.Parameters.AddWithValue("@userId", userId);
                                command.Parameters.AddWithValue("@chatId", chatId);

                                command.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                    
                }
            }
            catch (SqlException) { }
        }

        public Chat GetChat(int chatId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "SELECT [ChatType], [CreatorID] FROM [Chats] WHERE [ID] = @chatId";

                    command.Parameters.AddWithValue("@chatId", chatId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.HasRows)
                            return null;
                        reader.Read();
                        return new Chat
                        {
                            Id = chatId,
                            ChatType = (ChatTypes)reader.GetInt32(reader.GetOrdinal("ChatType")),
                            CreatorId = reader.GetInt32(reader.GetOrdinal("CreatorID")),
                            Members = GetChatUsers(chatId)
                    };
                    }
                }
            }
        }

        private Chat CreateChat(IEnumerable<int> members, string chatName, ChatTypes chatType)
        {
            if (members == null)
                return null;
            var membersList = members as int[] ?? members.ToArray();
            // check for default user
            if (membersList.Contains(0))
                return null;

            try
            {
                using (var scope = new TransactionScope())
                {
                    using (var connection = new SqlConnection(_connectionString))
                    {
                        connection.Open();
                        
                        var userIds = members as int[] ?? membersList.ToArray();

                        var chat = new Chat()
                        {
                            ChatType = chatType,
                            CreatorId = userIds[0],
                            Name = chatName
                        };

                        // Create new chat
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText =
                                "INSERT INTO [Chats] ([ChatType], [CreatorID], [Name]) OUTPUT INSERTED.[ID] VALUES (@chatType, @creatorId, @chatName)";

                            command.Parameters.AddWithValue("@chatType", (int)(chat.ChatType));
                            command.Parameters.AddWithValue("@creatorId", chat.CreatorId);
                            command.Parameters.AddWithValue("@chatName", chat.Name);

                            chat.Id = (int)command.ExecuteScalar();
                        }

                        // Add users to chat
                        foreach (var userId in userIds)
                        {
                            using (var command = connection.CreateCommand())
                            {
                                command.CommandText =
                                    "INSERT INTO [ChatsUsers] ([UserID], [ChatID]) VALUES (@userId, @chatId)";

                                command.Parameters.AddWithValue("@userId", userId);
                                command.Parameters.AddWithValue("@chatId", chat.Id);

                                command.ExecuteNonQuery();
                            }
                        }

                        scope.Complete();
                        chat.Members = userIds.Select(x => UsersRepository.GetUser(x));
                        return chat;
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        public Chat CreateGroupChat(IEnumerable<int> members, string title)
        {
            return CreateChat(members, title, ChatTypes.GroupChat);
        }

        public Chat CreateDialog(int member1, int member2)
        {
            UsersRepository u = new UsersRepository(_connectionString);
            // string name1 = u.GetUser(member1).Login;
            //string name2 = u.GetUser(member2).Login;
            //return CreateChat(new[] { member1, member2 }, "Чат " + name1 + " и " + name2 , ChatTypes.Dialog);
            return CreateChat(new[] { member1, member2 }, string.Empty, ChatTypes.Dialog);
        }

        public void DeleteChat(int chatId)
        {

            using (var connection = new SqlConnection(_connectionString))
            {

                connection.Open();
                // Delete chat, ChatUsers entries should cascade
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM [Chats] WHERE [Chats].[ID] = @chatId";

                    command.Parameters.AddWithValue("@chatId", chatId);

                    command.ExecuteNonQuery();
                }
            }

        }

        public IEnumerable<Chat> GetUserChats(int userId)
        {
            if (userId == 0)
                yield break;
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // get chat ids
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "SELECT [Chats].[ID] ID, [Chats].[ChatType] ChatType, [Chats].[CreatorID] CreatorID, [Chats].[Name] Name " +
                        "FROM [ChatsUsers], [Chats] WHERE [Chats].[ID] = [ChatsUsers].[ChatID] AND [ChatsUsers].[UserID] = @userId";

                    command.Parameters.AddWithValue("@userId", userId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.HasRows)
                            yield break;
                        while (reader.Read())
                        {
                            yield return new Chat
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("ID")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                ChatType = (ChatTypes)reader.GetInt32(reader.GetOrdinal("ChatType")),
                                CreatorId = reader.GetInt32(reader.GetOrdinal("CreatorID")),
                            };
                        }
                    }
                }
            }
        }
        public IEnumerable<User> GetChatUsers(int chatId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [UserID] FROM [ChatsUsers] WHERE [ChatsUsers].[ChatID] = @chatId";

                    command.Parameters.AddWithValue("@chatId", chatId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.HasRows)
                            yield break;
                        while (reader.Read())
                        {
                            yield return UsersRepository.GetUser(reader.GetInt32(reader.GetOrdinal("UserID")));
                        }
                    }
                }
            }
        }

        public void KickUser(int chatId, int userId)
        {
            if (userId == 0)
                return;
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // check if user is creator
                if (SqlHelper.DoesDoubleKeyExist(connection, "Chats", "ID", chatId, "CreatorID", userId))
                    return;
                // check if chat is dialog
                if (SqlHelper.DoesDoubleKeyExist(connection, "Chats", "ID", chatId, "ChatType", (int)ChatTypes.Dialog))
                    return;
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "DELETE FROM [ChatsUsers] WHERE [ChatsUsers].[ChatID] = @chatId AND [ChatsUsers].[UserID] = @userId";

                    command.Parameters.AddWithValue("@chatId", chatId);
                    command.Parameters.AddWithValue("@userId", userId);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void KickUsers(int chatId, IEnumerable<int> kickedUsers)
        {
            if (kickedUsers == null)
                return;
            var idList = kickedUsers as int[] ?? kickedUsers.ToArray();
            if (idList.Contains(0))
                return;
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // check if creator is in range
                if (SqlHelper.IsSelectedRowFieldInRange(connection, "Chats", "ID", chatId, "CreatorID", idList))
                    return;
                // check if chat is dialog
                if (SqlHelper.DoesDoubleKeyExist(connection, "Chats", "ID", chatId, "ChatType", (int)ChatTypes.Dialog))
                    return;
                using (var transaction = connection.BeginTransaction())
                {
                    foreach (var userId in idList)
                    {
                        using (var command = connection.CreateCommand())
                        {
                            command.Transaction = transaction;
                            command.CommandText =
                                "DELETE FROM [ChatsUsers] WHERE [ChatsUsers].[ChatID] = @chatId AND [ChatsUsers].[UserID] = @userId";

                            command.Parameters.AddWithValue("@chatId", chatId);
                            command.Parameters.AddWithValue("@userId", userId);

                            command.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                }
            }
        }
       
        public void SetCreator(int chatId, int newCreator)
        {
            if (newCreator == 0)
                return;
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    // check if chat is dialog
                    if (SqlHelper.DoesDoubleKeyExist(connection, "Chats", "ID", chatId, "ChatType", (int)ChatTypes.Dialog))
                        return;
                    // check if user is in chat
                    if (!SqlHelper.DoesDoubleKeyExist(connection, "ChatsUsers", "UserID", newCreator, "ChatID", chatId))
                        return;

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "UPDATE [Chats] SET [CreatorID] = @newCreator WHERE [ID] = @chatId";

                        command.Parameters.AddWithValue("@chatId", chatId);
                        command.Parameters.AddWithValue("@newCreator", newCreator);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException) { }
        }
        
    }
}