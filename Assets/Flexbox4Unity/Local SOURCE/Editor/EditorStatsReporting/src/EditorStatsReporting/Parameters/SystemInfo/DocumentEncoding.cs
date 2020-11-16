﻿namespace EditorStatsReporting.Parameters.SystemInfo
{
    /// <summary>
    /// Specifies the character set used to encode the page / document.
    /// </summary>
    public class DocumentEncoding : Parameter
    {
        public DocumentEncoding(string value)
            : base(value)
        {
        }

        public override string Name
        {
            get { return "de"; }
        }
    }
}
