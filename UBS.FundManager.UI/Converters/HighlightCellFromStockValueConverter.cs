using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using UBS.FundManager.Messaging.Models.Fund;

namespace UBS.FundManager.UI.Converters
{
    /// <summary>
    /// Converter determines if a cell should be highlighted based on a set of rules
    /// which are as follows:
    ///     -   Market value < 0
    ///     -   Transaction Cost > Tolerance. 
    ///         Given:
    ///             -   StockType is Equity, Tolerance = 200,000
    ///             -   StockType is Bond, Tolerance = 100,000
    /// </summary>
    public class HighlightCellFromStockValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Stock)
            {
                Stock stock = (Stock)value;

                if(stock.ValueInfo.MarketValue < 0 || 
                    BondsStockToleranceFilter(stock.ValueInfo.TransactionCost) ||
                        EquityStockToleranceFilter(stock.ValueInfo.TransactionCost))
                {
                    return true;
                }
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }

        /// <summary>
        /// Filters to determine violation of tolerance levels per fund type
        /// </summary>
        Predicate<decimal> BondsStockToleranceFilter = (transactionCost) => transactionCost > 100000;
        Predicate<decimal> EquityStockToleranceFilter = (transactionCost) => transactionCost > 200000;
    }
}
