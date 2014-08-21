using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    public abstract class StateHolderStub<TState> : IStateHolder<TState>
    {
        public bool ReadStateWasCalled;
        public bool WriteStateWasCalled;
        public bool ClearStateWasCalled;

        public Task ReadStateAsync()
        {
            ReadStateWasCalled = true;
            return TaskDone.Done;
        }

        public Task WriteStateAsync()
        {
            WriteStateWasCalled = true;
            return TaskDone.Done;
        }

        public Task ClearStateAsync()
        {
            ClearStateWasCalled = true;
            return TaskDone.Done;
        }

        public void Reset()
        {
            ReadStateWasCalled = 
                WriteStateWasCalled = 
                    ClearStateWasCalled = false;
        }

        public string Etag
        {
            get; set;
        }

        public Dictionary<string, object> AsDictionary()
        {
            throw new NotImplementedException();
        }

        public void SetAll(Dictionary<string, object> values)
        {
            throw new NotImplementedException();
        }

        public TState State
        {
            get; set;
        }
    }
}
