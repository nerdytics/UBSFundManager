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
    /// PubSub event for broadcasting summaries of each stock type
    /// (used in rendering visualizations)
    /// </summary>
    public class FundSummaryEvent : PubSubEvent<FundSummaryData>
    {

    }

    public class EnlargeChartFundSummaryEvent : PubSubEvent<FundSummaryData>
    {

    }

    public class EnlargeChartEvent : PubSubEvent { }
    public class FundModuleChartEvent : PubSubEvent { }
}
