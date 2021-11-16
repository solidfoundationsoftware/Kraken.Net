using Kraken.Net.Interfaces;
using Kraken.Net.Interfaces.Clients.Rest.Spot;
using Kraken.Net.UnitTests.TestImplementations;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using CryptoExchange.Net.Interfaces;
using Kraken.Net.Objects;

namespace Kraken.Net.UnitTests
{
    [TestFixture]
    public class JsonTests
    {
        private JsonToObjectComparer<IKrakenClientSpot> _comparer = new JsonToObjectComparer<IKrakenClientSpot>((json) => TestHelpers.CreateResponseClient(json, new KrakenClientSpotOptions()
        { ApiCredentials = new CryptoExchange.Net.Authentication.ApiCredentials("1234", "1234"), OutputOriginalData = true, RateLimiters = new List<IRateLimiter>() }));
        
        [Test]
        public async Task ValidateSpotAccountCalls()
        {   
            await _comparer.ProcessSubject("Account", c => c.Account,
                useNestedJsonPropertyForAllCompare: new List<string> { "result" },
                useNestedJsonPropertyForCompare: new Dictionary<string, string> {
                    { "GetOrderBookAsync", "XXBTZUSD" } ,
                }
                );
        }

        [Test]
        public async Task ValidateSpotExchangeDataCalls()
        {
            await _comparer.ProcessSubject("ExchangeData", c => c.ExchangeData,
                useNestedJsonPropertyForAllCompare: new List<string> { "result" },
                useNestedJsonPropertyForCompare: new Dictionary<string, string> {
                    { "GetOrderBookAsync", "XXBTZUSD" } ,
                }
                );
        }

        [Test]
        public async Task ValidateSpotTradingCalls()
        {
            await _comparer.ProcessSubject("Trading", c => c.Trading,
                useNestedJsonPropertyForAllCompare: new List<string> { "result" },
                useNestedJsonPropertyForCompare: new Dictionary<string, string> {
                    { "GetOrderBookAsync", "XXBTZUSD" } ,
                }
                );
        }

    }
}
