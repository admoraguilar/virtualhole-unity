﻿using EditorStatsReporting.Validators;

namespace EditorStatsReporting.Parameters.EnhancedECommerce
{
   /// <summary>
    /// The variant of the product.
   /// </summary>
    public class ProductImpressionVariant : Parameter
    {
       public byte ProductIndex { get; set; }

        public byte ListIndex { get; set; }

        public ProductImpressionVariant(string value, byte productIndex = 1, byte listIndex = 1)
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

                return string.Format("il{0}pi{1}va",ListIndex, ProductIndex);
            }
        }
    }
}
