using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.ExchangeInterfaces;
using CryptoExchange.Net.Logging;
using CryptoExchange.Net.Objects;
using Kraken.Net.Enums;
using Kraken.Net.Interfaces.Clients.SpotApi;
using Kraken.Net.Objects;
using Kraken.Net.Objects.Models;

namespace Kraken.Net.Clients.SpotApi
{
    /// <inheritdoc cref="IKrakenClientSpotApi" />
    public class KrakenClientSpotApi : RestApiClient, IKrakenClientSpotApi, IExchangeClient
    {
        #region fields
        internal KrakenClientOptions ClientOptions { get; }
        private KrakenClient _baseClient;
        private Log _log;

        internal static TimeSyncState TimeSyncState = new TimeSyncState();
        #endregion

        #region Api clients

        /// <inheritdoc />
        public IKrakenClientSpotApiAccount Account { get; }
        /// <inheritdoc />
        public IKrakenClientSpotApiExchangeData ExchangeData { get; }
        /// <inheritdoc />
        public IKrakenClientSpotApiTrading Trading { get; }
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
        internal KrakenClientSpotApi(Log log, KrakenClient baseClient, KrakenClientOptions options)
            : base(options, options.SpotApiOptions)
        {
            ClientOptions = options;
            _baseClient = baseClient;
            _log = log;

            Account = new KrakenClientSpotApiAccount(this);
            ExchangeData = new KrakenClientSpotApiExchangeData(this);
            Trading = new KrakenClientSpotApiTrading(this);
        }
        #endregion

        /// <inheritdoc />
        protected override AuthenticationProvider CreateAuthenticationProvider(ApiCredentials credentials)
            => new KrakenAuthenticationProvider(credentials, ClientOptions.NonceProvider ?? new KrakenNonceProvider());

        #region common interface

#pragma warning disable 1066
        async Task<WebCallResult<IEnumerable<ICommonSymbol>>> IExchangeClient.GetSymbolsAsync()
        {
            var exchangeInfo = await ExchangeData.GetSymbolsAsync().ConfigureAwait(false);
            return exchangeInfo.As<IEnumerable<ICommonSymbol>>(exchangeInfo.Data?.Select(d => d.Value));
        }

        async Task<WebCallResult<ICommonTicker>> IExchangeClient.GetTickerAsync(string symbol)
        {
            var ticker = await ExchangeData.GetTickerAsync(symbol).ConfigureAwait(false);
            return ticker.As<ICommonTicker>(ticker.Data?.Select(d => d.Value).FirstOrDefault());
        }

        async Task<WebCallResult<IEnumerable<ICommonTicker>>> IExchangeClient.GetTickersAsync()
        {
            var assets = await ExchangeData.GetSymbolsAsync().ConfigureAwait(false);
            if (!assets)
                return new WebCallResult<IEnumerable<ICommonTicker>>(assets.ResponseStatusCode, assets.ResponseHeaders, null, assets.Error);

            var ticker = await ExchangeData.GetTickersAsync(assets.Data.Select(d => d.Key).ToArray()).ConfigureAwait(false);
            return ticker.As<IEnumerable<ICommonTicker>>(ticker.Data?.Select(d => d.Value));
        }

        async Task<WebCallResult<IEnumerable<ICommonKline>>> IExchangeClient.GetKlinesAsync(string symbol, TimeSpan timespan, DateTime? startTime = null, DateTime? endTime = null, int? limit = null)
        {
            if (endTime != null)
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
            var tradesResult = await ExchangeData.GetTradeHistoryAsync(symbol).ConfigureAwait(false);
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
            return result.As<ICommonOrder>(result.Data?.FirstOrDefault().Value);
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
            if (result.Data?.Pending.Any() != true)
                return WebCallResult<ICommonOrderId>.CreateErrorResult(result.ResponseStatusCode, result.ResponseHeaders, result.Error ?? new ServerError("No orders canceled"));

            return result.As<ICommonOrderId>(result ? new KrakenOrder() { ReferenceId = result.Data.Pending.First().ToString() } : null);
        }

        async Task<WebCallResult<IEnumerable<ICommonBalance>>> IExchangeClient.GetBalancesAsync(string? accountId = null)
        {
            var result = await Account.GetBalancesAsync().ConfigureAwait(false);
            return result.As<IEnumerable<ICommonBalance>>(result.Data?.Select(d => new KrakenBalance() { Asset = d.Key, Balance = d.Value }));
        }

        #endregion

        internal Uri GetUri(string endpoint)
        {
            return new Uri(BaseAddress.AppendPath(endpoint));
        }

        internal void InvokeOrderPlaced(ICommonOrderId id)
        {
            OnOrderPlaced?.Invoke(id);
        }

        internal void InvokeOrderCanceled(ICommonOrderId id)
        {
            OnOrderCanceled?.Invoke(id);
        }

        internal Task<WebCallResult<T>> Execute<T>(Uri url, HttpMethod method, CancellationToken ct, Dictionary<string, object>? parameters = null, bool signed = false, int weight = 1)
            => _baseClient.Execute<T>(this, url, method, ct, parameters, signed, weight);
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

        /// <inheritdoc />
        protected override Task<WebCallResult<DateTime>> GetServerTimestampAsync()
            => ExchangeData.GetServerTimeAsync();

        /// <inheritdoc />
        protected override TimeSyncInfo GetTimeSyncInfo()
            => new TimeSyncInfo(_log, ClientOptions.SpotApiOptions.AutoTimestamp, TimeSyncState);

        /// <inheritdoc />
        public override TimeSpan GetTimeOffset()
            => TimeSyncState.TimeOffset;
    }
}
