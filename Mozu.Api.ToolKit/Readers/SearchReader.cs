using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mozu.Api.Contracts.ProductAdmin.Search;
using Mozu.Api.Resources.Commerce.Catalog.Admin;

namespace Mozu.Api.ToolKit.Readers
{
    public class SearchReader : BaseReader
    {
        private SynonymDefinitionPagedCollection _results;
        protected async override Task<bool> GetDataAsync()
        {
            if (!Context.SiteId.HasValue && string.IsNullOrEmpty(Context.Locale))
                throw new Exception("SiteId and Locale is required");

            var searchResource = new SearchResource(Context);

            _results = await searchResource.GetSynonymDefinitionsAsync(startIndex: StartIndex, pageSize: PageSize,
                sortBy: SortBy, filter: Filter, responseFields: ResponseFields, ct: CancellationToken).ConfigureAwait(false);

            TotalCount = _results.TotalCount;
            PageCount = _results.PageCount;
            PageSize = _results.PageSize;
            return _results.Items != null && _results.TotalCount > 0;

        }

        public List<SynonymDefinition> Items
        {
            get { return _results.Items; }
        }
    }
}