using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Model
{
    public class NiceRequest : BaseEntity
    {
        public double PriceValue { get; set; }
        public string Currency { get; set; }

        public string Price { get { return PriceValue + " " + Currency; } }

        public string Description { get; set; }

        public bool IsClosed { get; set; }

        public User User { get; set; }

        public List<Comment> FriendComments { get { return _friendComments; } }
        private readonly List<Comment> _friendComments = new List<Comment>();
        
        #region Constructors

        public NiceRequest(string id)
            : base(id)
        {
        }

        public NiceRequest()
            : base()
        {
        }

        #endregion
    }
}
