using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mozu.Api.Contracts.Customer.Credit;
using Mozu.Api.Resources.Commerce.Customer;

namespace Mozu.Api.ToolKit.Readers
{
    public class CreditReader : BaseReader
    {
        private CreditCollection _results = null;

        protected override async Task<bool> GetDataAsync()
        {
            var resource = new CreditResource(Context);
            _results = await resource.GetCreditsAsync(startIndex: StartIndex, pageSize: PageSize, sortBy: SortBy, filter: Filter, responseFields: ResponseFields);

            TotalCount = _results.TotalCount;
            PageCount = _results.PageCount;
            PageSize = _results.PageSize;
            return _results.Items != null && _results.Items.Count > 0;
        }

        public List<Credit> Items
        {
            get { return _results.Items; }
        }
    }
}
