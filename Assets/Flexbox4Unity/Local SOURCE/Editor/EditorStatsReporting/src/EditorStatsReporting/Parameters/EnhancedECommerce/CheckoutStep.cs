﻿using System;

namespace EditorStatsReporting.Parameters.EnhancedECommerce
{
   /// <summary>
    /// The step number in a checkout funnel. This is an additional parameter that can be sent when Product Action is set to 'checkout'. 
   /// </summary>
    public class CheckoutStep : Parameter
    {
        public CheckoutStep(int value)
            : base(value)
        {
        }

        public override string Name
        {
            get { return "cos"; }
        }

        public override Type ValueType
        {
            get { return typeof(int); }
        }
    }
}
