using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    namespace Persistence.GenericState
    {
        [Serializable, Immutable]
        public class SetValue : Command
        {
            public int Value;
        }

        [Serializable, Immutable]
        public class ClearValue : Command
        {}

        [Serializable, Immutable]
        public class GetValue : Query<string>
        {}

        [Handles(typeof(SetValue))]
        [Handles(typeof(ClearValue))]
        [Answers(typeof(GetValue))]
        [ExtendedPrimaryKey]
        public interface ITestGenericStatePersistentGrain : IMessageBasedGrain
        {
            [Handler] Task HandleCommand(object cmd);
            [Handler] Task<object> AnswerQuery(object query);
        }
    }    
    
    namespace Persistence.ExplicitStatePassing
    {
        [Serializable, Immutable]
        public class SetValue : Command
        {
            public int Value;
        }

        [Serializable, Immutable]
        public class ClearValue : Command
        {}

        [Serializable, Immutable]
        public class GetValue : Query<string>
        {}

        [Handles(typeof(SetValue))]
        [Handles(typeof(ClearValue))]
        [Answers(typeof(GetValue))]
        [ExtendedPrimaryKey]
        public interface ITestExplicitStatePassingPersistentGrain : IMessageBasedGrain
        {
            [Handler] Task HandleCommand(object cmd);
            [Handler] Task<object> AnswerQuery(object query);
        }
    }
}
