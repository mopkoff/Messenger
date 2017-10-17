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
            Assert.IsTrue(Members.Contains(users[0], new UserEqualityComparer()), "Chat Creator kicked - failed");
            Assert.IsFalse(Members.Contains(users[1], new UserEqualityComparer()), "Group of users kick failed");
            Assert.IsFalse(Members.Contains(users[2], new UserEqualityComparer()), "Solo user kick failed");
            Assert.IsFalse(Members.Contains(users[3], new UserEqualityComparer()), "Group of users kick failed");
            Assert.IsTrue(Members.Contains(users[4], new UserEqualityComparer()), "Group of users with chat creator kicked - failed");
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
            chatRepository.SetCreator(chat.Id, users[1].Id);
            chatRepository.KickUsers(chat.Id, new int[] { users[0].Id, users[4].Id });
            chatRepository.KickUsers(chat.Id, new int[] { users[1].Id, users[3].Id });


            var Members = chatRepository.GetChatUsers(chat.Id);
            //asserts
            Assert.AreEqual(false, Members.Contains(users[0], new UserEqualityComparer()), "Creator change failed");
            Assert.AreEqual(true, Members.Contains(users[1], new UserEqualityComparer()), "New creator was kicked - Creator change failed");
            Assert.AreEqual(true, Members.Contains(users[3], new UserEqualityComparer()), "Group of users with chat creator was kicked - Creator change failed");
            Assert.AreEqual(false, Members.Contains(users[4], new UserEqualityComparer()), "Group of users with old chat creator wasn't kicked - Creator change failed");
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
            Assert.IsTrue(chat.Members.Contains(users[0], new UserEqualityComparer()), "Initialization addition failed");
            Assert.IsTrue(chat.Members.Contains(users[1], new UserEqualityComparer()), "Initialization addition failed");
            Assert.IsTrue(chat.Members.Contains(users[2], new UserEqualityComparer()), "Solo addition failed");
            Assert.IsTrue(chat.Members.Contains(users[3], new UserEqualityComparer()), "Group addition failed");
            Assert.IsTrue(chat.Members.Contains(users[4], new UserEqualityComparer()), "Group addition failed");
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
