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
    public class KrakenClientOptions : RestClientOptions
    {
        /// <summary>
        /// Default options for the spot client
        /// </summary>
        public static KrakenClientOptions Default { get; set; } = new KrakenClientOptions()
        {
            OptionsSpot = new RestSubClientOptions
            {
                BaseAddress = "https://api.kraken.com",
                RateLimiters = new List<IRateLimiter>
                {
                     new RateLimiter()
                        .AddApiKeyLimit(15, TimeSpan.FromSeconds(45), false, false)
                        .AddEndpointLimit(new [] { "/private/AddOrder", "/private/CancelOrder", "/private/CancelAll", "/private/CancelAllOrdersAfter" }, 60, TimeSpan.FromSeconds(60), null, true),

                }
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

        public RestSubClientOptions OptionsSpot { get; set; }


        /// <summary>
        /// Ctor
        /// </summary>
        public KrakenClientOptions()
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
        public new void Copy<T>(T input, T def) where T : KrakenClientOptions
        {
            base.Copy(input, def);

            input.NonceProvider = def.NonceProvider;
            input.StaticTwoFactorAuthenticationPassword = def.StaticTwoFactorAuthenticationPassword;

            input.OptionsSpot = new RestSubClientOptions();
            def.OptionsSpot.Copy(input.OptionsSpot, def.OptionsSpot);
        }
    }

    public class KrakenSubSocketClientOptions : SocketSubClientOptions
    {
        /// <summary>
        /// The base address for the authenticated websocket
        /// </summary>
        public string BaseAddressAuthenticated { get; set; }

                public new void Copy<T>(T input, T def) where T : KrakenSubSocketClientOptions
        {
            base.Copy(input, def);

            input.BaseAddressAuthenticated = def.BaseAddressAuthenticated;
        }
    }

    /// <summary>
    /// Options for the Kraken socket client
    /// </summary>
    public class KrakenSocketClientOptions : SocketClientOptions
    {
        /// <summary>
        /// Default options for the spot client
        /// </summary>
        public static KrakenSocketClientOptions Default { get; set; } = new KrakenSocketClientOptions()
        {
            OptionsSpot = new KrakenSubSocketClientOptions
            {
                BaseAddress = "wss://ws.kraken.com",
                BaseAddressAuthenticated = "wss://ws-auth.kraken.com/"
            },
            SocketSubscriptionsCombineTarget = 10
        };

        public KrakenSubSocketClientOptions OptionsSpot { get; set; }

        /// <summary>
        /// Ctor
        /// </summary>
        public KrakenSocketClientOptions()
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
        public new void Copy<T>(T input, T def) where T : KrakenSocketClientOptions
        {
            base.Copy(input, def);

            input.OptionsSpot = new KrakenSubSocketClientOptions();
            def.OptionsSpot.Copy(input.OptionsSpot, def.OptionsSpot);
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
        public IKrakenSocketClient? SocketClient { get; }

        /// <summary>
        /// </summary>
        /// <param name="client">The client to use for the socket connection. When using the same client for multiple order books the connection can be shared.</param>
        public KrakenOrderBookOptions(IKrakenSocketClient? client = null)
        {
            SocketClient = client;
        }
    }
}

