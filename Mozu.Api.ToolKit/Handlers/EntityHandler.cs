using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mozu.Api.Logging;
using Mozu.Api.Resources.Platform.Entitylists;
using Mozu.Api.ToolKit.Config;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Mozu.Api.ToolKit.Handlers
{
    public interface IEntityHandler
    {
        Task<T> AddEntityAsync<T>(IApiContext apiContext, String listName, T obj,CancellationToken ct = default(CancellationToken));
        Task<T> UpdateEntityAsync<T>(IApiContext apiContext, object id, String listName, T obj, CancellationToken ct = default(CancellationToken));
        Task<T> UpsertEntityAsync<T>(IApiContext apiContext, object id, String listName, T obj, CancellationToken ct = default(CancellationToken));
        Task<T> GetEntityAsync<T>(IApiContext apiContext, object id, string listName, CancellationToken ct = default(CancellationToken));
        Task DeleteEntityAsync(IApiContext apiContext, object id, string listName, CancellationToken ct = default(CancellationToken));
        Task<EntityCollection<T>> GetEntitiesAsync<T>(IApiContext apiContext, string listName, CancellationToken ct = default(CancellationToken));
        Task<EntityCollection<T>> GetEntitiesAsync<T>(IApiContext apiContext, string listName, int? pageSize, int? startIndex, string filter, string sortBy, string responseFileds, CancellationToken ct = default(CancellationToken));
    }

    public class EntityHandler : IEntityHandler
    {

        private readonly IAppSetting _appSetting;
        private readonly ILogger _logger = LogManager.GetLogger(typeof(EntityHandler));

        public JsonSerializer SerializerSettings = new JsonSerializer
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        };

        public EntityHandler(IAppSetting appSetting)
        {
            _appSetting = appSetting;
        }

        public async Task<T> GetEntityAsync<T>(IApiContext apiContext, object id, string listName, CancellationToken ct = default(CancellationToken))
        {
            var entityResource = new EntityResource(apiContext);
            var listFQN = ValidateListName(listName);
            try
            {
                var jobject = await entityResource.GetEntityAsync(listFQN, id.ToString(), ct: ct).ConfigureAwait(false);
                if (jobject == null)
                    return default(T);
                return jobject.ToObject<T>(SerializerSettings);
            }
            catch (AggregateException ae)
            {
                if (ae.InnerException != null && ae.InnerException.GetType() == typeof(ApiException)) throw;
                var aex = (ApiException)ae.InnerException;
                _logger.Error(aex.Message, aex);
                throw aex;
            }

        }

        public async Task<T> AddEntityAsync<T>(IApiContext apiContext, String listName, T obj, CancellationToken ct = default(CancellationToken))
        {
            var entityResource = new EntityResource(apiContext);
            var jobject = JObject.FromObject(obj, SerializerSettings);
            var listFQN = ValidateListName(listName);
            jobject = await entityResource.InsertEntityAsync(jobject, listFQN,ct: ct).ConfigureAwait(false);
            return jobject.ToObject<T>();
        }

        public async Task<T> UpdateEntityAsync<T>(IApiContext apiContext, object id, String listName, T obj, CancellationToken ct = default(CancellationToken))
        {
            var entityResource = new EntityResource(apiContext);
            var jobject = JObject.FromObject(obj, SerializerSettings);
            var listFQN = ValidateListName(listName);

            jobject = await entityResource.UpdateEntityAsync(jobject, listFQN, id.ToString(), ct:ct).ConfigureAwait(false);

            return jobject.ToObject<T>();
        }

        public async Task<T> UpsertEntityAsync<T>(IApiContext apiContext, object id, String listName, T obj, CancellationToken ct = default(CancellationToken))
        {
            var existing = await GetEntityAsync<T>(apiContext, id.ToString(), listName);

            return existing == null
                ? await AddEntityAsync(apiContext, listName, obj, ct: ct)
                : await UpdateEntityAsync(apiContext, id, listName, obj, ct: ct);
        }

        public async Task DeleteEntityAsync(IApiContext apiContext, object id, string listName, CancellationToken ct = default(CancellationToken))
        {
            var entityResource = new EntityResource(apiContext);
            var listFQN = ValidateListName(listName);
            await entityResource.DeleteEntityAsync(listFQN, id.ToString(), ct:ct);
        }


        public async Task<EntityCollection<T>> GetEntitiesAsync<T>(IApiContext apiContext, string listName, CancellationToken ct = default(CancellationToken))
        {
            var entities = await GetEntitiesAsync<T>(apiContext, listName, null, null, null, null, null, ct: ct);
            return entities;
        }

        public async Task<EntityCollection<T>> GetEntitiesAsync<T>(IApiContext apiContext, string listName,int? pageSize, int? startIndex, string filter, string sortBy, string responseFileds, CancellationToken ct = default(CancellationToken))
        {
            var entityResource = new EntityResource(apiContext);
            var listFQN = ValidateListName(listName);
            var entities = await entityResource.GetEntitiesAsync(listFQN, pageSize, startIndex, filter, sortBy, responseFileds,ct: ct);

            var entityCollection = new EntityCollection<T>
            {
                PageCount = entities.PageCount,
                PageSize = entities.PageSize,
                StartIndex = entities.StartIndex,
                TotalCount = entities.TotalCount
            };

            entityCollection.Items = entities.Items.ConvertAll(JObjectConverter<T>);

            return entityCollection;
        }

        private T JObjectConverter<T>(JObject input)
        {
            var obj = input.ToObject<T>(SerializerSettings);
            return obj;
        }

        private string ValidateListName(String listName)
        {
            if (!listName.Contains("@")) listName = string.Format("{0}@{1}", listName, _appSetting.Namespace);
            return listName;
        }
    }


    public class EntityCollection<T>
    {
        public List<T> Items { get; set; }
        public int PageSize { get; set; }
        public int StartIndex { get; set; }
        public int TotalCount { get; set; }
        public int PageCount { get; set; }
    }
}
