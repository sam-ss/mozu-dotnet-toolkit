using System.Collections.Generic;
using System.Threading.Tasks;
using Mozu.Api.Contracts.ProductAdmin;
using Mozu.Api.Resources.Commerce.Catalog.Admin.Pricelists;

namespace Mozu.Api.ToolKit.Readers
{
    public class PriceListEntryReader : BaseReader
    {
        private PriceListEntryCollection _results;
        public string PriceListCode { get; set; }
        protected async override Task<bool> GetDataAsync()
        {
            var resource = new PriceListEntryResource(Context);
            _results = await resource.GetPriceListEntriesAsync(PriceListCode,startIndex: StartIndex, pageSize: PageSize, sortBy: SortBy, 
                filter: Filter, responseFields: ResponseFields, ct: CancellationToken).ConfigureAwait(false);

            TotalCount = _results.TotalCount;
            return _results.Items != null && _results.Items.Count > 0;
        }

        public List<PriceListEntry> Items
        {
            get { return _results.Items; }
        }
    }
}