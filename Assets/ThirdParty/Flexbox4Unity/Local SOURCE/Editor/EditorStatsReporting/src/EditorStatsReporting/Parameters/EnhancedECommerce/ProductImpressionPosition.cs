﻿using System;
using EditorStatsReporting.Validators;

namespace EditorStatsReporting.Parameters.EnhancedECommerce
{
    /// <summary>
    /// The product's position in a list or collection.
    /// </summary>
    public class ProductImpressionPosition : Parameter
    {
       public byte ProductIndex { get; set; }

        public byte ListIndex { get; set; }

        public ProductImpressionPosition(string value, byte productIndex = 1, byte listIndex = 1)
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

                return string.Format("il{0}pi{1}ps",ListIndex, ProductIndex);
            }
        }

        public override Type ValueType
        {
            get { return typeof(int); }
        }
    }
}
