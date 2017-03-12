function getById(fundId) {
    var collection = getContext().getCollection();

    var isAccepted = collection.queryDocuments(
        collection.getSelfLink(),
        'SELECT udf.udf_generate_name(r.name, r.stockInfo.type), udf.udf_calc_stock_value(r.stockInfo) FROM root r where r.id= "' + fundId + '"',
        function (err, feed, options) {
            if (err) throw err;

            if (!feed || !feed.length) {
                getContext().getResponse().setBody('no docs found');
            }
            else {

                var isAcceptedCount = collection.queryDocuments(
                    collection.getSelfLink(),
                    'SELECT VALUE COUNT(r.id) FROM root r where r.stockInfo.type= "' + feed[0].$2.type + '"',
                    function (error, count, responseOption) {
                        if (error) throw error;

                        var stock = {
                            name: feed[0].$1 + count,
                            stockInfo: feed[0].$2
                        }
                        getContext().getResponse().setBody(JSON.stringify(stock));
                    });

                if (!isAcceptedCount) throw "Unable to count number of stocks.";
            }
        });

    if (!isAccepted) throw new Error('The query was not accepted by the server.');
}