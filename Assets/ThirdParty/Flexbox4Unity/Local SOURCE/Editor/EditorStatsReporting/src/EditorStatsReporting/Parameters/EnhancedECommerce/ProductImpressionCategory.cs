﻿using EditorStatsReporting.Validators;

namespace EditorStatsReporting.Parameters.EnhancedECommerce
{
    /// <summary>
    /// The category to which the product belongs. 
    /// </summary>
    public class ProductImpressionCategory : Parameter
    {
        public byte ProductIndex { get; set; }

        public byte ListIndex { get; set; }

        public ProductImpressionCategory(string value, byte productIndex = 1, byte listIndex = 1)
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

                return string.Format("il{0}pi{1}ca",ListIndex, ProductIndex);
            }
        }
    }
}
