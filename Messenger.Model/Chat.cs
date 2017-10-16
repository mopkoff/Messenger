using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Messenger.Model.Enums;

namespace Messenger.Model
{
    public class Chat
    {
        public int Id { get; set; }
        public int CreatorId { get; set; }
        public string Name { get; set; }
        public ChatTypes ChatType { get; set; }

        public IEnumerable<User> Members { get; set; }

        public Chat(int id, ChatTypes chatType, int creatorId)
        {
            Id = id;
            ChatType = chatType;
            CreatorId = creatorId;
        }

        public Chat(int id)
        {
            Id = id;
            ChatType = ChatTypes.Dialog;
            CreatorId = 0;
        }

        public Chat()
        {
            Id = 0;
            ChatType = ChatTypes.Dialog;
            CreatorId = 0;
        }
    }
}
