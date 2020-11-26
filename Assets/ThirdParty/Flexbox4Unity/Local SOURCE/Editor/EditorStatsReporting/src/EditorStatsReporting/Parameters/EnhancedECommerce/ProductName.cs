﻿using EditorStatsReporting.Validators;

namespace EditorStatsReporting.Parameters.EnhancedECommerce
{
    /// <summary>
    /// The name of the product. For analytics.js the Enhanced Ecommerce plugin must be installed before using this field.
    /// </summary>
    public class ProductName : Parameter
    {
       public byte ProductIndex { get; set; }

       public ProductName(string value, byte productIndex = 1)
            : base(value)
        {
            ProductIndex = productIndex;
        }

        public override string Name
        {
            get
            {
                IndexValidator.ValidateProductIndex(ProductIndex);

                return string.Format("pr{0}nm", ProductIndex);
            }
        }
    }
}
