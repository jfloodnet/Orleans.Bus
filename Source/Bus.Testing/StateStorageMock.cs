using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    public class StateStorageMock : IStateStorage
    {
        public readonly List<RecordedStorageOperation> Recorded =
                    new List<RecordedStorageOperation>();

        public Task ReadStateAsync()
        {
            Recorded.Add(new RecordedStorageOperation { IsReadState = true });
            return TaskDone.Done;
        }

        public Task WriteStateAsync()
        {
            Recorded.Add(new RecordedStorageOperation { IsWriteState = true });
            return TaskDone.Done;
        }

        public Task ClearStateAsync()
        {
            Recorded.Add(new RecordedStorageOperation { IsClearState = true });
            return TaskDone.Done;
        }

        public void Reset()
        {
            Recorded.Clear();
        }
    }

    public class RecordedStorageOperation
    {
        public bool IsReadState     { get; internal set; }
        public bool IsWriteState    { get; internal set; }
        public bool IsClearState    { get; internal set; }
    }
}
