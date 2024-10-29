using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace LOYALTY.CloudMessaging
{
    public class FCMNotification
    {
        private readonly IConfiguration _config;
        private static readonly HttpClient client = new HttpClient();
        public FCMNotification(IConfiguration configuration)
        {
            _config = configuration;
        }

        public async Task<string> SendNotification(string DeviceToken, string type, string title, string msg, Guid? extend_id)
        {
            try
            {
                client.DefaultRequestHeaders.Remove("Authorization");

                client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "key=" + FCMConsts.FCM_SERVER_KEY);

                var request_send_body = new
                {
                    to = DeviceToken,
                    notification = new
                    {
                        title = title,
                        body = msg,
                        icon = ""
                    },
                    data = new
                    {
                        action_type = type,
                        record_id = extend_id != null ? extend_id.ToString() : "",
                        redirect_to = ""
                    }
                };

                var responseOcdFront = await client.PostAsync(FCMConsts.FCM_ADDRESS_SENDER, new StringContent(JsonConvert.SerializeObject(request_send_body), Encoding.UTF8, "application/json"));
                responseOcdFront.EnsureSuccessStatusCode();
            } catch (Exception ex)
            {
                return ex.Message;
            }

            return "OK";
        }


        public async Task<string> SendNotifications(List<string> DeviceTokens, string type, string title, string msg, double? extend_id)
        {
            try
            {
                // Add to Group
                client.DefaultRequestHeaders.Remove("Authorization");
                client.DefaultRequestHeaders.Remove("project_id");

                client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "key=" + FCMConsts.FCM_SERVER_KEY);
                client.DefaultRequestHeaders.TryAddWithoutValidation("project_id", FCMConsts.FCM_SENDER_ID);

                string group_name = "Customer" + getRandomStringNoti(8);
                var request_send_create = new
                {
                    operation = "create",
                    notification_key_name = group_name,
                    registration_ids = DeviceTokens
                };

                var responseAdd = await client.PostAsync(FCMConsts.FCM_CREATE_GROUP, new StringContent(JsonConvert.SerializeObject(request_send_create), Encoding.UTF8, "application/json"));
                responseAdd.EnsureSuccessStatusCode();


                var content = await responseAdd.Content.ReadAsStringAsync();
                JObject dataResponse = (JObject)JsonConvert.DeserializeObject(content);

                try
                {
                    AddNotiGroupResponse dataResponse2 = dataResponse.ToObject<AddNotiGroupResponse>();
                    if (dataResponse2 != null && dataResponse2.notification_key != null && dataResponse2.notification_key.Length > 0)
                    {
                        client.DefaultRequestHeaders.Remove("Authorization");
                        client.DefaultRequestHeaders.Remove("project_id");

                        client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
                        client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "key=" + FCMConsts.FCM_SERVER_KEY);


                        var request_send_body = new
                        {
                            to = dataResponse2.notification_key,
                            notification = new
                            {
                                title = title,
                                body = msg,
                                icon = ""
                            },
                            data = new
                            {
                                action_type = type,
                                record_id = extend_id != null ? extend_id.ToString() : "",
                                redirect_to = ""
                            }
                        };

                        var response = await client.PostAsync(FCMConsts.FCM_ADDRESS_SENDER, new StringContent(JsonConvert.SerializeObject(request_send_body), Encoding.UTF8, "application/json"));
                        response.EnsureSuccessStatusCode();
                    }
                } catch (Exception ex3)
                {
                    return ex3.Message;
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return "OK";
        }

        private static string getRandomStringNoti(int length)
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

        public class AddNotiGroupResponse
        {
            public string notification_key { get; set; }
        }
    }
}
