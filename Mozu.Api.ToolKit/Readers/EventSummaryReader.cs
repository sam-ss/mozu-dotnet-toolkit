using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mozu.Api.Contracts.Event;
using Mozu.Api.Resources.Event.Push.Subscriptions;

namespace Mozu.Api.ToolKit.Readers
{
    public class EventSummaryReader : BaseReader
    {
        public string SubscriptionId { get; set; }
        private EventDeliverySummaryCollection _results;
        protected override async Task<bool> GetDataAsync()
        {
            var eventDeliverySummaryResource = new EventDeliverySummaryResource(Context);

            if (String.IsNullOrEmpty(SubscriptionId))
                throw new Exception("SubscriptionId is required for Get Event Delivery summary");


            _results = await eventDeliverySummaryResource.GetDeliveryAttemptSummariesAsync(SubscriptionId,startIndex: StartIndex, pageSize: PageSize, sortBy: SortBy, filter: Filter, responseFields: ResponseFields);

            TotalCount = _results.TotalCount;
            PageCount = _results.PageCount;
            PageSize = _results.PageSize;
            return _results.Items != null && _results.Items.Count > 0;
        }

        public List<EventDeliverySummary> Items
        {
            get { return _results.Items; }
        } 
    }
}
