using System.Collections.Generic;
using Messenger.Model.Enums;
using Messenger.Model;

namespace Messenger.WebApi.Models
{
    public class ChatCredentials
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CreatorId { get; set; }
        public Message LastMessage { get; set; }
        public IEnumerable<User> Members { get; set; }

        public ChatCredentials(Chat chat, Message lastMessage)
        {
            Id = chat.Id;
            Name = chat.Name;
            CreatorId = chat.CreatorId;
            Members = chat.Members;
            LastMessage = lastMessage;
        }
    }
}