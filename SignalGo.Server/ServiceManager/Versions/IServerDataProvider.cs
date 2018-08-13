using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Server.ServiceManager.Versions
{
    public interface IServerDataProvider
    {
        void Start(ServerBase serverBase, int port);
    }
}
