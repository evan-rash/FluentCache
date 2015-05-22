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
        public object GetMemberSource<T>(T parameterSource, MemberExpression memberExpression)
        {
            if (memberExpression.Expression == null)
            {
                //This is a static member
                return null;
            }
            else if (memberExpression.Expression is ConstantExpression)
            {
                var sourceExpression = memberExpression.Expression as ConstantExpression;
                return sourceExpression.Value;
            }
            else if (memberExpression.Expression is ParameterExpression)
            {
                //The member source is a parameter
                //In this case we just return parameterSource
                return parameterSource;
            }

            throw UnableToProcessMemberSource(memberExpression);
        }

        private List<object> GetMethodParameters<T>(Cache<T> cache, MethodCallExpression methodCallExpression)
        {
            //Get the parameters
            List<object> parameters = new List<object>();
            foreach (Expression parameterExpression in methodCallExpression.Arguments)
            {
                object parameter;
                if (!TryProcessParameter(cache.Source, parameterExpression, checkForParameterDoNotCache: true, parameterValue: out parameter))
                    continue;

                parameters.Add(parameter);
            }

            return parameters;
        }

        public CacheStrategyIncomplete CreateCacheStrategyFromMethod<T>(Cache<T> cache, MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression == null)
                throw new ArgumentNullException("methodCallExpression");

            //Get the region from the calling type
            string region = methodCallExpression.Method.DeclaringType.Name;

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

            //Get the region from the calling type
            string region = memberExpression.Member.DeclaringType.Name;

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

            var methodCallExpression = method.Body as MethodCallExpression;
            if (methodCallExpression != null)
                incompleteStrat = CreateCacheStrategyFromMethod(cache, methodCallExpression);

            var memberExpression = method.Body as MemberExpression;
            if (memberExpression != null)
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

            var methodCallExpression = method.Body as MethodCallExpression;
            if (methodCallExpression != null)
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

            if (parameterExpression is MethodCallExpression)
            {
                //Check for Parameter.DoNotCache()
                //Ignore this parameter
                MethodCallExpression parameterCallExpression = (MethodCallExpression)parameterExpression;
                if (checkForParameterDoNotCache && parameterCallExpression.Method.DeclaringType == typeof(Parameter)
                    && parameterCallExpression.Method.Name == "DoNotCache")
                {
                    return false;
                }
                else
                {
                    throw UnableToProcessParameter(parameterCallExpression);
                }
            }
            else if (parameterExpression is ConstantExpression)
            {
                var constantExpression = (ConstantExpression)parameterExpression;
                parameterValue = constantExpression.Value;
                return true;
            }
            else if (parameterExpression is MemberExpression)
            {
                var memberExpression = (MemberExpression)parameterExpression;

                object memberSource = GetMemberSource(source, memberExpression);

                var memberInfo = memberExpression.Member;
                if (memberInfo is System.Reflection.PropertyInfo)
                {
                    var propInfo = (System.Reflection.PropertyInfo)memberInfo;
                    parameterValue = propInfo.GetValue(memberSource);
                    return true;
                }
                else if (memberInfo is System.Reflection.FieldInfo)
                {
                    var fieldInfo = (System.Reflection.FieldInfo)memberInfo;
                    parameterValue = fieldInfo.GetValue(memberSource);
                    return true;
                }
                else
                {
                    throw UnableToProcessParameter(parameterExpression);
                }
            }
            else
            {
                throw UnableToProcessParameter(parameterExpression);
            }
        }

        private BulkCacheStrategyIncomplete<TKey, TResult> CreateBulkCacheStrategyFromMethod<T, TKey, TResult>(Cache<T> cache, MethodCallExpression methodCallExpression)
        {
            //Get the region from the calling type
            string region = methodCallExpression.Method.DeclaringType.Name;

            //Get the operation name
            string baseKey = methodCallExpression.Method.Name;

            if (methodCallExpression.Arguments.Count < 1)
                throw UnableToProcessBulkMethod<TKey>(methodCallExpression);

            object parameterValue;
            Expression parameterExpression = methodCallExpression.Arguments.First();
            if (!TryProcessParameter<T>(cache.Source, parameterExpression, checkForParameterDoNotCache: false, parameterValue: out parameterValue))
                throw UnableToProcessBulkMethod<TKey>(methodCallExpression);

            ICollection<TKey> keys = parameterValue as ICollection<TKey>;
            if (keys == null)
                throw UnableToProcessBulkMethod<TKey>(methodCallExpression);

            List<object> otherParameters = new List<object>();
            foreach (Expression otherParameter in methodCallExpression.Arguments.Skip(1))
            {
                object otherParameterValue;
                if (!TryProcessParameter<T>(cache.Source, otherParameter, checkForParameterDoNotCache: true, parameterValue: out otherParameterValue))
                    throw UnableToProcessParameter(otherParameter);

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

        private FluentCacheException UnableToProcessParameter(Expression parameterExpression)
        {
            return new InvalidCachingExpressionException(String.Format("Unable to parse parameter '{0}'. Only constant expressions, member expressions, and Parameter.DoNotCache() are allowed", parameterExpression.ToString()));
        }
        private FluentCacheException UnableToProcessMethodCallSource(MethodCallExpression methodCallExpression)
        {
            return new InvalidCachingExpressionException(String.Format("Unable to parse method '{0}'. Unable to determine the source value. Methods may only be called on constant expressions", methodCallExpression.ToString()));
        }
        private FluentCacheException UnableToProcessMemberSource(MemberExpression memberExpression)
        {
            return new InvalidCachingExpressionException(String.Format("Unable to parse member '{0}'. Unable to determine the source value. Members may only be called on constant expressions", memberExpression.ToString()));
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
    }
}
