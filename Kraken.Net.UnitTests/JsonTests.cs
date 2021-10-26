using Kraken.Net.Interfaces;
using Kraken.Net.UnitTests.TestImplementations;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kraken.Net.UnitTests
{
    [TestFixture]
    public class JsonTests
    {
        private JsonToObjectComparer<IKrakenClient> _comparer = new JsonToObjectComparer<IKrakenClient>((json) => TestHelpers.CreateResponseClient(json, new KrakenClientOptions()
        { ApiCredentials = new CryptoExchange.Net.Authentication.ApiCredentials("1234", "1234"), OutputOriginalData = true }));
        
        [Test]
        public async Task ValidateCalls()
        {   
            await _comparer.ProcessSubject(c => c,
                useNestedJsonPropertyForAllCompare: new List<string> { "result" },
                useNestedJsonPropertyForCompare: new Dictionary<string, string> {
                    { "GetOrderBookAsync", "XXBTZUSD" } ,
                }
                );
        }

    }
}
