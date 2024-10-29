using Microsoft.Extensions.Configuration;

namespace LOYALTY.Extensions
{
    public class Consts
    {
        public readonly IConfiguration _configuration;

        public Consts(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public const int PAGE_SIZE = 20;
        public static string TOKEN_EXPIRE_DAY = Startup.StaticConfig["Time_token"];
        public const string USER_TYPE_WEB_ADMIN = "web_admin";
        public const string USER_TYPE_WEB_PARTNER = "web_partner";
        public const string USER_TYPE_CUSTOMER = "customer";

        //public const string DB_CONNECT = "Data Source=tcp:192.168.68.43,1433;Initial Catalog=LOYALTY;User ID=loyalty;Password=Loyalty@123;";
        public const string DB_CONNECT = "Data Source=tcp:192.168.68.75,1433;Initial Catalog=LOYALTY-DEV;User ID=loyalty;Password=Loyalty@123;";
        //public const string DB_CONNECT = "Data Source=tcp:210.86.231.73,7001;Initial Catalog=LOYALTY;User ID=loyalty;Password=Loyalty@123;";
        //public const string DB_CONNECT = "Data Source=tcp:210.86.231.73,7075;Initial Catalog=LOYALTY-DEV;User ID=loyalty;Password=Loyalty@123;";

        // ESMS KEY
        public const string ESMS_API_KEY = "E3DD9959C65E2B42459F5B2D2BAE52";
        public const string ESMS_SECRET_KEY = "070967A4A3CE3D2D4717E9EEC80C9B";
        public const string ESMS_BRAND_NAME = "CashPlus";

        // VNPT EKYC LOYALTY
        //public const string VNPT_ACCESS_TOKEN = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI3NzU0MjcwMS05MDgwLTExZWMtYjEzMy00OTYwYmMxNzI1N2YiLCJhdWQiOlsicmVzdHNlcnZpY2UiXSwidXNlcl9uYW1lIjoibXlib25kdmlldG5hbUBnbWFpbC5jb20iLCJzY29wZSI6WyJyZWFkIl0sImlzcyI6Imh0dHBzOi8vbG9jYWxob3N0IiwibmFtZSI6Im15Ym9uZHZpZXRuYW1AZ21haWwuY29tIiwidXVpZF9hY2NvdW50IjoiNzc1NDI3MDEtOTA4MC0xMWVjLWIxMzMtNDk2MGJjMTcyNTdmIiwiYXV0aG9yaXRpZXMiOlsiVVNFUiJdLCJqdGkiOiJiYTE4ZTI5ZC1jZjFhLTQxZmUtYTE5Zi0wMmU0MTI0NDhjNmIiLCJjbGllbnRfaWQiOiJhZG1pbmFwcCJ9.vU_zZez7116Yk4X7oL5_WCaNTS2GQC8YglYPuYFc7OxlfVYk9LWLktWbIUUETED3eGUAEiEcJopa0Q7bAr9JP4dyK-Wm-qc7jvgny-5n57thAmd1JOJ9FxRt-oHoSYVc6nfVR4ElmqOi_lXN9qDWwNjrPSOssXNFuky3F7WTDRpz8X-tDwGYb-hz5zIUro-OD7t51p3AmflGdd54_3pP4mkBfq4Gtt0UQ5wstIaDqZ22eKL9Ht0z3qk3PP2NSEOEmfZhUjVj5VGJr0-ljThg5yFWssDmJsn5ZfSr_cBhcSOXzDpoEGqQQtApmMNeVDndtFJmTKaauqDjWfUYKz8ooA";
        //public const string VNPT_TOKEN_KEY = "MFwwDQYJKoZIhvcNAQEBBQADSwAwSAJBALeANAtppXUUT2o0o1+WjRGvH9nxNKtT5IBwN99AMFXCEouOCPZUkouZg1cGKb+VVsN84jq+RFP3jQmyigcrsksCAwEAAQ==";
        //public const string VNPT_TOKEN_ID = "d845a604-ec83-117e-e053-62199f0aa542";

        // LINK CONST
        public const string LINK_SHARE = "https://cashplus.vn/dang-ky-thong-tin-nguoi-gioi-thieu?sharecode=";

        // MAIL ADMIN
        public const string ADMIN_MAIL = "admin@cashplus.vn";
        public const string WEB_CSPL = "https://cashplus.vn/chinh-sach-phap-ly";
        public const string PORTAL_URL = "https://cashplus.vn";

        // ID USER ADMIN
        public const string USER_ADMIN_ID = "4864E494-21EA-4394-419C-08DB837FD034";

        // CASHPLUS
        public const string CP_BK_PARTNER_CODE = "ZmHdQmFQwiVydaMH1eCGow=="; //ATS1

        public const string private_key = "MIICWgIBAAKBgFoXsAEEvLG6mQSY4GzsBit6/pGeIDnmK/1eMZglpbImtwiW4KlS\r\nnoybya+eSH03CDNkAQIghTcctP9qbUg2bRFLNcNwDbSBSRUfKfm24HdrFLJ46ziZ\r\n94eruqswGqRQ8JbM8cY43h5c7eEmZB4vYw0pyO9i+jmSv+lIr2NZcrEfAgMBAAEC\r\ngYAYyXGcJiCASZV2BVWhwiJEbjeB+t5k76XktLiyYpE+/ZXYICK5k0iZ6PbJgaPy\r\nB2UTNo1sd2QPcK9/ollkx8yj5KJLR9yXnyjxohbSyLQXkNzxVzQIQRyu53Oh+WwQ\r\nkuC8kyDOKxeAGHzONXtR2WT13JiMtjKxOuGmt9Rt8SOYkQJBAKNdj7GY6EcOdikC\r\nF23Kef81H9CoUwqEyZ0eoEwdt2pMGyAsgGGng5TZqfXWiRQ33IG7FFMSdXoqrW3c\r\nWHO/8mkCQQCNLbAoJXCHDy2/gIlfH2KRIqBFVxRXBvPkoiCZIwSgUQlQUtIESB87\r\nlEimGmQoAZZDEuCaU+7H0+Tt4C3CSAZHAkB6TiyrMKgNsqUB9J/nwaPuTi6Af9ST\r\n1nA+4lPuSH0t5saUIt0Gv2wCf6b/91rvORcsRQxlWTd8fAEVc9cA6Z6pAkBzehmK\r\n3QTsFEhRSewTeHKBUJdT4GRswu0f6FVNrU0NbPt3TicnBW82ppW99/xQlOu5tWku\r\nEtVPckzhHeuP7KXlAkBB5NPqQqaxzetKz42mbh8sXWGXhsdJV2kfrRowelnsDm5M\r\nA5t+VK3oayGvursKRD8R3TkpJfJPBd12Mn5ocRUq";

        public const string Error_Permissions = "Bạn không có quyền truy cập chức năng này";
    }
}
