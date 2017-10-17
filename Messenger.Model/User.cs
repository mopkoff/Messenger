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
        //password Hash
        public string Password { get; set; }

        public UserInfo userInfo { get; set; }
                
        public User()
        {
            Id = defaultId;
        }

        public User(string Login, string Password)
        {
            this.Login = Login;
            this.Password = Password.GetHashCode().ToString();
        }

        public User(int Id, string Login, string Password)
        {
            this.Id = Id;
            this.Login = Login;
            this.Password = Password.GetHashCode().ToString();
        }


    }

    public class UserEqualityComparer : IEqualityComparer<User>
    {
        bool IEqualityComparer<User>.Equals(User x, User y)
        {
            if (x == null && y == null)
                return true;
            else if (x == null | y == null)
                return false;
            else if (x.Id == y.Id)
                return true;
            else
                return false;
        }

        int IEqualityComparer<User>.GetHashCode(User obj)
        {
            return obj.GetHashCode();
        }
    }
}
