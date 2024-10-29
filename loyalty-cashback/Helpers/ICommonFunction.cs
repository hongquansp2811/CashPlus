using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;

namespace LOYALTY.Helpers
{
    public interface ICommonFunction
    {
        public string ComputeSha256Hash(string rawData);
        public DateTime convertStringFullToDate(string stringDate);
        public DateTime convertStringSortToDate(string stringDate);
        public string convertDateToStringSort(DateTime? dateObject);
        public string convertDateToStringFull(DateTime? dateObject);
        public TimeSpan convertStringSortToTimeSpan(string stringDate);
        public string convertTimeSpanToStringSort(TimeSpan? dateObject);
        public string replaceRandomStringTo(string replaceString);
        public DataTable excuteQuery(string query);
        public DataTable excuteQueryGetTeam(decimal customer_id);

        public bool ValidatePassword(string password);  
    }
}
