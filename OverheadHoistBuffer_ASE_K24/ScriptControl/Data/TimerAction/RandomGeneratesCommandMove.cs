// ***********************************************************************
// Assembly         : ScriptControl
// Author           : 
// Created          : 03-31-2016
//
// Last Modified By : 
// Last Modified On : 03-24-2016
// ***********************************************************************
// <copyright file="BCSystemStatusTimer.cs" company="">
//     Copyright ©  2014
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using com.mirle.ibg3k0.bcf.Data.TimerAction;
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data.DAO;
using com.mirle.ibg3k0.sc.Data.SECS;
using NLog;

namespace com.mirle.ibg3k0.sc.Data.TimerAction
{
    /// <summary>
    /// Class BCSystemStatusTimer.
    /// </summary>
    /// <seealso cref="com.mirle.ibg3k0.bcf.Data.TimerAction.ITimerAction" />
    public class RandomGeneratesCommandMove : ITimerAction
    {
        /// <summary>
        /// The logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// The sc application
        /// </summary>
        protected SCApplication scApp = null;
        private List<TranTask> tranTasks = null;

        public Dictionary<string, List<TranTask>> dicTranTaskSchedule_Clear_Dirty = null;
        public List<String> SourcePorts_None = null;
        public List<String> SourcePorts_Clear = null;
        public List<String> SourcePorts_Dirty = null;


        Random rnd_Index = new Random(Guid.NewGuid().GetHashCode());

        /// <summary>
        /// Initializes a new instance of the <see cref="BCSystemStatusTimer"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="intervalMilliSec">The interval milli sec.</param>
        public RandomGeneratesCommandMove(string name, long intervalMilliSec)
            : base(name, intervalMilliSec)
        {

        }
        /// <summary>
        /// Initializes the start.
        /// </summary>
        public override void initStart()
        {
            scApp = SCApplication.getInstance();

            tranTasks = scApp.CMDBLL.loadTranTasks();

        }
        /// <summary>
        /// Timer Action的執行動作
        /// </summary>
        /// <param name="obj">The object.</param>
        private long syncPoint = 0;
        public override void doProcess(object obj)
        {
            if (!DebugParameter.CanAutoRandomGeneratesCommand) return;
            if (System.Threading.Interlocked.Exchange(ref syncPoint, 1) == 0)
            {
                try
                {
                    List<AVEHICLE> vhs = scApp.VehicleBLL.cache.loadVhs();
                    foreach (AVEHICLE vh in vhs)
                    {
                        if (vh.isTcpIpConnect &&
                            vh.MODE_STATUS == ProtocolFormat.OHTMessage.VHModeStatus.AutoRemote &&
                            vh.ACT_STATUS == ProtocolFormat.OHTMessage.VHActionStatus.NoCommand &&
                            !SCUtility.isEmpty(vh.CUR_ADR_ID) &&
                            !scApp.CMDBLL.isCMD_OHTCExcuteByVh(vh.VEHICLE_ID))
                        {
                            List<PortDef> can_avoid_port = scApp.PortDefBLL.cache.loadCanAvoidPortDefs();

                            bool has_command_to_12206 = scApp.CMDBLL.cache.IsExcuteCmdByToAdr("12206");
                            if (has_command_to_12206)
                                can_avoid_port = can_avoid_port.Where(port => !SCUtility.isMatche(port.ADR_ID, "12206")).ToList();
                            var avoid_port = can_avoid_port.OrderBy(port => port.AvoidCount).FirstOrDefault();
                            string avoid_adr = ""; ;
                            if (avoid_port != null)
                            {
                                avoid_adr = avoid_port.ADR_ID;
                                avoid_port.AvoidCount++;
                            }
                            else
                            {
                                return;
                            }


                            if (!SCUtility.isMatche(avoid_adr, vh.CUR_ADR_ID))
                            {
                                scApp.CMDBLL.doCreatTransferCommand(vh.VEHICLE_ID,
                                                                    cmd_type: E_CMD_TYPE.Move,
                                                                    destination_address: avoid_adr);
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception");
                }
                finally
                {
                    System.Threading.Interlocked.Exchange(ref syncPoint, 0);
                }
            }
            //scApp.BCSystemBLL.reWriteBCSystemRunTime();
        }
    }

}

