using System;
using System.Net;
using EditorStatsReporting.Parameters.ECommerce;
using EditorStatsReporting.Parameters.Hit;

namespace EditorStatsReporting.Requests
{
    public class TransactionRequest : RequestBase
    {
        public TransactionRequest(bool useSsl = false, IWebProxy proxy = null)
            : base(useSsl, proxy)
        {
            HitType = HitTypes.Transaction;
            Parameters.Add(new HitType(HitTypes.Transaction));
        }

        protected override void ValidateRequestParams()
        {
            base.ValidateRequestParams();

            if (!Parameters.Exists(p => p is TransactionId))
            {
                throw new ApplicationException("TransactionId parameter is missing.");
            }
        }
    }
}
