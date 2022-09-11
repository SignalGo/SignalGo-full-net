using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SignalGo.Publisher.Shared.Helpers
{
    public class HashHelper
    {
        public static string ComputeHash(byte[] input, HashAlgorithm algorithm)
        {
            Byte[] hashedBytes = algorithm.ComputeHash(input);
            return BitConverter.ToString(hashedBytes);
        }
        /// <summary>
        /// compute input hash using SHA256 Algoritm
        /// </summary>
        public static string ComputeHash(byte[] input)
        {
            byte[] hashedBytes = SHA256.Create().ComputeHash(input);
            return BitConverter.ToString(hashedBytes);
        }
        public static string ComputeHash(byte[] input, HashAlgorithm algorithm, Byte[] salt)
        {
            // Combine salt and input bytes
            Byte[] saltedInput = new Byte[salt.Length + input.Length];
            salt.CopyTo(saltedInput, 0);
            input.CopyTo(saltedInput, salt.Length);

            Byte[] hashedBytes = algorithm.ComputeHash(saltedInput);
            return BitConverter.ToString(hashedBytes);
        }
    }
}
