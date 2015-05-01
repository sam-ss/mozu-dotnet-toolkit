using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        Task<GeneralSettings> GetGeneralSettings(IApiContext apiContext);
        Task<TimeZone> GetTimezone(IApiContext apiContext, GeneralSettings generalSettings = null);

        Task<String> GetSiteDomain(IApiContext apiContext, Site site = null);
        Task<Site> GetSite(IApiContext apiContext);
    }

    public class SiteHandler : ISiteHandler
    {

        public async Task<GeneralSettings> GetGeneralSettings(IApiContext apiContext)
        {
            if (apiContext.SiteId.GetValueOrDefault(0) == 0)
                throw new Exception("Site ID is missing in api context");

            var settingResource = new GeneralSettingsResource(apiContext);
            return await settingResource.GetGeneralSettingsAsync();
        }

        public async Task<TimeZone> GetTimezone(IApiContext apiContext, GeneralSettings generalSettings = null)
        {
            if (generalSettings == null)
                generalSettings = await GetGeneralSettings(apiContext);

            var referenceApi = new ReferenceDataResource();
            var timeZones = await referenceApi.GetTimeZonesAsync();
            return timeZones.Items.SingleOrDefault(x => x.Id.Equals(generalSettings.SiteTimeZone));
        }

        public async Task<String> GetSiteDomain(IApiContext apiContext, Site site = null)
        {
            if (site == null)
             site = await GetSite(apiContext);
            return (string.IsNullOrEmpty(site.PrimaryCustomDomain) ? site.Domain : site.PrimaryCustomDomain);
        }


        public async Task<Site> GetSite(IApiContext apiContext)
        {
            if (apiContext.SiteId.GetValueOrDefault(0) == 0)
                throw new Exception("Site ID is missing in api context");

            var tenant = apiContext.Tenant;
            if (tenant == null)
            {
                var tenantResource = new TenantResource();
                tenant = await tenantResource.GetTenantAsync(apiContext.TenantId);
            }

            var site = tenant.Sites.SingleOrDefault(x => x.Id == apiContext.SiteId);
            if (site == null)
                throw new Exception("Site " + apiContext.SiteId + " not found for tenant " + tenant.Name);
            return site;
        }
    }
}
