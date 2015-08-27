using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mozu.Api.Contracts.ProductAdmin;
using Mozu.Api.Resources.Commerce.Catalog.Admin;
using Mozu.Api.Resources.Commerce.Catalog.Admin.Couponsets;

namespace Mozu.Api.ToolKit.Readers
{
    public class CouponReader : BaseReader
    {
        private CouponCollection _results;
        public string CouponSetCode { get; set; }

        protected async override Task<bool> GetDataAsync()
        {
            if (!Context.CatalogId.HasValue)
                throw new Exception("CatalogId is required");

            var couponResource = new CouponResource(Context);

            if (string.IsNullOrEmpty(CouponSetCode))
                throw new Exception("CouponSetCode is requried to get Coupons");

            _results = await couponResource.GetCouponsAsync(CouponSetCode,startIndex: StartIndex, pageSize: PageSize, sortBy: SortBy, filter: Filter, responseFields: ResponseFields);

            TotalCount = _results.TotalCount;
            PageCount = _results.PageCount;
            PageSize = _results.PageSize;
            return _results.Items != null && _results.Items.Count > 0;
        }

        public List<Coupon> Items
        {
            get { return _results.Items; }
        } 
    }
}
