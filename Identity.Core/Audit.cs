using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Core
{
    public class Audit<T> : IAudit
    {
        private readonly T _entity;

        public T Entity
        {
            get
            {
                return _entity;
            }
        }

        public DateTime CreateDate { get; set; }
        public Guid CreateBy { get; set; }
        public DateTime ModifyDate { get; set; }
        public Guid ModifyBy { get; set; }

        [DebuggerStepThrough]
        public Audit(T entity, Guid accountKey)
        {
            _entity = entity;
            ModifyBy = accountKey;
            CreateBy = accountKey;
        }

        internal Audit(T entity)
        {
            _entity = entity;
        }

        internal Audit()
        {
            _entity = default(T);
        }
    }

    public class Audit : IAudit
    {
        public Audit(Guid key)
        {
            CreateBy = key;
            ModifyBy = key;
        }

        public DateTime CreateDate { get; set; }
        public Guid CreateBy { get; set; }
        public DateTime ModifyDate { get; set; }
        public Guid ModifyBy { get; set; }
    }

    public interface IAudit
    {
        Guid CreateBy { get; set; }
        DateTime CreateDate { get; set; }
        Guid ModifyBy { get; set; }
        DateTime ModifyDate { get; set; }
    }
}
