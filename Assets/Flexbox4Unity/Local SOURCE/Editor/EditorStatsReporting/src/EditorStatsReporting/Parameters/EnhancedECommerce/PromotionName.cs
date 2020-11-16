﻿using EditorStatsReporting.Validators;

namespace EditorStatsReporting.Parameters.EnhancedECommerce
{
   /// <summary>
    /// The name of the promotion. 
   /// </summary>
    public class PromotionName : Parameter
    {
        public byte PromoIndex { get; set; }

        public PromotionName(string value, byte promoIndex = 1)
            : base(value)
        {
            PromoIndex = promoIndex;
        }

        public override string Name
        {
            get
            {
                IndexValidator.ValidatePromotionIndex(PromoIndex);

                return string.Format("promo{0}nm", PromoIndex);
            }
        }
    }
}
