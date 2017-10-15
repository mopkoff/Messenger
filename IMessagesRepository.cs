using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messenger.DataLayer
{
    interface IMessagesRepository
    {
        Message AddMessage(int senderId, int chatId, string text, IEnumerable<Attachment> attachments = null, DateTime expirationDate = null);

        Message GetMessage(int messageId);
        IEnumerable<Attachment> GetMessageAttachments(int messageId);

        DateTime GetMessageExpirationDate(int messageId);

        IEnumerable<Message> GetChatMessages(int chatId);
        IEnumerable<Message> GetChatMessagesFrom(int chatId, DateTime dateFrom);
        IEnumerable<Message> GetChatMessageTo(int chatId, DateTime dateTo);
        IEnumerable<Message> GetChatMessagesInRange(int chatId, DateTime dateFrom, DateTime dateTo);

        IEnumerable<Message> SearchString(int chatId, string searchString);

        // Execute sql-template for clearing messages that should be deleted
        void DeleteExpiredMessages();
    }
}
