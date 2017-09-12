using System.Collections.Generic;
using System.Threading.Tasks;
using Mozu.Api.Contracts.Customer;
using Mozu.Api.Resources.Commerce.Customer;
using Mozu.Api.Resources.Commerce.Customer.Accounts;

namespace Mozu.Api.ToolKit.Readers
{
    public class CustomerPurchaseOrderAccountReader : BaseReader
    {
        private CustomerPurchaseOrderAccountCollection _results;
      
        protected async override Task<bool> GetDataAsync()
        {
            var resource = new CustomerAccountResource(Context);
            _results =await resource.GetCustomersPurchaseOrderAccountsAsync(startIndex: StartIndex, pageSize: PageSize, sortBy: SortBy, 
                responseFields: ResponseFields, ct: CancellationToken).ConfigureAwait(false);

            TotalCount = _results.TotalCount;
            PageCount = _results.PageCount;
            PageSize = _results.PageSize;
            return _results.Items != null && _results.Items.Count > 0;
        }

        public List<CustomerPurchaseOrderAccount> Items
        {
            get { return _results.Items; }
        }
    }
}