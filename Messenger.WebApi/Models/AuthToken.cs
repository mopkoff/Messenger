using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Messenger.WebApi.Models
{
    public class AuthToken
    {
        public Guid auth_token { get; }
        public int Id { get; set; }
        public string username { get; set; }

        public AuthToken()
        {
        }

        public AuthToken(Guid auth_token, int Id, string username)
        {
            this.auth_token = auth_token;
            this.username = username;
            this.Id = Id;
        }
    }
}