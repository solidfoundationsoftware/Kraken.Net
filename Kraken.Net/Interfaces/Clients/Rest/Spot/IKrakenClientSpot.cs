using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using Kraken.Net.Converters;
using Kraken.Net.Enums;
using Kraken.Net.Objects;
using Kraken.Net.Objects.Socket;

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