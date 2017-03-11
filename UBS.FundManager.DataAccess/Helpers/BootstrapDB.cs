using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UBS.FundManager.Common;
using UBS.FundManager.Common.Helpers;

namespace UBS.FundManager.DataAccess.Helpers
{
    public class BootstrapDB
    {
        public BootstrapDB(ILogging logger)
        {
            _logger = logger;
            _docClient = new DocumentClient(svcEndpoint, authKey);
        }

        /// <summary>
        /// Returns an instance of the doc client
        /// </summary>
        public DocumentClient Client
        {
            get
            {
                if (_docClient == null)
                {
                    _docClient = new DocumentClient(svcEndpoint, authKey);
                }

                return _docClient;
            }
        }

        public void SetupRepository()
        {
            Initialize();
        }

        /// <summary>
        /// Generates a uri based on the specified entity
        /// </summary>
        /// <param name="uriType">Entity to generate uri for</param>
        /// <param name="objectId">Entity's additional metadata (i.e stored proc id)</param>
        /// <returns></returns>
        public Uri GetUri(UriType uriType, string objectId = null)
        {
            Uri uri = default(Uri);

            switch (uriType)
            {
                case UriType.Database:
                    uri = UriFactory.CreateDatabaseUri(_databaseId);
                    break;

                case UriType.Collection:
                    uri = UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId);
                    break;

                case UriType.Document:
                    uri = UriFactory.CreateDocumentUri(_databaseId, _collectionId, objectId);
                    break;

                case UriType.SProcs:
                    uri = UriFactory.CreateStoredProcedureUri(_databaseId, _collectionId, objectId);
                    break;

                case UriType.UDF:
                    uri = UriFactory.CreateUserDefinedFunctionUri(_databaseId, _collectionId, objectId);
                    break;

                default:
                    throw new NotSupportedException("Uri type is not supported");
            }

            return uri;
        }

        /// <summary>
        /// Creates database if not already in existence.
        /// </summary>
        /// <returns></returns>
        private async Task CreateDatabaseIfNotExistsAsync()
        {
            try
            {
                await _docClient.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(_databaseId));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await _docClient.CreateDatabaseAsync(new Database { Id = _databaseId });
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Creates a collection if not already existing
        /// </summary>
        /// <returns>Collection metadata</returns>
        private async Task<DocumentCollection> CreateCollectionIfNotExistsAsync()
        {
            DocumentCollection collection = null;

            try
            {
                collection = await _docClient.ReadDocumentCollectionAsync(
                                                UriFactory.CreateDocumentCollectionUri(
                                                                _databaseId, _collectionId));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    collection = await _docClient.CreateDocumentCollectionAsync(
                                                    UriFactory.CreateDatabaseUri(_databaseId),
                                                    new DocumentCollection { Id = _collectionId },
                                                    new RequestOptions { OfferThroughput = 1000 });
                }
                else
                {
                    throw;
                }
            }

            return collection;
        }

        /// <summary>
        /// Creates stored procedure
        /// </summary>
        /// <param name="colSelfLink">Collection Uri</param>
        /// <returns></returns>
        private async Task CreateProcedure(string colSelfLink)
        {
            try
            {
                string[] sp_files = Directory.GetFiles(
                                                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, 
                                                            Constants.SP_BASE_DIRECTORY), 
                                                            Constants.SP_FILE_SEARCH_PATTERN);
                foreach (string file in sp_files)
                {
                    _logger.Log(LogLevel.Info, $"file: {file}");
                    StoredProcedure sproc = new StoredProcedure
                    {
                        Id = Path.GetFileNameWithoutExtension(file),
                        Body = File.ReadAllText(file)
                    };

                    await TryDeleteStoredProcedure(colSelfLink, sproc.Id);
                    sproc = await _docClient.CreateStoredProcedureAsync(colSelfLink, sproc);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Tries to delete a stored procedure, if it exists
        /// </summary>
        /// <param name="colSelfLink">Collection uri</param>
        /// <param name="sprocId">SPRoc Id</param>
        /// <returns></returns>
        private async Task TryDeleteStoredProcedure(string colSelfLink, string sprocId)
        {
            try
            {
                StoredProcedure sproc = _docClient.CreateStoredProcedureQuery(colSelfLink)
                                  .Where(s => s.Id == sprocId)
                                  .AsEnumerable().FirstOrDefault();
                if (sproc != null)
                {
                    await _docClient.DeleteStoredProcedureAsync(sproc.SelfLink);
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Creates a user function 
        /// </summary>
        /// <param name="colSelfLink">Collection Uri</param>
        /// <returns></returns>
        private async Task CreateUserFunction(string colSelfLink)
        {
            try
            {
                string[] udf_files = Directory.GetFiles(
                                                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, 
                                                            Constants.UDF_BASE_DIRECTORY), 
                                                            Constants.UDF_FILE_SEARCH_PATTERN);

                foreach (string file in udf_files)
                {
                    _logger.Log(LogLevel.Info, $"udf file: {file}");
                    UserDefinedFunction udf = new UserDefinedFunction
                    {
                        Id = Path.GetFileNameWithoutExtension(file),
                        Body = File.ReadAllText(file)
                    };

                    await TryDeleteUDF(colSelfLink, udf.Id);
                    udf = await _docClient.CreateUserDefinedFunctionAsync(colSelfLink, udf);
                }
            }
            catch
            {
                throw;
            }

        }

        /// <summary>
        /// Tries to delete a user defined function, if it already exists.
        /// </summary>
        /// <param name="colSelfLink">Collection Uri</param>
        /// <param name="udfId">Id of user function</param>
        /// <returns></returns>
        private async Task TryDeleteUDF(string colSelfLink, string udfId)
        {
            UserDefinedFunction udf = _docClient.CreateUserDefinedFunctionQuery(colSelfLink)
                                             .Where(u => u.Id == udfId)
                                             .AsEnumerable()
                                             .FirstOrDefault();
            if (udf != null)
            {
                await _docClient.DeleteUserDefinedFunctionAsync(udf.SelfLink);
            }
        }

        private void Initialize()
        {
            CreateDatabaseIfNotExistsAsync().Wait();
            DocumentCollection documentCollection = CreateCollectionIfNotExistsAsync().Result;

            if (documentCollection != null)
            {
                CreateProcedure(documentCollection.SelfLink).Wait();
                CreateUserFunction(documentCollection.SelfLink).Wait();
            }
        }

        #region Fields
        private readonly string _databaseId = ConfigManager.GetSetting<string>(Constants.DATABASE_ID);
        private readonly string _collectionId = ConfigManager.GetSetting<string>(Constants.COLLECTION_ID);
        private readonly Uri svcEndpoint = new Uri(ConfigManager.GetSetting<string>(Constants.ENDPOINT));
        private readonly string authKey = ConfigManager.GetSetting<string>(Constants.AUTH_KEY);

        private ILogging _logger;
        private static DocumentClient _docClient;
        #endregion
    }

    /// <summary>
    /// Enum describing the type of URI to compose
    /// </summary>
    public enum UriType { Database, Collection, SProcs, UDF, Document }
}
