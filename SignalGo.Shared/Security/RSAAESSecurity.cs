#if (!PORTABLE)
using System;
using System.Security.Cryptography;

namespace SignalGo.Shared.Security
{
    public class AESKey
    {
        public byte[] Key { get; set; }
        public byte[] IV { get; set; }
    }

    public class RSAAESSecurity : ISecurityAlgoritm
    {
        public RSAAESSecurity()
        {
            RSAKey keys = RSASecurity.GenerateRandomKey();
            RSAReceiverEncryptKey = keys.PublicKey;
            RSADecryptKey = RSASecurity.StringToKey(keys.PrivateKey);
        }

        public string RSAReceiverEncryptKey { get; set; }//send key to Client, client must encrypt her data by this key
        public RSAParameters RSAEncryptKey { get; set; }//encrypt for client,encrypt client data
        public RSAParameters RSADecryptKey { get; set; }//decrypt data
        public byte[] AESKey { get; set; }
        public byte[] AESIV { get; set; }

        public byte[] Decrypt(byte[] bytes)
        {
            return AESSecurity.DecryptBytes(bytes, AESKey, AESIV);
        }

        public byte[] Encrypt(byte[] bytes)
        {
            return AESSecurity.EncryptBytes(bytes, AESKey, AESIV);
        }

        public void SetAESKeys(byte[] key, byte[] IV)
        {
            AESKey = RSASecurity.Decrypt(key, RSADecryptKey);
            AESIV = RSASecurity.Decrypt(IV, RSADecryptKey);
        }

        public static AESKey GenerateAESKeys()
        {
#if (NETSTANDARD || NETCOREAPP)
            throw new NotSupportedException("not support for this .net standard version!");
#else
            using (RijndaelManaged myRijndael = new RijndaelManaged())
            {

                myRijndael.GenerateKey();
                myRijndael.GenerateIV();

                return new Security.AESKey() { Key = myRijndael.Key, IV = myRijndael.IV };
            }
#endif
        }
    }
}
#endif