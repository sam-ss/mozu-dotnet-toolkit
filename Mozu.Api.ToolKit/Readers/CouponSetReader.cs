using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mozu.Api.Contracts.ProductAdmin;
using Mozu.Api.Resources.Commerce.Catalog.Admin;

namespace Mozu.Api.ToolKit.Readers
{
    public class CouponSetReader : BaseReader
    {
        private CouponSetCollection _results;
        protected async override Task<bool> GetDataAsync()
        {

            if (!Context.CatalogId.HasValue)
                throw new Exception("CatalogId is required");

            var couponSetResource = new CouponSetResource(Context);

            _results = await couponSetResource
                .GetCouponSetsAsync(startIndex: StartIndex, pageSize: PageSize, sortBy: SortBy, filter: Filter,
                    responseFields: ResponseFields, ct: CancellationToken).ConfigureAwait(false);

            TotalCount = _results.TotalCount;
            PageCount = _results.PageCount;
            PageSize = _results.PageSize;
            return _results.Items != null && _results.Items.Count > 0;
        }

        public List<CouponSet> Items
        {
            get { return _results.Items; }
        } 
    }
}
