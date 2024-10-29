using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace LOYALTY.Extensions
{
    public class ExcuteStringQuery
    {
        private readonly IConfiguration _configuration;
        public ExcuteStringQuery(IConfiguration configuration)
        {
            _configuration = configuration;
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
    }
}
