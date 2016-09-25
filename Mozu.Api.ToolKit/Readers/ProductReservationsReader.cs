using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mozu.Api.Contracts.ProductAdmin;
using Mozu.Api.Resources.Commerce.Catalog.Admin;

namespace Mozu.Api.ToolKit.Readers
{
    public class ProductReservationsReader : BaseReader
    {
        private ProductReservationCollection _results = null;

        protected override async Task<bool> GetDataAsync()
        {
            var resource = new ProductReservationResource(Context);
            _results = await resource.GetProductReservationsAsync(startIndex: StartIndex, pageSize: PageSize, sortBy: SortBy, filter: Filter, responseFields: ResponseFields);

            TotalCount = _results.TotalCount;
            PageCount = _results.PageCount;
            PageSize = _results.PageSize;
            return _results.Items != null && _results.Items.Count > 0;
        }

        public List<ProductReservation> Items
        {
            get { return _results.Items; }
        }
    }
}
