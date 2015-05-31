using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Core
{
    public interface IIdentityConnection
    {
        ConnectionStringSettings Connection { get; set; } 
    }
}
