using CryptoExchange.Net.Interfaces;
using System;

namespace Kraken.Net.Interfaces.Clients.Rest.Spot
{
    /// <summary>
    /// Client for accessing the Kraken API. 
    /// </summary>
    public interface IKrakenClientSpot: IDisposable
    {
        /// <summary>
        /// Endpoints related to account settings, info or actions
        /// </summary>
        IKrakenClientSpotAccount Account { get; }

        /// <summary>
        /// Endpoints related to retrieving market and system data
        /// </summary>
        IKrakenClientSpotExchangeData ExchangeData { get; }

        /// <summary>
        /// Endpoints related to orders and trades
        /// </summary>
        IKrakenClientSpotTrading Trading { get; }
    }
}