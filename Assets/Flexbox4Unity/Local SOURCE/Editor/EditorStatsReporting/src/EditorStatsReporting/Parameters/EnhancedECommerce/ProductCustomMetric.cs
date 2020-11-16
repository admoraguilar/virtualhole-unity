﻿using System;
using EditorStatsReporting.Validators;

namespace EditorStatsReporting.Parameters.EnhancedECommerce
{
   /// <summary>
    /// A product-level custom metric where index is a positive integer between 0 and 200.
    /// For analytics.js the Enhanced Ecommerce plugin must be installed before using this field.
   /// </summary>
    public class ProductCustomMetric : Parameter
    {
        public byte ProductIndex { get; set; }

        public byte MetricIndex { get; set; }

        public ProductCustomMetric(string value, byte productIndex = 1, byte metricIndex = 1)
            : base(value)
        {
            ProductIndex = productIndex;
            MetricIndex = metricIndex;
        }

        public override string Name
        {
            get
            {
                IndexValidator.ValidateProductIndex(ProductIndex);
                IndexValidator.ValidateMetricIndex(MetricIndex);

                return string.Format("pr{0}cm{1}",ProductIndex, MetricIndex);
            }
        }

        public override Type ValueType
        {
            get { return typeof(int); }
        }
    }

}
