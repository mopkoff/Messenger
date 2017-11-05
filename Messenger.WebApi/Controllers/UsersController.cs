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
                 new CustomLogger(LogLevel.Debug, "Fetching chat with id: {0}", id))
            {
                timeLog.Start();
                var user = RepositoryBuilder.UsersRepository.GetUser(id);
                NLogger.Logger.Info("Fetched chat with id {0}", id);
                return user;
            }
        }

        //[Route("")]
        [HttpPost]
        // [ChatUserAuthorization(RegexString = RegexString)]
        public User CreateUser([FromBody] UserCredentials userCredentials)
        {
            var user = RepositoryBuilder.UsersRepository.CreateUser(new User(userCredentials.Username, userCredentials.Password));
            if (user == null)
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, "No user found"));
            return user;
        }

    }
}
