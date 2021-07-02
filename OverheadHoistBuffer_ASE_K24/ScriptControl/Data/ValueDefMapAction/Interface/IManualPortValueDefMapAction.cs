using com.mirle.ibg3k0.bcf.Data.ValueDefMapAction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.Data.ValueDefMapAction.Interface
{
    public interface IManualPortValueDefMapAction : IValueDefMapAction
    {
        event EventHandler OnWaitIn;
    }
}