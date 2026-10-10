using System.ComponentModel;
namespace Utility
{
    public static class EnumHelper
    {
        public enum Status
        {
            Approved,
            Unapproved
        }
        public enum OtpStatus
        {
            New,
            Used,
            Resend,
            Expired
        }
        public enum MobileBillingType
        {
            Prepaid,
            Postpaid
        }
        public enum MobileOperator
        {
            GrameenPhone,
            Teletalk,
            Banglalink,
            Robi,
            Airtel,
            Skitto
        }
        public enum ErrorCode
        {
            [Description("Success")]
            Success = 200,
            [Description("Unauthorized")]
            Unauthorized = 401,
            [Description("Unprocessable Entity")]
            ExpectedResultNotFound = 422,
            [Description("Internal Server Error")]
            InternalServerError = 500
        }
        public enum TransactionType_NPSB
        {
            [Description("A2A")]
            AAN,
            [Description("A2C")]
            APN,
            [Description("C2C")]
            PPN,
            [Description("C2A")]
            PAN
        }
        public enum TrxStatus
        {
            Initiated = 0,
            Success = 1,
            Failed = 2,
            Reversed = 3,
            Authorized = 4,
        }
        public enum HttpRequestType
        {
            GET = 1,
            POST = 2,
            POSTHttpFormURLEncoded = 3,
            POSTXML = 4
        }
        public enum Currency
        {
            BDT,
            USD,
            EUR,
            GBP,
            INR,
            JPY,
            AUD,
            CAD,
            CNY,
            CHF
        }
        public static class CurrencyCode
        {
            public static readonly Dictionary<Currency, string> NumericCodes = new Dictionary<Currency, string>
            {
                { Currency.BDT, "050" },
                { Currency.USD, "840" },
                { Currency.EUR, "978" },
                { Currency.GBP, "826" },
                { Currency.INR, "356" },
                { Currency.JPY, "392" },
                { Currency.AUD, "036" },
                { Currency.CAD, "124" },
                { Currency.CNY, "156" },
                { Currency.CHF, "756" },
            };
        }
        public enum ImageFormat
        {
            jpeg,
            png,
            PDF,
            unknown,
            gif
        }
        public enum Roles
        {
            USER,
            ADMIN,
            SUPER_ADMIN,
            CONSUMER,
            AGENT,
            MERCHANT
        }
        public enum TrxType
        {
            SendMoney
        }
        public enum TrxParticipant
        {
            S,
            R
        }
    }
}
