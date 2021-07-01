using NLog;
using NLog.LayoutRenderers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.Common.LayOutRenderer
{
    [LayoutRenderer("vehicle_sec_id")]
    public class VhSecIDLayoutRenderer : LayoutRenderer
    {
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            var provider = logEvent.FormatProvider;
            var vh = provider as AVEHICLE;
            if (vh != null)
            {
                builder.Append(SCUtility.Trim(vh.CUR_SEC_ID, true));
            }
        }
    }
    [LayoutRenderer("vehicle_adr_id")]
    public class VhAdrIDLayoutRenderer : LayoutRenderer
    {
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            var provider = logEvent.FormatProvider;
            var vh = provider as AVEHICLE;
            if (vh != null)
            {
                builder.Append(SCUtility.Trim(vh.CUR_ADR_ID, true));
            }
        }
    }
}
