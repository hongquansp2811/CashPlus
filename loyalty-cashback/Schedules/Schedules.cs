using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.Models;
using LOYALTY.Extensions;
using LOYALTY.Helpers;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace LOYALTY.Data
{
    public static class Schedules
    {
        public static void RunAffiliate(LOYALTYContext context)
        {
            var transaction = context.Database.BeginTransaction();

            try
            {
                // Xử lý affiliate
                var configSettings = context.AffiliateConfigs.Where(x => x.code == "GENERAL").FirstOrDefault();

                if (configSettings == null)
                {
                    return;
                }

                // Tìm ngày gần nhất trước đó 
                // Ngày hôm nay
                var yearNow = DateTime.Now.Year;
                var monthNow = DateTime.Now.Month;
                var dayNow = DateTime.Now.Day;
                var hourNow = DateTime.Now.Hour;
                var minuteNow = DateTime.Now.Minute;

                // Ngày cấu hình
                var dayConfig = configSettings.date_return;
                TimeSpan dateConvert = (TimeSpan)configSettings.hours_return;
                var stringConfig = dateConvert.ToString("hh\\:mm");

                var hourConfig = stringConfig.Split(":")[0];
                var minuteConfig = stringConfig.Split(":")[1];
                var stringDateInMonth = dayConfig.ToString().PadLeft(2, '0') + "/" + monthNow.ToString().PadLeft(2, '0') + "/" + yearNow
                    + " " + hourConfig.ToString().PadLeft(2, '0') + ":" + minuteConfig.ToString().PadLeft(2, '0') + ":00";
                var dateInMonth = DateTime.ParseExact(stringDateInMonth, "dd/MM/yyyy HH:mm:ss", null);
                var startInMonth = DateTime.ParseExact("01/" + monthNow.ToString().PadLeft(2, '0') + "/" + yearNow
                    + " 00:00:01", "dd/MM/yyyy HH:mm:ss", null);

                // Nếu thời gian cấu hình chưa tới ngày hiện tại thì chưa chạy
                if (dateInMonth > DateTime.Now)
                {
                    return;
                }

                // Nếu đến thời gian rồi xem đã chạy trong tháng chưa
                var objSchedule = context.ScheduleJobss.Where(x => x.date_created >= startInMonth).FirstOrDefault();
                if (objSchedule != null)
                {
                    return;
                }

                // Thêm mới bảng schedule jobs
                var newObjSchedule = new ScheduleJobs();
                newObjSchedule.id = Guid.NewGuid();
                newObjSchedule.name = "RUN_AFFILIATE";
                newObjSchedule.date_created = DateTime.Now;
                newObjSchedule.types = "Ngày " + dayNow + " - Tháng " + monthNow + " - năm " + yearNow;

                context.ScheduleJobss.Add(newObjSchedule);

                context.SaveChanges();

                // Chạy affiliate
                excuteQuery("UPDATE CustomerPointHistory SET status = 5, point_type = 'AVAIABLE' WHERE order_type like 'AFF%'");

                var lstUsers = context.Users.Where(x => x.status == 1 && x.point_affiliate > 0).ToList();

                if (lstUsers.Count > 0)
                {
                    for (int i = 0; i < lstUsers.Count; i++)
                    {
                        var totalAffiliate = lstUsers[i].point_affiliate;
                        lstUsers[i].point_avaiable += totalAffiliate;
                        lstUsers[i].point_affiliate = 0;
                    }
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                transaction.Dispose();
                return;
            }
            transaction.Commit();
            transaction.Dispose();
        }

        public static DataTable excuteQuery(string query)
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
