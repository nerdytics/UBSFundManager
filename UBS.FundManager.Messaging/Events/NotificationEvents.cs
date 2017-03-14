using Prism.Events;
using System.Collections.Generic;
using UBS.FundManager.Messaging.Models.Fund;

namespace UBS.FundManager.Messaging.Events
{
    /// <summary>
    /// PubSub Event for broadcasting funds download action
    /// </summary>
    public class DownloadedFundsListEvent : PubSubEvent<IEnumerable<Fund>>
    {

    }

    /// <summary>
    /// PubSub event listened on by the fundsummary viewmodel to receive downloaded funds
    /// </summary>
    public class FundSummaryDownloadListEvent : PubSubEvent<IEnumerable<Fund>>
    {

    }

    /// <summary>
    /// PubSub event for broadcasting new funds added action
    /// </summary>
    public class NewFundAddedEvent : PubSubEvent<Fund>
    {

    }

    public class StockWithUpdatedValueEvent : PubSubEvent<Fund>
    {

    }

    /// <summary>
    /// PubSub event for broadcasting undelivarable messages
    /// </summary>
    public class UndeliveredMessageEvent : PubSubEvent<dynamic>
    {

    }

    /// <summary>
    /// PubSub event used by the FundListView to alert the FundSummaryViewModel
    /// to re-compute the summary of the portfolio value due to the addition of 
    /// a new stock.
    /// </summary>
    public class FundSummaryEvent : PubSubEvent<FundSummaryData>
    {

    }

    /// <summary>
    /// PubSub event used by the FundSummaryViewModel to alert the FunListViewModel
    /// to update its FundSammaryData
    /// </summary>
    public class FundsListViewSummaryEvent : PubSubEvent<FundSummaryData>
    {

    }

    public class EnlargeChartFundSummaryEvent : PubSubEvent<FundSummaryData>
    {

    }

    public class EnlargeChartEvent : PubSubEvent { }
    public class FundModuleChartEvent : PubSubEvent { }
}
