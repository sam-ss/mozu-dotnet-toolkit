using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mozu.Api.Contracts.Customer.Credit;
using Mozu.Api.Resources.Commerce.Customer.Credits;

namespace Mozu.Api.ToolKit.Readers
{
    public class CreditAuditEntryReader : BaseReader
    {
        private CreditAuditEntryCollection _results = null;
        public string Code { get; set; }

        protected override async Task<bool> GetDataAsync()
        {
            var resource = new CreditAuditEntryResource(Context);
            _results = await resource.GetAuditEntriesAsync(Code, startIndex: StartIndex, pageSize: PageSize, sortBy: SortBy, filter: Filter, responseFields: ResponseFields);

            TotalCount = _results.TotalCount;
            PageCount = _results.PageCount;
            PageSize = _results.PageSize;
            return _results.Items != null && _results.Items.Count > 0;
        }

        public List<CreditAuditEntry> Items
        {
            get { return _results.Items; }
        }
    }
}
