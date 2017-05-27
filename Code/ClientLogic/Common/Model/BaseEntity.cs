using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Model
{
    public abstract class BaseEntity
    {
        public BaseEntity()
        {
            this.ID = new Guid().ToString();
        }

        public BaseEntity(string id)
        {
            this.ID = id;
        }

        public string ID { get; private set; }
    }
}
