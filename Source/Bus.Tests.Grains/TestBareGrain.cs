using System.Threading.Tasks;

namespace Orleans.Bus
{
    public class TestBareGrain : MessageBasedGrain, ITestBareGrain
    {
        public Task HandleCommand(object cmd)
        {
            return TaskDone.Done;
        }
    }
}