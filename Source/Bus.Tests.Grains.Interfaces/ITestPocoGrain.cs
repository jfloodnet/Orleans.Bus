using System;
using System.Linq;

namespace Orleans.Bus
{
    [Immutable, Serializable]
    public class DoFoo : Command
    {
        public readonly string Text;

        public DoFoo(string text)
        {
            Text = text;
        }
    }

    [Immutable, Serializable]
    public class DoBar : Command
    {
        public readonly string Text;

        public DoBar(string text)
        {
            Text = text;
        }
    }

    [Immutable, Serializable]
    public class ThrowException : Command
    {}

    [Immutable, Serializable]
    public class GetFoo : Query<string>
    {}

    [Immutable, Serializable]
    public class GetBar : Query<string>
    {}

    [Immutable, Serializable]
    public class PublishFoo : Command
    {
        public readonly string Foo;

        public PublishFoo(string foo)
        {
            Foo = foo;
        }
    }

    [Serializable]
    public class FooPublished : Event
    {
        public readonly string Foo;

        public FooPublished(string foo)
        {
            Foo = foo;
        }
    }

    [Immutable, Serializable]
    public class PublishBar : Command
    {
        public readonly string Bar;

        public PublishBar(string bar)
        {
            Bar = bar;
        }
    }

    [Serializable]
    public class BarPublished : Event
    {
        public readonly string Bar;

        public BarPublished(string bar)
        {
            Bar = bar;
        }
    }

    [Handles(typeof(DoFoo))]
    [Handles(typeof(DoBar))]
    [Handles(typeof(ThrowException))]
    [Answers(typeof(GetFoo))]
    [Answers(typeof(GetBar))]
    [Handles(typeof(PublishFoo))]
    [Handles(typeof(PublishBar))]
    [Notifies(typeof(FooPublished))]
    [Notifies(typeof(BarPublished))]
    [ExtendedPrimaryKey]
    public interface ITestPocoGrain : IPocoGrain
    {}
}