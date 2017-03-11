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
                var stock = {
                    name: feed[0].$1,
                    stockInfo: feed[0].$2
                }
                getContext().getResponse().setBody(JSON.stringify(stock));
            }
        });

    if (!isAccepted) throw new Error('The query was not accepted by the server.');
}
