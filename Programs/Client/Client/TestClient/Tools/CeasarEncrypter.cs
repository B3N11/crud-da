using System;
using System.Text;

namespace CarCRUD.Tools
{
    class CeasarEncrypt
    {
        /// <summary>
        /// Encrypts or decrypts a data string.
        /// </summary>
        /// <param name="data">The data to be crypted.</param>
        /// <param name="encrypt">True to encrypt, false to decrypt.</param>
        public static string Encrypt(string data, bool encrypt, int key)
        {
            if (encrypt)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(data);

                for (int i = 0; i < bytes.Length; i++)
                    BitPush(ref bytes[i], encrypt, key);

                data = Convert.ToBase64String(bytes);
                return data;
            }
            else
            {
                byte[] bytes = Convert.FromBase64String(data);

                for (int i = 0; i < bytes.Length; i++)
                    BitPush(ref bytes[i], encrypt, key);

                data = Encoding.UTF8.GetString(bytes);
                return data;
            }
        }

        /// <summary>
        /// Does bitpush on a byte.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="encrypt"></param>
        /// <param name="key"></param>
        private static void BitPush(ref byte data, bool encrypt, int key)
        {
            string dataByte = Convert.ToString(data, 2).PadLeft(8, '0');
            char[] result = new char[8];
            int validKey = key % 8;

            //Encrypt
            if (encrypt)
            {
                for (int i = 0; i < 8; i++)
                {
                    int position = i + validKey;
                    if (position >= 8) { position -= 8; }
                    result[position] = dataByte[i];
                }
            }
            //Decrypt
            else
            {
                for (int i = 0; i < 8; i++)
                {
                    int position = i - validKey;
                    if (position < 0) { position += 8; }
                    result[position] = dataByte[i];
                }
            }

            //Return
            data = Convert.ToByte(new string(result), 2);
        }
    }
}
