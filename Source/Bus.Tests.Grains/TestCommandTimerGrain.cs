using System;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    public class TestCommandTimerGrain : MessageBasedGrain, ITestCommandTimerGrain
    {
        ITimerCollection timers;
        string text = "NONE";

        public override Task ActivateAsync()
        {
            timers = new TimerCollection(this, MessageBus.Instance);
            return TaskDone.Done;
        }

        public Task HandleCommand(object cmd)
        {
            this.Handle((dynamic)cmd);
            return TaskDone.Done;
        }

        public void Handle(RegisterTimer cmd)
        {
            timers.Register(TimeSpan.Zero, TimeSpan.FromSeconds(0.5), 
                new SetTextByTimer(cmd.Text)
            );
        }        
        
        public void Handle(UnregisterTimer cmd)
        {
            timers.Unregister<SetTextByTimer>();
        }

        public void Handle(SetTextByTimer cmd)
        {
            text = cmd.Text;
        }

        public Task<object> AnswerQuery(object query)
        {
            return Task.FromResult((object)text);
        }
    }
}