﻿using System;
using EditorStatsReporting.Validators;

namespace EditorStatsReporting.Parameters.EnhancedECommerce
{
    /// <summary>
    /// The quantity of a product. For analytics.js the Enhanced Ecommerce plugin must be installed before using this field.
    /// </summary>
    public class ProductQuantity : Parameter
    {
        public byte ProductIndex { get; set; }

        public ProductQuantity(string value, byte productIndex = 1)
            : base(value)
        {
            ProductIndex = productIndex;
        }

        public override string Name
        {
            get
            {
                IndexValidator.ValidateProductIndex(ProductIndex);

                return string.Format("pr{0}qt", ProductIndex);
            }
        }

        public override Type ValueType
        {
            get { return typeof(int); }
        }
    }
}
