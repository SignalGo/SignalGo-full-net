using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalGo.Shared.Security
{
    public interface ISecurityAlgoritm
    {
        byte[] Encrypt(byte[] bytes);
        byte[] Decrypt(byte[] bytes);
    }
}
