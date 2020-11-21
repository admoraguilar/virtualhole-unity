using EditorStatsReporting.Validators;

namespace EditorStatsReporting.Parameters.EnhancedECommerce
{
    /// <summary>
    /// The promotion ID. 
    /// </summary>
    public class PromotionId : Parameter
    {
       public byte PromoIndex { get; set; }

       public PromotionId(string value, byte promoIndex = 1)
            : base(value)
        {
            PromoIndex = promoIndex;
        }

        public override string Name
        {
            get
            {
                IndexValidator.ValidatePromotionIndex(PromoIndex);

                return string.Format("promo{0}id", PromoIndex);
            }
        }
    }
}
