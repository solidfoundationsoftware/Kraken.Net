using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sockets;
using Kraken.Net.Enums;
using Kraken.Net.Objects.Models;
using Kraken.Net.Objects.Models.Socket;

namespace Kraken.Net.Interfaces.Clients.Socket
{
    /// <summary>
    /// Interface for the Kraken socket client
    /// </summary>
    public interface IKrakenSocketClient: ISocketClient
    {
        IKrakenSocketClientSpotMarket SpotMarket { get; }
    }
}