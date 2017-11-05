using System.Collections.Generic;
using Messenger.Model.Enums;

namespace Messenger.WebApi.Models
{
    public class ChatCredentials
    {
        public string Name { get; set; }
        public ChatTypes ChatType { get; set; }
        public string Title { get; set; }
        public IEnumerable<int> Members { get; set; }
    }
}