using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Model
{
    public class Comment : BaseEntity
    {
        public User User { get; set; }

        public string Message { get; set; }

        public int Score { get; set; }

        public bool IsCompleted { get { return Score > 0; } }

        #region Constructors

        public const int NoScore = -999;

        public Comment(string id)
            : base(id)
        {
            this.Score = NoScore;
        }

        public Comment()
            : base()
        {
            this.Score = NoScore;
        }

        #endregion
    }
}
