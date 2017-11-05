using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Messenger.DataLayer.SqlServer;
//using Messenger.DataLayer.SqlServer.Exceptions;
using Messenger.Model;
using Messenger.Model.Enums;
//using Messenger.WebApi.Filters.Authentication;
//using Messenger.WebApi.Filters.Authorization;
using Messenger.WebApi.Models;
//using Messenger.WebApi.Principals;
namespace Messenger.WebApi.Controllers
{
    [RoutePrefix("api/chats")]
   // [TokenAuthentication]
   // [Authorize]
    public class ChatsController : ApiController
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
        public Chat GetChatById(int id)
        {
            var chat = RepositoryBuilder.ChatsRepository.GetChat(id);
            if (chat == null)
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, "No chat found"));
            return chat;
        }

        //[Route("")]
        [HttpPost]
        // [ChatUserAuthorization(RegexString = RegexString)]
        public Chat CreateChat([FromBody] ChatCredentials chatCredentials)
        {            
            var chat = RepositoryBuilder.ChatsRepository.CreateGroupChat(chatCredentials.Members, chatCredentials.Title);
            if (chat == null)
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, "No chat found"));
            return chat;
        }

    }
}
