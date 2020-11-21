using System;
using System.Net;
using EditorStatsReporting.Parameters.EventTracking;
using EditorStatsReporting.Parameters.Hit;

namespace EditorStatsReporting.Requests
{
    public class EventRequest : RequestBase
    {
        public EventRequest(bool useSsl = false, IWebProxy proxy = null)
            : base(useSsl, proxy)
        {
            HitType = HitTypes.Event;
            Parameters.Add(new HitType(HitTypes.Event));
        }

        protected override void ValidateRequestParams()
        {
            base.ValidateRequestParams();

            if (!Parameters.Exists(p => p is EventCategory))
            {
                throw new ApplicationException("EventCategory parameter is missing.");
            }

            if (!Parameters.Exists(p => p is EventAction))
            {
                throw new ApplicationException("EventAction parameter is missing.");
            }
        }
    }
}
