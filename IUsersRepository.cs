using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Messenger.Model;

namespace Messenger.DataLayer
{
    public interface IUsersRepository
    {        
        User CreateUser(User user);
        void DeleteUser(int id);
        User GetUser(int id);

        User PersistUser(User user);
        void SetPassword(int userId, string newHash);
    }
}
