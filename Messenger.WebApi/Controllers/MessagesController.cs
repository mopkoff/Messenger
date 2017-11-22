using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Messenger.DataLayer.SqlServer;
using Messenger.WebApi.Models;
using Messenger.Logger;
using Messenger.Model;
using NLog;
using Messenger.WebApi.Principals;
using System.Security.Principal;
using System.Web.Http.Cors;
using Newtonsoft.Json.Linq;
using System.Threading;

namespace Messenger.WebApi.Controllers
{
    [RoutePrefix("api/messages")]
    public class MessagesController : ApiController
    {
        [Route("{chatId:int}/{userId:int}")]
        [HttpPost]
        public Message StoreMessage(int chatId, int userId, [FromBody] string messageText)
        {
            Message message = new Message(chatId, userId, messageText);

            NLogger.Logger.Debug("Called with arguments CID:{0}, UID:{1}, MSG:{2}", chatId, userId, message);
            if (message.DestroyDate == DateTime.MinValue)
            {
                var msg = RepositoryBuilder.MessagesRepository.AddMessage(userId, chatId, message.Text,
                    message.Attachments);
                NLogger.Logger.Info("Successfully stored message from UID:{0} to CID:{1}. Message: {2}",
                    userId, chatId, msg);
                NLogger.Logger.Debug("Notifying subscribers about a new message");
                return msg;
            }
            else
            {
                var msg = RepositoryBuilder.MessagesRepository.AddTemporaryMessage(userId, chatId, message.Text,
                    (DateTime)message.DestroyDate, message.Attachments);
                NLogger.Logger.Info("Successfully stored message with e.d from UID: {0} to CID:{1}. Message: {2}", userId, chatId, msg);
                NLogger.Logger.Debug("Notifying subscribers about a new message");
                return msg;
            }
        }
        /// <summary>
        /// Gets the last message in the chat
        /// </summary>
        /// <param name="chatId">The id of the chat</param>
        /// <returns>Last message</returns>
        [Route("{chatId:int}")]
        [HttpGet]
        public Message[] GetChatMessages(int chatId)
        {
            NLogger.Logger.Debug("Called with argument: {0}", chatId);
            var message = RepositoryBuilder.MessagesRepository.GetChatMessages(chatId);
            NLogger.Logger.Info("Successfully fetched messages from chat id: {0}", chatId);
            var messageArray = message.ToArray();
            return messageArray;
        }


        /// <summary>
        /// Gets the last message in the chat
        /// </summary>
        /// <param name="chatId">The id of the chat</param>
        /// <returns>Last message</returns>
        [Route("subscribe/{chatId:int}")]
        [HttpPut]
        public Message[] UpdateMessages(int chatId,[FromBody] string lastMessageId)
        {
            NLogger.Logger.Debug("Called with argument: {0}", chatId);

            if (Int32.Parse(lastMessageId)!= 0)
            {
                var message = RepositoryBuilder.MessagesRepository.GetMessage(Int32.Parse(lastMessageId));
                var messages = RepositoryBuilder.MessagesRepository.GetChatMessagesFromId(chatId, message.Id);
                while (messages.ToArray().Length == 0)
                {
                    Thread.Sleep(1000);
                    messages = RepositoryBuilder.MessagesRepository.GetChatMessagesFromId(chatId, message.Id);
                }
                NLogger.Logger.Info("Some new messages from chat id: {0}", chatId);
                var messageArray = messages.ToArray();
                return messageArray;
            }
            else
            {
                NLogger.Logger.Info("Bad request with empty lastMessageId from chat id: {0}", chatId);
                return null;
            }
        }
    }
}


