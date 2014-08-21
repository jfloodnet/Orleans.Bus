using System;
using System.Linq;

using NUnit.Framework;

namespace Orleans.Bus
{
    [TestFixture]
    public class MessageBusMockFixture
    {
        IMessageBus bus;
        MessageBusMock mock;

        [SetUp]
        public void SetUp()
        {
            bus = mock = new MessageBusMock();
        }

        [Test]
        public async void Returns_default_result_for_unmatched_queries()
        {
            Assert.AreEqual(default(int), await bus.Query<int>("some-id", new TestQuery()));
            Assert.AreEqual(default(object), await bus.Query<object>("some-id", new TestQuery()));
        }

        [Test]
        public void Matches_command_when_destination_and_type_do_match()
        {
            mock.Expect("some-id", 
                new CommandExpectation<TestCommand>(_=> true)
                    .Throw(new ApplicationException("boo!")));

            Assert.Throws<ApplicationException>(
                async ()=> await bus.Send("some-id", new TestCommand()));
        }
        
        [Test]
        public async void Matches_query_when_destination_and_type_do_match()
        {
            mock.Expect("some-id", 
                new QueryExpectation<TestQuery>(_=> true)
                    .Return(111));

            Assert.That(await bus.Query<int>("some-id", new TestQuery()), 
                Is.EqualTo(111));
        }

        [Test]
        public async void Matches_when_expression_match()
        {
            mock.Expect("some-id",
                new QueryExpectation<TestQuery>(x => 
                        x.Field == "foo" && 
                        x.AnotherField == "bar")
                    .Return(111));

            var query = new TestQuery {Field = "foo", AnotherField = "bar"};
            Assert.That(await bus.Query<int>("some-id", query),
                Is.EqualTo(111));
        }

        [Test]
        public async void Does_not_match_when_expression_doesnt()
        {
            mock.Expect("some-id",
                new QueryExpectation<TestQuery>(x => 
                        x.Field == "foo" && 
                        x.AnotherField == "^_^")
                    .Return(111));

            var query = new TestQuery {Field = "foo", AnotherField = "bar"};
            Assert.AreEqual(default(int), await bus.Query<int>("some-id", query));
        }

        [Test]
        public async void Matches_indefinite_number_of_times_by_default()
        {
            mock.Expect("some-id",
                new QueryExpectation<TestQuery>()
                    .Return(111));

            Assert.That(await bus.Query<int>("some-id", new TestQuery()),
                Is.EqualTo(111));

            Assert.That(await bus.Query<int>("some-id", new TestQuery()),
                Is.EqualTo(111));
        }        
        
        [Test]
        public async void Matches_specified_number_of_times_when_configured()
        {
            mock.Expect("some-id",
                new QueryExpectation<TestQuery>()
                    .Return(111).Once());

            Assert.That(await bus.Query<int>("some-id", new TestQuery()),
                Is.EqualTo(111));

            Assert.That(await bus.Query<int>("some-id", new TestQuery()),
                Is.EqualTo(default(int)));
        }

        [Test]
        public async void When_multiple_expectations_match_will_use_the_first_one_in_order()
        {
            mock.Expect("some-id",
                new QueryExpectation<TestQuery>()
                    .Return(111).Once());

            mock.Expect("some-id",
                new QueryExpectation<TestQuery>()
                    .Return(222));

            Assert.That(await bus.Query<int>("some-id", new TestQuery()),
                Is.EqualTo(111));

            Assert.That(await bus.Query<int>("some-id", new TestQuery()),
                Is.EqualTo(222));
        }

        public class TestCommand
        {
            public string Field;
            public string AnotherField;
        }

        public class TestQuery
        {
            public string Field;
            public string AnotherField;
        }
    }
}
