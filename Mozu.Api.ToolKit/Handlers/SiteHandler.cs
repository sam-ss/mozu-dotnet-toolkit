using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mozu.Api.Contracts.SiteSettings.General;
using Mozu.Api.Contracts.Tenant;
using Mozu.Api.Resources.Commerce.Settings;
using Mozu.Api.Resources.Platform;
using TimeZone = Mozu.Api.Contracts.Reference.TimeZone;

namespace Mozu.Api.ToolKit.Handlers
{
    public interface ISiteHandler
    {
        Task<GeneralSettings> GetGeneralSettings(IApiContext apiContext, CancellationToken ct = default(CancellationToken));
        Task<TimeZone> GetTimezone(IApiContext apiContext, GeneralSettings generalSettings = null, CancellationToken ct = default(CancellationToken));

        Task<String> GetSiteDomain(IApiContext apiContext, Site site = null, CancellationToken ct = default(CancellationToken));
        Task<Site> GetSite(IApiContext apiContext, CancellationToken ct = default(CancellationToken));
    }

    public class SiteHandler : ISiteHandler
    {

        public async Task<GeneralSettings> GetGeneralSettings(IApiContext apiContext, CancellationToken ct = default(CancellationToken))
        {
            if (apiContext.SiteId.GetValueOrDefault(0) == 0)
                throw new Exception("Site ID is missing in api context");

            var settingResource = new GeneralSettingsResource(apiContext);
            return await settingResource.GetGeneralSettingsAsync(ct: ct).ConfigureAwait(false);
        }

        public async Task<TimeZone> GetTimezone(IApiContext apiContext, GeneralSettings generalSettings = null, CancellationToken ct = default(CancellationToken))
        {
            if (generalSettings == null)
                generalSettings = await GetGeneralSettings(apiContext);

            var referenceApi = new ReferenceDataResource();
            var timeZones = await referenceApi.GetTimeZonesAsync(ct:ct).ConfigureAwait(false);
            return timeZones.Items.SingleOrDefault(x => x.Id.Equals(generalSettings.SiteTimeZone));
        }

        public async Task<String> GetSiteDomain(IApiContext apiContext, Site site = null, CancellationToken ct = default(CancellationToken))
        {
            if (site == null)
             site = await GetSite(apiContext,ct);
            return (string.IsNullOrEmpty(site.PrimaryCustomDomain) ? site.Domain : site.PrimaryCustomDomain);
        }


        public async Task<Site> GetSite(IApiContext apiContext, CancellationToken ct = default(CancellationToken))
        {
            if (apiContext.SiteId.GetValueOrDefault(0) == 0)
                throw new Exception("Site ID is missing in api context");

            var tenant = apiContext.Tenant;
            if (tenant == null)
            {
                var tenantResource = new TenantResource();
                tenant = await tenantResource.GetTenantAsync(apiContext.TenantId, ct:ct).ConfigureAwait(false);
            }

            var site = tenant.Sites.SingleOrDefault(x => x.Id == apiContext.SiteId);
            if (site == null)
                throw new Exception("Site " + apiContext.SiteId + " not found for tenant " + tenant.Name);
            return site;
        }
    }
}
