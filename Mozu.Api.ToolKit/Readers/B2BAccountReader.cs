using System.Collections.Generic;
using System.Threading.Tasks;
using Mozu.Api.Contracts.Customer;
using Mozu.Api.Resources.Commerce.Customer;

namespace Mozu.Api.ToolKit.Readers
{
    public class B2BAccountReader : BaseReader
    {
        private B2BAccountCollection _results = null;

        protected override async Task<bool> GetDataAsync()
        {
            var resource = new B2BAccountResource(Context);
            _results = await resource.GetB2BAccountsAsync(startIndex: StartIndex, pageSize: PageSize, sortBy: SortBy,
                    filter: Filter, q: Q, qLimit: QLimit, responseFields: ResponseFields, ct: CancellationToken)
                .ConfigureAwait(false);

            TotalCount = _results.TotalCount;
            PageCount = _results.PageCount;
            PageSize = _results.PageSize;
            return _results.Items != null && _results.Items.Count > 0;
        }

        public List<B2BAccount> Items
        {
            get { return _results.Items; }
        }
    }
}