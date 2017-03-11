function getAll() {
    var isAccepted = __.queryDocuments(
                        __.getSelfLink(),
                        'SELECT udf.udf_generate_name(r.name, r.stockInfo.type), udf.udf_calc_stock_value(r.stockInfo) FROM root r',
                        function (err, feed, options) {
                            if (err) throw err;

                            if (!feed || !feed.length) {
                                getContext().getResponse().setBody('no docs found');
                            }
                            else {
                                //Execute MapR functions on the resultset for equity stocks
                                var equityStocks = feed.filter(equityStockFilter);
                                equityStocks = equityStocks.map(function (equityStock, index, equityStocks) {
                                    var stock = {
                                        name: equityStock.$1.concat(index + 1),
                                        stockInfo: equityStock.$2
                                    };

                                    return stock;
                                });

                                //Execute MapR functions on the resultset for bond stocks
                                var bondStocks = feed.filter(bondStockFilter);
                                bondStocks = bondStocks.map(function (bondStock, index, bondStocks) {
                                    var bstock = {
                                        name: bondStock.$1.concat(index + 1),
                                        stockInfo: bondStock.$2
                                    };

                                    return bstock;
                                });

                                //Merge processed resultsets and return to client
                                var allStocks = equityStocks.concat(bondStocks);
                                getContext().getResponse().setBody(JSON.stringify(allStocks));
                            }
                        });

    if (!isAccepted) throw new Error('The query was not accepted by the server.');
}

function equityStockFilter(value) {
    return value.$2.type == 'Equity';
}

function bondStockFilter(value) {
    return value.$2.type == 'Bond';
}
