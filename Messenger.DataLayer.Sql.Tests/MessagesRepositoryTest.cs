using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Messenger.DataLayer.SqlServer;
using Messenger.Model;

namespace Messenger.DataLayer.Sql.Tests
{
    [TestClass]
    public class MessagesRepositoryTest
    {
        private const string ConnectionString = @"Data Source=MSI\MESSENGER;
                Initial Catalog=messenger;
                Integrated Security=True;";
        
        private readonly List<int> _tempMessages = new List<int>();

        private ChatsRepository chatsRepository;
        private UsersRepository usersRepository;
        private MessagesRepository messagesRepository;
        int userId, chatId;
        [TestInitialize]
        public void InitRepos()
        {
            chatsRepository = new ChatsRepository(ConnectionString);
            usersRepository = new UsersRepository(ConnectionString);
            messagesRepository = new MessagesRepository(ConnectionString);

            userId = usersRepository.CreateUser(new User("MrFreeman", "BlackMesa")).Id;
            chatId = chatsRepository.CreateGroupChat(new[] { userId }, "FreedomRadio").Id;
        }

        [TestMethod]
        public void ShouldAddAndReturnMessage()
        {
            // arrange
            const string text = "Sampletext";
            
            // act
            messagesRepository.AddMessage(userId, chatId, text);
            var msg = messagesRepository.GetChatMessages(chatId).Single();
            // assert
            Assert.IsNotNull(msg);
            Assert.AreEqual(msg.SenderId, userId);
            Assert.AreEqual(msg.ChatId, chatId);
            Assert.IsTrue(!msg.Attachments.Any());
            Assert.IsNull(msg.DestroyDate);
            Assert.AreEqual(msg.Text, text);
        }
    }
}
