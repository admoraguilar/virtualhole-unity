﻿namespace EditorStatsReporting.Parameters.TrafficSources
{
   /// <summary>
    /// Specifies the campaign medium.
   /// </summary>
    public class CampaignMedium : Parameter
    {
        public CampaignMedium(string value)
            : base(value)
        {
        }

        public override string Name
        {
            get { return "cm"; }
        }
    }
}
