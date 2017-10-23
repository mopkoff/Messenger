using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Messenger.Model;
using Messenger.DataLayer;
using Messenger.DataLayer.SqlServer;
using System.Configuration;

namespace Messenger.DataLayer.Sql.Tests
{
    [TestClass]
    public class UsersRepositoryTests
    {
        private const string ConnectionString =  @"Data Source=MSI\MESSENGER;
                Initial Catalog=messenger;
                Integrated Security=True;";

        private readonly List<int> _tempUsers = new List<int>();
        private UsersRepository usersRepository;

        [TestInitialize]
        public void InitRepos()
        {
            usersRepository = new UsersRepository(ConnectionString);          
        }

        [TestMethod]
        public void ShouldCreateGetDeleteUser()
        {
            //arrange

            var user = new User("testUser", "password");

            //act
            var result1 = usersRepository.CreateUser(user);
            var result2 = usersRepository.GetUser(user.Id);
            usersRepository.DeleteUser(user.Id);
            var result3 = usersRepository.GetUser(user.Id);
            
            //asserts
            
            Assert.AreEqual(user.Login, result1.Login, "Creation failed");
            Assert.AreEqual(user.Password, result1.Password, "Creation failed");
            Assert.AreEqual(user.Login, result2.Login, "Receiving failed");
            Assert.AreEqual(user.Password, result2.Password, "Receiving failed");
            Assert.IsNull(result3, "Remove failed");
        }

        [TestMethod]
        public void ShouldAddUserInfo()
        {
            //arrange

            var user = new User("testUser", "password");
            var userInfo = new UserInfo() { FirstName = "testFirstName", LastName = "testLastName", About = "XD", Gender = Model.Enums.GenderTypes.Male };

            //act
            user = usersRepository.CreateUser(user);
            _tempUsers.Add(user.Id);
            usersRepository.UpdateUserInfo(user, userInfo);
            user.userInfo = usersRepository.GetUserInfo(user.Id);
            
            //asserts
            Assert.IsTrue(userInfo.Equals(user.userInfo), "Adding userInfo Failed");
        }

        [TestMethod]
        public void ShouldChangePasswordAndPersist()
        {
            //arrange
            var user = new User("testUser", "password");

            //act
            user = usersRepository.CreateUser(user);
            _tempUsers.Add(user.Id);
            usersRepository.SetPassword(user.Id, "newPassword");
            var result1 = usersRepository.GetUser(user.Id);
            usersRepository.PersistUser(new User(user.Id, "newName", "superNewPassword"));
            var result2 = usersRepository.GetUser(user.Id);

            //asserts

            Assert.AreEqual(result1.Password, "newPassword".GetHashCode().ToString(), "Password change failed");
            Assert.AreEqual(user.Id, result2.Id, "Persist failed");
            Assert.AreEqual(result2.Login, "newName", "Persist failed");
        }


        [TestCleanup]
        public void Clean()
        {
            foreach (var id in _tempUsers)
                new UsersRepository(ConnectionString).DeleteUser(id);
        }
    }
}
