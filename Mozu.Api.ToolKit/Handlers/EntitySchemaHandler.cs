using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mozu.Api.Contracts.MZDB;
using Mozu.Api.Logging;
using Mozu.Api.Resources.Platform;
using Mozu.Api.ToolKit.Config;

namespace Mozu.Api.ToolKit.Handlers
{

    public enum EntityScope
    {
        Tenant,
        Site
    }

    public enum EntityDataType
    {
        String, Integer, Decimal, Date, Boolean
    }

    public interface IEntitySchemaHandler
    {
        Task<EntityList> InstallSchemaAsync(IApiContext apiContext, EntityList entityList, EntityScope scope, List<IndexedProperty> indexedProperties, CancellationToken ct = default(CancellationToken));
        Task<EntityList> InstallSchemaAsync(IApiContext apiContext, EntityList entityList, EntityScope scope, IndexedProperty idProperty, List<IndexedProperty> indexedProperties, CancellationToken ct = default(CancellationToken));

        Task<EntityList> GetEntityListAsync(IApiContext apiContext, String name, CancellationToken ct = default(CancellationToken));
        Task<EntityList> GetEntityListAsync(IApiContext apiContext, String name, string nameSpace, CancellationToken ct = default(CancellationToken));
        IndexedProperty GetIndexedProperty(String name, EntityDataType entityDataType);
    }

    public class EntitySchemaHandler : IEntitySchemaHandler
    {
        private readonly IAppSetting _appSetting;
        private readonly ILogger _logger = LogManager.GetLogger(typeof(EntitySchemaHandler));

        public EntitySchemaHandler(IAppSetting appSetting)
        {
            _appSetting = appSetting;
        }

        public async Task<EntityList> GetEntityListAsync(IApiContext apiContext, String name, CancellationToken ct = default(CancellationToken))
        {
            return await GetEntityListAsync(apiContext, name, _appSetting.Namespace,ct);
        }

        public async Task<EntityList> GetEntityListAsync(IApiContext apiContext, String name, string nameSpace, CancellationToken ct = default(CancellationToken))
        {
            var entityListResource = new EntityListResource(apiContext);
            String listFQN = GetListFQN(name, nameSpace);
            EntityList entityList = null;
            try
            {
                entityList = await entityListResource.GetEntityListAsync(listFQN,ct:ct).ConfigureAwait(false);
            }
            catch (AggregateException ae)
            {
                if (ae.InnerException != null && ae.InnerException.GetType() == typeof (ApiException)) throw;
                var aex = (ApiException)ae.InnerException;
                _logger.Error(aex.Message, aex);
                throw aex;
            }
            return entityList;
        }

        public async Task<EntityList> InstallSchemaAsync(IApiContext apiContext, EntityList entityList, EntityScope scope,
            List<IndexedProperty> indexedProperties, CancellationToken ct = default(CancellationToken))
        {
            return await InstallSchemaAsync(apiContext, entityList, scope, null, indexedProperties, ct);
        }

        public async Task<EntityList> InstallSchemaAsync(IApiContext apiContext, EntityList entityList, EntityScope scope, 
            IndexedProperty idProperty,List<IndexedProperty> indexedProperties, CancellationToken ct = default(CancellationToken))
        {

            if (indexedProperties != null && indexedProperties.Count > 4) throw new Exception("Only 4 indexed properties are supported");
            if (string.IsNullOrEmpty(entityList.Name)) throw new Exception("Entity name is missing");

            entityList.TenantId = apiContext.TenantId;
            entityList.ContextLevel = scope.ToString();

            if (indexedProperties != null) { 
                entityList.IndexA = indexedProperties.Count >= 1 ? indexedProperties[0] : null;
                entityList.IndexB = indexedProperties.Count >= 2 ? indexedProperties[1] : null;
                entityList.IndexC = indexedProperties.Count >= 3 ? indexedProperties[2] : null;
                entityList.IndexD = indexedProperties.Count >= 4 ? indexedProperties[3] : null;
            }
            
            if (idProperty == null) entityList.UseSystemAssignedId = true;
            else entityList.IdProperty = idProperty;

            if (string.IsNullOrEmpty(entityList.NameSpace)) 
                entityList.NameSpace = _appSetting.Namespace;

            var entityListResource = new EntityListResource(apiContext);
            var listFQN = GetListFQN(entityList.Name, entityList.NameSpace);
            var existing = await GetEntityListAsync(apiContext, entityList.Name,ct:ct).ConfigureAwait(false);

            try
            {
                existing = existing != null
                    ? await entityListResource.UpdateEntityListAsync(entityList, listFQN,ct:ct).ConfigureAwait(false)
                    : await entityListResource.CreateEntityListAsync(entityList, ct:ct).ConfigureAwait(false);
            }
            catch (AggregateException ae)
            {
                if (ae.InnerException != null && ae.InnerException.GetType() == typeof(ApiException)) throw;
                var aex = (ApiException)ae.InnerException;
                _logger.Error(aex.Message, aex);
                throw aex;
            }

            return entityList;
        }

        public IndexedProperty GetIndexedProperty(string name, EntityDataType entityDataType)
        {
            return new IndexedProperty {PropertyName = name, DataType = entityDataType.ToString()};
        }

        private string GetListFQN(String listName, string nm)
        {
            if (String.IsNullOrEmpty(nm)) nm = _appSetting.Namespace;
            return String.Format("{0}@{1}", listName, nm);
        }
    }
}
