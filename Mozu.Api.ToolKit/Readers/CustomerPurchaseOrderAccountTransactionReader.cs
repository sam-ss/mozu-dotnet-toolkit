using System.Collections.Generic;
using System.Threading.Tasks;
using Mozu.Api.Contracts.Customer;
using Mozu.Api.Resources.Commerce.Customer;
using Mozu.Api.Resources.Commerce.Customer.Accounts;

namespace Mozu.Api.ToolKit.Readers
{
    public class CustomerPurchaseOrderAccountTransactionReader : BaseReader
    {
        private PurchaseOrderTransactionCollection _results;
        public int AccountId { get; set; }
        protected async override Task<bool> GetDataAsync()
        {
            var resource = new CustomerPurchaseOrderAccountResource(Context);
            _results = await resource.GetCustomerPurchaseOrderTransactionsAsync(AccountId, startIndex: StartIndex, pageSize: PageSize, sortBy: SortBy,responseFields: ResponseFields);

            TotalCount = _results.TotalCount;
            PageCount = _results.PageCount;
            PageSize = _results.PageSize;
            return _results.Items != null && _results.Items.Count > 0;
        }

        public List<PurchaseOrderTransaction> Items
        {
            get { return _results.Items; }
        }
    }
}