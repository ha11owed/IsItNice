using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ClientLogic.Model
{
    /// <summary>
    /// A comment for a NiceRequest.
    /// </summary>
    public class Comment : BaseEntity
    {
        public string Message { get; set; }

        public int Score { get; set; }
        public DateTime CreatedAt { get; set; }

        public long FromId { get; set; }
        public long RequestOwnerId { get; set; }
        public long RequestId { get; set; }
        
        #region Constructors

        public Comment()
            : base()
        {
        }

        #endregion
    }
}
