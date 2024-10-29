using Microsoft.Extensions.Configuration;


namespace LOYALTY.PaymentGate
{

    public class BKConsts
    {

        public readonly IConfiguration _configuration;

        public BKConsts(IConfiguration configuration)
        {
            _configuration = configuration;

            //URL_REGISTER_VA = Startup.StaticConfig["BKContstVA:URL_REGISTER_VA"];
            //PARTNER_CODE = Startup.StaticConfig["BKContstVA:PARTNER_CODE"];
            //VA_EPAY_PUBLIC_KEY = Startup.StaticConfig["BKContstVA:VA_EPAY_PUBLIC_KEY"];


            //API_URL = Startup.StaticConfig["FIRMBANK/API_URL"];
            //FB_PRIVATE_KEY = Startup.StaticConfig["FIRMBANK:FB_PRIVATE_KEY"];
            //FB_EPAY_PUBLIC_KEY_PATH = Startup.StaticConfig["FIRMBANK:FB_EPAY_PUBLIC_KEY_PATH"];

            //PG_BASE_URL_LIST_PAYMENT = Startup.StaticConfig["PaymentGate:PG_BASE_URL_LIST_PAYMENT"];
            //PG_BASE_URL_SEND = Startup.StaticConfig["PaymentGate:PG_BASE_URL_SEND"];
            //PG_BASE_URL_IPN = Startup.StaticConfig["PaymentGate:PG_BASE_URL_IPN"];
            //PG_BASE_URL_RETURN = Startup.StaticConfig["PaymentGate:PG_BASE_URL_RETURN"];
            //PG_KEY = Startup.StaticConfig["PaymentGate:PG_KEY"];
            //PG_SECRET = Startup.StaticConfig["PaymentGate:PG_SECRET"];
            //PG_EMAIL = Startup.StaticConfig["PaymentGate:PG_EMAIL"];
            //PG_PASSWORD = Startup.StaticConfig["PaymentGate:PG_PASSWORD"];
            //PG_MERCHANT_ID_1 = Startup.StaticConfig["PaymentGate:PG_MERCHANT_ID_1"];
            //PG_MERCHANT_EMAIL_1 = Startup.StaticConfig["PaymentGate:PG_MERCHANT_EMAIL_1"];
            //PG_MERCHANT_PASSWORD_1 = Startup.StaticConfig["PaymentGate:PG_MERCHANT_PASSWORD_1"];
        }

        //public static string a = _configuration["ConnectionStrings:CoreDB"];

        // DỊCH VỤ VA
        //public const string URL_REGISTER_VA = "https://devtest.baokim.vn/Sandbox/Collection/V2";
        //public const string PARTNER_CODE = "CASHPLUS";
        //public const string VA_EPAY_PUBLIC_KEY = "MIIBITANBgkqhkiG9w0BAQEFAAOCAQ4AMIIBCQKCAQBhiNqAdehyGvr8+MBln+k1a2B9pVvtCVXONDr6HPE4hHIw/2nUGo7cSQr5jHvIFZl1YZjpGpJc4eONQtwy5GSkgeoxY21fq3lh/6F7aS15wub1+sEsj0bqltUYG9n3afUT67V0UvA6du0dmlBwE8pFT3/u6aEeIIFtsm/lKPiCKqVO0FyhiRi690es+5LvF5AnbUZFoaPETq2xA63iQ9XTZW5P3EZmMo6+LQRXIf3jgDOgLzX5NHDH9q/eUNAxrB/d8N/P37eSX5CztgBGhdk4BoT02Fr4mCgcP1nHG6Z9pRrIhuVxHdO0awW5yQFnRUI12kT2f4QhlVp5lpgfYJ+FAgMBAAE=";

        // DỊCH VỤ VA
        public static string URL_REGISTER_VA = Startup.StaticConfig["BKContstVA:URL_REGISTER_VA"];
        public static string PARTNER_CODE = Startup.StaticConfig["BKContstVA:PARTNER_CODE"];
        public static string VA_EPAY_PUBLIC_KEY = Startup.StaticConfig["BKContstVA:VA_EPAY_PUBLIC_KEY"];

        //// DỊCH VỤ FIRMBANK LIVE
        //public const string API_URL = "https://bws.baokim.vn/ibft/live";
        //public const string FB_PRIVATE_KEY = "MIICWgIBAAKBgFoXsAEEvLG6mQSY4GzsBit6/pGeIDnmK/1eMZglpbImtwiW4KlSnoybya+eSH03CDNkAQIghTcctP9qbUg2bRFLNcNwDbSBSRUfKfm24HdrFLJ46ziZ94eruqswGqRQ8JbM8cY43h5c7eEmZB4vYw0pyO9i+jmSv+lIr2NZcrEfAgMBAAECgYAYyXGcJiCASZV2BVWhwiJEbjeB+t5k76XktLiyYpE+/ZXYICK5k0iZ6PbJgaPyB2UTNo1sd2QPcK9/ollkx8yj5KJLR9yXnyjxohbSyLQXkNzxVzQIQRyu53Oh+WwQkuC8kyDOKxeAGHzONXtR2WT13JiMtjKxOuGmt9Rt8SOYkQJBAKNdj7GY6EcOdikCF23Kef81H9CoUwqEyZ0eoEwdt2pMGyAsgGGng5TZqfXWiRQ33IG7FFMSdXoqrW3cWHO/8mkCQQCNLbAoJXCHDy2/gIlfH2KRIqBFVxRXBvPkoiCZIwSgUQlQUtIESB87lEimGmQoAZZDEuCaU+7H0+Tt4C3CSAZHAkB6TiyrMKgNsqUB9J/nwaPuTi6Af9ST1nA+4lPuSH0t5saUIt0Gv2wCf6b/91rvORcsRQxlWTd8fAEVc9cA6Z6pAkBzehmK3QTsFEhRSewTeHKBUJdT4GRswu0f6FVNrU0NbPt3TicnBW82ppW99/xQlOu5tWkuEtVPckzhHeuP7KXlAkBB5NPqQqaxzetKz42mbh8sXWGXhsdJV2kfrRowelnsDm5MA5t+VK3oayGvursKRD8R3TkpJfJPBd12Mn5ocRUq";
        //public const string FB_EPAY_PUBLIC_KEY_PATH = "./KEY_RSA/FB/public.pem";

        //DỊCH VỤ FIRMBANK TEST
        //public const string API_URL = "https://devtest.baokim.vn/Sandbox/FirmBanking";
        //public const string FB_PRIVATE_KEY = "MIGeMA0GCSqGSIb3DQEBAQUAA4GMADCBiAKBgGc0IUF783iOqKsGp6ZoySvhG5EQAUXH7XsKSz4TtenSOYngCotSzYaVACSILURNHBfMoU+WzCGSX4Dl7gYfPExYEoti+7qIKqp209pLohbxh3HvUYxztNCrzEDHsnAQQsaLsHlvSvt+yDhEHJiHzCWsiufrHq8LlxgInmPDrT+JAgMBAAE=";
        //public const string FB_EPAY_PUBLIC_KEY_PATH = "./KEY_RSA/FB/public.pem";

        //DỊCH VỤ FIRMBANK TEST
        public static string API_URL = Startup.StaticConfig["FIRMBANK:API_URL"];
        public static string FB_PRIVATE_KEY = Startup.StaticConfig["FIRMBANK:FB_PRIVATE_KEY"];
        public static string FB_EPAY_PUBLIC_KEY_PATH = Startup.StaticConfig["FIRMBANK:FB_EPAY_PUBLIC_KEY_PATH"];


        //// DỊCH VỤ CỔNG THANH TOÁN LIVE
        //public const string PG_BASE_URL_LIST_PAYMENT = "https://api.baokim.vn/payment/api/v5/bpm/list";
        //public const string PG_BASE_URL_SEND = "https://api.baokim.vn/payment/api/v5/order/send";
        //public const string PG_BASE_URL_IPN = "https://apigw.cashplus.vn/api/bktrans/pgcallback";
        //public const string PG_BASE_URL_RETURN = "https://socket.cashplus.vn/baokim/return";
        //public const string PG_KEY = "qSzfYJLx5rzr1RbiqX5vszjfdSQY94jr";
        //public const string PG_SECRET = "OacvcKLl8G9FL12GIrQuGHnDtm46zGob";
        //public const string PG_EMAIL = "sonpa@cashplus.vn";
        //public const string PG_PASSWORD = "Baokim@2023";
        //public const string PG_MERCHANT_ID_1 = "TKvwJQjw2WwoTZj8YXO/xw==";  //36274
        //public const string PG_MERCHANT_EMAIL_1 = "sonpa@cashplus.vn";
        //public const string PG_MERCHANT_PASSWORD_1 = "Baokim@2023";

        // DỊCH VỤ CỔNG THANH TOÁN TEST
        //public const string PG_BASE_URL_LIST_PAYMENT = "https://dev-api.baokim.vn/payment/api/v5/bpm/list";
        //public const string PG_BASE_URL_SEND = "https://dev-api.baokim.vn/payment/api/v5/order/send";
        //public const string PG_BASE_URL_IPN = "https://apigw.cashplus.vn/api/bktrans/pgcallback";
        //public const string PG_BASE_URL_RETURN = "https://socket.cashplus.vn/baokim/return";
        //public const string PG_KEY = "jvdkHHe2YaJ0r5Shoo3r8Y11FrCBWVmp";
        //public const string PG_SECRET = "vDTy0log29cGnLcDJqKeup44Bzby9w5H";
        //public const string PG_EMAIL = "info@cashplus.vn";
        //public const string PG_PASSWORD = "12345678";
        //public const string PG_MERCHANT_ID_1 = "OB0ZBPNUlflL/+Obfb7MVA==";//401064;
        //public const string PG_MERCHANT_EMAIL_1 = "info1@cashplus.vn";
        //public readonly string PG_MERCHANT_PASSWORD_1;

        // DỊCH VỤ CỔNG THANH TOÁN TEST
        public static string PG_BASE_URL_LIST_PAYMENT = Startup.StaticConfig["PaymentGate:PG_BASE_URL_LIST_PAYMENT"];
        public static string PG_BASE_URL_SEND = Startup.StaticConfig["PaymentGate:PG_BASE_URL_SEND"];
        public static string PG_BASE_URL_IPN = Startup.StaticConfig["PaymentGate:PG_BASE_URL_IPN"];
        public static string PG_BASE_URL_RETURN = Startup.StaticConfig["PaymentGate:PG_BASE_URL_RETURN"];
        public static string PG_KEY = Startup.StaticConfig["PaymentGate:PG_KEY"];
        public static string PG_SECRET = Startup.StaticConfig["PaymentGate:PG_SECRET"];
        public static string PG_EMAIL = Startup.StaticConfig["PaymentGate:PG_EMAIL"];
        public static string PG_PASSWORD = Startup.StaticConfig["PaymentGate:PG_PASSWORD"];
        public static string PG_MERCHANT_ID_1 = Startup.StaticConfig["PaymentGate:PG_MERCHANT_ID_1"];
        public static string PG_MERCHANT_EMAIL_1 = Startup.StaticConfig["PaymentGate:PG_MERCHANT_EMAIL_1"];
        public static  string PG_MERCHANT_PASSWORD_1 = Startup.StaticConfig["PaymentGate:PG_MERCHANT_PASSWORD_1"];

        //Baokim Email

        public static string EMAIL_BK = Startup.StaticConfig["Email_BK"];
    }
}
