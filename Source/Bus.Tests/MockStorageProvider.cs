using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    public class MockStorageProvider : StorageProvider<TestPersistentState>
    {
        public MockStorageProvider()
        {
            Reset();
        }

        public static string ReadStateGrainId
        {
            get { return GetEnv("ReadStateGrainId"); }
            set { SetEnv("ReadStateGrainId", value); }
        }

        public static string ReadStateGrainType
        {
            get { return GetEnv("ReadStateGrainType"); }
            set { SetEnv("ReadStateGrainType", value); }
        }

        public static int ReadStateReturnValue
        {
            get { return int.Parse(GetEnv("ReadStateReturnValue")); }
            set { SetEnv("ReadStateReturnValue", value.ToString()); }
        }

        public static string WriteStateGrainId
        {
            get { return GetEnv("WriteStateGrainId"); }
            set { SetEnv("WriteStateGrainId", value); }
        }

        public static string WriteStateGrainType
        {
            get { return GetEnv("WriteStateGrainType"); }
            set { SetEnv("WriteStateGrainType", value); }
        }

        public static int WriteStatePassedValue
        {
            get { return int.Parse(GetEnv("WriteStatePassedValue")); }
            set { SetEnv("WriteStatePassedValue", value.ToString()); }
        }

        public static string ClearStateGrainId
        {
            get { return GetEnv("ClearStateGrainId"); }
            set { SetEnv("ClearStateGrainId", value); }
        }

        public static string ClearStateGrainType
        {
            get { return GetEnv("ClearStateGrainType"); }
            set { SetEnv("ClearStateGrainType", value); }
        }

        public static int ClearStatePassedValue
        {
            get { return int.Parse(GetEnv("ClearStatePassedValue")); }
            set { SetEnv("ClearStatePassedValue", value.ToString()); }
        }

        public static int DefaultValue
        {
            get { return int.Parse(GetEnv("DefaultValue")); }
            set { SetEnv("DefaultValue", value.ToString()); }
        }

        static string GetEnv(string name)
        {
            return Environment.GetEnvironmentVariable("GenericState." + name);
        }

        static void SetEnv(string name, string value)
        {
            Environment.SetEnvironmentVariable("GenericState." + name, value);
        }

        public static void Reset()
        {
            ReadStateReturnValue =
                WriteStatePassedValue =
                ClearStatePassedValue = -1;

            ReadStateGrainId =
                WriteStateGrainId =
                ClearStateGrainId = "";

            ReadStateGrainType =
                WriteStateGrainType =
                ClearStateGrainType = "";
        }

        public override Task Init(IDictionary<string, string> properties)
        {
            DefaultValue = int.Parse(properties["DefaultValue"]);
            ReadStateReturnValue = DefaultValue;
            return TaskDone.Done;
        }

        public override Task ReadStateAsync(string id, GrainType type, TestPersistentState state)
        {
            ReadStateGrainId = id;
            ReadStateGrainType = type.FullName;
            state.Total = ReadStateReturnValue;
            return TaskDone.Done;
        }

        public override Task WriteStateAsync(string id, GrainType type, TestPersistentState state)
        {
            WriteStateGrainId = id;
            WriteStateGrainType = type.FullName;
            WriteStatePassedValue = state.Total;
            return TaskDone.Done;
        }

        public override Task ClearStateAsync(string id, GrainType type, TestPersistentState state)
        {
            ClearStateGrainId = id;
            ClearStateGrainType = type.FullName;
            ClearStatePassedValue = state.Total;
            return TaskDone.Done;
        }
    }
}