﻿using EditorStatsReporting.Validators;

namespace EditorStatsReporting.Parameters.EnhancedECommerce
{
   /// <summary>
    /// The brand associated with the product.
   /// </summary>
    public class ProductImpressionBrand : Parameter
    {
        public byte ProductIndex { get; set; }

        public byte ListIndex { get; set; }

        public ProductImpressionBrand(string value, byte productIndex = 1, byte listIndex = 1)
            : base(value)
        {
            ProductIndex = productIndex;
            ListIndex = listIndex;
        }

        public override string Name
        {
            get
            {
                IndexValidator.ValidateProductIndex(ProductIndex);
                IndexValidator.ValidateListIndex(ListIndex);

                return string.Format("il{0}pi{1}br",ListIndex, ProductIndex);
            }
        }
    }
}
