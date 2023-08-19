﻿using CryptoExchange.Net.Attributes;

namespace Kraken.Net.Enums
{
    /// <summary>
    /// Status of an open order
    /// </summary>
    public enum OpenOrderStatus
    {
        /// <summary>
        /// The entire size of the order is unfilled
        /// </summary>
        [Map("untouched")]
        Untouched,
        /// <summary>
        /// The size of the order is partially but not entirely filled
        /// </summary>
        [Map("partiallyFilled")]
        PartiallyFilled
    }
}
