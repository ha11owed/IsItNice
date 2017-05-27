using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ClientLogic.Model
{
    public class User : BaseEntity
    {
        public string UserId { get; set; }
        public string LiveId { get; set; }

        [IgnoreDataMember]
        public string FirstName { get; set; }

        [IgnoreDataMember]
        public string LastName { get; set; }

        [IgnoreDataMember]
        public string Name { get; set; }

        [IgnoreDataMember]
        public string ImageURI
        {
            get
            {
                if (_userImage == null) { return Settings.MissingUserImageURI; }
                else { return _userImage; }
            }
            set { _userImage = value; }
        }
        private volatile string _userImage = null;

        [IgnoreDataMember]
        public bool IsBlocked { get; set; }

    }
}
