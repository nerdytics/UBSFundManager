namespace UBS.FundManager.Common
{
    public static class Constants
    {
        public const string DEFAULT_EXCHANGE = "ubs.fund.manager";
        /// <summary>
        /// Add Fund Queue Binding Info
        /// </summary>
        public const string ADDFUND_REQUEST_QUEUE = "fundmanager.addfund.request";
        public const string ADDFUND_REQUEST_QUEUE_BINDING = "fund.manager.addfund.request";
        public const string ADDFUND_RESPONSE_QUEUE = "fundmanager.addfund.response";
        public const string ADDFUND_RESPONSE_QUEUE_BINDING = "fund.manager.addfund.response";

        /// <summary>
        /// Download Fundslist Queue Binding Info
        /// </summary>      
        public const string DOWNLOADFUND_REQUEST_QUEUE = "fundmanager.downloadfunds.request";
        public const string DOWNLOADFUND_REQUEST_QUEUE_BINDING = "fund.manager.downloadfunds.request";
        public const string DOWNLOAD_FUNDS_RESPONSE_QUEUE = "fundmanager.downloadfunds.response";
        public const string DOWNLOAD_FUNDS_RESPONSE_QUEUE_BINDING = "fund.manager.downloadfunds.response";

        /// <summary>
        /// Dead Letter Exchange Info
        /// </summary>
        public const string DEAD_LETTER_EXCHANGE = "ubs.fund.manager.dlx";
        public const string DEAD_LETTER_QUEUE = "ubs.fund.manager.dlq";
        public const string DEAD_LETTER_KEY = "x-dead-letter-exchange";

        /// <summary>
        /// Azure config file connection string keys
        /// </summary>
        public const string ENDPOINT = "endpoint";
        public const string AUTH_KEY = "authKey";

        /// <summary>
        /// Azure DocumentDB entities config file keys
        /// </summary>
        public const string DATABASE_ID = "databaseId";
        public const string COLLECTION_ID = "collectionId";

        /// <summary>
        /// JSON media type
        /// </summary>
        public const string JSON_MEDIA_TYPE = "application/json";

        /// <summary>
        /// Metadata for location of stored procedure and user defined function scripts
        /// </summary>
        public const string SP_BASE_DIRECTORY = @"DbFunctions\SProcs";
        public const string SP_FILE_SEARCH_PATTERN = "sp_*.js";
        public const string UDF_BASE_DIRECTORY = @"DbFunctions\UDF";
        public const string UDF_FILE_SEARCH_PATTERN = "udf_*.js";
    }
}
