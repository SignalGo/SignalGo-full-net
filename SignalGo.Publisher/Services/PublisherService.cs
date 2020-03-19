using System;
using SignalGo.Shared.DataTypes;

namespace SignalGo.Publisher.Services
{
    [ServiceContract("Publisher", ServiceType.OneWayService, InstanceType.SingleInstance)]
    public class PublisherService
    {

        public bool StopServer(Guid serverKey, string name)
        {

            return true;
        }

        public bool StartServer(Guid serverKey, string name)
        {

            return true;
        }

        public bool RestartServer(Guid serverKey, string name, bool force = false)
        {

            return true;
        }

    }
}
