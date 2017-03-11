using Microsoft.Azure.Documents;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UBS.FundManager.Common.Helpers;
using UBS.FundManager.DataAccess.Helpers;
using UBS.FundManager.Messaging.Models.Fund;

namespace UBS.FundManager.DataAccess
{
    public interface IFundRepository : IDataRepository<Fund> { }

    /// <summary>
    /// Provides an implementation for interacting with the cloud db (Azure DocumentDB).
    /// Could be any DB, this can easily be replaced with an implementation for another DB
    /// without any code change anywhere else.
    /// </summary>
    public class FundRepository : IFundRepository
    {
        private BootstrapDB _dbUtil;
        private string _continuationToken;

        public FundRepository(BootstrapDB dbUtil)
        {
            _dbUtil = dbUtil;
        }

        /// <summary>
        /// Save item to collection
        /// </summary>
        /// <param name="item">Item to save</param>
        /// <returns>Saved item</returns>
        public async Task<Fund> CreateItemAsync(Fund item)
        {
            Uri collectionUri = _dbUtil.GetUri(UriType.Collection);
            Document document = await _dbUtil.Client.CreateDocumentAsync(collectionUri, item);

            return await GetItemAsync(document.Id);
        }

        /// <summary>
        /// Deletes item from collection in db
        /// </summary>
        /// <param name="id">Id of item to delete</param>
        /// <returns></returns>
        public async Task DeleteItemAsync(string id)
        {
            Uri docUri = _dbUtil.GetUri(UriType.Document, id);
            await _dbUtil.Client.DeleteDocumentAsync(docUri);
        }

        /// <summary>
        /// Gets an item from a collection in the db
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Fund> GetItemAsync(string id)
        {
            Uri sprocUri = _dbUtil.GetUri(UriType.SProcs, "sp_load_fund_by_id");
            string response = await _dbUtil.Client.ExecuteStoredProcedureAsync<string>(sprocUri, id);

            return response.Deserialise<Fund>();
        }

        /// <summary>
        /// Gets all items in a collection (executes SPROC on the database server).
        /// </summary>
        /// <param name="dataSize"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Fund>> GetAll(int dataSize)
        {
            Uri sprocUri = _dbUtil.GetUri(UriType.SProcs, "sp_load_all_funds");
            string response = await _dbUtil.Client.ExecuteStoredProcedureAsync<string>(sprocUri, dataSize);

            return response.Deserialise<IEnumerable<Fund>>();
        }

        /// <summary>
        /// Updates item in collection
        /// </summary>
        /// <param name="id"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<Fund> UpdateDocumentAsync(string id, Fund item)
        {
            Uri docUri = _dbUtil.GetUri(UriType.Document, id);
            return (Fund)(dynamic)await _dbUtil.Client.ReplaceDocumentAsync(docUri, item);
        }
    }
}
