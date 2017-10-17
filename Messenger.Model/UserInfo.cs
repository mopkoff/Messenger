using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Messenger.Model.Enums;
using System.IO;

namespace Messenger.Model
{
    public class UserInfo
    {
        public static readonly Image defaultAvatar = Image.FromFile("C:\\Users\\Shureek\\source\\repos\\Messenger\\Messenger.Model\\defaultAvatar.bmp");
        public string FirstName {get; set;}
        public string LastName { get; set; }
        public Image Avatar { get; set; }
        public string About { get; set; }
        public GenderTypes Gender { get; set; }

        public UserInfo()
        {
            Avatar = defaultAvatar;
        }

        public byte[] GetAvatarAsByteArray()
        {
            MemoryStream ms = new MemoryStream();
            Avatar.Save(ms, Avatar.RawFormat);
            return ms.ToArray();
        }

        public void SetAvatarUsingByteArray(byte[] byteArrayIn)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn);
            Avatar = Image.FromStream(ms);
        }
        public override bool Equals(object objuInfo)
        {
            UserInfo uInfo = objuInfo as UserInfo;
            if (this == null && uInfo == null)
                return true;
            else if (this == null | uInfo == null)
                return false;
            else if ((this.FirstName == uInfo.FirstName) && (this.LastName == uInfo.LastName) && (this.GetAvatarAsByteArray().SequenceEqual(uInfo.GetAvatarAsByteArray())) && (this.About == uInfo.About) && (this.Gender == uInfo.Gender))
                return true;
            else
                return false;
        }
        public override int GetHashCode()
        {
            return FirstName.GetHashCode() ^ Avatar.GetHashCode() ^ LastName.GetHashCode() ^ About.GetHashCode() ^ Gender.GetHashCode() ;
        }
    }
    
}
