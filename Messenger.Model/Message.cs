using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messenger.Model
{
    public class Message
    {
        public long Id { get; set; }
        public int ChatId { get; set; }
        public int SenderId { get; set; }
        public string Text { get; set; }
        public DateTime Date { get; set; }
        public DateTime DestroyDate { get; set; }
        public IEnumerable<Attachment> Attachments { get; set; }

        public Message()
        {
            Attachments = null;
        }
        public Message(int chatId, int senderId, string text)
        {
            this.ChatId = chatId;
            this.SenderId = senderId;
            this.Text = text;
        }
        public Message(int chatId, int senderId, string text, DateTime date)
        {
            this.ChatId = chatId;
            this.SenderId = senderId;
            this.Text = text;
            this.Date = date;
        }
    }
}










