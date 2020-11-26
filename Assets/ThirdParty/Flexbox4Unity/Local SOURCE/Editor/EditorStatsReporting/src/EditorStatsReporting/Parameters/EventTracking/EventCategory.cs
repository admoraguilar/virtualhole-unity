﻿using System.Collections.Generic;

namespace EditorStatsReporting.Parameters.EventTracking
{
    /// <summary>
    /// Specifies the event category. Must not be empty.
    /// </summary>
    public class EventCategory : Parameter
    {
        public EventCategory(string value)
            : base(value)
        {
        }

        public override string Name
        {
            get { return "ec"; }
        }

        public override List<string> SupportedHitTypes
        {
            get { return new List<string> { HitTypes.Event }; }
        }
    }
}
