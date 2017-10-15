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
        public int UserId { get; set; }
        public string Text { get; set; }
        public int ChatId { get; set; }
        public DateTime Date { get; set; }
        public DateTime DestroyDate { get; set; }
        public IEnumerable<Attachment> Attachments { get; set; }

        public Message(){
            Attachments = null;
        }
    }
}
