using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mozu.Api.Contracts.Event;
using Mozu.Api.Resources.Event;

namespace Mozu.Api.ToolKit.Readers
{
    public class EventReader : BaseReader
    {
        private EventCollection _results;
        protected override async Task<bool> GetDataAsync()
        {
            var eventNotificationResource = new EventNotificationResource(Context);

            _results = await eventNotificationResource.GetEventsAsync(startIndex: StartIndex, pageSize: PageSize, sortBy: SortBy, filter: Filter, responseFields: ResponseFields);

            TotalCount = _results.TotalCount;
            PageCount = _results.PageCount;
            PageSize = _results.PageSize;
            return _results.Items != null && _results.Items.Count > 0;
        }

        public List<Event> Items
        {
            get { return _results.Items; }
        } 
    }
}
