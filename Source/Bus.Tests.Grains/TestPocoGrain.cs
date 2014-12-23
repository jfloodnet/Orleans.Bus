using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    public class TestPocoGrain : MessageBasedGrain, ITestPocoGrain
    {
        TestPoco poco;

        public override Task ActivateAsync()
        {
            poco = new TestPoco(Identity.Of(this), Notify);
            return poco.Activate();
        }

        public Task OnCommand(object cmd)
        {
            return poco.Handle((dynamic)cmd);
        }

        public async Task<object> OnQuery(object query)
        {
            return await poco.Answer((dynamic) query);
        }
    }

    public class TestPoco
    {
        readonly string id;
        readonly Action<Event> notify;

        string fooText = "";
        string barText = "";

        public TestPoco(string id, Action<Notification[]> notify)
        {
            this.id = id;
            this.notify = x => notify(new[]{new Notification(x.GetType(), x)});
        }

        public Task Activate()
        {
            return TaskDone.Done;
        }

        public Task Handle(DoFoo cmd)
        {
            Console.WriteLine(id + " is executing " + cmd.Text);
            fooText = cmd.Text;

            return TaskDone.Done;
        }

        public Task Handle(DoBar cmd)
        {
            Console.WriteLine(id + " is executing " + cmd.Text);
            barText = cmd.Text;

            return TaskDone.Done;
        }        
        
        public Task Handle(ThrowException cmd)
        {
            throw new ApplicationException("Test exception unwrapping");
        }

        public Task<string> Answer(GetFoo query)
        {
            return Task.FromResult(fooText + "-" + id);
        }

        public Task<string> Answer(GetBar query)
        {
            return Task.FromResult(barText + "-" + id);
        }

        public Task Handle(PublishFoo cmd)
        {
            notify(new FooPublished(cmd.Foo));

            return TaskDone.Done;
        }        
        
        public Task Handle(PublishBar cmd)
        {
            notify(new BarPublished(cmd.Bar));

            return TaskDone.Done;
        }
    }
}
