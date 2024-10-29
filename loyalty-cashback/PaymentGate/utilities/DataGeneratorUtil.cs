using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.PaymentGate.utilities
{
    public class DataGeneratorUtil
    {
        public static string getVietNamDateTime(string format)
        {
            DateTime dateTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "SE Asia Standard Time");
            return dateTime.ToString(format);
        }

        public static string getRandomTransId(string prefix, int length)
        {
            string ran = getRandomString((length - 1) - prefix.Length);
            if (prefix.Equals(""))
            {
                return ran;
            }
            else
            {
                return prefix + "_" + ran;
            }
        }

        private static string getRandomString(int length)
        {
            string stock = "abcdefghijklmnopqrstuvwxyz0123456789";
            string ranStr = "";
            Random random = new Random();
            for (int i = 0; i < length; i++)
            {
                ranStr += stock[random.Next(stock.Length - 1)];
            }
            return ranStr;
        }
    }
}
