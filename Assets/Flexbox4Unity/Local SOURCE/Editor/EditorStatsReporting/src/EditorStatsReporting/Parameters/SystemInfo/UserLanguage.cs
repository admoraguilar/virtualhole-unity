﻿namespace EditorStatsReporting.Parameters.SystemInfo
{
    /// <summary>
    /// Specifies the language.
    /// </summary>
    public class UserLanguage : Parameter
    {
        public UserLanguage(string value)
            : base(value)
        {
        }

        public override string Name
        {
            get { return "ul"; }
        }
    }
}
