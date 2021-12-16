using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.Service.Interface
{
    public interface IVehicleTransferHandler
    {
        (bool isContinue, string RemaneBox) IDReadMismatchHappend(string vhID, string readBOXID);
        bool CommandCompleteByIDMismatch(string vhID, string finishCommandID);
        (bool isContinue, string RemaneBox) IDReadFailHappend(string vhID, string readBOXID);
        bool CommandCompleteByIDReadFail(string vhID, string finishCommandID);

        void CommandCompleteByCancel(string vhID, string finishCommandID);
        void CommandCompleteByAbort(string vhID, string finishCommandID);

    }
}
