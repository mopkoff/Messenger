using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Transactions;
using Messenger.Model;
using Messenger.Model.Enums;

namespace Messenger.DataLayer.SqlServer
{
    public class MessagesRepository : IMessagesRepository
    {
        private readonly string _connectionString;

        public MessagesRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public Message AddMessage(int senderId, int chatId, string text, IEnumerable<Attachment> attachments = null)
        {
            try
            {
                if (string.IsNullOrEmpty(text) && attachments == null)
                    return null;
                if (senderId == 0)
                    return null;
                using (var scope = new TransactionScope())
                {
                    using (var connection = new SqlConnection(_connectionString))
                    {
                        connection.Open();

                        // Insert the message
                        var message = new Message(chatId, senderId, text, DateTime.Now);
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = "DECLARE @T TABLE (ID INT, MessageDate DATETIME)\n" +
                                                  "INSERT INTO [Messages] ([ChatID], [SenderID], [MessageText], [MessageDate]) " +
                                                  "OUTPUT INSERTED.[ID], INSERTED.[MessageDate] INTO @T " +
                                                  "VALUES (@chatId, @senderId, @messageText, @messageDate)\n" +
                                                  "SELECT [ID], [MessageDate] FROM @T";

                            command.Parameters.AddWithValue("@chatId", chatId);
                            command.Parameters.AddWithValue("@senderId", senderId);
                            command.Parameters.AddWithValue("@messageDate", message.Date);
                            if (text == null)
                                command.Parameters.AddWithValue("@messageText", DBNull.Value);
                            else
                                command.Parameters.AddWithValue("@messageText", text);

                            using (var reader = command.ExecuteReader())
                            {
                                reader.Read();
                                message.Id = reader.GetInt32(reader.GetOrdinal("ID"));
                                message.Date = reader.GetDateTime(reader.GetOrdinal("MessageDate"));
                            }
                        }

                        //Insert attachments if not null

                        if (attachments == null)
                        {
                            message.Attachments = null;
                        }
                        else
                        {
                            foreach (var attachment in attachments)
                                using (var command = connection.CreateCommand())
                                {
                                    command.CommandText =
                                        "INSERT INTO [Attachments] ([Type], [Attachment], [MessageID]) " +
                                        "OUTPUT INSERTED.[ID] " +
                                        "VALUES (@type, @attachment, @messageId)";

                                    command.Parameters.AddWithValue("@type", (int)attachment.Type);
                                    command.Parameters.AddWithValue("@messageId", message.Id);
                                    var attachFile =
                                        new SqlParameter("@attachment", SqlDbType.VarBinary, attachment.File.Length)
                                        { Value = attachment.File };
                                    command.Parameters.Add(attachFile);

                                    attachment.Id = (int)command.ExecuteScalar();
                                }
                        }
                        scope.Complete();
                        return message;
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        public Message AddTemporaryMessage(int senderId, int chatId, string text, DateTime destroyDate,
            IEnumerable<Attachment> attachments = null)
        {
            // if message already expired
            if (destroyDate <= DateTime.Now)
                return null;
            using (var scope = new TransactionScope())
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var message = AddMessage(senderId, chatId, text, attachments);

                    if (message == null)
                        return null;
                    // Insert expiration date into queue
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "INSERT INTO [Messages] ([MessageID], [Destroy])" +
                                              "VALUES (@messageId, @destroyDate)";

                        command.Parameters.AddWithValue("@messageId", message.Id);
                        command.Parameters.AddWithValue("@destroyDate", destroyDate);

                        command.ExecuteNonQuery();
                    }

                    scope.Complete();
                    return message;
                }
            }
        }

        public Message GetMessage(int messageId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [ChatID], [SenderID], [MessageText], [MessageDate] FROM [Messages] " +
                                          "WHERE [ID] = @messageId";

                    command.Parameters.AddWithValue("@messageId", messageId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.HasRows)
                            return null;
                        reader.Read();
                        return new Message
                        {
                            Id = messageId,
                            ChatId = reader.GetInt32(reader.GetOrdinal("ChatID")),
                            SenderId = reader.GetInt32(reader.GetOrdinal("SenderID")),
                            Text = reader.IsDBNull(reader.GetOrdinal("MessageText")) ? null : reader.GetString(reader.GetOrdinal("MessageText")),
                            Date = reader.GetDateTime(reader.GetOrdinal("MessageDate")),
                        };
                    }
                }
            }
        }

        public IEnumerable<Attachment> GetMessageAttachments(int messageId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [ID], [Type], [Attachment] FROM [Attachments] " +
                                          "WHERE [MessageID] = @messageId";

                    command.Parameters.AddWithValue("@messageId", messageId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.HasRows)
                            yield break;
                        while (reader.Read())
                        {
                            yield return new Attachment
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("ID")),
                                Type = (AttachmentTypes)reader.GetInt32(reader.GetOrdinal("Type")),
                                File = reader[reader.GetOrdinal("Attachment")] as byte[]
                            };
                        }
                    }
                }
            }
        }

        public IEnumerable<Message> GetChatMessages(int chatId)
        {
            return GetChatMessagesInRange(chatId, DateTime.MinValue, DateTime.MaxValue);
        }
        public Message GetLastMessage(int chatId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT TOP 1 [ID], [SenderID], [MessageText], [MessageDate] FROM [Messages] " +
                                          "WHERE [ChatID] = @chatId ORDER BY [ID] DESC";
                    command.Parameters.AddWithValue("@chatId", chatId);

                    using (var reader = command.ExecuteReader())
                    {
                        return new Message
                        {
                            Id = reader.GetInt64(reader.GetOrdinal("ID")),
                            ChatId = chatId,
                            Text = reader.IsDBNull(reader.GetOrdinal("MessageText")) ? null : reader.GetString(reader.GetOrdinal("MessageText")),
                            SenderId = reader.GetInt32(reader.GetOrdinal("SenderID")),
                            Date = reader.GetDateTime(reader.GetOrdinal("MessageDate")),
                        };
                    }
                }
            }
        }
        public IEnumerable<Message> GetChatMessagesFrom(int chatId, DateTime dateFrom)
        {
            return GetChatMessagesInRange(chatId, dateFrom, DateTime.MaxValue);
        }

        public IEnumerable<Message> GetChatMessagesTo(int chatId, DateTime dateTo)
        {
            return GetChatMessagesInRange(chatId, DateTime.MinValue, dateTo);
        }

        public IEnumerable<Message> GetChatMessagesFromId(int chatId, long idFrom)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [ID], [SenderID], [MessageText], [MessageDate] FROM [Messages] " +
                                          "WHERE [ChatID] = @chatId";
                    command.Parameters.AddWithValue("@chatId", chatId);

                    command.CommandText += " AND [ID] > @idFrom";
                    command.Parameters.AddWithValue("@idFrom", idFrom);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            yield return new Message
                            {
                                Id = reader.GetInt64(reader.GetOrdinal("ID")),
                                ChatId = chatId,
                                Text = reader.IsDBNull(reader.GetOrdinal("MessageText")) ? null : reader.GetString(reader.GetOrdinal("MessageText")),
                                SenderId = reader.GetInt32(reader.GetOrdinal("SenderID")),
                                Date = reader.GetDateTime(reader.GetOrdinal("MessageDate")),
                            };
                        }
                    }
                }
            }
        }

        public IEnumerable<Message> GetChatMessagesInRange(int chatId, DateTime dateFrom, DateTime dateTo)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [ID], [SenderID], [MessageText], [MessageDate] FROM [Messages] " +
                                          "WHERE [ChatID] = @chatId";
                    command.Parameters.AddWithValue("@chatId", chatId);

                    if (dateFrom != DateTime.MinValue)
                    {
                        command.CommandText += " AND [MessageDate] >= @dateFrom";
                        command.Parameters.AddWithValue("@dateFrom", dateFrom);
                    }
                    if (dateTo != DateTime.MaxValue)
                    {
                        command.CommandText += " AND [MessageDate] <= @dateTo";
                        command.Parameters.AddWithValue("@dateTo", dateTo);
                    }

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            yield return new Message
                            {
                                Id = reader.GetInt64(reader.GetOrdinal("ID")),
                                ChatId = chatId,
                                Text = reader.IsDBNull(reader.GetOrdinal("MessageText")) ? null : reader.GetString(reader.GetOrdinal("MessageText")),
                                SenderId = reader.GetInt32(reader.GetOrdinal("SenderID")),
                                Date = reader.GetDateTime(reader.GetOrdinal("MessageDate")),
                            };
                        }
                    }
                }
            }
        }

        public IEnumerable<Message> SearchString(int chatId, string searchString)
        {
            if (string.IsNullOrEmpty(searchString))
                yield break;
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [ID], [SenderID], [Text], [Date] FROM [Messages] " +
                                          "WHERE [ChatID] = @chatId AND CONTAINS([Text], @searchString)";

                    command.Parameters.AddWithValue("@chatId", chatId);
                    command.Parameters.AddWithValue("@searchString", "*" + searchString + "*");

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            yield return new Message()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("ID")),
                                ChatId = chatId,
                                Text = reader.GetString(reader.GetOrdinal("Text")),
                                SenderId = reader.GetInt32(reader.GetOrdinal("SenderID")),
                                Date = reader.GetDateTime(reader.GetOrdinal("Date")),
                            };
                        }
                    }
                }
            }
        }

        public DateTime? GetMessageExpirationDate(int messageId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "SELECT [DestroyDate] FROM [Messages] WHERE [MessageID] = @messageId";
                    command.Parameters.AddWithValue("@messageId", messageId);

                    return (DateTime?)command.ExecuteScalar();
                }
            }
        }

        public void DeleteExpiredMessages()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "DeleteExpiredMessages";

                    command.ExecuteNonQuery();
                }
            }
        }

    }
}