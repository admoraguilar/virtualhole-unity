﻿namespace EditorStatsReporting.Parameters.AppTracking
{
    /// <summary>
    /// Specifies the application name.
    /// </summary>
    public class ApplicationName : Parameter
    {
        public ApplicationName(string value)
            : base(value)
        {
        }

        public override string Name
        {
            get { return "an"; }
        }
    }
}
