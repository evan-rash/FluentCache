using FluentCache.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache.Test
{
    [TestClass]
    public class ClosureTest
    {
        [TestMethod]
        public void Test()
        {
            Func<int, int> myAction = i => i + 1;
            int localVariable = 10;

            int analyzed = (int)GetFirstParameterValue(this, () => myAction(localVariable));

            Assert.AreEqual(localVariable, analyzed);
        }

        public object GetFirstParameterValue<T>(T source, Expression<Action> expression)
        {
            var invocationCall = expression.Body as InvocationExpression;

            ExpressionAnalyzer analyzer = new ExpressionAnalyzer();

            bool dontIgnore = analyzer.TryProcessParameter(source, invocationCall.Arguments.First(), checkForParameterDoNotCache: true, parameterValue: out object parameterValue);
            if (!dontIgnore)
                Assert.Fail("Should not have been ignore");

            return parameterValue;
        }
    }
}
