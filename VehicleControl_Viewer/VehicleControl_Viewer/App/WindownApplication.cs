using Quartz;
using Quartz.Impl;
using STAN.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleControl_Vierwer.Common;
using VehicleControl_Viewer.BLL;
using VehicleControl_Viewer.Common;
using VehicleControl_Viewer.Data;
using VehicleControl_Viewer.Protots;
using VehicleControl_Viewer.UI.Components;

namespace VehicleControl_Viewer.App
{
    public class WindownApplication
    {

        public const string DateTimeFormat_23 = "yyyy-MM-dd HH:mm:ss.fff";

        public const string NATS_SUBJECT_VH_INFO_0 = "NATS_SUBJECT_KEY_VH_INFO_{0}_TEST";
        public const string NATS_SUBJECT_RAIL_STATUS_CHANGE = "NATS_SUBJECT_RAIL_STATUS_CHANGE";
        public const string NATS_SUBJECT_TRANSFER_COMMAND_CHANGE = "NATS_SUBJECT_TRANSFER_COMMAND_CHANGE";
        public const string NATS_SUBJECT_TASK_COMMAND_CHANGE = "NATS_SUBJECT_TASK_COMMAND_CHANGE";

        private IScheduler Scheduler { get; set; }

        private RedisCacheManager redisCacheManager = null;
        public ObjCacheManager objCacheManager { get; private set; }
        public NatsManager NatsManager { get; private set; }
        public VehicleBLL VehicleBLL { get; private set; }
        public RailBLL RailBLL { get; private set; }
        public TransferCommandBLL TransferCommandBLL { get; private set; }
        public TaskCommandBLL TaskCommandBLL { get; private set; }

        public VehicleControlService VehicleControlService { get; private set; }

        private static Object _lock = new Object();
        private static WindownApplication application;
        public static WindownApplication getInstance()
        {
            if (application == null)
            {
                lock (_lock)
                {
                    if (application == null)
                    {
                        application = new WindownApplication();
                    }
                }
            }
            return application;
        }
        WindownApplication()
        {
            NatsManager = new NatsManager("ASE_K24", "test-cluster", "viewer_ohxc2");

            VehicleControlService = new VehicleControlService();

            redisCacheManager = new RedisCacheManager("");

            VehicleBLL = new VehicleBLL(this);
            RailBLL = new RailBLL(this);
            TransferCommandBLL = new TransferCommandBLL(this);
            objCacheManager = new ObjCacheManager(this);
            TaskCommandBLL = new TaskCommandBLL(this);
        }
        public async void Start()
        {
            await NatsManager.StartConnection();

            SubscriberNatsEvent();
        }

        private void SubscriberNatsEvent()
        {
            try
            {
                VehicleBLL.SubscriberVehicleInfos();
                RailBLL.SubscriberRailStatusChangeEvent();
                TransferCommandBLL.SubscriberTransferCommandInfoChangeEvent();
                TaskCommandBLL.SubscriberTaskCommandInfoChangeEvent();
            }
            catch (Exception ex)
            {
                //logger.Error(ex, "Exception");
            }
        }



    }
}
