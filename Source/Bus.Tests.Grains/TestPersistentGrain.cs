using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    [StorageProvider(ProviderName = "TestPersistentGrainStorageProvider")]
    public class TestPersistentGrain : MessageBasedGrain<TestPersistentState>, ITestPersistentGrain
    {
        Task ITestPersistentGrain.HandleCommand(object cmd)
        {
            return this.Handle((dynamic) cmd);
        }

        async Task<object> ITestPersistentGrain.AnswerQuery(object query)
        {
            return await this.Answer((dynamic) query);
        }

        public Task Handle(SetValue cmd)
        {
            State.Total = cmd.Value;
            return Storage.WriteStateAsync();
        }

        public async Task Handle(ClearValue cmd)
        {
            await Storage.ClearStateAsync();
            State.Total = -1;
        }

        public Task<int> Answer(GetValue query)
        {
            return Task.FromResult(State.Total);
        }
    }

    public class TestPersistentState
    {
        public int Total { get; set; }
    }
}
