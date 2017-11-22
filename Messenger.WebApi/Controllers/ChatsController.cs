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
using Newtonsoft.Json.Linq;
//using Messenger.WebApi.Principals;
namespace Messenger.WebApi.Controllers
{
    [RoutePrefix("api/chats")]
   // [TokenAuthentication]
   // [Authorize]
    public class ChatsController : ApiController
    {
        /// <summary>
        /// Gets chat information by its id. User must be in chat
        /// </summary>
        /// <param name="id">The id of the chat</param>
        /// <returns>All chat information</returns>
        [Route("{token:Guid}")]
        [HttpGet]
        public ChatCredentials[] GetChatByToken(Guid token)
        {
            var userId = RepositoryBuilder.TokensRepository.GetUserIdByToken(token);
            var chats = RepositoryBuilder.ChatsRepository.GetUserChats(userId).ToArray();
            List<ChatCredentials> chatsArray = new List<ChatCredentials>();
            
            if (chats.Length == 0)
                return null;
                        
            foreach (var chat in chats)
            {
                chat.Members = RepositoryBuilder.ChatsRepository.GetChatUsers(chat.Id).ToArray();
                chatsArray.Add(new ChatCredentials(chat, RepositoryBuilder.MessagesRepository.GetLastMessage(chat.Id)));
            }
            return chatsArray.ToArray();
        }

        //[Route("")]
        [HttpPut]
        // [ChatUserAuthorization(RegexString = RegexString)]
        public Chat CreateChat([FromBody] ChatCredentials chatCredentials)
        {            
            var chat = RepositoryBuilder.ChatsRepository.CreateGroupChat(RepositoryBuilder.UsersRepository.GetUserIdsByLogins(chatCredentials.Members.Select(m=>m.Login)), chatCredentials.Name);      
            return chat;
        }
    }
}
