using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Core
{
    public class IdentityConnection : IIdentityConnection, IDisposable
    {
        public ConnectionStringSettings Connection { get; set; }

        public IdentityConnection(ConnectionStringSettings connection)
        {
            Connection = connection;
        }

        public void Dispose()
        {
            if (Connection != null)
            {
                Connection = null;
            }
        }
    }
}
