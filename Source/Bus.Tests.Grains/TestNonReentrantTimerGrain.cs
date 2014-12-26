using System;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    public class TestNonReentrantTimerGrain : MessageBasedGrain, ITestNonReentrantTimerGrain
    {
        ITimerCollection timers;
        string text = "NONE";

        public override Task ActivateAsync()
        {
            timers = new TimerCollection(this);
            return TaskDone.Done;
        }

        public override Task OnTimer(string id, object state)
        {
            text = (string) state;
            return TaskDone.Done;
        }

        public Task HandleCommand(object cmd)
        {
            this.Handle((dynamic)cmd);
            return TaskDone.Done;
        }

        public void Handle(RegisterTimer cmd)
        {
            timers.Register("change-text", TimeSpan.Zero, TimeSpan.FromSeconds(0.5), cmd.Text);
        }        
        
        public void Handle(UnregisterTimer cmd)
        {
            timers.Unregister("change-text");
        }

        public Task<object> AnswerQuery(object query)
        {
            return Task.FromResult((object)text);
        }
    }
}