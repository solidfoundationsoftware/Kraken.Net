using System;
using System.Collections.Generic;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using Kraken.Net.Interfaces.Clients.Socket;

namespace Kraken.Net.Objects
{
    /// <summary>
    /// Options for the Kraken client
    /// </summary>
    public class KrakenClientSpotOptions : RestClientOptions
    {
        /// <summary>
        /// Default options for the spot client
        /// </summary>
        public static KrakenClientSpotOptions Default { get; set; } = new KrakenClientSpotOptions()
        {
            BaseAddress = "https://api.kraken.com",
            RateLimiters = new List<IRateLimiter>
            {
                 new RateLimiter()
                    .AddApiKeyLimit(15, TimeSpan.FromSeconds(45), false, false)
                    .AddEndpointLimit(new [] { "/private/AddOrder", "/private/CancelOrder", "/private/CancelAll", "/private/CancelAllOrdersAfter" }, 60, TimeSpan.FromSeconds(60), null, true),

            }
        };

        /// <summary>
        /// The static password configured as two-factor authentication for the API key. Will be send as otp parameter on private requests.
        /// </summary>
        public string? StaticTwoFactorAuthenticationPassword { get; set; }

        /// <summary>
        /// Optional nonce provider for signing requests. Careful providing a custom provider; once a nonce is sent to the server, every request after that needs a higher nonce than that
        /// </summary>
        public INonceProvider? NonceProvider { get; set; }

        /// <summary>
        /// Ctor
        /// </summary>
        public KrakenClientSpotOptions()
        {
            if (Default == null)
                return;

            Copy(this, Default);
        }

        /// <summary>
        /// Copy the values of the def to the input
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <param name="def"></param>
        public new void Copy<T>(T input, T def) where T : KrakenClientSpotOptions
        {
            base.Copy(input, def);

            input.NonceProvider = def.NonceProvider;
            input.StaticTwoFactorAuthenticationPassword = def.StaticTwoFactorAuthenticationPassword;
        }
    }

    /// <summary>
    /// Options for the Kraken socket client
    /// </summary>
    public class KrakenSocketClientSpotOptions : SocketClientOptions
    {
        /// <summary>
        /// Default options for the spot client
        /// </summary>
        public static KrakenSocketClientSpotOptions Default { get; set; } = new KrakenSocketClientSpotOptions()
        {
            BaseAddress = "wss://ws.kraken.com",
            SocketSubscriptionsCombineTarget = 10
        };

        private string _authBaseAddress = "wss://ws-auth.kraken.com/";
        /// <summary>
        /// The base address for authenticated subscriptions
        /// </summary>
        public string AuthBaseAddress
        {
            get => _authBaseAddress;
            set
            {
                var newValue = value;
                if (!newValue.EndsWith("/"))
                    newValue += "/";
                _authBaseAddress = newValue;
            }
        }

        /// <summary>
        /// Ctor
        /// </summary>
        public KrakenSocketClientSpotOptions()
        {
            if (Default == null)
                return;

            Copy(this, Default);
        }

        /// <summary>
        /// Copy the values of the def to the input
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <param name="def"></param>
        public new void Copy<T>(T input, T def) where T : KrakenSocketClientSpotOptions
        {
            base.Copy(input, def);

            input.AuthBaseAddress = def.AuthBaseAddress;
        }
    }

    /// <summary>
    /// Options for the Kraken symbol order book
    /// </summary>
    public class KrakenOrderBookOptions : OrderBookOptions
    {
        /// <summary>
        /// The client to use for the socket connection. When using the same client for multiple order books the connection can be shared.
        /// </summary>
        public IKrakenSocketClientSpot? SocketClient { get; }

        /// <summary>
        /// </summary>
        /// <param name="client">The client to use for the socket connection. When using the same client for multiple order books the connection can be shared.</param>
        public KrakenOrderBookOptions(IKrakenSocketClientSpot? client = null)
        {
            SocketClient = client;
        }
    }
}

