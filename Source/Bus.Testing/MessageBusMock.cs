using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    public class MessageBusMock : IMessageBus
    {
        public readonly RecordedCommandCollection RecordedCommands = new RecordedCommandCollection();
        public readonly RecordedQueryCollection RecordedQueries = new RecordedQueryCollection();

        readonly List<Route> routes = new List<Route>();

        public void Expect(string destination, IExpectation expectation)
        {
            var route = routes.Find(x => x.Match(destination));
            
            if (route == null)
            {
                route = new Route(destination);
                routes.Add(route);
            }

            route.Add(expectation);
        }

        Task IMessageBus.Send(string destination, object command)
        {
            return ((IMessageBus)this).Send(destination, command.GetType(), command);
        }

        Task IMessageBus.Send(string destination, Type command, object message)
        {
            RecordedCommands.Add(new RecordedCommand(destination, command, message));

            var route = routes.Find(x => x.Match(destination, message));
            if (route != null)
                route.Apply(message);

            return TaskDone.Done;
        }

        Task<TResult> IMessageBus.Query<TResult>(string destination, object query)
        {
            return ((IMessageBus)this).Query<TResult>(destination, query.GetType(), query);
        }

        public Task<TResult> Query<TResult>(string destination, Type query, object message)
        {
            RecordedQueries.Add(new RecordedQuery(destination, query, message, typeof(TResult)));

            var route = routes.Find(x => x.Match(destination, message));
            return route == null
                    ? Task.FromResult(default(TResult))
                    : Task.FromResult((TResult)route.Apply(message));
        }

        class Route
        {
            readonly List<IExpectation> expectations = new List<IExpectation>();

            readonly string destination;
            
            public Route(string destination)
            {
                this.destination = destination;
            }

            public bool Match(string destination)
            {
                return this.destination == destination;
            }

            public bool Match(string destination, object message)
            {
                return Match(destination) && Match(message);
            }

            bool Match(object message)
            {
                return expectations.Any(expectation => expectation.Match(message));
            }

            public void Add(IExpectation expectation)
            {
                expectations.Add(expectation);
            }

            public object Apply(object message)
            {
                return expectations.First(x => x.Match(message)).Apply();
            }
        }
    }

    public class RecordedCommand
    {
        public readonly string Destination;
        public readonly Type Command;
        public readonly object Message;

        public RecordedCommand(string destination, Type command, object message)
        {
            Destination = destination;
            Command = command;
            Message = message;
        }
    }

    public class RecordedCommandCollection : IEnumerable<RecordedCommand>
    {
        readonly List<RecordedCommand> commands = new List<RecordedCommand>();

        internal void Add(RecordedCommand command)
        {
            commands.Add(command);
        }

        public IEnumerator<RecordedCommand> GetEnumerator()
        {
            return commands.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerable<object> this[string destination]
        {
            get { return commands.Where(x => x.Destination == destination).Select(x => x.Message); }
        }

        public void Clear()
        {
            commands.Clear();
        }
    }

    public class RecordedQuery
    {
        public readonly string Destination;
        public readonly Type Query;
        public readonly object Message;
        public readonly Type Result;

        public RecordedQuery(string destination, Type query, object message, Type result)
        {
            Destination = destination;
            Query = query;
            Message = message;
            Result = result;
        }
    }

    public class RecordedQueryCollection : IEnumerable<RecordedQuery>
    {
        readonly List<RecordedQuery> queries = new List<RecordedQuery>();

        internal void Add(RecordedQuery query)
        {
            queries.Add(query);
        }

        public IEnumerator<RecordedQuery> GetEnumerator()
        {
            return queries.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerable<object> this[string destination]
        {
            get { return queries.Where(x => x.Destination == destination).Select(x => x.Message); }
        }

        public void Clear()
        {
            queries.Clear();
        }
    }
}
