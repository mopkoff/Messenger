using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messenger.Model
{
    public class User
    {
        public static readonly int defaultId = -1;

        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }

        public UserInfo userInfo { get; set; }
                
        public User()
        {
            Id = defaultId;
        }

        public User(int Id, string Password)
        {
            this.Id = Id;
            this.Password = Password.GetHashCode().ToString();
        }
    }
}
