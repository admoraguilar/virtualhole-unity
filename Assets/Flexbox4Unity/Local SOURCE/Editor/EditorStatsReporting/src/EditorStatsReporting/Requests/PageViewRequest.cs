using System;
using System.Net;
using EditorStatsReporting.Parameters.ContentInformation;
using EditorStatsReporting.Parameters.Hit;

namespace EditorStatsReporting.Requests
{
    public class PageViewRequest : RequestBase
    {
        public PageViewRequest(bool useSsl = false, IWebProxy proxy = null) : base(useSsl, proxy)
        {
            HitType = HitTypes.PageView;
            Parameters.Add(new HitType(HitTypes.PageView));
        }

        protected override void ValidateRequestParams()
        {
            base.ValidateRequestParams();

            if (!Parameters.Exists(p => p is DocumentPath))
            {
                throw new ApplicationException("DocumentPath parameter is missing.");
            }
        }
    }
}
