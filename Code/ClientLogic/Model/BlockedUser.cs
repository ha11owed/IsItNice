using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ClientLogic.Model
{
    public class BlockedUser : BaseEntity
    {
        public long OwnerId { get; set; }

        public long BlockedUserId { get; set; }
    }
}
