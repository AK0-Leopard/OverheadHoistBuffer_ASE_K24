using com.mirle.ibg3k0.sc.BLL.Interface;
using com.mirle.ibg3k0.sc.Data.ValueDefMapAction.Events;
using com.mirle.ibg3k0.sc.Data.ValueDefMapAction.Interface;
using com.mirle.ibg3k0.sc.Service.Interface;
using System;

namespace com.mirle.ibg3k0.sc.Service
{
    public class ManualPortService : IManualPortService
    {
        private readonly IManualPortValueDefMapAction manualPortValueDefMapAction;
        private readonly IReportBLL reportBll;
        private readonly IPortDefBLL portDefBLL;
        private readonly IShelfDefBLL shelfDefBLL;
        private readonly ICassetteDataBLL cassetteDataBLL;
        private readonly ICMDBLL commandBLL;
        private readonly IAlarmBLL alarmBLL;

        public ManualPortService(IManualPortValueDefMapAction manualPortValueDefMapAction,
                                 IReportBLL reportBll,
                                 IPortDefBLL portDefBLL,
                                 IShelfDefBLL shelfDefBLL,
                                 ICassetteDataBLL cassetteDataBLL,
                                 ICMDBLL commandBLL,
                                 IAlarmBLL alarmBLL)
        {
            this.manualPortValueDefMapAction = manualPortValueDefMapAction;
            this.reportBll = reportBll;
            this.portDefBLL = portDefBLL;
            this.shelfDefBLL = shelfDefBLL;
            this.cassetteDataBLL = cassetteDataBLL;
            this.commandBLL = commandBLL;
            this.alarmBLL = alarmBLL;

            RegisterEvent();
        }

        private void RegisterEvent()
        {
            manualPortValueDefMapAction.OnLoadPresenceChanged += ManualPortValueDefMapAction_OnLoadPresenceChanged; ;
            manualPortValueDefMapAction.OnWaitIn += ManualPortValueDefMapAction_OnWaitIn;
            manualPortValueDefMapAction.OnBcrReadDone += ManualPortValueDefMapAction_OnBcrReadDone;
            manualPortValueDefMapAction.OnWaitOut += ManualPortValueDefMapAction_OnWaitOut;
            manualPortValueDefMapAction.OnCstRemoved += ManualPortValueDefMapAction_OnCstRemoved;
            manualPortValueDefMapAction.OnDirectionChanged += ManualPortValueDefMapAction_OnDirectionChanged;
            manualPortValueDefMapAction.OnInServiceChanged += ManualPortValueDefMapAction_OnInServiceChanged;
            manualPortValueDefMapAction.OnAlarmHappen += ManualPortValueDefMapAction_OnAlarmHappen;
            manualPortValueDefMapAction.OnAlarmClear += ManualPortValueDefMapAction_OnAlarmClear;
        }

        private void ManualPortValueDefMapAction_OnLoadPresenceChanged(object sender, ManualPortEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void ManualPortValueDefMapAction_OnWaitIn(object sender, ManualPortEventArgs args)
        {
            var info = args.ManualPortPLCInfo;

            if (cassetteDataBLL.GetCarrierByBoxId(info.CarrierIdOfStage1, out var duplicateCarrierId))
            {
                return;
            }

            var cassetteData = new CassetteData();
            cassetteData.BOXID = info.CarrierIdOfStage1;
            reportBll.ReportCarrierWaitIn(cassetteData);
        }

        private void ManualPortValueDefMapAction_OnBcrReadDone(object sender, ManualPortEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void ManualPortValueDefMapAction_OnWaitOut(object sender, ManualPortEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void ManualPortValueDefMapAction_OnCstRemoved(object sender, ManualPortEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void ManualPortValueDefMapAction_OnDirectionChanged(object sender, ManualPortEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void ManualPortValueDefMapAction_OnInServiceChanged(object sender, ManualPortEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void ManualPortValueDefMapAction_OnAlarmHappen(object sender, ManualPortEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void ManualPortValueDefMapAction_OnAlarmClear(object sender, ManualPortEventArgs args)
        {
            throw new NotImplementedException();
        }
    }
}