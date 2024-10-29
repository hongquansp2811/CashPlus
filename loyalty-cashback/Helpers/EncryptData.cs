using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace LOYALTY.Helpers
{
    public class EncryptData : IEncryptData
    {
        public string EncryptDataFunction(string publicKey, object dataEncrypt)
        {
            string stringEncrypt = JsonConvert.SerializeObject(dataEncrypt);
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(publicKey);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(stringEncrypt);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);

        }

        public object DecryptDataFunction(string publicKey, string dataDecrypt)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(dataDecrypt);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(publicKey);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            string stringReturn = streamReader.ReadToEnd();
                            object dataReturn = JsonConvert.DeserializeObject(stringReturn);
                            return dataReturn;
                        }
                    }
                }
            }
        }

        //public string EncryptDataFunction(string publicKey, object dataEncrypt)
        //{
        //    string stringEncrypt = JsonConvert.SerializeObject(dataEncrypt);
        //    byte[] plainTextBytes = Encoding.UTF8.GetBytes(stringEncrypt);
        //    string stringDecode = Convert.ToBase64String(plainTextBytes);
        //    CspParameters CSApars = new CspParameters();
        //    CSApars.KeyContainerName = publicKey;

        //    RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(CSApars);

        //    byte[] byteText = Encoding.UTF8.GetBytes(stringDecode);
        //    byte[] byteEntry = rsa.Encrypt(byteText, false);

        //    return Convert.ToBase64String(byteEntry);
        //}


        //public object DecryptDataFunction(string publicKey, string dataDecrypt)
        //{
        //    CspParameters CSApars = new CspParameters();
        //    CSApars.KeyContainerName = publicKey;

        //    RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(CSApars);

        //    byte[] byteEntry = Convert.FromBase64String(dataDecrypt);
        //    byte[] byteText = rsa.Decrypt(byteEntry, false);

        //    string base64String = Encoding.UTF8.GetString(byteText);
        //    byte[] plainTextBytes = Convert.FromBase64String(base64String);
        //    string stringDecryptJson = Encoding.UTF8.GetString(plainTextBytes);
        //    object dataReturn = JsonConvert.DeserializeObject(stringDecryptJson);

        //    return dataReturn;
        //}

        public static string Encrypt(
          string plainText,
          string passPhrase,
          string saltValue,
          string hashAlgorithm,
          int passwordIterations,
          string initVector,
          int keySize)
        {
            byte[] bytes1 = Encoding.ASCII.GetBytes(initVector);
            byte[] bytes2 = Encoding.ASCII.GetBytes(saltValue);
            byte[] bytes3 = Encoding.UTF8.GetBytes(plainText);
            byte[] bytes4 = new PasswordDeriveBytes(passPhrase, bytes2, hashAlgorithm, passwordIterations).GetBytes(keySize / 8);
            RijndaelManaged rijndaelManaged = new RijndaelManaged();
            rijndaelManaged.Mode = CipherMode.CBC;
            ICryptoTransform encryptor = rijndaelManaged.CreateEncryptor(bytes4, bytes1);
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write);
            cryptoStream.Write(bytes3, 0, bytes3.Length);
            cryptoStream.FlushFinalBlock();
            byte[] array = memoryStream.ToArray();
            memoryStream.Close();
            cryptoStream.Close();
            return Convert.ToBase64String(array);
        }


        public static string Decrypt(
          string cipherText,
          string passPhrase,
          string saltValue,
          string hashAlgorithm,
          int passwordIterations,
          string initVector,
          int keySize)
        {
            byte[] bytes1 = Encoding.ASCII.GetBytes(initVector);
            byte[] bytes2 = Encoding.ASCII.GetBytes(saltValue);
            byte[] buffer = Convert.FromBase64String(cipherText);
            byte[] bytes3 = new PasswordDeriveBytes(passPhrase, bytes2, hashAlgorithm, passwordIterations).GetBytes(keySize / 8);
            RijndaelManaged rijndaelManaged = new RijndaelManaged();
            rijndaelManaged.Mode = CipherMode.CBC;
            ICryptoTransform decryptor = rijndaelManaged.CreateDecryptor(bytes3, bytes1);
            MemoryStream memoryStream = new MemoryStream(buffer);
            CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read);
            byte[] numArray = new byte[buffer.Length];
            int count = cryptoStream.Read(numArray, 0, numArray.Length);
            memoryStream.Close();
            cryptoStream.Close();
            return Encoding.UTF8.GetString(numArray, 0, count);
        }
    }
}
