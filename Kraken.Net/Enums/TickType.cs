﻿using CryptoExchange.Net.Attributes;

namespace Kraken.Net.Enums
{
    /// <summary>
    /// Tick type
    /// </summary>
    public enum TickType
    {
        /// <summary>
        /// Spot price
        /// </summary>
        [Map("spot")]
        Spot,
        /// <summary>
        /// Mark price
        /// </summary>
        [Map("mark")]
        Mark,
        /// <summary>
        /// Trade price
        /// </summary>
        [Map("trade")]
        Trade
    }
}
