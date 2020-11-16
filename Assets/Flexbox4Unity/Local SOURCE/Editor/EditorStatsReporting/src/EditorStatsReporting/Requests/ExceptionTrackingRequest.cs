using System.Net;
using EditorStatsReporting.Parameters.Hit;

namespace EditorStatsReporting.Requests
{
    public class ExceptionTrackingRequest : RequestBase
    {
        public ExceptionTrackingRequest(bool useSsl = false, IWebProxy proxy = null)
            : base(useSsl, proxy)
        {
            HitType = HitTypes.Exception;
            Parameters.Add(new HitType(HitTypes.Exception));
        }
    }
}
