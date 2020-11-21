﻿using EditorStatsReporting.Validators;

namespace EditorStatsReporting.Parameters.EnhancedECommerce
{
   /// <summary>
    /// The list or collection to which a product belongs.
   /// </summary>
    public class ProductImpressionListName : Parameter
    {
        public byte ListIndex { get; set; }

        public ProductImpressionListName(string value, byte listIndex = 1)
            : base(value)
        {
            ListIndex = listIndex;
        }

        public override string Name
        {
            get
            {
                IndexValidator.ValidateListIndex(ListIndex);

                return string.Format("il{0}nm", ListIndex); 
            }
        }
    }
}
