using CryptoExchange.Net.Interfaces;

namespace Kraken.Net.Interfaces.Clients.Rest.Spot
{
    /// <summary>
    /// Interface for Kraken Rest API
    /// </summary>
    public interface IKrakenClientSpot : IRestClient
    {
        IKrakenClientSpotAccount Account { get; }
        IKrakenClientSpotExchangeData ExchangeData { get; }
        IKrakenClientSpotTrading Trading { get; }

        /// <summary>
        /// Set the API key and secret
        /// </summary>
        /// <param name="apiKey">The api key</param>
        /// <param name="apiSecret">The api secret</param>
        /// <param name="nonceProvider">Optional nonce provider. Careful providing a custom provider; once a nonce is sent to the server, every request after that needs a higher nonce than that</param>
        void SetApiCredentials(string apiKey, string apiSecret, INonceProvider? nonceProvider);
    }
}