using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Messenger.Model.Enums;

namespace Messenger.Model
{
    public class Attachment
    {
        public int Id { get; set; }
        public AttachmentTypes Type { get; set; }
        public byte[] File { get; set; }
    }
}
