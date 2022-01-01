using Kraken.Net.Clients;
using Kraken.Net.Interfaces.Clients;
using Kraken.Net.Objects;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text.RegularExpressions;

namespace Kraken.Net
{
    /// <summary>
    /// Helper methods for Kraken
    /// </summary>
    public static class KrakenHelpers
    {
        /// <summary>
        /// Add the IKrakenClient and IKrakenSocketClient to the sevice collection so they can be injected
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="defaultOptionsCallback">Set default options for the client</param>
        /// <returns></returns>
        public static IServiceCollection AddKraken(this IServiceCollection services, Action<KrakenClientOptions, KrakenSocketClientOptions>? defaultOptionsCallback = null)
        {
            if (defaultOptionsCallback != null)
            {
                var options = new KrakenClientOptions();
                var socketOptions = new KrakenSocketClientOptions();
                defaultOptionsCallback?.Invoke(options, socketOptions);

                KrakenClient.SetDefaultOptions(options);
                KrakenSocketClient.SetDefaultOptions(socketOptions);
            }

            return services.AddTransient<IKrakenClient, KrakenClient>()
                           .AddScoped<IKrakenSocketClient, KrakenSocketClient>();
        }

        /// <summary>
        /// Validate the string is a valid Kraken symbol.
        /// </summary>
        /// <param name="symbolString">string to validate</param>
        public static string ValidateKrakenSymbol(this string symbolString)
        {
            if (string.IsNullOrEmpty(symbolString))
                throw new ArgumentException("Symbol is not provided");
            if (!Regex.IsMatch(symbolString, "^(([a-z]|[A-Z]|[0-9]|\\.){5,})$"))
                throw new ArgumentException($"{symbolString} is not a valid Kraken symbol. Should be [BaseAsset][QuoteAsset], e.g. ETHXBT");
            return symbolString;
        }

        /// <summary>
        /// Validate the string is a valid Kraken websocket symbol.
        /// </summary>
        /// <param name="symbolString">string to validate</param>
        public static void ValidateKrakenWebsocketSymbol(this string symbolString)
        {
            if (string.IsNullOrEmpty(symbolString))
                throw new ArgumentException("Symbol is not provided");
            if (!Regex.IsMatch(symbolString, "^(([A-Z]|[0-9]|[.]){2,})[/](([A-Z]|[0-9]){2,})$"))
                throw new ArgumentException($"{symbolString} is not a valid Kraken websocket symbol. Should be [BaseAsset]/[QuoteAsset] in ISO 4217-A3 standardized names, e.g. ETH/XBT" +
                                            "Websocket names for pairs are returned in the GetSymbols method in the WebsocketName property.");
        }
    }
}
