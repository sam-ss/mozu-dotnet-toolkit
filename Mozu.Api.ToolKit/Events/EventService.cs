using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Autofac;
using Mozu.Api.Contracts.Event;
using Mozu.Api.Events;
using Mozu.Api.Logging;
using Mozu.Api.Resources.Commerce.Settings;
using Mozu.Api.ToolKit.Handlers;
using Mozu.Api.ToolKit.Models;
using Newtonsoft.Json;

namespace Mozu.Api.ToolKit.Events
{
    public class EventService : IEventService
    {
        private readonly ILogger _logger = LogManager.GetLogger(typeof(EventService));
        private readonly IComponentContext _container;
        private readonly IEmailHandler _emailHandler;

        public EventService(IComponentContext container, IEmailHandler emailHandler)
        {
            _container = container;
            _emailHandler = emailHandler;
        }

        public async Task ProcessEventAsync(IApiContext apiContext, Event eventPayLoad)
        {
            try
            {
                Trace.CorrelationManager.ActivityId = !String.IsNullOrEmpty(apiContext.CorrelationId)
                    ? Guid.Parse(apiContext.CorrelationId)
                    : Guid.NewGuid();

                _logger.Info(String.Format("Got Event {0} for tenant {1}", eventPayLoad.Topic, apiContext.TenantId));


                var eventType = eventPayLoad.Topic.Split('.');
                var topic = eventType[0];

                if (String.IsNullOrEmpty(topic))
                    throw new ArgumentException("Topic cannot be null or empty");

                var eventCategory = (EventCategory) (Enum.Parse(typeof (EventCategory), topic, true));
                var eventProcessor = _container.ResolveKeyed<IEventProcessor>(eventCategory);
                await eventProcessor.ProcessAsync(_container, apiContext, eventPayLoad);
            }
            catch (Exception exc)
            {
                _emailHandler.SendErrorEmail(new ErrorInfo{Message = "Error Processing Event : "+ JsonConvert.SerializeObject(eventPayLoad), Context = apiContext, Exception = exc});
                throw exc;
            }
        }

    }

}
