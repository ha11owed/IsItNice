using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ClientLogic.Model
{
    public class Channel : BaseEntity
    { 
        [DataMember(Name = "uri")]
        public string Uri { get; set; }

        public string UserId { get; set; }
        public string LiveId { get; set; }
    }
 
}
