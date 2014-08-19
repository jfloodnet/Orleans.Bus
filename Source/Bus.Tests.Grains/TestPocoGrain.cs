﻿using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    public class TestPocoGrain : PocoGrain, ITestPocoGrain
    {
        TestPoco poco;

        public TestPocoGrain()
        {
            OnActivate =()=>
            {
                poco = new TestPoco(Id(), Notify);
                return poco.Activate();
            };

            OnCommand = cmd => poco.Handle((dynamic)cmd);
            OnQuery = async query => await poco.Answer((dynamic)query);
        }
    }

    public class TestPoco
    {
        readonly string id;
        readonly Action<Event> notify;

        string fooText = "";
        string barText = "";

        public TestPoco(string id, Action<Event> notify)
        {
            this.id = id;
            this.notify = notify;
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

        public Task Handle(PublishText cmd)
        {
            notify(new TextPublished(cmd.Text));

            return TaskDone.Done;
        }
    }
}