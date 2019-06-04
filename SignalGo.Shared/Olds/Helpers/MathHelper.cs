using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalGo.Shared.Helpers
{
    public static class MathHelper
    {
        public static bool ByteArrayCompare(byte[] bytes1, byte[] bytes2)
        {
            if (bytes1.Length != bytes2.Length)
                return false;

            for (int i = 0; i < bytes1.Length; i++)
                if (bytes1[i] != bytes2[i])
                    return false;

            return true;
        }
    }
}
