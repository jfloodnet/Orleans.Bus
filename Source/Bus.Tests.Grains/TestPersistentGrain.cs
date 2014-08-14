using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    [StorageProvider(ProviderName = "PersistentGrainStorageProvider")]
    public class TestPersistentGrain : MessageBasedGrain<int>, ITestPersistentGrain
    {
        Task ITestPersistentGrain.HandleCommand(object cmd)
        {
            return this.Handle((dynamic)cmd);
        }

        async Task<object> ITestPersistentGrain.AnswerQuery(object query)
        {
            return await this.Answer((dynamic)query);
        }

        public Task Handle(ChangeValue cmd)
        {
            State = cmd.Value;
            return Storage.WriteStateAsync();
        }

        public async Task Handle(ClearValue cmd)
        {
            await Storage.ClearStateAsync();
            State = -1;
        }

        public Task<int> Answer(GetValue query)
        {
            return Task.FromResult(State);
        }
    }
}
