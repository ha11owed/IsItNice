using ClientLogic.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ClientLogic.Model
{
    public enum EntityState
    {
        Invalid,
        New,
        Deleted,
        Default
    }

    /// <summary>
    /// Base Entity for all database items
    /// <remarks>
    /// C# does not allow the use of the keyword friend
    /// </remarks>
    /// </summary>
    public abstract class BaseEntity
    {
        [DataMember(Name = "id")]
        public long ID { get; internal set; }
        
        [IgnoreDataMember]
        public EntityState EntityState
        {
            get { return _entityState; }
        }
        internal EntityState _entityState = EntityState.Invalid;

        #region Overwitten methods

        public override int GetHashCode()
        {
            return GetType().GetHashCode() + (int)(ID % 103);
        }

        public override bool Equals(object obj)
        {
            return (this == obj) || (null != obj && obj is BaseEntity
                && ID != default(long)
                && (obj as BaseEntity).ID == ID
                && obj.GetType().Equals(GetType()));
        }

        public override string ToString()
        {
            return GetType().Name + " ID: " + ID;
        }

        #endregion
    }
}
