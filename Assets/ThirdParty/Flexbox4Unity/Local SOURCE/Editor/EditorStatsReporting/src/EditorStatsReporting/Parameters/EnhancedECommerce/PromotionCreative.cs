﻿using EditorStatsReporting.Validators;

namespace EditorStatsReporting.Parameters.EnhancedECommerce
{
    /// <summary>
    /// The creative associated with the promotion.
    /// </summary>
    public class PromotionCreative : Parameter
    {
        public byte PromoIndex { get; set; }

        public PromotionCreative(string value, byte promoIndex = 1)
            : base(value)
        {
            PromoIndex = promoIndex;
        }

        public override string Name
        {
            get
            {
                IndexValidator.ValidatePromotionIndex(PromoIndex);

                return string.Format("promo{0}cr", PromoIndex);
            }
        }
    }
}
