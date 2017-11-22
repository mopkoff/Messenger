using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Messenger.DataLayer.SqlServer;
using Messenger.Model;
using Messenger.WebApi.Models;
using Messenger.Logger;
using NLog;

namespace Messenger.WebApi.Controllers
{
    [RoutePrefix("api/users")]
    public class UsersController : ApiController
    {
        private const string RegexString = @".*\/chats\/([^\/]+)\/?";

        /// <summary>
        /// Gets chat information by its id. User must be in chat
        /// </summary>
        /// <param name="id">The id of the chat</param>
        /// <returns>All chat information</returns>
        [Route("{id:int}")]
        [HttpGet]
        // [ChatUserAuthorization(RegexString = RegexString)]
        public User GetUserById(int id)
        {
            using (var timeLog =
                 new CustomLogger(LogLevel.Debug, "Fetching user with id: {0}", id))
            {
                timeLog.Start();
                var user = RepositoryBuilder.UsersRepository.GetUser(id);
                NLogger.Logger.Info("Fetched user with id {0}", id);
                user.Password = "";
                return user;
            }
        }

        [Route("find/{pattern}")]
        [HttpGet]
        // [ChatUserAuthorization(RegexString = RegexString)]
        public User[] GetUsersByPatter(string pattern)
        {
            using (var timeLog =
                 new CustomLogger(LogLevel.Debug, "Find user with pattern: {0}", pattern))
            {
                timeLog.Start();
                var users = RepositoryBuilder.UsersRepository.GetUsersByPattern(pattern).ToArray();
                //NLogger.Logger.Info("Fetched chat with id {0}", id);
                return users;
            }
        }

        //[Route("")]
        [HttpPost]
        // [ChatUserAuthorization(RegexString = RegexString)]
        public User CreateUser([FromBody] UserCredentials userCredentials)
        {
            var user = RepositoryBuilder.UsersRepository.CreateUser(new User(userCredentials.Login, userCredentials.Password));
            if (user == null)
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, "No user found"));
            return user;
        }

    }
}
