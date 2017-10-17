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
    public class ChatsRepositoryTests
    {
        private const string ConnectionString = @"Data Source=MSI\MESSENGER;
                Initial Catalog=messenger;
                Integrated Security=True;";

        private readonly List<int> _tempUsers = new List<int>();
        private readonly List<int> _tempChats = new List<int>();

        [TestMethod]
        public void ShouldStartChatWithUser()
        {
            //arrange
            var user = new User
            {
                Login = "testUser",
                Password = "password"
            };

            const string chatName = "testChat";

            //act
            var usersRepository = new UsersRepository(ConnectionString);
            var result = usersRepository.CreateUser(user);

            _tempUsers.Add(result.Id);

            var chatRepository = new ChatsRepository(ConnectionString, usersRepository);
            var chat = chatRepository.CreateGroupChat(new int[] { _tempUsers[0] }, chatName);
            var userChats = chatRepository.GetUserChats(user.Id);

            //asserts
            Assert.AreEqual(chatName, chat.Name);
            Assert.AreEqual(user.Id, chat.Members.Single().Id);
            Assert.AreEqual(chat.Id, userChats.Single().Id);
            Assert.AreEqual(chat.Name, userChats.Single().Name);

        }

        [TestMethod]
        public void ShouldKickUsers()
        {
            //arrange
            List<User> users = new List<User>(5);
            var usersRepository = new UsersRepository(ConnectionString);
            var chatRepository = new ChatsRepository(ConnectionString, usersRepository);
            List<int> userIds = new List<int>(5);

            for (int i = 0; i < 5; i++)
            {
                users.Add(new User("testUser" + i, "password"));
                usersRepository.CreateUser(users[i]);
                userIds.Add(users[i].Id);
            }
            _tempUsers.AddRange(userIds);
            const string chatName = "testChat";

            var chat = chatRepository.CreateGroupChat(userIds, chatName);

            chatRepository.KickUser(chat.Id, users[0].Id);
            chatRepository.KickUser(chat.Id, users[2].Id);
            chatRepository.KickUsers(chat.Id, new int[] { users[0].Id, users[4].Id });
            chatRepository.KickUsers(chat.Id, new int[] { users[1].Id, users[3].Id });


            var Members = chatRepository.GetChatUsers(chat.Id);
            //asserts
            Assert.AreEqual(true, Members.Contains(users[0], new UserEqualityComparer()));
            Assert.AreEqual(false, Members.Contains(users[1], new UserEqualityComparer()));
            Assert.AreEqual(false, Members.Contains(users[2], new UserEqualityComparer()));
            Assert.AreEqual(false, Members.Contains(users[3], new UserEqualityComparer()));
            Assert.AreEqual(true, Members.Contains(users[4], new UserEqualityComparer()));
        }

        [TestMethod]
        public void ShouldSetCreator()
        {
            //arrange
            List<User> users = new List<User>(5);
            var usersRepository = new UsersRepository(ConnectionString);
            var chatRepository = new ChatsRepository(ConnectionString, usersRepository);
            List<int> userIds = new List<int>(5);

            for (int i = 0; i < 5; i++)
            {
                users.Add(new User("testUser" + i, "password"));
                usersRepository.CreateUser(users[i]);
                userIds.Add(users[i].Id);
            }
            _tempUsers.AddRange(userIds);
            const string chatName = "testChat";

            var chat = chatRepository.CreateGroupChat(userIds, chatName);

            chatRepository.KickUser(chat.Id, users[0].Id);
            chatRepository.KickUser(chat.Id, users[2].Id);
            chatRepository.SetCreator(chat.Id, users[1].Id);
            chatRepository.KickUsers(chat.Id, new int[] { users[0].Id, users[4].Id });
            chatRepository.KickUsers(chat.Id, new int[] { users[1].Id, users[3].Id });


            var Members = chatRepository.GetChatUsers(chat.Id);
            //asserts
            Assert.AreEqual(false, Members.Contains(users[0], new UserEqualityComparer()));
            Assert.AreEqual(true, Members.Contains(users[1], new UserEqualityComparer()));
            Assert.AreEqual(false, Members.Contains(users[2], new UserEqualityComparer()));
            Assert.AreEqual(true, Members.Contains(users[3], new UserEqualityComparer()));
            Assert.AreEqual(false, Members.Contains(users[4], new UserEqualityComparer()));
        }

        [TestMethod]
        public void ShouldAddUsers()
        {
            //arrange
            List<User> users = new List<User>(5);
            var usersRepository = new UsersRepository(ConnectionString);
            var chatRepository = new ChatsRepository(ConnectionString, usersRepository);
            List<int> userIds = new List<int>(5);

            for (int i = 0; i < 5; i++)
            {
                users.Add(new User("testUser" + i, "password"));
                usersRepository.CreateUser(users[i]);
                userIds.Add(users[i].Id);
            }
            _tempUsers.AddRange(userIds);
            const string chatName = "testChat";

            var chat = chatRepository.CreateGroupChat(new int[] { userIds[0], userIds[1] }, chatName);
            chatRepository.AddUser(chat.Id, userIds[2]);
            chatRepository.AddUsers(chat.Id, new int[] { userIds[3], userIds[4] });
            chat = chatRepository.GetChat(chat.Id);

            //asserts
            Assert.AreEqual(true, chat.Members.Contains(users[0], new UserEqualityComparer()));
            Assert.AreEqual(true, chat.Members.Contains(users[1], new UserEqualityComparer()));
            Assert.AreEqual(true, chat.Members.Contains(users[2], new UserEqualityComparer()));
            Assert.AreEqual(true, chat.Members.Contains(users[3], new UserEqualityComparer()));
            Assert.AreEqual(true, chat.Members.Contains(users[4], new UserEqualityComparer()));
        }

        [TestCleanup]
        public void Clean()
        {
            foreach (var id in _tempUsers)
                new UsersRepository(ConnectionString).DeleteUser(id);

            foreach (var id in _tempChats)
                new ChatsRepository(ConnectionString).DeleteChat(id);
        }
    }
}
