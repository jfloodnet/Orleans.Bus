using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

using Orleans;
using Orleans.Bus;

namespace Sample
{
    [Serializable]
    public class Search
    {
        public readonly string Subject;

        public Search(string subject)
        {
            Subject = subject;
        }
    }

    [Serializable]
    public class AvailabilityChanged : Event
    {
        public readonly string Api;
        public readonly bool Available;

        public AvailabilityChanged(string api, bool available)
        {
            Api = api;
            Available = available;
        }
    }

    [Answers(typeof(Search))]
    [Notifies(typeof(AvailabilityChanged))]
    [ExtendedPrimaryKey]
    public interface IApi : IObservableMessageBasedGrain
    {
        [Handler] Task<object> OnQuery(object query);
    }

    [Serializable]
    public class ApiUnavailableException : ApplicationException
    {
        public ApiUnavailableException(string api)
            : base(api + " api is unavailable. Try later!")
        {}

        protected ApiUnavailableException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {}
    }
}
