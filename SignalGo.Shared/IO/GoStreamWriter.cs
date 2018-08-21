//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//#if (!PORTABLE)
//using System.Net.Sockets;
//#endif
//using System.Text;

//namespace SignalGo.Shared.IO
//{
//    /// <summary>
//    /// signalGo stream Writer helper
//    /// </summary>
//    public static class GoStreamWriter
//    {
//        /// <summary>
//        /// write a block to end of udpClient
//        /// </summary>
//        /// <param name="udpClient">client</param>
//        /// <param name="data">bytes of data to write</param>
//#if (NETSTANDARD || NETCOREAPP)
//        /// <param name="iPEndPoint">address to write</param>
//        public static async void WriteToEnd(UdpClient udpClient, IPEndPoint iPEndPoint, byte[] data)
//#elif (PORTABLE)
//        public static async void WriteToEnd(Sockets.Plugin.UdpSocketClient udpClient, byte[] data)

//#else
//        public static void WriteToEnd(UdpClient udpClient, IPEndPoint iPEndPoint, byte[] data)
//#endif
//        {
//            int count = data.Length;
//            int lengthWrited = 0;
//            while (lengthWrited < count)
//            {
//                int countToWrite = count;
//                if (lengthWrited + countToWrite > count)
//                {
//                    countToWrite = count - lengthWrited;
//                }
//                byte[] bytes = data.ToList().GetRange(lengthWrited, count - lengthWrited).ToArray();
//#if (NETSTANDARD || NETCOREAPP)
//                var writeCount = await udpClient.SendAsync(bytes, data.Length, iPEndPoint);
//#elif (PORTABLE)
//                var writeCount = data.Length;
//                await udpClient.SendAsync(bytes, data.Length);
//#else
//                var writeCount = udpClient.Send(bytes, data.Length, iPEndPoint);
//#endif
//                if (writeCount == 0)
//                    break;
//                lengthWrited += writeCount;
//            }
//        }

//        public static void WriteToStream(System.IO.Stream stream, byte[] data, bool IsWebSocket)
//        {
//            if (IsWebSocket)
//            {
//                var encode = EncodeMessageToSend(data);
//                stream.Write(encode, 0, encode.Length);
//            }
//            else
//            {
//                stream.Write(data, 0, data.Length);
//            }
//        }

//        public static void WriteBlockToStream(System.IO.Stream stream, byte[] data)
//        {
//            var size = BitConverter.GetBytes(data.Length);
//            stream.Write(size, 0, size.Length);
//            stream.Write(data, 0, data.Length);
//        }

//        private static byte[] EncodeMessageToSend(byte[] bytesRaw)
//        {
//            byte[] response;
//            byte[] frame = new byte[10];

//            int indexStartRawData = -1;
//            int length = bytesRaw.Length;

//            frame[0] = (byte)129;
//            if (length <= 125)
//            {
//                frame[1] = (byte)length;
//                indexStartRawData = 2;
//            }
//            else if (length >= 126 && length <= 65535)
//            {
//                frame[1] = (byte)126;
//                frame[2] = (byte)((length >> 8) & 255);
//                frame[3] = (byte)(length & 255);
//                indexStartRawData = 4;
//            }
//            else
//            {
//                frame[1] = (byte)127;
//                frame[2] = (byte)((length >> 56) & 255);
//                frame[3] = (byte)((length >> 48) & 255);
//                frame[4] = (byte)((length >> 40) & 255);
//                frame[5] = (byte)((length >> 32) & 255);
//                frame[6] = (byte)((length >> 24) & 255);
//                frame[7] = (byte)((length >> 16) & 255);
//                frame[8] = (byte)((length >> 8) & 255);
//                frame[9] = (byte)(length & 255);

//                indexStartRawData = 10;
//            }

//            response = new byte[indexStartRawData + length];

//            int i, reponseIdx = 0;

//            //Add the frame bytes to the reponse
//            for (i = 0; i < indexStartRawData; i++)
//            {
//                response[reponseIdx] = frame[i];
//                reponseIdx++;
//            }

//            //Add the data bytes to the response
//            for (i = 0; i < length; i++)
//            {
//                response[reponseIdx] = bytesRaw[i];
//                reponseIdx++;
//            }

//            return response;
//        }
//    }
//}
