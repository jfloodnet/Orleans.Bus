using System;
using System.Linq;

namespace Sample
{
    public abstract class Command
    {}

    public abstract class Query
    {}    
    
    public abstract class Query<TResult> : Query
    {}

    public abstract class Event
    {}
}
