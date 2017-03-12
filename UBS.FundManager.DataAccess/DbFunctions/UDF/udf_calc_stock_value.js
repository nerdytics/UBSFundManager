/**
* This is run as user defined function and does the following:
*   calculates the value of a stock by evaluating the market value
*   and transaction cost.
*
* @param {Stock} stockInfo - details of the stock (i.e purchaseInfo or valueInfo).
*/
function calcStockValue(stockInfo) {
    var purchaseInfo = stockInfo.purchaseInfo;
    var valueInfo = stockInfo.valueInfo;

    stockInfo.valueInfo.marketValue = calcMarketValue(purchaseInfo);
    stockInfo.valueInfo.transactionCost = calcTranCost(stockInfo.type, valueInfo.marketValue);

    return stockInfo;
}

/**
* This is run as user defined function and does the following:
*   calculates the market value of a stock by multiplying the unit
*   price and quantity of stock purchased.
*
* @param {PurchaseInfo} purchaseInfo - details of the stock purchase (i.e price per unit, quantity).
*/
function calcMarketValue(purchaseInfo) {
    return (purchaseInfo.unitPrice * purchaseInfo.purchasedQ).toFixed(3);
}

/**
* This is run as user defined function and does the following:
*   calculates the transaction cost of a stock by evaluating the following:
*   equity => stock market value * 0.5%
*   bond => stock market value * 2%
*
* @param {StockType} stockType - stock type (i.e equity or bond).
* @param {decimal} marketValue - current valuation of stock
*/
function calcTranCost(stockType, marketValue) {
    var equityValAlg = (marketValue * 0.5 / 100).toFixed(3);
    var bondValueAlg = (marketValue * 2 / 100).toFixed(3);

    switch (stockType) {
        case "Equity":
            return equityValAlg;

        case "Bond":
            return bondValueAlg;
    }
}