using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Model
{
    public class User : BaseEntity
    {
        // Personal info
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Name { get; set; }
        public string UserImage { get; set; }

        #region Constructors

        public User(string id)
            : base(id)
        {
        }

        public User()
            : base()
        {
        }

        #endregion
    }
}
