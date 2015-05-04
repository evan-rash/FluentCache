using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using FluentCache.Expressions;

namespace FluentCache.Test
{
    [TestClass]
    public class ParameterReplacerTests
    {
        [TestMethod]
        public void TestReplaceArgumentWithParameter()
        {
            List<int> values = new List<int> { 4, 5, 6 };

            Expression<Func<ParameterReplacerTests, int>> expression = t => t.Sum(values);

            Expression<Func<ParameterReplacerTests, IList<int>, int>> newExpression = ArgumentReplacer.ReplaceFirstArgumentInLambdaWithParameter<ParameterReplacerTests, IList<int>, int>(expression, "nums");

            Func<ParameterReplacerTests, IList<int>, int> newFunc = newExpression.Compile();

            Assert.AreEqual(Sum(values), newFunc(this, values));
        }

        private int Sum(IList<int> inputs)
        {
            return inputs.Sum();
        }
    }
}
