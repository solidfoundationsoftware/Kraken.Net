using CryptoExchange.Net;
using CryptoExchange.Net.Converters;
using CryptoExchange.Net.Objects;
using Kraken.Net.Converters;
using Kraken.Net.Enums;
using Kraken.Net.Interfaces.Clients.Rest.Spot;
using Kraken.Net.Objects;
using Kraken.Net.Objects.Socket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kraken.Net.Clients.Rest.Spot
{
    public class KrakenClientSpotTrading: IKrakenClientSpotTrading
    {
        private KrakenClientSpot _baseClient;

        internal KrakenClientSpotTrading(KrakenClientSpot baseClient)
        {
            _baseClient = baseClient;
        }


        /// <inheritdoc />
        public async Task<WebCallResult<OpenOrdersPage>> GetOpenOrdersAsync(uint? clientOrderId = null, string? twoFactorPassword = null, CancellationToken ct = default)
        {
            var parameters = new Dictionary<string, object>();
            parameters.AddOptionalParameter("trades", true);
            parameters.AddOptionalParameter("userref", clientOrderId);
            parameters.AddOptionalParameter("otp", twoFactorPassword ?? _baseClient.ClientOptions.StaticTwoFactorAuthenticationPassword);
            return await _baseClient.Execute<OpenOrdersPage>(_baseClient.GetUri("0/private/OpenOrders"), HttpMethod.Post, ct, parameters, true).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<KrakenClosedOrdersPage>> GetClosedOrdersAsync(uint? clientOrderId = null, DateTime? startTime = null, DateTime? endTime = null, int? resultOffset = null, string? twoFactorPassword = null, CancellationToken ct = default)
        {
            var parameters = new Dictionary<string, object>();
            parameters.AddOptionalParameter("trades", true);
            parameters.AddOptionalParameter("userref", clientOrderId);
            parameters.AddOptionalParameter("start", startTime.HasValue ? JsonConvert.SerializeObject(startTime.Value, new TimestampSecondsConverter()) : null);
            parameters.AddOptionalParameter("end", endTime.HasValue ? JsonConvert.SerializeObject(endTime.Value, new TimestampSecondsConverter()) : null);
            parameters.AddOptionalParameter("ofs", resultOffset);
            parameters.AddOptionalParameter("otp", twoFactorPassword);
            return await _baseClient.Execute<KrakenClosedOrdersPage>(_baseClient.GetUri("0/private/ClosedOrders"), HttpMethod.Post, ct, parameters, true).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public Task<WebCallResult<Dictionary<string, KrakenOrder>>> GetOrderAsync(string? orderId = null, uint? clientOrderId = null, string? twoFactorPassword = null, CancellationToken ct = default)
            => GetOrdersAsync(orderId == null ? null : new[] { orderId }, clientOrderId, twoFactorPassword, ct);

        /// <inheritdoc />
        public async Task<WebCallResult<Dictionary<string, KrakenOrder>>> GetOrdersAsync(IEnumerable<string>? orderIds = null, uint? clientOrderId = null, string? twoFactorPassword = null, CancellationToken ct = default)
        {
            var parameters = new Dictionary<string, object>();
            parameters.AddOptionalParameter("trades", true);
            parameters.AddOptionalParameter("userref", clientOrderId);
            parameters.AddOptionalParameter("txid", orderIds?.Any() == true ? string.Join(",", orderIds) : null);
            parameters.AddOptionalParameter("otp", twoFactorPassword ?? _baseClient.ClientOptions.StaticTwoFactorAuthenticationPassword);
            return await _baseClient.Execute<Dictionary<string, KrakenOrder>>(_baseClient.GetUri("0/private/QueryOrders"), HttpMethod.Post, ct, parameters, true).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<WebCallResult<KrakenUserTradesPage>> GetUserTradesAsync(DateTime? startTime = null, DateTime? endTime = null, int? resultOffset = null, string? twoFactorPassword = null, CancellationToken ct = default)
        {
            var parameters = new Dictionary<string, object>();
            parameters.AddOptionalParameter("trades", true);
            parameters.AddOptionalParameter("start", startTime.HasValue ? JsonConvert.SerializeObject(startTime.Value, new TimestampSecondsConverter()) : null);
            parameters.AddOptionalParameter("end", endTime.HasValue ? JsonConvert.SerializeObject(endTime.Value, new TimestampSecondsConverter()) : null);
            parameters.AddOptionalParameter("ofs", resultOffset);
            parameters.AddOptionalParameter("otp", twoFactorPassword ?? _baseClient.ClientOptions.StaticTwoFactorAuthenticationPassword);
            var result = await _baseClient.Execute<KrakenUserTradesPage>(_baseClient.GetUri("0/private/TradesHistory"), HttpMethod.Post, ct, parameters, true).ConfigureAwait(false);
            if (result)
            {
                foreach (var item in result.Data.Trades)
                    item.Value.Id = item.Key;
            }

            return result;
        }

        /// <inheritdoc />
        public Task<WebCallResult<Dictionary<string, KrakenUserTrade>>> GetUserTradeDetailsAsync(string tradeId, string? twoFactorPassword = null, CancellationToken ct = default)
            => GetUserTradeDetailsAsync(new string[] { tradeId }, twoFactorPassword, ct);


        /// <inheritdoc />
        public async Task<WebCallResult<Dictionary<string, KrakenUserTrade>>> GetUserTradeDetailsAsync(IEnumerable<string> tradeIds, string? twoFactorPassword = null, CancellationToken ct = default)
        {
            var parameters = new Dictionary<string, object>();
            parameters.AddOptionalParameter("trades", true);
            parameters.AddOptionalParameter("txid", tradeIds?.Any() == true ? string.Join(",", tradeIds) : null);
            parameters.AddOptionalParameter("otp", twoFactorPassword ?? _baseClient.ClientOptions.StaticTwoFactorAuthenticationPassword);
            return await _baseClient.Execute<Dictionary<string, KrakenUserTrade>>(_baseClient.GetUri("0/private/QueryTrades"), HttpMethod.Post, ct, parameters, true).ConfigureAwait(false);
        }


        /// <inheritdoc />
        public async Task<WebCallResult<KrakenPlacedOrder>> PlaceOrderAsync(
            string symbol,
            OrderSide side,
            OrderType type,
            decimal quantity,
            uint? clientOrderId = null,
            decimal? price = null,
            decimal? secondaryPrice = null,
            decimal? leverage = null,
            DateTime? startTime = null,
            DateTime? expireTime = null,
            bool? validateOnly = null,
            string? twoFactorPassword = null,
            CancellationToken ct = default)
        {
            symbol.ValidateKrakenSymbol();
            var parameters = new Dictionary<string, object>()
            {
                { "pair", symbol },
                { "type", JsonConvert.SerializeObject(side, new OrderSideConverter(false)) },
                { "ordertype", JsonConvert.SerializeObject(type, new OrderTypeConverter(false)) },
                { "volume", quantity.ToString(CultureInfo.InvariantCulture) },
                { "trading_agreement", "agree" }
            };
            parameters.AddOptionalParameter("price", price?.ToString(CultureInfo.InvariantCulture));
            parameters.AddOptionalParameter("userref", clientOrderId);
            parameters.AddOptionalParameter("price2", secondaryPrice?.ToString(CultureInfo.InvariantCulture));
            parameters.AddOptionalParameter("otp", twoFactorPassword ?? _baseClient.ClientOptions.StaticTwoFactorAuthenticationPassword);
            parameters.AddOptionalParameter("leverage", leverage?.ToString(CultureInfo.InvariantCulture));
            parameters.AddOptionalParameter("starttm", startTime.HasValue ? JsonConvert.SerializeObject(startTime.Value, new TimestampSecondsConverter()) : null);
            parameters.AddOptionalParameter("expiretm", expireTime.HasValue ? JsonConvert.SerializeObject(expireTime.Value, new TimestampSecondsConverter()) : null);
            if (validateOnly == true)
                parameters.AddOptionalParameter("validate", true);
            var result = await _baseClient.Execute<KrakenPlacedOrder>(_baseClient.GetUri("0/private/AddOrder"), HttpMethod.Post, ct, parameters, true).ConfigureAwait(false);
            if (result)
                _baseClient.InvokeOrderPlaced(result.Data);
            return result;
        }

        /// <inheritdoc />
        public async Task<WebCallResult<KrakenCancelResult>> CancelOrderAsync(string orderId, string? twoFactorPassword = null, CancellationToken ct = default)
        {
            orderId.ValidateNotNull(nameof(orderId));
            var parameters = new Dictionary<string, object>()
            {
                {"txid", orderId}
            };
            parameters.AddOptionalParameter("otp", twoFactorPassword ?? _baseClient.ClientOptions.StaticTwoFactorAuthenticationPassword);
            var result = await _baseClient.Execute<KrakenCancelResult>(_baseClient.GetUri("0/private/CancelOrder"), HttpMethod.Post, ct, parameters, true).ConfigureAwait(false);
            if (result)
                _baseClient.InvokeOrderCanceled(new KrakenOrder { Id = orderId });
            return result;
        }
    }
}
