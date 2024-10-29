using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;
using LOYALTY.Interfaces;
using LOYALTY.Extensions;

namespace LOYALTY.Helpers
{
    public class Authen : IJwtAuth
    {
        private readonly string key;
        public Authen(string key)
        {
            this.key = key;
        }
        public string Authentication(string username, Guid id, string password, string usertype, string allpermissions)
        {
            if (!(username.Equals(username) || password.Equals(password)))
            {
                return null;
            }

            // 1. Create Security Token Handler
            var tokenHandler = new JwtSecurityTokenHandler();

            // 2. Create Private Key to Encrypted
            var tokenKey = Encoding.ASCII.GetBytes(key);

            //3. Create JETdescriptor
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(
                    new Claim[]
                    {
                        new Claim(ClaimTypes.Name, username),
                        new Claim(ClaimTypes.NameIdentifier, usertype),
                        new Claim(ClaimTypes.Role, allpermissions),
                    }),
                Expires = DateTime.UtcNow.AddDays(int.Parse(Consts.TOKEN_EXPIRE_DAY)),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
            };
            //4. Create Token
            var token = tokenHandler.CreateToken(tokenDescriptor);

            // 5. Return Token from method
            return tokenHandler.WriteToken(token);
        }

        public string BranchAuthentication(string username, string password, string usertype, Guid customer_id)
        {
            if (!(username.Equals(username) || password.Equals(password)))
            {
                return null;
            }

            // 1. Create Security Token Handler
            var tokenHandler = new JwtSecurityTokenHandler();

            // 2. Create Private Key to Encrypted
            var tokenKey = Encoding.ASCII.GetBytes(key);

            //3. Create JETdescriptor
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(
                    new Claim[]
                    {
                        new Claim(ClaimTypes.Name, username),
                        new Claim(ClaimTypes.NameIdentifier, usertype),
                        new Claim(ClaimTypes.Surname, customer_id.ToString()),
                    }),
                Expires = DateTime.UtcNow.AddDays(int.Parse(Consts.TOKEN_EXPIRE_DAY)),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
            };
            //4. Create Token
            var token = tokenHandler.CreateToken(tokenDescriptor);

            // 5. Return Token from method
            return tokenHandler.WriteToken(token);
        }

        public string BKAuthentication(string apiKey, string secretkey)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(apiKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var tokenDescriptor = new JwtSecurityToken(secretkey, secretkey, null, null, signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }
    }
}
