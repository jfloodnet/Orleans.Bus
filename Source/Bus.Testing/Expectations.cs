﻿using System;
using System.Linq;
using System.Linq.Expressions;

namespace Orleans.Bus
{
    public interface IExpectation
    {
        bool Match(object message);
        object Apply();
    }

    public sealed class Expectation<TMessage> : IExpectation
    {
        readonly object result;
        readonly Exception exception;

        readonly Expression<Func<TMessage, bool>> expression;
        int times;

        public Expectation(Expression<Func<TMessage, bool>> expression, object result, Exception exception, int times)
        {
            this.expression = expression;
            this.result = result;
            this.exception = exception;
            this.times = times;
        }

        public bool Match(object message)
        {
            var match = MessageMatches(message) && ExpressionMatches(message);

            if (!match)
                return false;

            return times > 0;
        }

        static bool MessageMatches(object message)
        {
            return message.GetType() == typeof(TMessage);
        }

        bool ExpressionMatches(object query)
        {
            if (expression == null)
                return true;

            var applied = expression.Body.ApplyParameter(query);
            var lambda = Expression.Lambda<Func<bool>>(applied);

            return lambda.Compile()();
        }

        public object Apply()
        {
            times--;

            if (exception != null)
                throw exception;

            return result;
        }
    }

    public interface IRepeatExpectation : IExpectation
    {
        IExpectation Times(int times);
    }

    public sealed class CommandExpectation<TCommand> : IRepeatExpectation
    {
        Expectation<TCommand> expectation;
        readonly Expression<Func<TCommand, bool>> expression;
        Exception exception;

        public CommandExpectation(Expression<Func<TCommand, bool>> expression = null)
        {
            this.expression = expression;
        }

        public IRepeatExpectation Throw(Exception exception)
        {
            this.exception = exception;
            expectation = Create();
            return this;
        }

        IExpectation IRepeatExpectation.Times(int times)
        {
            expectation = Create(times);
            return this;
        }

        Expectation<TCommand> Create(int times = int.MaxValue)
        {
            return new Expectation<TCommand>(expression, null, exception, times);
        }

        bool IExpectation.Match(object message)
        {
            if (expectation == null)
                throw new InvalidOperationException("Expectation is uncomplete. Cofigure the expectation by calling 'Throw()' method");

            return expectation.Match(message);
        }

        object IExpectation.Apply()
        {
            if (expectation == null)
                throw new InvalidOperationException("Expectation is uncomplete. Cofigure the expectation by calling 'Throw()' method");

            return expectation.Apply();
        }
    }

    public sealed class QueryExpectation<TQuery> :  IRepeatExpectation
    {
        Expectation<TQuery> expectation;
        readonly Expression<Func<TQuery, bool>> expression;
        Exception exception;
        object result;

        public QueryExpectation(Expression<Func<TQuery, bool>> expression = null)
        {
            this.expression = expression;
        }

        public IRepeatExpectation Throw(Exception exception)
        {
            this.exception = exception;
            expectation = Create();
            return this;
        }

        public IRepeatExpectation Return<TResult>(TResult result)
        {
            this.result = result;
            expectation = Create();
            return this;
        }
        
        IExpectation IRepeatExpectation.Times(int times)
        {
            expectation = Create(times);
            return this;
        }

        Expectation<TQuery> Create(int times = int.MaxValue)
        {
            return new Expectation<TQuery>(expression, result, exception, times);
        }

        bool IExpectation.Match(object message)
        {
            if (expectation == null)
                throw new InvalidOperationException("Expectation is uncomplete. Cofigure the expectation by calling either 'Throw()' or 'Return()' methods");

            return expectation.Match(message);
        }

        object IExpectation.Apply()
        {
            if (expectation == null)
                throw new InvalidOperationException("Expectation is uncomplete. Cofigure the expectation by calling either 'Throw()' or 'Return()' methods");

            return expectation.Apply();
        }
    }

    public static class ExpectationExtensions
    {
        public static IExpectation Once(this IRepeatExpectation repetition)
        {
            return repetition.Times(1);
        }
    }
}
