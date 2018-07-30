using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentCache.Strategies;

namespace FluentCache.Expressions
{
    internal class ExpressionAnalyzer
    {

        private List<object> GetMethodParameters<T>(Cache<T> cache, MethodCallExpression methodCallExpression)
        {
            //Get the parameters
            List<object> parameters = new List<object>();
            foreach (Expression parameterExpression in methodCallExpression.Arguments)
            {
                if (!TryProcessParameter(cache.Source, parameterExpression, checkForParameterDoNotCache: true, parameterValue: out object parameter))
                    continue;

                parameters.Add(parameter);
            }

            return parameters;
        }

        public CacheStrategyIncomplete CreateCacheStrategyFromMethod<T>(Cache<T> cache, MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression == null)
                throw new ArgumentNullException("methodCallExpression");

            //Get the region from the cache source
            string region = cache.Source.GetType().Name;

            //Get the operation name
            string baseKey = methodCallExpression.Method.Name;

            //Get the method parameters
            List<object> parameters = GetMethodParameters(cache, methodCallExpression);

            return cache.ThisMethod(baseKey)
                        .WithRegion(region)
                        .WithParameters(parameters.ToArray());
        }

        public CacheStrategyIncomplete CreateCacheStrategyFromMember<T>(Cache<T> cache, MemberExpression memberExpression)
        {
            if (memberExpression == null)
                throw new ArgumentNullException("memberExpression");

            //check to be sure that the member expression is not nested
            //if this is a nested property, then memberExpression.Expression will not be a TypedParameterExpression
            if (memberExpression.Expression.NodeType != ExpressionType.Parameter)
                throw NestedMemberExpression(memberExpression);


            //Get the region from the cache source
            string region = cache.Source.GetType().Name;

            //Get the member name
            string member = memberExpression.Member.Name;

            return cache.WithKey(member)
                        .WithRegion(region);
        }

        public MethodCacheStrategy<TResult> CreateCacheStrategy<T, TResult>(Cache<T> cache, Expression<Func<T, TResult>> method)
        {
            Func<T, TResult> compiledMethod = method.Compile();
            Func<TResult> retrieve = () =>
                {
                    return compiledMethod(cache.Source);
                };

            CacheStrategyIncomplete incompleteStrat = null;

            if (method.Body is MethodCallExpression methodCallExpression)
                incompleteStrat = CreateCacheStrategyFromMethod(cache, methodCallExpression);

            if (method.Body is MemberExpression memberExpression)
                incompleteStrat = CreateCacheStrategyFromMember(cache, memberExpression);

            if (incompleteStrat == null)
                throw UnsupportedExpression(method.Body);

            MethodCacheStrategy<TResult> methodStrat = new MethodCacheStrategy<TResult>(incompleteStrat.Cache, retrieve, incompleteStrat.BaseKey);
            methodStrat.CopyFrom(incompleteStrat);

            return methodStrat;
        }

        public AsyncMethodCacheStrategy<TResult> CreateAsyncCacheStrategy<T, TResult>(Cache<T> cache, Expression<Func<T, Task<TResult>>> method)
        {
            Func<T, Task<TResult>> compiledRetrieve = method.Compile();
            Func<Task<TResult>> retrieve = () => compiledRetrieve(cache.Source);

            CacheStrategyIncomplete incompleteStrat = null;

            if (method.Body is MethodCallExpression methodCallExpression)
                incompleteStrat = CreateCacheStrategyFromMethod(cache, methodCallExpression);

            //Note: we purposely do not support MemberExpressions here because a task-returning property would be a very strange design...

            if (incompleteStrat == null)
                throw UnsupportedExpression(method.Body);

            AsyncMethodCacheStrategy<TResult> methodStrat = new AsyncMethodCacheStrategy<TResult>(incompleteStrat.Cache, retrieve, incompleteStrat.BaseKey);
            methodStrat.CopyFrom(incompleteStrat);

            return methodStrat;
        }

        public bool TryProcessParameter<T>(T source, Expression parameterExpression, bool checkForParameterDoNotCache, out object parameterValue)
        {
            parameterValue = null;
            if (parameterExpression is ConstantExpression constantExpression)
            {
                parameterValue = constantExpression.Value;
                return true;
            }
            else if (parameterExpression is MethodCallExpression parameterCallExpression)
            {
                //Check for Parameter.DoNotCache()
                //Ignore this parameter
                if (checkForParameterDoNotCache && parameterCallExpression.Method.DeclaringType == typeof(Parameter)
                    && parameterCallExpression.Method.Name == "DoNotCache")
                {
                    return false;
                }
                else
                {
                    parameterValue = EvaluateParameterExpression(parameterExpression);
                    return true;
                }
            }
            else
            {
                parameterValue = EvaluateParameterExpression(parameterExpression);
                return true;
            }
        }
        private static object EvaluateParameterExpression(Expression parameterExpression)
        {
            return Expression.Lambda(parameterExpression).Compile().DynamicInvoke();
        }

        private BulkCacheStrategyIncomplete<TKey, TResult> CreateBulkCacheStrategyFromMethod<T, TKey, TResult>(Cache<T> cache, MethodCallExpression methodCallExpression)
        {
            //Get the region from the calling type
            string region = methodCallExpression.Method.DeclaringType.Name;

            //Get the operation name
            string baseKey = methodCallExpression.Method.Name;

            if (methodCallExpression.Arguments.Count < 1)
                throw UnableToProcessBulkMethod<TKey>(methodCallExpression);

            Expression parameterExpression = methodCallExpression.Arguments.First();
            if (!TryProcessParameter(cache.Source, parameterExpression, checkForParameterDoNotCache: false, parameterValue: out object parameterValue))
                throw UnableToProcessBulkMethod<TKey>(methodCallExpression);

            ICollection<TKey> keys = parameterValue as ICollection<TKey>;
            if (keys == null)
                throw UnableToProcessBulkMethod<TKey>(methodCallExpression);

            List<object> otherParameters = new List<object>();
            foreach (Expression otherParameter in methodCallExpression.Arguments.Skip(1))
            {
                if (TryProcessParameter(cache.Source, otherParameter, checkForParameterDoNotCache: true, parameterValue: out object otherParameterValue))
                    otherParameters.Add(otherParameterValue);
            }

            return new BulkCacheStrategyIncomplete<TKey, TResult>(cache, baseKey, keys)
                            .WithRegion(region)
                            .WithParameters(otherParameters.ToArray());
        }

        public BulkMethodCacheStrategy<TKey, TResult> CreateBulkCacheStrategy<T, TKey, TResult>(Cache<T> cache, Expression<Func<T, ICollection<KeyValuePair<TKey, TResult>>>> method)
        {
            var methodCallExpression = method.Body as MethodCallExpression;
            if (methodCallExpression == null)
                throw UnsupportedExpression(method.Body);

            BulkCacheStrategyIncomplete<TKey, TResult> incompleteStrat = CreateBulkCacheStrategyFromMethod<T, TKey, TResult>(cache, methodCallExpression);

            //Take the given expression (t => t.MyMethod(args)) and translate it into (t, keys) => t.MyMethod(keys)
            Expression<Func<T, ICollection<TKey>, ICollection<KeyValuePair<TKey, TResult>>>> expandedExpression = ArgumentReplacer.ReplaceFirstArgumentInLambdaWithParameter<T, ICollection<TKey>, ICollection<KeyValuePair<TKey, TResult>>>(method, "keys");
            Func<T, ICollection<TKey>, ICollection<KeyValuePair<TKey, TResult>>> expandedFunction = expandedExpression.Compile();

            //Curry the expandedFunction down to a function we can use to retrieve the value via cache.Source
            Func<ICollection<TKey>, ICollection<KeyValuePair<TKey, TResult>>> retrieve = keys => expandedFunction(cache.Source, keys);

            BulkMethodCacheStrategy<TKey, TResult> strat = new BulkMethodCacheStrategy<TKey, TResult>(cache, retrieve, incompleteStrat.BaseKey, incompleteStrat.Keys);
            strat.CopyFrom(incompleteStrat);

            return strat;
        }

        public AsyncBulkMethodCacheStrategy<TKey, TResult> CreateAsyncBulkCacheStrategy<T, TKey, TResult>(Cache<T> cache, Expression<Func<T, Task<ICollection<KeyValuePair<TKey, TResult>>>>> method)
        {
            var methodCallExpression = method.Body as MethodCallExpression;
            if (methodCallExpression == null)
                throw UnsupportedExpression(method.Body);

            BulkCacheStrategyIncomplete<TKey, TResult> incompleteStrat = CreateBulkCacheStrategyFromMethod<T, TKey, TResult>(cache, methodCallExpression);

            //Take the given expression (t => t.MyMethod(args)) and translate it into (t, keys) => t.MyMethod(keys)
            Expression<Func<T, ICollection<TKey>, Task<ICollection<KeyValuePair<TKey, TResult>>>>> expandedExpression = ArgumentReplacer.ReplaceFirstArgumentInLambdaWithParameter<T, ICollection<TKey>, Task<ICollection<KeyValuePair<TKey, TResult>>>>(method, "keys");
            Func<T, ICollection<TKey>, Task<ICollection<KeyValuePair<TKey, TResult>>>> expandedFunction = expandedExpression.Compile();

            //Curry the expandedFunction down to a function we can use to retrieve the value via cache.Source
            Func<ICollection<TKey>, Task<ICollection<KeyValuePair<TKey, TResult>>>> retrieve = keys => expandedFunction(cache.Source, keys);

            AsyncBulkMethodCacheStrategy<TKey, TResult> strat = new AsyncBulkMethodCacheStrategy<TKey, TResult>(cache, retrieve, incompleteStrat.BaseKey, incompleteStrat.Keys);
            strat.CopyFrom(incompleteStrat);

            return strat;
        }

        private FluentCacheException UnsupportedExpression(Expression expression)
        {
            string message = String.Format("Unsupported expression type '{0}'. May only cache expressions of type MethodCallExpression", expression.GetType().Name);
            return new InvalidCachingExpressionException(message);
        }
        private FluentCacheException UnableToProcessBulkMethod<TKey>(MethodCallExpression methodCallExpression)
        {
            return new InvalidCachingExpressionException(String.Format("Unable to parse method '{0}'. Unable to determine the keys parameter. The method must accept at least one parameter, and the first parameter must be ICollection<{1}>", methodCallExpression, typeof(TKey).Name));
        }
        private FluentCacheException NestedMemberExpression(MemberExpression expression)
        {
            string message = $"Unsupported expression '{expression.ToString()}'. Caching of nested members is not supported";
            return new InvalidCachingExpressionException(message);
        }
    }
}
