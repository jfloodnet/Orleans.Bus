using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    namespace Persistence.GenericState
    {
        [StorageProvider(ProviderName = "GenericStateStorageProvider")]
        public class TestGenericStatePersistentGrain : MessageBasedGrain<int>, ITestGenericStatePersistentGrain
        {
            Task ITestGenericStatePersistentGrain.HandleCommand(object cmd)
            {
                return this.Handle((dynamic) cmd);
            }

            async Task<object> ITestGenericStatePersistentGrain.AnswerQuery(object query)
            {
                return await this.Answer((dynamic) query);
            }

            public Task Handle(SetValue cmd)
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

    namespace Persistence.ExplicitStatePassing
    {
        [StorageProvider(ProviderName = "ExplicitStatePassingStorageProvider")]
        public class TestExplicitStatePassingPersistentGrain : MessageBasedGrain<int, int, int>, ITestExplicitStatePassingPersistentGrain
        {
            Task ITestExplicitStatePassingPersistentGrain.HandleCommand(object cmd)
            {
                return this.Handle((dynamic) cmd);
            }

            async Task<object> ITestExplicitStatePassingPersistentGrain.AnswerQuery(object query)
            {
                return await this.Answer((dynamic) query);
            }

            public Task Handle(SetValue cmd)
            {
                return Storage.WriteStateAsync(cmd.Value);
            }

            public async Task Handle(ClearValue cmd)
            {
                await Storage.ClearStateAsync(-1);
            }

            public Task<int> Answer(GetValue query)
            {
                return Storage.ReadStateAsync();
            }
        }
    }
}
