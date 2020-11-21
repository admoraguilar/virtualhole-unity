﻿namespace EditorStatsReporting.Parameters.TrafficSources
{
    /// <summary>
    /// pecifies the Google Display Ads Id.
    /// </summary>
    public class GoogleDisplayAdsId : Parameter
    {
        public GoogleDisplayAdsId(string value)
            : base(value)
        {
        }

        public override string Name
        {
            get { return "dclid"; }
        }
    }
}
