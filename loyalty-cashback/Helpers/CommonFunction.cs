using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
using System.Text;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using LOYALTY.Extensions;
using System.Text.RegularExpressions;


namespace LOYALTY.Helpers
{
    public class CommonFunction : ICommonFunction
    {
        private readonly IConfiguration _configuration;
        public CommonFunction(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public DateTime convertStringFullToDate(string stringDate)
        {
            return DateTime.ParseExact(stringDate, "dd/MM/yyyy HH:mm:tt", null);
        }

        public DateTime convertStringSortToDate(string stringDate)
        {
            return DateTime.ParseExact(stringDate, "dd/MM/yyyy", null);
        }

        public TimeSpan convertStringSortToTimeSpan(string stringDate)
        {
            return TimeSpan.ParseExact(stringDate, "hh\\:mm", CultureInfo.InvariantCulture);
        }

        public string convertTimeSpanToStringSort(TimeSpan? dateObject)
        {
            if (dateObject == null)
            {
                return DateTime.Now.ToString("HH:mm");
            }
            else
            {
                TimeSpan dateConvert = (TimeSpan)dateObject;
                return dateConvert.ToString("hh\\:mm");
            }

        }
        public string convertDateToStringSort(DateTime? dateObject)
        {
            if (dateObject == null)
            {
                return DateTime.Now.ToString("dd/MM/yyyy");
            }
            else
            {
                DateTime dateConvert = (DateTime)dateObject;
                return dateConvert.ToString("dd/MM/yyyy");
            }
        }

        public string convertDateToStringFull(DateTime? dateObject)
        {
            if (dateObject == null)
            {
                return DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            }
            else
            {
                DateTime dateConvert = (DateTime)dateObject;
                return dateConvert.ToString("dd/MM/yyyy HH:mm:ss");
            }
        }

        public string replaceRandomStringTo(string replaceString)
        {
            if (replaceString.Length < 4)
            {
                return replaceString;
            }

            StringBuilder stringReturn = new StringBuilder(replaceString);
            for (int i = 2; i < stringReturn.Length - 2; i++)
            {
                stringReturn[i] = '*';
            }
            return stringReturn.ToString();
        }

        public string replaceRandomStringSortTo(string replaceString)
        {
            if (replaceString.Length < 4)
            {
                return replaceString;
            }

            StringBuilder stringReturn = new StringBuilder(replaceString);
            for (int i = 2; i < stringReturn.Length - 2; i++)
            {
                stringReturn[i] = '*';
            }
            return stringReturn.ToString();
        }

        public DataTable excuteQuery(string query)
        {
            DataTable table = new DataTable();
            string sqlDataSource = Consts.DB_CONNECT;

            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();
                }
            }

            return table;
        }

        public DataTable excuteQueryGetTeam(decimal customer_id)
        {
            string query = "WITH tree (id, share_person, level, full_name, date_created, rn) as "
                           + "( SELECT id, share_person, 0 as level, full_name, date_created, convert(varchar(max), right(row_number() over(order by id), 10)) rn "
                           + " FROM Customer c where c.id = "
                           + customer_id
                           + " UNION ALL "
                           + " SELECT c2.id, c2.share_person, tree.level + 1, c2.full_name, c2.date_created, "
                           + " rn + '/' + convert(varchar(max), right(row_number() over(order by tree.id), 10)) "
                           + " FROM Customer c2  INNER JOIN tree ON tree.id = c2.share_person )"
                           + " SELECT * FROM tree where level > 0 and level < 4";

            DataTable table = new DataTable();
            string sqlDataSource = Consts.DB_CONNECT;

            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();
                }
            }

            return table;
        }

        public DataTable excuteQueryGetTeamByTime(decimal customer_id)
        {
            string query = "WITH tree (id, share_person, level, full_name, date_created, rn) as "
                           + "( SELECT id, share_person, 0 as level, full_name, date_created, convert(varchar(max), right(row_number() over(order by id), 10)) rn "
                           + " FROM Customer c where c.id = "
                           + customer_id
                           + " UNION ALL "
                           + " SELECT c2.id, c2.share_person, tree.level + 1, c2.full_name, c2.date_created, "
                           + " rn + '/' + convert(varchar(max), right(row_number() over(order by tree.id), 10)) "
                           + " FROM Customer c2  INNER JOIN tree ON tree.id = c2.share_person )"
                           + " SELECT * FROM tree where level > 0 and level < 4";

            DataTable table = new DataTable();
            string sqlDataSource = Consts.DB_CONNECT;

            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();
                }
            }

            return table;
        }

        public bool ValidatePassword(string password)
        {
            var hasNumber = new Regex(@"[0-9]+");
            var hasUpperChar = new Regex(@"[A-Z]+");
            var hasSpecialChar = new Regex(@"[!@#$%^&*()_+=\[{\]};:<>|./?,-]");

            if (!hasNumber.IsMatch(password) || !hasUpperChar.IsMatch(password) || !hasSpecialChar.IsMatch(password) || password.Length < 6)
                return false;

            return true;
        }
    }
}
