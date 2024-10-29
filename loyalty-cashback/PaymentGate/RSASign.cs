using System;
using System.Text;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.IO;

namespace LOYALTY
{
    public class RSASign
    {
        public static string readFile(String path)
        {
            return File.ReadAllText(path);
        }

        public static string getPublicKey(string key_path)
        {
            string rawKey = readFile(key_path);
            string public_key = rawKey.Replace("-----BEGIN PUBLIC KEY-----", "");
            public_key = public_key.Replace("-----END PUBLIC KEY-----", "");
            public_key = public_key.Replace("\n", "");
            return public_key;
        }

        public static string getPrivateKey(string key_path)
        {
            string rawKey = readFile(key_path);
            string private_key = rawKey.Replace("-----BEGIN RSA PRIVATE KEY-----", "");
            private_key = private_key.Replace("-----END RSA PRIVATE KEY-----", "");
            private_key = private_key.Replace("\n", "");
            return private_key;
        }

        public static string sign(string rawData, string private_key)
        {
            try
            {
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                rsa.ImportRSAPrivateKey(Convert.FromBase64String(private_key), out _);
                byte[] signByte = rsa.SignData(Encoding.ASCII.GetBytes(rawData), SHA1.Create());
                String sign = Convert.ToBase64String(signByte);
                return sign;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return "";
            }
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString().ToUpper();
        }

        public static byte[] Hex2Byte(string hex)
        {
            if ((hex.Length % 2) != 0)
            {
                throw new ArgumentException();
            }
            char[] chArray = hex.ToCharArray();
            byte[] buffer = new byte[hex.Length / 2];
            int index = 0;
            int num2 = 0;
            int length = hex.Length;
            while (index < length)
            {
                int num4 = Convert.ToInt16("" + chArray[index++] + chArray[index], 0x10) & 0xff;
                buffer[num2] = Convert.ToByte(num4);
                index++;
                num2++;
            }
            return buffer;
        }

        public static string signWith256Base64(string rawData, string private_key)
        {
            try
            {
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                rsa.ImportRSAPrivateKey(Convert.FromBase64String(private_key), out _);
                byte[] signByte = rsa.SignData(Encoding.ASCII.GetBytes(rawData), SHA256.Create());
                String sign = Convert.ToBase64String(signByte);
                return sign;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return "";
            }
        }

        public static string signWithSha1Base64(string rawData, string private_key)
        {
            try
            {
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                rsa.ImportRSAPrivateKey(Convert.FromBase64String(private_key), out _);
                byte[] signByte = rsa.SignData(Encoding.ASCII.GetBytes(rawData), SHA1.Create());
                String sign = Convert.ToBase64String(signByte);
                return sign;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return "";
            }
        }

        public static string signWith256(string rawData, string private_key)
        {
            try
            {
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                rsa.ImportRSAPrivateKey(Convert.FromBase64String(private_key), out _);
                byte[] signByte = rsa.SignData(Encoding.ASCII.GetBytes(rawData), SHA256.Create());
                String sign = ByteArrayToString(signByte);
                return sign;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return "";
            }
        }

        public static bool verifySign(string signedData, string signature, string publicKey)
        {
            try
            {
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(publicKey), out _);
                return rsa.VerifyData(Encoding.ASCII.GetBytes(signedData), SHA1.Create(), Convert.FromBase64String(signature));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public static bool verifySign256(string signedData, string signature, string publicKey)
        {
            try
            {
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(publicKey), out _);
                return rsa.VerifyData(Encoding.ASCII.GetBytes(signedData), SHA256.Create(), Hex2Byte(signature));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public static bool verifySignSha1(string signedData, string signature, string publicKey)
        {
            try
            {
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(publicKey), out _);
                return rsa.VerifyData(Encoding.ASCII.GetBytes(signedData), SHA1.Create(), Hex2Byte(signature));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }


        public static string rplRSAPublicKey(string? key_path)
        {
            if (key_path == null)
            {
                return "";
            }
            string public_key = key_path.Replace("-----BEGIN PUBLIC KEY-----", "");
            public_key = public_key.Replace("-----END PUBLIC KEY-----", "");
            public_key = public_key.Replace("\n", "");
            return public_key;
        }

        public static string rplRSAPrivateKey(string? key_path)
        {
            if (key_path == null)
            {
                return "";
            }
            string private_key = key_path.Replace("-----BEGIN RSA PRIVATE KEY-----", "");
            private_key = private_key.Replace("-----END RSA PRIVATE KEY-----", "");
            private_key = private_key.Replace("\n", "");
            return private_key;
        }
    }
}
