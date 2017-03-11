/**
* This is run as user defined function and does the following:
*   generates a name based on the stock type and index in datastore.
*
* @param {StockType} type - the type of stock (i.e Equity or Bond).
* @param {integer} index - index in datastore
*/
function generateName(name, type) {
    if (!name) {
        return type;
    }

    return name.concat(type);
}