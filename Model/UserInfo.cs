using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Messenger.Model.Enums;


namespace Messenger.Model
{
    public class UserInfo
    {
        public static readonly Image defaultAvatar = Image.FromFile("defaultAvatar.bmp");
        public string FirstName {get; set;}
        public string LastSurename { get; set; }
        public Image Avatar { get; set; }
        public string About { get; set; }
        public GenderTypes Gender { get; set; }

        public UserInfo()
        {
            Avatar = defaultAvatar;
        }
    }
}
