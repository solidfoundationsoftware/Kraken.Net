﻿using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Logging;
using CryptoExchange.Net.Objects;
using Kraken.Net.Objects;
using Kraken.Net.UnitTests.TestImplementations;

namespace Kraken.Net.UnitTests
{
    [TestFixture]
    public class KrakenClientTests
    {
        [TestCase()]
        public void TestConversions()
        {
            var ignoreMethods = new string[]
            {
                "GetMarkets",
                "GetOrderBook",
            };
            var defaultParameterValues = new Dictionary<string, object>
            {
                { "assets", new [] { "XBTUSD" } },
                {"clientOrderId", null }
            };

            var methods = typeof(KrakenClient).GetMethods(BindingFlags.Public | BindingFlags.Instance);
            var callResultMethods = methods.Where(m => m.ReturnType.IsGenericType && m.ReturnType.GetGenericTypeDefinition() == typeof(WebCallResult<>));
            foreach (var method in callResultMethods)
            {
                if (ignoreMethods.Contains(method.Name))
                    continue;

                var expectedType = method.ReturnType.GetGenericArguments()[0];
                var expected = typeof(TestHelpers).GetMethod("CreateObjectWithTestParameters").MakeGenericMethod(expectedType).Invoke(null, null);
                var parameters = TestHelpers.CreateParametersForMethod(method, defaultParameterValues);
                var client = TestHelpers.CreateResponseClient(SerializeExpected(expected), new KrakenClientOptions(){ ApiCredentials = new ApiCredentials("Test", "Test"), LogVerbosity = LogVerbosity.Debug});

                // act
                var result = method.Invoke(client, parameters);
                var callResult = result.GetType().GetProperty("Success").GetValue(result);
                var data = result.GetType().GetProperty("Data").GetValue(result);

                // assert
                Assert.AreEqual(true, callResult);
                Assert.IsTrue(TestHelpers.AreEqual(expected, data), method.Name);
            }
        }


        [TestCase()]
        public void TestErrorResult_Should_ResultInFailedCall()
        {
            // arrange
            var client = TestHelpers.CreateAuthResponseClient($"{{\"error\": [\"first error\", \"another error\"], \"result\": null}}");

            // act
            var result = client.GetMarkets();

            // assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Error.Message.Contains("first error"));
            Assert.IsTrue(result.Error.Message.Contains("another error"));
        }

        public string SerializeExpected<T>(T data)
        {
            var result = new KrakenResult<T>()
            {
                Result = data,
                Error = new string[] {}
            };

            return JsonConvert.SerializeObject(result);
        }
    }
}