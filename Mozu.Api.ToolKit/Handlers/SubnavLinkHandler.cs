using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mozu.Api.Contracts.MZDB;
using Mozu.Api.Resources.Commerce.Settings;
using Mozu.Api.Resources.Platform.Entitylists;
using Mozu.Api.ToolKit.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Mozu.Api.ToolKit.Handlers
{

    public enum Parent //for burger menu
    {
        Orders,
        Catalog,
        Products,
        Fulfillment,
        Customers,
        Marketing,
        Sitebuilder, 
        Settings, 
        Locations, 
        Publishing, 
        Reports,
        Reporting,
        SiteBuilder,
        Schema,
        Customization,
        Structure,
        Permissions,
        Analytics,
        Capability,
        CustomRoutes,
        StoreCredits,
        CustomerAttributes,
        Categories,
        Inventory,
        Discounts,
        CouponSets,
        ProductRankings,
        Provisioning,
        Themes,
        Shipping,
        Localization,
        CustomSchema,
        GeneralSettings,
        OrderAttributes,
        Roles,
        ProductTypes,
        Attributes,
        FileManager,
        Channels,
        LocationTypes,
        Website,
        LocationInventory,
        Redirects,
        ActionManagement,
        IpBlocking

    }


    public enum LinkType
    {
        Menu,
        Grid,
        Edit
    }

    public enum DisplayMode
    {
        Navigate,
        Modal
    }

    /*public enum Context
    {
        Orders,
        Customer,
        Discounts,
        Location,
        Products
    }
    */

    public interface ISubnavLinkHandler
    {

        Task AddUpdateExtensionLinkAsync(int tenantId, Parent parent, string href, string windowTitle, String[] path, /*Context? requiredContext = null,*/ LinkType? type = null, DisplayMode? displayMode=null);
        Task AddUpdateExtensionLinkAsync(int tenantId, SubnavLink subnavLink,LinkType? type=null);
        Task Delete(int tenantId, SubnavLink subnavLink=null);

    }

    public class SubnavLinkHandler : ISubnavLinkHandler
    {
        private const string SubnavLinkEntityName = "subnavlinks@mozu";
        private readonly List<String> _validBurgerMenus = new List<string> {
           "Catalog","Fulfillment","Customers","Marketing","Sitebuilder","Settings","Publishing","Reporting","SiteBuilder","Schema","Customization","Structure","Permissions"}; 

        private readonly List<String> _validGridEditItems = new List<string>
        {
            "Orders","Products", "Locations", "Reports","Analytics","Capability","CustomRoutes","StoreCredits","CustomerAttributes","Categories","Inventory","Discounts","CouponSets","ProductRankings","Provisioning","Themes","Shipping","Localization","CustomSchema","GeneralSettings","OrderAttributes","Roles","ProductTypes","Attributes","FileManager","Channels","LocationTypes","Website","LocationInventory","Redirects","ActionManagement","IpBlocking"
        }; 

        public async Task AddUpdateExtensionLinkAsync(int tenantId, Parent parent, string href, string windowTitle, String[] path, /*Context? requiredContext =null,*/ LinkType? type = null, DisplayMode? displayMode = null)
        {

            var link = new SubnavLink
            {
                ParentId = parent,
                WindowTitle = windowTitle,
                Href = href,
                Path = path
            };
            if (displayMode.HasValue)
                link.DisplayMode = displayMode.Value;
            
           

            /*if (requiredContext.HasValue)
                link.RequiredContext = requiredContext.Value;*/
            await AddUpdateExtensionLinkAsync(tenantId, link, type);
        }
        
        public async Task AddUpdateExtensionLinkAsync(int tenantId, SubnavLink subnavLink, LinkType? type = null)
        {
            var apiContext = new ApiContext(tenantId);
            subnavLink.AppId = await GetAppId(apiContext);
            if (type.HasValue)
            {
                subnavLink.Location = string.Format("{0}{1}", subnavLink.ParentId.ToString().ToLower(), type.ToString().ToLower());
                if (!subnavLink.DisplayMode.HasValue)
                    subnavLink.DisplayMode = DisplayMode.Modal;
            }


            var entityResource = new EntityResource(apiContext);
            var tenantSettingsJobj = await entityResource.GetEntityAsync("tenantadminsettings@mozu", "global");
            var tenantSettings = tenantSettingsJobj.ToObject<TenantAdminSettings>();

            if (tenantSettings.EnableBetaAdmin)
            {

                //validate combo
                if (type.HasValue && type.Value == LinkType.Menu && !_validBurgerMenus.Contains(subnavLink.ParentId.ToString()))
                    throw new Exception("Invalid Parent option for Menu type. Valid options are "+_validBurgerMenus.Aggregate((x,y)=>x+","+y));
                if (type.HasValue && (type.Value == LinkType.Edit || type.Value ==LinkType.Grid) && !_validGridEditItems.Contains(subnavLink.ParentId.ToString()))
                    throw new Exception("Invalid Parent option for "+type.ToString()+" type. Valid options are " + _validGridEditItems.Aggregate((x, y) => x + "," + y));

                subnavLink.ParentId = null;
                subnavLink.WindowTitle = null;

                

            }

            await AddUpdateSubNavLink(apiContext, subnavLink);
        }

        public async Task Delete(int tenantId, SubnavLink subNavlink = null)
        {
            var apiContext = new ApiContext(tenantId);
            var entityContainerResource = new EntityContainerResource(apiContext);
            var collection = await entityContainerResource.GetEntityContainersAsync(SubnavLinkEntityName, 200);
            var entityResource = new EntityResource(apiContext);

            if (subNavlink == null)
            {
                var appId = await GetAppId(apiContext);
                foreach (
                    var item in
                        collection.Items.Where(subnavLink => subnavLink.Item.ToObject<SubnavLink>().AppId.Equals(appId))
                    )
                {
                    await entityResource.DeleteEntityAsync(SubnavLinkEntityName, item.Id);
                }
            }
            else
            {
                if (subNavlink.ParentId == null || subNavlink.Path == null || !subNavlink.Path.Any())
                    throw new Exception("ParentId and Path is required to delete a link");
                var existing = collection.Items.SingleOrDefault(x => subNavlink.Path.SequenceEqual(x.Item.ToObject<SubnavLink>().Path)
                    && (subNavlink.ParentId == x.Item.ToObject<SubnavLink>().ParentId || subNavlink.Location == x.Item.ToObject<SubnavLink>().Location ));
                
                if (existing != null)
                    await entityResource.DeleteEntityAsync(SubnavLinkEntityName, existing.Id);
            }

        }

        private async Task<EntityContainer> GetExistingLink(IApiContext apiContext, SubnavLink subnavLink)
        {
            var entityContainerResource = new EntityContainerResource(apiContext);
            var collection = await entityContainerResource.GetEntityContainersAsync(SubnavLinkEntityName, 200);

            var existing = collection.Items.FirstOrDefault(x => subnavLink.Path.SequenceEqual(x.Item.ToObject<SubnavLink>().Path)
                && subnavLink.ParentId == x.Item.ToObject<SubnavLink>().ParentId);
            return existing;
        } 

        private async Task<SubnavLink> AddUpdateSubNavLink(IApiContext apiContext, SubnavLink subnavLink)
        {
            var entityResource = new EntityResource(apiContext);
            JObject jObject = null;
            if (subnavLink.Path == null || !subnavLink.Path.Any()) return null;

            var existing = await GetExistingLink(apiContext, subnavLink);

            jObject = FromObject(subnavLink);

            if (existing == null)
                jObject = await entityResource.InsertEntityAsync(jObject, SubnavLinkEntityName);
            else
                jObject = await entityResource.UpdateEntityAsync(jObject, SubnavLinkEntityName, existing.Id);

            return jObject.ToObject<SubnavLink>();
        }

        private async Task<string> GetAppId(IApiContext apiContext)
        {

            var applicationResource = new ApplicationResource(apiContext);
            var app = await applicationResource.ThirdPartyGetApplicationAsync();
            return app.AppId;
        }

        private JObject FromObject<T>(T value)
        {
            var serializer = new JsonSerializer
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
                
            };
            return JObject.FromObject(value, serializer);
        }
        
      
    }
}
