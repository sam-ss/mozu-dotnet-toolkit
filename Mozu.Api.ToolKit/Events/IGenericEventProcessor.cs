using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mozu.Api.Contracts.Event;

namespace Mozu.Api.ToolKit.Events
{
    public interface IGenericEventProcessor
    {
        Task ProcessEvent(IApiContext apiContext, Event evt);
    }
}
