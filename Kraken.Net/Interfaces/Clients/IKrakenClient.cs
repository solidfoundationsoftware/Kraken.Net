using CryptoExchange.Net.Interfaces;

namespace Kraken.Net.Interfaces.Clients.Rest.Spot
{
    /// <summary>
    /// Client for accessing the Kraken API. 
    /// </summary>
    public interface IKrakenClient : IRestClient
    {
        /// <summary>
        /// Endpoints related to account settings, info or actions
        /// </summary>
        IKrakenClientSpot SpotApi { get; }

    }
}