using System.Net;
using EditorStatsReporting.Parameters.Hit;

namespace EditorStatsReporting.Requests
{
    public class ScreenTrackingRequest : RequestBase
    {
        public ScreenTrackingRequest(bool useSsl = false, IWebProxy proxy = null)
            : base(useSsl, proxy)
        {
            HitType = HitTypes.ScreenView;
            Parameters.Add(new HitType(HitTypes.ScreenView));
        }
    }
}
