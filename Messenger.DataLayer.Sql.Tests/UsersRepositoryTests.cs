using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Messenger.Model;
using Messenger.DataLayer;
using Messenger.DataLayer.SqlServer;


namespace Messenger.DataLayer.Sql.Tests
{
    [TestClass]
    public class UsersRepositoryTests
    {
        private const string ConnectionString = @"Data Source=MSI\MESSENGER;
                Initial Catalog=messenger;
                Integrated Security=True;";

        private readonly List<int> _tempUsers = new List<int>();

        [TestMethod]
        public void ShouldCreateUser()
        {
            //arrange
            var user = new User
            {
                Login = "testUser",                
                Password = "password"
            };

            //act
            var repository = new UsersRepository(ConnectionString);
            var result = repository.CreateUser(user);

            _tempUsers.Add(result.Id);

            //asserts
            Assert.AreEqual(user.Login, result.Login);
            Assert.AreEqual(user.Password, result.Password);
        }

        [TestMethod]
        public void ShouldStartChatWithUser()
        {
            //arrange
            var user = new User
            {
                Login = "testUser",
                Password = "password"
            };

            const string chatName = "чатик";

            //act
            var usersRepository = new UsersRepository(ConnectionString);
            var result = usersRepository.CreateUser(user);

            _tempUsers.Add(result.Id);

            var chatRepository = new ChatsRepository(ConnectionString, usersRepository);
            var chat = chatRepository.CreateGroupChat(new[] { user.Id }, chatName);
            var userChats = chatRepository.GetUserChats(user.Id);
            //asserts
            Assert.AreEqual(chatName, chat.Name);
            Assert.AreEqual(user.Id, chat.Members.Single().Id);
            Assert.AreEqual(chat.Id, userChats.Single().Id);
            Assert.AreEqual(chat.Name, userChats.Single().Name);
        }

        [TestCleanup]
        public void Clean()
        {
            foreach (var id in _tempUsers)
                new UsersRepository(ConnectionString).DeleteUser(id);
        }
    }
}
