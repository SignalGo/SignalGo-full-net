using SignalGo.Publisher.Engine.Models;
using SignalGo.Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SignalGo.Publisher.Engines.Providers
{
    public class PublisherServer : IPublisherServer
    {

        public PublisherServer()
        {

        }

        public StreamInfo DownloadFile(FileCheckSum checkSum, string password)
        {
            throw new NotImplementedException();
        }

        public void UploadFile(StreamInfo fileUpload)
        {
            throw new NotImplementedException();
        }
    }
}
