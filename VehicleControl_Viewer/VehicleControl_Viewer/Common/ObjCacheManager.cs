using com.mirle.AK0.ProtocolFormat;
using com.mirle.AK0.ProtocolFormat.VehicleControlPublishMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleControl_Viewer.App;
using VehicleControl_Viewer.Data;
using VehicleControl_Viewer.Vo;

namespace VehicleControl_Viewer.UI.Components
{
    public class ObjCacheManager
    {
        public event EventHandler RailStatusChanged;
        public event EventHandler TrackStatusChanged;
        public Dictionary<string, Vehicle> VehiclesInfo { get; } = new Dictionary<string, Vehicle>();
        public Dictionary<string, PortInfo> PortsInfo { get; } = new Dictionary<string, PortInfo>();
        public List<SegmentInfo> Segments { get; private set; }
        public List<TransferCommand> TransferCommandInfos { get; private set; } = new List<TransferCommand>();
        public List<TaskCommand> TaskCommandInfos { get; private set; } = new List<TaskCommand>();
        VehicleControlService vehicleControlService = null;
        public List<string>TrackStatusChangeLines = new List<string>();
        public List<Alarm>alarmList = new List<Alarm>();
        public LineInfo LineInfo { get; private set; } = new LineInfo();
        
        public ObjCacheManager(WindownApplication app)
        {
            vehicleControlService = app.VehicleControlService;

            initialVhObject();
            initialRailObjectInfo();
            initialPortObject();
        }

        private void initialPortObject()
        {
            var request_result = vehicleControlService.RequestPortsInfo();
            if (request_result.isRequestSunncess)
            {
                foreach (var info in request_result.replyResult.PortsInfo)
                {
                    PortsInfo.Add(info.PortId, info);
                }
            }
        }

        public void refreshTransferCommandInfos(List<TransferCommand> newTranCmdInfos)
        {
            TransferCommandInfos = newTranCmdInfos;
        }
        public void refreshTaskCommandInfos(List<TaskCommand> newTaskCmdInfos)
        {
            TaskCommandInfos = newTaskCmdInfos;
        }
        public void refreshAlarmList(List<alarm> _alarmList)
        {
            //alarmList = _alarmList;
            alarmList.Clear();
            foreach(var v in _alarmList)
            {
                Alarm temp = new Alarm();
                temp.EQ_ID = v.EQID;
                temp.Unit_ID = v.UnitID;
                temp.RPT_dateTime = v.RPTDateTime;
                temp.Code = v.Code;
                temp.level = v.Level;
                temp.alarmStatus = v.AlarmStatus;
                temp.alarmAffectCount = v.AlarmAffectCount;
                temp.Description = v.Description;
                alarmList.Add(temp);
            }
        }

        private void initialRailObjectInfo()
        {
            Segments = vehicleControlService.GetSegmentInfos();
        }
        public void refreshRailObjectInfo()
        {
            Segments = vehicleControlService.GetSegmentInfos();
            RailStatusChanged?.Invoke(this, EventArgs.Empty);
        }
        public void refreshTrackStatusChange()
        {
            //在OHBC告訴Viewer，Section狀態有異動，我們要再問OHBC有哪些section不能用~
            TrackStatusChangeLines = vehicleControlService.GetLineDisable();
            //問完不能用的sec就要觸發轉轍器修改的工作了~
            TrackStatusChanged?.Invoke(this, EventArgs.Empty);
        }
        public void refreshLineInfo(LineInfo newLineInfo)
        {
            LineInfo = newLineInfo;
        }
        public Vehicle GetVehicle(string vhID)
        {
            return VehiclesInfo[vhID];
        }
        public List<SegmentInfo> getDisableSegment()
        {
            if (Segments == null || Segments.Count == 0) return new List<SegmentInfo>();
            return Segments.Where(seg => seg.Status == SegmentStatus.Closed).ToList();
        }
        public List<Vehicle> LoadAllVehicles()
        {
            return VehiclesInfo.Values.ToList();
        }
        private void initialVhObject()
        {
            List<Vehicle> vhs = new List<Vehicle>();
            var requset_result = vehicleControlService.RequestVehicleSummary();
            if (requset_result.isRequestSunncess)
            {
                int i = 1;
                foreach (var vh in requset_result.vhSummary.VehiclesSummary)
                {
                    string vh_id = vh.VEHICLEID;
                    var v = new Vehicle(i, vh_id);
                    VehiclesInfo.Add(vh_id, v);
                    i++;
                }
            }
            else
            {
                for (int i = 1; i <= 36; i++)
                {
                    string vh_id = $"OHx{i.ToString("00")}";
                    var vh = new Vehicle(i, vh_id);
                    VehiclesInfo.Add(vh_id, vh);
                }
            }
        }
    }
}
