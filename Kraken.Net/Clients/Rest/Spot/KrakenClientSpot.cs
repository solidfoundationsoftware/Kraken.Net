using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Converters;
using CryptoExchange.Net.ExchangeInterfaces;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using Kraken.Net.Converters;
using Kraken.Net.Enums;
using Kraken.Net.Interfaces;
using Kraken.Net.Interfaces.Clients.Rest.Spot;
using Kraken.Net.Objects;
using Kraken.Net.Objects.Socket;
using Newtonsoft.Json;

namespace Kraken.Net.Clients.Rest.Spot
{
    /// <summary>
    /// Client for the Kraken Rest API
    /// </summary>
    public class KrakenClientSpot: RestClient, IKrakenClientSpot, IExchangeClient
    {
        #region fields
        public new KrakenClientSpotOptions ClientOptions { get; }
        #endregion

        #region Subclients
        public IKrakenClientSpotAccount Account { get; }
        public IKrakenClientSpotExchangeData ExchangeData { get; }
        public IKrakenClientSpotTrading Trading { get; }
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
        public KrakenClientSpot() : this(KrakenClientSpotOptions.Default)
        {
        }

        /// <summary>
        /// Create a new instance of KrakenClient using provided options
        /// </summary>
        /// <param name="options">The options to use for this client</param>
        public KrakenClientSpot(KrakenClientSpotOptions options) : base("Kraken", options, options.ApiCredentials == null ? null : new KrakenAuthenticationProvider(options.ApiCredentials, options.NonceProvider))
        {
            ClientOptions = options;
            requestBodyFormat = RequestBodyFormat.FormData;

            Account = new KrakenClientSpotAccount(this);
            ExchangeData = new KrakenClientSpotExchangeData(this);
            Trading = new KrakenClientSpotTrading(this);
        }
        #endregion

        #region methods
        /// <summary>
        /// Set the default options to be used when creating new clients
        /// </summary>
        /// <param name="options"></param>
        public static void SetDefaultOptions(KrakenClientSpotOptions options)
        {
            KrakenClientSpotOptions.Default = options;
        }

        /// <summary>
        /// Set the API key and secret
        /// </summary>
        /// <param name="apiKey">The api key</param>
        /// <param name="apiSecret">The api secret</param>
        /// <param name="nonceProvider">Optional nonce provider. Careful providing a custom provider; once a nonce is sent to the server, every request after that needs a higher nonce than that</param>
        public void SetApiCredentials(string apiKey, string apiSecret, INonceProvider? nonceProvider = null)
        {
            SetAuthenticationProvider(new KrakenAuthenticationProvider(new ApiCredentials(apiKey, apiSecret), nonceProvider));
        }
                
        #endregion

        #region common interface

#pragma warning disable 1066
        async Task<WebCallResult<IEnumerable<ICommonSymbol>>> IExchangeClient.GetSymbolsAsync()
        {
            var exchangeInfo = await ExchangeData.GetSymbolsAsync().ConfigureAwait(false);
            return exchangeInfo.As<IEnumerable<ICommonSymbol>>(exchangeInfo.Data?.Select(d => d.Value));
        }

        async Task<WebCallResult<ICommonTicker>> IExchangeClient.GetTickerAsync(string symbol)
        {
            var ticker = await ExchangeData.GetTickerAsync(symbol, default).ConfigureAwait(false);
            return ticker.As<ICommonTicker>(ticker.Data?.Select(d => d.Value).FirstOrDefault());
        }

        async Task<WebCallResult<IEnumerable<ICommonTicker>>> IExchangeClient.GetTickersAsync()
        {
            var assets = await ExchangeData.GetSymbolsAsync().ConfigureAwait(false);
            if(!assets)
                return new WebCallResult<IEnumerable<ICommonTicker>>(assets.ResponseStatusCode, assets.ResponseHeaders, null, assets.Error);

            var ticker = await ExchangeData.GetTickersAsync(assets.Data.Select(d => d.Key).ToArray(), default).ConfigureAwait(false);
            return ticker.As<IEnumerable<ICommonTicker>>(ticker.Data?.Select(d => d.Value));
        }

        async Task<WebCallResult<IEnumerable<ICommonKline>>> IExchangeClient.GetKlinesAsync(string symbol, TimeSpan timespan, DateTime? startTime = null, DateTime? endTime = null, int? limit = null)
        {
            if(endTime != null)
                return WebCallResult<IEnumerable<ICommonKline>>.CreateErrorResult(new ArgumentError(
                    $"Kraken doesn't support the {nameof(endTime)} parameter for the method {nameof(IExchangeClient.GetKlinesAsync)}"));

            if (limit != null)
                return WebCallResult<IEnumerable<ICommonKline>>.CreateErrorResult(new ArgumentError(
                    $"Kraken doesn't support the {nameof(limit)} parameter for the method {nameof(IExchangeClient.GetKlinesAsync)}"));

            var klines = await ExchangeData.GetKlinesAsync(symbol, GetKlineIntervalFromTimespan(timespan), since: startTime).ConfigureAwait(false);
            if (!klines.Success)
                return WebCallResult<IEnumerable<ICommonKline>>.CreateErrorResult(klines.ResponseStatusCode, klines.ResponseHeaders, klines.Error!);
            return klines.As<IEnumerable<ICommonKline>>(klines.Data.Data);
        }
        
        async Task<WebCallResult<ICommonOrderBook>> IExchangeClient.GetOrderBookAsync(string symbol)
        {
            var book = await ExchangeData.GetOrderBookAsync(symbol).ConfigureAwait(false);
            return book.As<ICommonOrderBook>(book.Data);
        }

        async Task<WebCallResult<IEnumerable<ICommonRecentTrade>>> IExchangeClient.GetRecentTradesAsync(string symbol)
        {
            var tradesResult = await ExchangeData.GetTradeHistoryAsync(symbol, null).ConfigureAwait(false);
            if (!tradesResult.Success)
                return WebCallResult<IEnumerable<ICommonRecentTrade>>.CreateErrorResult(tradesResult.ResponseStatusCode, tradesResult.ResponseHeaders, tradesResult.Error!);

            return tradesResult.As<IEnumerable<ICommonRecentTrade>>(tradesResult.Data?.Data);
        }

        async Task<WebCallResult<ICommonOrderId>> IExchangeClient.PlaceOrderAsync(string symbol, IExchangeClient.OrderSide side, IExchangeClient.OrderType type, decimal quantity, decimal? price = null, string? accountId = null)
        {
            var result = await Trading.PlaceOrderAsync(symbol, GetOrderSide(side), GetOrderType(type), quantity, price: price).ConfigureAwait(false);
            return result.As<ICommonOrderId>(result.Data);
        }

        async Task<WebCallResult<ICommonOrder>> IExchangeClient.GetOrderAsync(string orderId, string? symbol)
        {
            var result = await Trading.GetOrderAsync(orderId).ConfigureAwait(false);
            return result.As<ICommonOrder> (result.Data?.FirstOrDefault().Value);
        }

        async Task<WebCallResult<IEnumerable<ICommonTrade>>> IExchangeClient.GetTradesAsync(string orderId, string? symbol = null)
        {
            var result = await Trading.GetUserTradesAsync().ConfigureAwait(false);
            return result.As<IEnumerable<ICommonTrade>>(result.Data?.Trades.Where(t => t.Value.OrderId == orderId).Select(o => (ICommonTrade)o.Value));
        }

        async Task<WebCallResult<IEnumerable<ICommonOrder>>> IExchangeClient.GetOpenOrdersAsync(string? symbol)
        {
            var result = await Trading.GetOpenOrdersAsync().ConfigureAwait(false);
            return result.As<IEnumerable<ICommonOrder>>(result.Data?.Open.Select(d => d.Value));
        }

        async Task<WebCallResult<IEnumerable<ICommonOrder>>> IExchangeClient.GetClosedOrdersAsync(string? symbol)
        {
            var result = await Trading.GetClosedOrdersAsync().ConfigureAwait(false);
            return result.As<IEnumerable<ICommonOrder>>(result.Data?.Closed.Select(d => d.Value));
        }

        async Task<WebCallResult<ICommonOrderId>> IExchangeClient.CancelOrderAsync(string orderId, string? symbol)
        {
            var result = await Trading.CancelOrderAsync(orderId).ConfigureAwait(false);
            if(result.Data?.Pending.Any() != true)
                return WebCallResult<ICommonOrderId>.CreateErrorResult(result.ResponseStatusCode, result.ResponseHeaders, result.Error ?? new ServerError("No orders canceled"));

            return result.As<ICommonOrderId>(result? new KrakenOrder(){ ReferenceId  = result.Data.Pending.First().ToString() } : null);
        }

        async Task<WebCallResult<IEnumerable<ICommonBalance>>> IExchangeClient.GetBalancesAsync(string? accountId = null)
        {
            var result = await Account.GetBalancesAsync().ConfigureAwait(false);
            return result.As<IEnumerable<ICommonBalance>>(result.Data?.Select(d => new KrakenBalance() { Asset = d.Key, Balance = d.Value}));
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

        internal Uri GetUri(string endpoint)
        {
            return new Uri(ClientOptions.BaseAddress + endpoint);
        }

        internal void InvokeOrderPlaced(ICommonOrderId id)
        {
            OnOrderPlaced?.Invoke(id);
        }

        internal void InvokeOrderCanceled(ICommonOrderId id)
        {
            OnOrderCanceled?.Invoke(id);
        }

        internal async Task<WebCallResult<T>> Execute<T>(Uri url, HttpMethod method, CancellationToken ct, Dictionary<string, object>? parameters = null, bool signed = false)
        {
            var result = await SendRequestAsync<KrakenResult<T>>(url, method, ct, parameters, signed).ConfigureAwait(false);
            if (!result)
                return new WebCallResult<T>(result.ResponseStatusCode, result.ResponseHeaders, default, result.Error);

            if (result.Data.Error.Any())
                return new WebCallResult<T>(result.ResponseStatusCode, result.ResponseHeaders, default, new ServerError(string.Join(", ", result.Data.Error)));

            return result.As<T>(result.Data.Result);
        }
#pragma warning restore 1066

        /// <summary>
        /// Get the name of a symbol for Kraken based on the base and quote asset
        /// </summary>
        /// <param name="baseAsset"></param>
        /// <param name="quoteAsset"></param>
        /// <returns></returns>
        public string GetSymbolName(string baseAsset, string quoteAsset) => (baseAsset + quoteAsset).ToUpperInvariant();

        private static KlineInterval GetKlineIntervalFromTimespan(TimeSpan timeSpan)
        {
            if (timeSpan == TimeSpan.FromMinutes(1)) return KlineInterval.OneMinute;
            if (timeSpan == TimeSpan.FromMinutes(5)) return KlineInterval.FiveMinutes;
            if (timeSpan == TimeSpan.FromMinutes(15)) return KlineInterval.FifteenMinutes;
            if (timeSpan == TimeSpan.FromMinutes(30)) return KlineInterval.ThirtyMinutes;
            if (timeSpan == TimeSpan.FromHours(1)) return KlineInterval.OneHour;
            if (timeSpan == TimeSpan.FromHours(4)) return KlineInterval.FourHour;
            if (timeSpan == TimeSpan.FromDays(1)) return KlineInterval.OneDay;
            if (timeSpan == TimeSpan.FromDays(7)) return KlineInterval.OneWeek;
            if (timeSpan == TimeSpan.FromDays(15)) return KlineInterval.FifteenDays;

            throw new ArgumentException("Unsupported timespan for Kraken Klines, check supported intervals using Kraken.Net.Objects.KlineInterval");
        }

        private static OrderSide GetOrderSide(IExchangeClient.OrderSide side)
        {
            if (side == IExchangeClient.OrderSide.Sell) return OrderSide.Sell;
            if (side == IExchangeClient.OrderSide.Buy) return OrderSide.Buy;

            throw new ArgumentException("Unsupported order side for Kraken order: " + side);
        }

        private static OrderType GetOrderType(IExchangeClient.OrderType type)
        {
            if (type == IExchangeClient.OrderType.Limit) return OrderType.Limit;
            if (type == IExchangeClient.OrderType.Market) return OrderType.Market;

            throw new ArgumentException("Unsupported order type for Kraken order: " + type);
        }
    }
}
