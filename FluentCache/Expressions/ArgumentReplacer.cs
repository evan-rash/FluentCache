using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace FluentCache.Expressions
{
    internal static class ArgumentReplacer
    {
        public static Expression<Func<T, TArgument, TResult>> ReplaceFirstArgumentInLambdaWithParameter<T, TArgument, TResult>(Expression<Func<T, TResult>> lambda, string paramName)
        {
            MethodCallExpression method = lambda.Body as MethodCallExpression;

            if (method.Arguments.Count < 1)
                throw new InvalidCachingExpressionException(String.Format("Method '{0}' must have at least one argument", lambda.ToString()));

            TypeInfo parameterType = method.Method.GetParameters().First().ParameterType.GetTypeInfo();
            if (!parameterType.IsAssignableFrom(typeof(TArgument).GetTypeInfo()))
                throw new InvalidCachingExpressionException(String.Format("Method '{0}' must have first argument of type '{1}'", lambda.ToString(), typeof(TArgument).FullName));

            ParameterExpression instance = lambda.Parameters.First();
            
            ParameterExpression newParameter = Expression.Parameter(typeof(TArgument), paramName);
            
            List<Expression> parameters = method.Arguments.Skip(1).ToList();
            parameters.Insert(0, newParameter);

            MethodCallExpression newMethod = Expression.Call(instance, method.Method, parameters);

            Expression<Func<T, TArgument, TResult>> newExpression = Expression.Lambda<Func<T, TArgument, TResult>>(newMethod, instance, newParameter);

            return newExpression;
        }
    }
}
