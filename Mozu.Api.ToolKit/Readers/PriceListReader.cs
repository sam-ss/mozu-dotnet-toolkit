using System.Collections.Generic;
using System.Threading.Tasks;
using Mozu.Api.Contracts.ProductAdmin;
using Mozu.Api.Resources.Commerce.Catalog.Admin;

namespace Mozu.Api.ToolKit.Readers
{
    public class PriceListReader : BaseReader
    {
        private PriceListCollection _results;
        protected async override Task<bool> GetDataAsync()
        {
            var resource = new PriceListResource(Context);
            _results = await resource.GetPriceListsAsync(startIndex: StartIndex, pageSize: PageSize, sortBy: SortBy, filter: Filter, responseFields: ResponseFields);

            TotalCount = _results.TotalCount;
            return _results.Items != null && _results.Items.Count > 0;
        }

        public List<PriceList> Items
        {
            get { return _results.Items; }
        }
    }
}