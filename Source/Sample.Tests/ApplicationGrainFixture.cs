using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using PowerAssert;
using NUnit.Framework;

using Orleans.Bus;

namespace Sample
{
    public abstract class ApplicationGrainFixture
    {
        protected MessageBusMock Bus;
        protected TimerCollectionMock Timers;
        protected ReminderCollectionMock Reminders;

        [SetUp]
        public virtual void SetUp()
        {
            Bus = new MessageBusMock();
            Timers = new TimerCollectionMock();
            Reminders = new ReminderCollectionMock();
        }

        protected static void IsFalse([InstantHandle] Expression<Func<bool>> expression, string message = null)
        {
            var negated = Expression.Lambda<Func<bool>>(
                Expression.Not(expression.Body), 
                expression.Parameters);

            try
            {
                PAssert.IsTrue(negated);
            }
            catch (Exception e)
            {
                var expressionTrace = RemoveHeadline(e.Message);

                if (message != null)
                    Assert.Fail(message + Environment.NewLine + expressionTrace);

                Assert.Fail(expressionTrace);
            }
        }

        protected static void IsTrue([InstantHandle] Expression<Func<bool>> expression, string message = null)
        {
            try
            {
                PAssert.IsTrue(expression);
            }
            catch (Exception e)
            {
                var expressionTrace = RemoveHeadline(e.Message);

                if (message != null)
                    Assert.Fail(message + Environment.NewLine + expressionTrace);

                Assert.Fail(expressionTrace);
            }
        }

        static string RemoveHeadline(string error)
        {
            var lines = error.Split(new[] {"\n"}, StringSplitOptions.None).ToList();
            lines[0] = "";
            return string.Join("\n", lines);
        }

        protected void Throws<TException>([InstantHandle] Func<Task> action, string message = null) where TException : Exception
        {
            Assert.Throws<TException>(async ()=> await action(), message);
        }

        protected static QueryExpectation<TQuery> Query<TQuery>(Expression<Func<TQuery, bool>> expression = null)
        {
            return new QueryExpectation<TQuery>(expression);
        }

        protected static CommandExpectation<TCommand> Command<TCommand>(Expression<Func<TCommand, bool>> expression = null)
        {
            return new CommandExpectation<TCommand>(expression);
        }

        protected void Expect(string destination, IExpectation expectation)
        {
            Bus.Expect(destination, expectation);
        }

        protected RecordedTimer<TState> Timer<TState>(string id)
        {
            return (RecordedTimer<TState>) Timers[id];
        }

        protected RecordedReminder Reminder(string api)
        {
            return Reminders[api];
        }

        public DestinationAssertion That(string destination)
        {
            return new DestinationAssertion(destination, Bus);
        }

        protected void Reset()
        {
            Bus.RecordedQueries.Clear();
        }

        public class DestinationAssertion
        {
            readonly string destination;
            readonly MessageBusMock bus;

            public DestinationAssertion(string destination, MessageBusMock bus)
            {
                this.destination = destination;
                this.bus = bus;
            }

            public IEnumerable<object> Queries()
            {
                return bus.RecordedQueries[destination];
            }

            public bool DidNotReceiveAnyQueries()
            {
                return !bus.RecordedQueries[destination].Any();
            }

            public TQuery FirstQuery<TQuery>()
            {
                return bus.RecordedQueries[destination].OfType<TQuery>().FirstOrDefault();
            }
        }
    }
}
