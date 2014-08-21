using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    public class StorageProviderProxyMock : IStorageProviderProxy
    {
        public readonly List<RecordedStorageProviderOperation> Recorded = 
                    new List<RecordedStorageProviderOperation>();

        public Task ReadStateAsync()
        {
            Recorded.Add(new RecordedStorageProviderOperation { IsReadState = true });
            return TaskDone.Done;
        }

        public Task WriteStateAsync()
        {
            Recorded.Add(new RecordedStorageProviderOperation { IsWriteState = true });
            return TaskDone.Done;
        }

        public Task ClearStateAsync()
        {
            Recorded.Add(new RecordedStorageProviderOperation { IsClearState = true });
            return TaskDone.Done;
        }

        public void Reset()
        {
            Recorded.Clear();
        }
    }

    public class RecordedStorageProviderOperation
    {
        public bool IsReadState     { get; internal set; }
        public bool IsWriteState    { get; internal set; }
        public bool IsClearState    { get; internal set; }
    }

    public class StorageProviderProxyMock<TReadStateResult, TWriteStateArgument, TClearStateArgument> 
        : IStorageProviderProxy<TReadStateResult, TWriteStateArgument, TClearStateArgument>
    {
        public readonly List<RecordedExplicitStorageProviderOperation> Recorded =
                    new List<RecordedExplicitStorageProviderOperation>();

        public TReadStateResult ReadStateResult;

        public Task<TReadStateResult> ReadStateAsync()
        {
            return Task.FromResult(ReadStateResult);
        }

        public Task WriteStateAsync(TWriteStateArgument arg)
        {
            Recorded.Add(new RecordedExplicitStorageProviderOperation
            {
                IsWriteState = true, 
                OperationArgument = arg
            });
            return TaskDone.Done;
        }

        public Task ClearStateAsync(TClearStateArgument arg)
        {
            Recorded.Add(new RecordedExplicitStorageProviderOperation
            {
                IsClearState = true,
                OperationArgument = arg
            });
            return TaskDone.Done;
        }

        public void Reset()
        {
            Recorded.Clear();
            ReadStateResult = default(TReadStateResult);
        }
    }

    public class RecordedExplicitStorageProviderOperation
    {
        public bool IsWriteState { get; internal set; }
        public bool IsClearState { get; internal set; }

        internal object OperationArgument { get; set; }

        public T Argument<T>()
        {
            return (T) OperationArgument;
        }
    }
}
