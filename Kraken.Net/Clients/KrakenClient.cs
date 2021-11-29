using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.ExchangeInterfaces;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using Kraken.Net.Enums;
using Kraken.Net.Interfaces.Clients.Rest.Spot;
using Kraken.Net.Objects;
using Kraken.Net.Objects.Internal;
using Kraken.Net.Objects.Models;

namespace Kraken.Net.Clients.Rest.Spot
{
    /// <summary>
    /// Client for the Kraken Rest API
    /// </summary>
    public class KrakenClient: RestClient, IKrakenClient
    {
        #region fields
        public new KrakenClientOptions ClientOptions { get; }
        #endregion

        #region Subclients
        public IKrakenClientSpot SpotMarket { get; }
        #endregion

        /// <summary>
        /// Event triggered when an order is placed via this client
        /// </summary>
        public event Action<ICommonOrderId>? OnOrderPlaced;
        /// <summary>
        /// Event triggered when an order is canceled via this client
        /// </summary>
        public event Action<ICommonOrderId>? OnOrderCanceled;

        #region ctor
        /// <summary>
        /// Create a new instance of KrakenClient using the default options
        /// </summary>
        public KrakenClient() : this(KrakenClientOptions.Default)
        {
        }

        /// <summary>
        /// Create a new instance of KrakenClient using provided options
        /// </summary>
        /// <param name="options">The options to use for this client</param>
        public KrakenClient(KrakenClientOptions options) : base("Kraken", options)
        {
            ClientOptions = options;
            requestBodyFormat = RequestBodyFormat.FormData;

            SpotMarket = new KrakenClientSpot(this, options);
        }
        #endregion

        #region methods
        /// <summary>
        /// Set the default options to be used when creating new clients
        /// </summary>
        /// <param name="options"></param>
        public static void SetDefaultOptions(KrakenClientOptions options)
        {
            KrakenClientOptions.Default = options;
        }
                
        #endregion

        /// <inheritdoc />
        protected override void WriteParamBody(IRequest request, Dictionary<string, object> parameters, string contentType)
        {
            if (parameters.TryGetValue("nonce", out var nonce))
                log.Write(Microsoft.Extensions.Logging.LogLevel.Trace, $"[{request.RequestId}] Nonce: " + nonce);
            var stringData = string.Join("&", parameters.OrderBy(p => p.Key != "nonce").Select(p => $"{p.Key}={p.Value}"));
            request.SetContent(stringData, contentType);
        }

        internal async Task<WebCallResult<T>> Execute<T>(RestSubClient subClient, Uri url, HttpMethod method, CancellationToken ct, Dictionary<string, object>? parameters = null, bool signed = false, int weight = 1)
        {
            var result = await SendRequestAsync<KrakenResult<T>>(subClient, url, method, ct, parameters, signed, requestWeight: weight).ConfigureAwait(false);
            if (!result)
                return new WebCallResult<T>(result.ResponseStatusCode, result.ResponseHeaders, default, result.Error);

            if (result.Data.Error.Any())
                return new WebCallResult<T>(result.ResponseStatusCode, result.ResponseHeaders, default, new ServerError(string.Join(", ", result.Data.Error)));

            return result.As<T>(result.Data.Result);
        }
    }
}
