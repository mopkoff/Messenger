using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messenger.Model
{
    public class Message
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public string Text { get; set; }
        public int ChatId { get; set; }
        public DateTime Date { get; set; }
        public DateTime DestroyDate { get; set; }
        public IEnumerable<Attachment> Attachments { get; set; }

        public Message()
        {
            Attachments = null;
        }
        public Message(int id, int senderId, string text)
        {
            this.Id = id;
            this.SenderId = senderId;
            this.Text = text;
        }
        public Message(int id, int senderId, string text, int chatId, DateTime date)
        {
            this.Id = id;
            this.SenderId = senderId;
            this.Text = text;
            this.ChatId = chatId;
            this.Date = date;
        }
    }
}
