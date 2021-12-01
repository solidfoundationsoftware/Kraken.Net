using CryptoExchange.Net.Interfaces;
using Kraken.Net.Interfaces.Clients.SpotApi;

namespace Kraken.Net.Interfaces.Clients
{
    /// <summary>
    /// Interface for the Kraken socket client
    /// </summary>
    public interface IKrakenSocketClient : ISocketClient
    {
        IKrakenSocketClientSpotStreams SpotStreams { get; }
    }
}