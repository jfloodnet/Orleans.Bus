using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using Orleans;
using Orleans.Bus;
using Orleans.Concurrency;

namespace Sample
{
    [Serializable, Immutable]
    public class CreateTopic : Command
    {
        public readonly string Query;
        public readonly IReadOnlyDictionary<string, TimeSpan> Schedule;

        public CreateTopic(string query, IDictionary<string, TimeSpan> schedule)
        {
            Query = query;
            Schedule = new ReadOnlyDictionary<string, TimeSpan>(schedule);
        }
    }

    [Handles(typeof(CreateTopic))]
    [ExtendedPrimaryKey]
    public interface ITopic : IMessageBasedGrain, IRemindable
    {
        [Handler] Task OnCommand(object cmd);
    }
}