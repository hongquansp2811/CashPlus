using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System;

namespace LOYALTY.PaymentGate
{
    public static class DataFormatHelper
    {
        public static string EscapeStringValue(string value)
        {
            string dataEncode = string.Empty;
            var stringBuilder = new StringBuilder();
            using (var jsonWriter = new JsonTextWriter(new StringWriter(stringBuilder)))
            {
                jsonWriter.StringEscapeHandling = StringEscapeHandling.EscapeNonAscii;
                jsonWriter.WriteValue(value);
                jsonWriter.Close();
                dataEncode = stringBuilder.ToString();
            }

            dataEncode = dataEncode.Replace("\"{", "{").Replace("}\"", "}").Replace("\\\"", "\"").Replace("/", "\\/");

            return dataEncode;
        }
    }

    public class ConfigAPI
    {
        public void setValueCfg(string api_key, string api_secret)
        {
            BaoKimApi.API_KEY = api_key;
            BaoKimApi.API_SECRET = api_secret;
        }
    }

    public static class BaoKimApi
    {
        public static string API_KEY { get; set; }
        public static string API_SECRET { get; set; }

        private const int TOKEN_EXPIRE = 60;        // Token expire time in seconds
        private static string _jwt = string.Empty;

        public static string MERCHANT_ID = BKConsts.PG_MERCHANT_ID_1;

        public static string JWT
        {
            get
            {
                _jwt = RefreshToken(_jwt);
                return _jwt;
            }
        }

        public static string HmacSha256Encode(string data)
        {
            if (string.IsNullOrEmpty(data)) return string.Empty;

            data = DataFormatHelper.EscapeStringValue(data);

            UTF8Encoding encoding = new UTF8Encoding();
            byte[] keyBytes = encoding.GetBytes(API_SECRET);
            byte[] messageBytes = encoding.GetBytes(data);
            HMACSHA256 cryptographer = new HMACSHA256(keyBytes);

            byte[] bytes = cryptographer.ComputeHash(messageBytes);

            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

        private static string RefreshToken(string jwt)
        {
            if (!string.IsNullOrEmpty(jwt))
            {
                // Check token is expired
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(_jwt);
                var tokenDecode = jsonToken as JwtSecurityToken;
                if (tokenDecode.ValidTo > DateTime.UtcNow.AddSeconds(TOKEN_EXPIRE / 5))
                    return jwt;
            }

            // Create new jwt
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(API_SECRET));
            var signInCred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var currentTime = DateTime.UtcNow;

            var token = new JwtSecurityToken(
                    issuer: API_KEY,                                    // Issuer
                    notBefore: currentTime,                             // Not before
                    expires: currentTime.AddSeconds(TOKEN_EXPIRE),      // Expire
                    signingCredentials: signInCred                      // The signing key
                );

            token.Payload["iat"] = currentTime;                         // Issued at: time when the token was generated
            token.Payload["jti"] = "123";                               // Json Token Id: an unique identifier for the token
            token.Payload["form_params"] = new { };                     // Request body

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
