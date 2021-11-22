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
// 2020/04/17    Jason Wu       N/A            A0.01   加入NTB Type 選項與處理內容
// ***********************************************************************
using com.mirle.ibg3k0.bcf.Data.TimerAction;
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Common;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace com.mirle.ibg3k0.sc.Data.TimerAction
{
    /// <summary>
    /// Class BCSystemStatusTimer.
    /// </summary>
    /// <seealso cref="com.mirle.ibg3k0.bcf.Data.TimerAction.ITimerAction" />
    public class RandomGeneratesCommandTimerAction : ITimerAction
    {
        /// <summary>
        /// The logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// The sc application
        /// </summary>
        protected SCApplication scApp = null;
        List<ShelfDef> shelfDefs = null;
        List<APORT> AllTestAGVStationPorts = null;
        List<APORT> willTestAGVStationPorts = null;
        Dictionary<string, List<string>> cycleRunRecord_VhAndShelf = new Dictionary<string, List<string>>();
        Random rnd_Index = new Random(Guid.NewGuid().GetHashCode());

        /// <summary>
        /// Initializes a new instance of the <see cref="BCSystemStatusTimer"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="intervalMilliSec">The interval milli sec.</param>
        public RandomGeneratesCommandTimerAction(string name, long intervalMilliSec)
            : base(name, intervalMilliSec)
        {

        }
        /// <summary>
        /// Initializes the start.
        /// </summary>
        public override void initStart()
        {
            scApp = SCApplication.getInstance();
            AllTestAGVStationPorts = scApp.PortBLL.OperateCatch.loadAGVStationPorts();
            List<AVEHICLE> vhs = scApp.VehicleBLL.cache.loadVhs();
            foreach (var vh in vhs)
            {
                cycleRunRecord_VhAndShelf.Add(vh.VEHICLE_ID, new List<string>());
            }
        }
        /// <summary>
        /// Timer Action的執行動作
        /// </summary>
        /// <param name="obj">The object.</param>
        private long syncPoint = 0;
        public override void doProcess(object obj)
        {

            if (System.Threading.Interlocked.Exchange(ref syncPoint, 1) == 0)
            {
                try
                {
                    if (!DebugParameter.CanAutoRandomGeneratesCommand)
                    {
                        shelfDefs = null;
                        willTestAGVStationPorts = null;
                        return;
                    }

                    switch (DebugParameter.cycleRunType)
                    {
                        //case DebugParameter.CycleRunType.shelf:
                        //    ShelfTest();
                        //    break;
                        case DebugParameter.CycleRunType.shelfByOrder:
                            ShelfTestByOrder();
                            break;
                        //case DebugParameter.CycleRunType.AGVStation:
                        //    AGVStationTest();
                        //    break;
                        //case DebugParameter.CycleRunType.CV:
                        //    CVPortTest();
                        //    break;
                        //case DebugParameter.CycleRunType.NTB:
                        //    NTBPortTest(); // A0.01
                        //    break;
                        case DebugParameter.CycleRunType.shelfByManualCMD:
                            ShelfByManualMCS();
                            break;
                        case DebugParameter.CycleRunType.DemoRun:
                            DemoRun();
                            break;
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
        }

        private void ShelfTest()
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
                    List<CassetteData> cassetteDatas = scApp.CassetteDataBLL.loadCassetteData();
                    if (cassetteDatas == null || cassetteDatas.Count() == 0) return;
                    //找一份目前儲位的列表
                    if (shelfDefs == null || shelfDefs.Count == 0)
                        shelfDefs = scApp.ShelfDefBLL.LoadEnableShelf();
                    //如果取完還是空的 就跳出去
                    if (shelfDefs == null || shelfDefs.Count == 0)
                        return;
                    //取得目前當前在線內的Carrier
                    //找出在儲位中的Cassette
                    cassetteDatas = cassetteDatas.Where(cst => cst.Carrier_LOC.StartsWith("10") ||
                                                               cst.Carrier_LOC.StartsWith("11") ||
                                                               cst.Carrier_LOC.StartsWith("21") ||
                                                               cst.Carrier_LOC.StartsWith("20")).
                                                               ToList();
                    List<string> current_cst_at_shelf_id = cassetteDatas.
                        Select(cst => SCUtility.Trim(cst.Carrier_LOC, true)).
                        ToList();
                    //刪除目前cst所在的儲位，讓他排除在Cycle Run的列表中
                    foreach (var shelf in shelfDefs.ToList())
                    {
                        if (current_cst_at_shelf_id.Contains(SCUtility.Trim(shelf.ShelfID)))
                        {
                            shelfDefs.Remove(shelf);
                        }
                    }

                    foreach (var shelf in shelfDefs.ToList())
                    {
                        if (scApp.CMDBLL.hasExcuteCMDByTargetPortID(shelf.ShelfID))
                        {
                            shelfDefs.Remove(shelf);
                        }
                    }


                    foreach (var cst in cassetteDatas.ToList())
                    {
                        if (scApp.CMDBLL.hasExcuteCMDByBoxID(cst.BOXID))
                            cassetteDatas.Remove(cst);
                    }


                    //隨機找出一個要放置的shelf
                    CassetteData process_cst = cassetteDatas[0];
                    int task_RandomIndex = rnd_Index.Next(shelfDefs.Count - 1);
                    ShelfDef target_shelf_def = shelfDefs[task_RandomIndex];
                    scApp.MapBLL.getAddressID(process_cst.Carrier_LOC, out string from_adr);
                    bool isSuccess = true;
                    isSuccess &= scApp.CMDBLL.doCreatTransferCommand(vh.VEHICLE_ID, "", process_cst.CSTID.Trim(),
                                        E_CMD_TYPE.LoadUnload,
                                        process_cst.Carrier_LOC,
                                        target_shelf_def.ShelfID, 0, 0,
                                        process_cst.BOXID.Trim(), process_cst.LotID,
                                        from_adr, target_shelf_def.ADR_ID);
                    shelfDefs.Remove(target_shelf_def);
                }
            }
        }

        private void ShelfByManualMCS()
        {
            List<AVEHICLE> vhs = scApp.VehicleBLL.cache.loadVhs();
            int ready_vh_count = vhs.Where(vh => vh.IsReady()).Count();
            int current_mcs_cmd = scApp.CMDBLL.getCMD_MCSIsUnfinishedCount();
            if (ready_vh_count > current_mcs_cmd)
            {
                List<CassetteData> cassetteDatas = scApp.CassetteDataBLL.loadCassetteData();
                if (cassetteDatas == null || cassetteDatas.Count() == 0) return;
                //找一份目前儲位的列表
                if (shelfDefs == null || shelfDefs.Count == 0)
                    shelfDefs = scApp.ShelfDefBLL.LoadEnableShelf();
                //如果取完還是空的 就跳出去
                if (shelfDefs == null || shelfDefs.Count == 0)
                    return;
                shelfDefs = shelfDefs.Where(s => !SCUtility.isMatche(s.ADR_ID, "99999")).ToList();

                //取得目前當前在線內的Carrier
                //找出在儲位中的Cassette
                cassetteDatas = cassetteDatas.Where(cst => cst.Carrier_LOC.StartsWith("10") ||
                                                           cst.Carrier_LOC.StartsWith("11") ||
                                                           cst.Carrier_LOC.StartsWith("21") ||
                                                           cst.Carrier_LOC.StartsWith("20") ||
                                                           cst.Carrier_LOC.Contains("OHB01_CR")).
                                                           ToList();
                cassetteDatas = cassetteDatas.Where(cst => !SCUtility.isMatche(cst.CurrentAdrID(scApp), "99999")).ToList();

                List<string> current_cst_at_shelf_id = cassetteDatas.
                    Select(cst => SCUtility.Trim(cst.Carrier_LOC, true)).
                    ToList();
                //刪除目前cst所在的儲位，讓他排除在Cycle Run的列表中
                foreach (var shelf in shelfDefs.ToList())
                {
                    if (current_cst_at_shelf_id.Contains(SCUtility.Trim(shelf.ShelfID)))
                    {
                        shelfDefs.Remove(shelf);
                    }
                }

                foreach (var shelf in shelfDefs.ToList())
                {
                    if (scApp.CMDBLL.getCMD_MCSIsUnfinishedCountByPortID(shelf.ShelfID) > 0)
                    {
                        shelfDefs.Remove(shelf);
                    }
                }


                foreach (var cst in cassetteDatas.ToList())
                {
                    if (scApp.CMDBLL.getCMD_MCSIsUnfinishedCountByBoxID(cst.BOXID) > 0)
                        cassetteDatas.Remove(cst);
                }
                if (cassetteDatas == null || cassetteDatas.Count == 0)
                {
                    return;
                }

                //隨機找出一個要放置的 shelf
                //CassetteData process_cst = cassetteDatas[0];

                CassetteData process_cst = null;
                foreach (var cst in cassetteDatas)
                {
                    if (scApp.ShelfDefBLL.isEnable(cst.Carrier_LOC))
                    {
                        process_cst = cst;
                        break;
                    }
                }
                if (process_cst == null)
                    return;

                int cst_adr_id = scApp.TransferService.portINIData[process_cst.Carrier_LOC].ADR_ID;

                int shelf_adr_id = scApp.TransferService.portINIData[process_cst.Carrier_LOC].ADR_ID;


                int task_RandomIndex = rnd_Index.Next(shelfDefs.Count - 1);
                ShelfDef target_shelf_def = shelfDefs[task_RandomIndex];
                string result = scApp.TransferService.Manual_InsertCmd
                     (
                         source: process_cst.Carrier_LOC,
                         dest: target_shelf_def.ShelfID,
                         sourceCmd: "ShelfByManualMCS"
                     );
                if (SCUtility.isMatche(result, "OK"))
                {
                    shelfDefs.Remove(target_shelf_def);
                }
            }
        }
        private void ShelfTestByOrder()
        {
            var mcs_cmds = scApp.CMDBLL.loadACMD_MCSIsUnfinished();
            string cycle_run_vh = DebugParameter.cycleRunVh;
            AVEHICLE vh = scApp.VehicleBLL.cache.getVhByID(cycle_run_vh);
            if (vh == null) return;
            if (vh.ACT_STATUS == ProtocolFormat.OHTMessage.VHActionStatus.Commanding)
            {
                return;
            }
            if (vh.ERROR == ProtocolFormat.OHTMessage.VhStopSingle.StopSingleOn)
            {
                return;
            }
            if (vh.IS_INSTALLED)
            {
                return;
            }
            if (vh.HAS_CST == 1)
            {
                return;
            }
            bool has_cmd_excute = scApp.CMDBLL.isCMD_OHTCExcuteByVh(vh.VEHICLE_ID);
            if (has_cmd_excute)
            {
                return;
            }

            int mcs_cmds_excute_by_bay = mcs_cmds.Where(cmd =>
            {
                bool is_shelf = scApp.TransferService.isShelfPort(cmd.HOSTDESTINATION);
                if (!is_shelf) return false;
                string current_bay_id = getCurrentBayID(cmd.HOSTDESTINATION);
                if (SCUtility.isMatche(current_bay_id, DebugParameter.cycleRunBay))
                {
                    return true;
                }
                return false;
            }).Count();

            if (mcs_cmds_excute_by_bay > 0) return;

            List<CassetteData> cassetteDatas = scApp.CassetteDataBLL.loadCassetteData();
            if (shelfDefs == null || shelfDefs.Count == 0)
            {
                refreshCurrentCycleRunBay(cassetteDatas);
            }
            else
            {
                var current_cycle_run_bay = shelfDefs.FirstOrDefault().BayID;
                if (!SCUtility.isMatche(current_cycle_run_bay, DebugParameter.cycleRunBay))
                {
                    refreshCurrentCycleRunBay(cassetteDatas);
                }
            }

            var process_csts = cassetteDatas.Where(cst => SCUtility.isMatche(cst.CurrentBayID(scApp), DebugParameter.cycleRunBay)).ToList();
            if (process_csts == null) return;
            CassetteData process_cst = null;
            foreach (var cst in process_csts)
            {
                if (scApp.ShelfDefBLL.isEnable(cst.Carrier_LOC))
                {
                    process_cst = cst;
                    break;
                }
            }
            if (process_cst == null)
                return;

            var next_cycle_run_shelf = shelfDefs.FirstOrDefault();
            if (next_cycle_run_shelf == null) return;

            string result = scApp.TransferService.Manual_InsertCmd
                 (
                     source: process_cst.Carrier_LOC,
                     dest: next_cycle_run_shelf.ShelfID,
                     sourceCmd: "ShelfTestByOrder",
                     craneID: vh.VEHICLE_ID
                 );
            if (SCUtility.isMatche(result, "OK"))
            {
                shelfDefs.Remove(next_cycle_run_shelf);
            }

            //List<AVEHICLE> vhs = scApp.VehicleBLL.cache.loadVhs();
            //foreach (AVEHICLE vh in vhs)
            //{
            //    if (vh.isTcpIpConnect &&
            //        vh.MODE_STATUS == ProtocolFormat.OHTMessage.VHModeStatus.AutoRemote &&
            //        vh.ACT_STATUS == ProtocolFormat.OHTMessage.VHActionStatus.NoCommand &&
            //        !SCUtility.isEmpty(vh.CUR_ADR_ID) &&
            //        vh.HAS_CST == 0 &&
            //        !scApp.CMDBLL.isCMD_OHTCExcuteByVh(vh.VEHICLE_ID))
            //    {
            //        List<string> vh_has_been_excuted_shelf = cycleRunRecord_VhAndShelf[vh.VEHICLE_ID];
            //        List<CassetteData> cassetteDatas = scApp.CassetteDataBLL.loadCassetteData();
            //        if (cassetteDatas == null || cassetteDatas.Count() == 0) return;
            //        //找一份目前儲位的列表
            //        //如果取完還是空的 就跳出去
            //        if (shelfDefs == null || shelfDefs.Count == 0)
            //            return;
            //        //去除掉該vh已經跑過的shelf
            //        shelfDefs = shelfDefs.Where(s => !vh_has_been_excuted_shelf.Contains(s.ShelfID))
            //                             .OrderBy(s => s.ShelfID)
            //                             .ToList();


            //        //取得目前當前在線內的Carrier
            //        //找出在儲位中的Cassette
            //        cassetteDatas = cassetteDatas.
            //            Where(cst => scApp.TransferService.isShelfPort(cst.Carrier_LOC)).ToList();
            //        List<string> current_cst_at_shelf_id = cassetteDatas.
            //            Select(cst => SCUtility.Trim(cst.Carrier_LOC, true)).
            //            ToList();
            //        foreach (var shelf in shelfDefs.ToList())
            //        {
            //            if (current_cst_at_shelf_id.Contains(shelf.ShelfID))
            //            {
            //                shelfDefs.Remove(shelf);
            //            }
            //        }

            //        //刪除已經在搬送的
            //        foreach (var cst in cassetteDatas.ToList())
            //        {
            //            if (cst.hasCommandExcute(scApp.CMDBLL))
            //            {
            //                cassetteDatas.Remove(cst);
            //            }
            //        }

            //        foreach (var shelf in shelfDefs.ToList())
            //        {
            //            if (!scApp.GuideBLL.IsRoadWalkable(vh.CUR_ADR_ID, shelf.ADR_ID).isSuccess)
            //            {
            //                shelfDefs.Remove(shelf);
            //            }
            //        }


            //        //隨機找出一個要放置的shelf
            //        CassetteData process_cst = cassetteDatas[0];
            //        //int task_RandomIndex = rnd_Index.Next(shelfDefs.Count - 1);
            //        ShelfDef target_shelf_def = shelfDefs.First();
            //        scApp.MapBLL.getAddressID(process_cst.Carrier_LOC, out string from_adr);
            //        bool isSuccess = true;

            //        //isSuccess &= scApp.CMDBLL.doCreatTransferCommand(vh.VEHICLE_ID, "", process_cst.CSTID.Trim(),
            //        //                    E_CMD_TYPE.LoadUnload,
            //        //                    process_cst.Carrier_LOC,
            //        //                    target_shelf_def.ShelfID, 0, 0,
            //        //                    process_cst.BOXID.Trim(), process_cst.LotID,
            //        //                    from_adr, target_shelf_def.ADR_ID);

            //        scApp.TransferService.Manual_InsertCmd
            //            (
            //                source: process_cst.Carrier_LOC,
            //                dest: target_shelf_def.ShelfID,
            //                sourceCmd: "ShelfTestByOrder",
            //                craneID: vh.VEHICLE_ID
            //            );
            //        if (isSuccess)
            //        {
            //            cycleRunRecord_VhAndShelf[vh.VEHICLE_ID].Add(target_shelf_def.ShelfID);
            //        }
            //    }
            //}
        }

        private void DemoRun()
        {
            List<string> cycle_run_csts = stringToStringArray(DebugParameter.cycleRunCSTs);
            if (cycle_run_csts == null || cycle_run_csts.Count == 0)
            {
                return;
            }
            var mcs_cmds = scApp.CMDBLL.loadACMD_MCSIsUnfinished();
            string demo_run_bay = DebugParameter.cycleRunBay;

            int mcs_cmds_excute_by_bay = mcs_cmds.Where(cmd =>
            {
                bool is_shelf = scApp.TransferService.isShelfPort(cmd.HOSTDESTINATION);
                if (!is_shelf) return false;
                string current_bay_id = getCurrentBayID(cmd.HOSTDESTINATION);
                if (SCUtility.isMatche(current_bay_id, demo_run_bay))
                {
                    return true;
                }
                return false;
            }).Count();

            if (mcs_cmds_excute_by_bay >= 2) return;



            List<CassetteData> cassetteDatas = scApp.CassetteDataBLL.loadCassetteData();
            refreshCurrentCycleRunBay(cassetteDatas, mcs_cmds);

            var process_csts = cassetteDatas.Where(cst => SCUtility.isMatche(cst.CurrentBayID(scApp), demo_run_bay)).ToList();
            if (process_csts == null || process_csts.Count == 0) return;
            CassetteData process_cst = null;
            foreach (var cst in process_csts)
            {
                bool is_cycle_run_cst = cycle_run_csts.Contains(cst.BOXID);
                if (!is_cycle_run_cst)
                {
                    continue;
                }
                bool hsa_mcd_excute = mcs_cmds.Where(cmd => SCUtility.isMatche(cmd.BOX_ID, cst.BOXID)).Count() > 0;
                if (hsa_mcd_excute)
                {
                    continue;
                }
                if (!scApp.ShelfDefBLL.isEnable(cst.Carrier_LOC))
                {
                    continue;
                }
                process_cst = cst;
                break;
            }
            if (process_cst == null)
                return;

            ShelfDef shelf = scApp.ShelfDefBLL.GetShelfDataByID(process_cst.Carrier_LOC);


            ShelfDef next_cycle_run_shelf = null;
            if (shelf.SeqNo.CompareTo("020") > 0)
            {
                next_cycle_run_shelf = shelfDefs.FirstOrDefault();
            }
            else
            {
                next_cycle_run_shelf = shelfDefs.Last();
            }

            if (next_cycle_run_shelf == null) return;

            string result = scApp.TransferService.Manual_InsertCmd
                 (
                     source: process_cst.Carrier_LOC,
                     dest: next_cycle_run_shelf.ShelfID,
                     sourceCmd: "DemoRun"
                 );


        }
        List<string> stringToStringArray(string value)
        {
            if (value.Contains(","))
            {
                return value.Split(',').ToList();
            }
            else
            {
                if (SCUtility.isEmpty(value))
                {
                    return new List<string>();
                }
                else
                {
                    return new List<string>() { value };
                }
            }
        }
        public string getCurrentBayID(string shelfID)
        {
            if (shelfID == null || shelfID.Length < 6)
            {
                return "";
            }
            //從倒數第二個字取出兩個
            //100101
            string bay_id = shelfID.Substring(shelfID.Length - 2, 2);
            return bay_id;
        }

        private void refreshCurrentCycleRunBay(List<CassetteData> cassetteDatas, List<ACMD_MCS> cmdMCS = null)
        {
            shelfDefs = scApp.ShelfDefBLL.LoadEnableShelf();

            shelfDefs = shelfDefs.Where(s => SCUtility.isMatche(s.BayID, DebugParameter.cycleRunBay)).OrderBy(s => s.SeqNo).ToList();

            //取得目前當前在線內的Carrier
            //找出在儲位中的Cassette
            cassetteDatas = cassetteDatas.
                            Where(cst => scApp.TransferService.isShelfPort(cst.Carrier_LOC)).
                            ToList();
            List<string> current_cst_at_shelf_id = cassetteDatas.
                Select(cst => SCUtility.Trim(cst.Carrier_LOC, true)).
                ToList();
            foreach (var shelf in shelfDefs.ToList())
            {
                if (current_cst_at_shelf_id.Contains(shelf.ShelfID))
                {
                    shelfDefs.Remove(shelf);
                }
            }
            //過濾掉目的地已經有命令的儲位
            foreach (var shelf in shelfDefs.ToList())
            {
                bool has_cmd = cmdMCS.Where(cmd => SCUtility.isMatche(cmd.HOSTDESTINATION, shelf.ShelfID)).Count() > 0;

                if (has_cmd)
                {
                    shelfDefs.Remove(shelf);
                }
            }
        }


        private void AGVStationTest()
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

                    //找出目前的AGVStation Port
                    if (willTestAGVStationPorts == null || willTestAGVStationPorts.Count == 0)
                    {
                        willTestAGVStationPorts = scApp.PortBLL.OperateCatch.loadAGVStationPorts();
                        willTestAGVStationPorts = willTestAGVStationPorts.Where(port => scApp.TransferService.GetPLC_PortData(port.PORT_ID).IsReadyToLoad &&
                                                                                        scApp.TransferService.GetPLC_PortData(port.PORT_ID).IsAutoMode).
                                                                          ToList();
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(RandomGeneratesCommandTimerAction), Device: "OHTC",
                                 Data: $"Load ok:{string.Join(",", willTestAGVStationPorts.Select(port => port.PORT_ID).ToList())}");
                    }
                    //如果取完還是空的 就跳出去
                    if (willTestAGVStationPorts == null || willTestAGVStationPorts.Count == 0)
                    {
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(RandomGeneratesCommandTimerAction), Device: "OHTC",
                                 Data: $"no agv station list.");
                        return;
                    }


                    //找出目前Unload Ok的AGV Station
                    var unload_ok_port = AllTestAGVStationPorts.Where(port => scApp.TransferService.GetPLC_PortData(port.PORT_ID).IsReadyToUnload &&
                                                                              scApp.TransferService.GetPLC_PortData(port.PORT_ID).IsAutoMode).
                                                                FirstOrDefault();
                    //var unload_ok_port = AllTestAGVStationPorts.FirstOrDefault();
                    if (unload_ok_port == null)
                    {
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(RandomGeneratesCommandTimerAction), Device: "OHTC",
                                 Data: $"no unload ok of agv station, can't execute cycle run test.");
                        return;
                    }
                    willTestAGVStationPorts.Remove(unload_ok_port);
                    //找出目前load Ok的AGV Station
                    var load_ok_ports = willTestAGVStationPorts.Where(port => scApp.TransferService.GetPLC_PortData(port.PORT_ID).IsReadyToLoad &&
                                                                              scApp.TransferService.GetPLC_PortData(port.PORT_ID).IsAutoMode).
                                                                ToList();
                    if (load_ok_ports == null || load_ok_ports.Count == 0)
                    {
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(RandomGeneratesCommandTimerAction), Device: "OHTC",
                                 Data: $"no load ok of agv station, can't execute cycle run test.");
                        return;
                    }
                    //隨機找出一個要放置的port
                    var source_port_info = scApp.TransferService.GetPLC_PortData(unload_ok_port.PORT_ID);
                    string box_id = SCUtility.isEmpty(source_port_info.BoxID) ? "BOX01" : SCUtility.Trim(source_port_info.BoxID);
                    string cst_id = source_port_info.CassetteID.ToUpper().Contains("NO") ? "" : SCUtility.Trim(source_port_info.CassetteID);
                    int task_RandomIndex = rnd_Index.Next(load_ok_ports.Count - 1);
                    var target_port_def = load_ok_ports[task_RandomIndex];
                    bool isSuccess = true;
                    scApp.MapBLL.getAddressID(unload_ok_port.PORT_ID, out string from_adr);
                    scApp.MapBLL.getAddressID(target_port_def.PORT_ID, out string to_adr);

                    isSuccess &= scApp.CMDBLL.doCreatTransferCommand(vh.VEHICLE_ID, "", cst_id,
                                        E_CMD_TYPE.LoadUnload,
                                        unload_ok_port.PORT_ID,
                                        target_port_def.PORT_ID, 0, 0,
                                        box_id, "",
                                        from_adr, to_adr);
                    willTestAGVStationPorts.Remove(target_port_def);
                }
            }
        }
        private void CVPortTest()
        {
            List<AVEHICLE> vhs = scApp.VehicleBLL.cache.loadVhs();
            foreach (AVEHICLE vh in vhs)
            {
                if (vh.isTcpIpConnect &&
                    vh.MODE_STATUS == ProtocolFormat.OHTMessage.VHModeStatus.AutoRemote &&
                    vh.ACT_STATUS == ProtocolFormat.OHTMessage.VHActionStatus.NoCommand &&
                    vh.HAS_BOX == 0 &&
                    !SCUtility.isEmpty(vh.CUR_ADR_ID) &&
                    !scApp.CMDBLL.isCMD_OHTCExcuteByVh(vh.VEHICLE_ID))
                {
                    var all_cv_port = scApp.PortDefBLL.cache.loadCVPortDefs();
                    //1.嘗試找出目前是in mode
                    var all_cv_port_in_mode = all_cv_port.Where(port => IsGetReady(port)).ToList();
                    foreach (var in_mode_port in all_cv_port_in_mode)
                    {
                        var source_port_info = scApp.TransferService.GetPLC_PortData(in_mode_port.PLCPortID);
                        //if (scApp.CMDBLL.hasExcuteCMDFromAdr(in_mode_port.ADR_ID)) continue;
                        if (scApp.CMDBLL.hasExcuteCMDByBoxID(source_port_info.BoxID)) continue;
                        string finial_port_num = in_mode_port.PLCPortID.Substring((in_mode_port.PLCPortID.Length) - 2);//取得倒數兩個字
                        int port_num = Convert.ToInt32(finial_port_num, 16);
                        //確認是否可被2整除
                        bool is_even_num = port_num % 2 == 0;
                        int target_port_num = is_even_num ? port_num - 1 : port_num + 1;
                        string target_port_id = $"B7_OHBLOOP_T{Convert.ToString(target_port_num, 16).PadLeft(2, '0')}";

                        PortDef target_port = scApp.PortDefBLL.cache.getCVPortDef(target_port_id);
                        string box_id = SCUtility.isEmpty(source_port_info.BoxID) ? "BOX01" : SCUtility.Trim(source_port_info.BoxID);
                        string cst_id = source_port_info.CassetteID.ToUpper().Contains("NO") ? "" : SCUtility.Trim(source_port_info.CassetteID);
                        bool is_success = scApp.CMDBLL.doCreatTransferCommand(vh.VEHICLE_ID, "", cst_id,
                                             E_CMD_TYPE.LoadUnload,
                                             in_mode_port.PLCPortID,
                                             target_port.PLCPortID, 0, 0,
                                             box_id, "",
                                             in_mode_port.ADR_ID, target_port.ADR_ID);
                        if (is_success)
                        {
                            return;
                        }
                    }
                }
            }
        }
        private bool IsGetReady(PortDef port)
        {
            var transfer_service = scApp.TransferService;
            var plc_port_info = transfer_service.GetPLC_PortData(port.PLCPortID);
            if (plc_port_info == null)
            {
                return false;
            }
            else
            {
                return plc_port_info.IsInputMode &&
                       plc_port_info.PortWaitIn &&
                       plc_port_info.IsAutoMode &&
                       !SCUtility.isEmpty(plc_port_info.BoxID);
            }
        }
        private bool IsGetReady_outMode(PortDef port)
        {
            var transfer_service = scApp.TransferService;
            var plc_port_info = transfer_service.GetPLC_PortData(port.PLCPortID);
            if (plc_port_info == null)
            {
                return false;
            }
            else
            {
                return plc_port_info.IsOutputMode &&
                       plc_port_info.IsAutoMode &&
                       !SCUtility.isEmpty(plc_port_info.BoxID);
            }
        }

        private void NTBPortTest() //A0.01 
        {
            //確認卡夾在哪些位置上
            List<CassetteData> cassetteDatas = scApp.CassetteDataBLL.loadCassetteData();
            //NTB算是CVport 的一種
            List<AVEHICLE> vhs = scApp.VehicleBLL.cache.loadVhs();
            //是否有要執行且可執行之 NTB to Shelf命令
            foreach (AVEHICLE vh in vhs)
            {
                if (vh.isTcpIpConnect &&
                    vh.MODE_STATUS == ProtocolFormat.OHTMessage.VHModeStatus.AutoRemote &&
                    vh.ACT_STATUS == ProtocolFormat.OHTMessage.VHActionStatus.NoCommand &&
                    vh.HAS_BOX == 0 &&
                    !SCUtility.isEmpty(vh.CUR_ADR_ID) &&
                    !scApp.CMDBLL.isCMD_OHTCExcuteByVh(vh.VEHICLE_ID))
                {
                    GenerateNtbToShelf(vh, cassetteDatas);
                }
            }
            //是否有要執行且可執行之 Shelf to NTB命令
            foreach (AVEHICLE vh in vhs)
            {
                if (vh.isTcpIpConnect &&
                    vh.MODE_STATUS == ProtocolFormat.OHTMessage.VHModeStatus.AutoRemote &&
                    vh.ACT_STATUS == ProtocolFormat.OHTMessage.VHActionStatus.NoCommand &&
                    vh.HAS_BOX == 0 &&
                    !SCUtility.isEmpty(vh.CUR_ADR_ID) &&
                    !scApp.CMDBLL.isCMD_OHTCExcuteByVh(vh.VEHICLE_ID))
                {
                    GenerateShelfToNtb(vh, cassetteDatas);
                }
            }
        }

        private void GenerateNtbToShelf(AVEHICLE vh, List<CassetteData> cassetteDatas)//A0.01 
        {
            //1.找出所有CVport
            var all_cv_port = scApp.PortDefBLL.cache.loadCVPortDefs();
            //2.嘗試找出在CVport 中目前是in mode 的所有port
            var all_cv_port_in_mode = all_cv_port.Where(port => IsGetReady(port)).ToList();
            foreach (var in_mode_port in all_cv_port_in_mode)
            {
                //3. 若開頭為OHBLOOP的是連接LOOP的CV 而非NTB 故排除
                var source_port_info = scApp.TransferService.GetPLC_PortData(in_mode_port.PLCPortID);
                if (in_mode_port.PLCPortID.StartsWith("B7_OHBLOOP"))
                {
                    continue;
                }
                else
                {
                    //找一份目前儲位的列表
                    if (shelfDefs == null || shelfDefs.Count == 0)
                        shelfDefs = scApp.ShelfDefBLL.LoadEnableShelf();
                    //如果取完還是空的 就跳出去
                    if (shelfDefs == null || shelfDefs.Count == 0)
                        return;
                    //選出要放的shelf位置
                    ShelfDef target_shelf_def = FindRandomEmptyShelf(cassetteDatas);
                    bool isSuccess = true;
                    string box_id = SCUtility.isEmpty(source_port_info.BoxID) ? "BOX01" : SCUtility.Trim(source_port_info.BoxID);
                    string cst_id = source_port_info.CassetteID.ToUpper().Contains("NO") ? "" : SCUtility.Trim(source_port_info.CassetteID);
                    //從該選取的in mode plcPort 搬到要放置的shelf
                    isSuccess &= scApp.CMDBLL.doCreatTransferCommand(vh.VEHICLE_ID, "", cst_id,
                                        E_CMD_TYPE.LoadUnload,
                                        in_mode_port.PLCPortID,
                                        target_shelf_def.ShelfID, 0, 0,
                                        box_id, "",
                                        in_mode_port.ADR_ID, target_shelf_def.ADR_ID);
                    shelfDefs.Remove(target_shelf_def);
                }
            }
        }

        private void GenerateShelfToNtb(AVEHICLE vh, List<CassetteData> cassetteDatas)//A0.01 
        {
            //1.找出所有CVport
            var all_cv_port = scApp.PortDefBLL.cache.loadCVPortDefs();
            //2.嘗試找出在CVport 中目前是out mode 的所有port
            var all_cv_port_out_mode = all_cv_port.Where(port => IsGetReady_outMode(port)).ToList();
            foreach (var out_mode_port in all_cv_port_out_mode)
            {
                //3. 若開頭為OHBLOOP的是連接LOOP的CV 而非NTB 故排除
                var target_port_info = scApp.TransferService.GetPLC_PortData(out_mode_port.PLCPortID);
                if (out_mode_port.PLCPortID.StartsWith("B7_OHBLOOP"))
                {
                    continue;
                }
                else
                {
                    //找出在儲位中的Cassette
                    cassetteDatas = cassetteDatas.Where(cst => cst.Carrier_LOC.StartsWith("10") ||
                                                               cst.Carrier_LOC.StartsWith("11") ||
                                                               cst.Carrier_LOC.StartsWith("21") ||
                                                               cst.Carrier_LOC.StartsWith("20")).
                                                               ToList();
                    //取第一筆CST
                    CassetteData chosenCst = cassetteDatas[0];
                    scApp.MapBLL.getAddressID(chosenCst.Carrier_LOC, out string from_adr);
                    bool isSuccess = true;

                    //從該選取的CST shelf 位置搬到要放置的NTB
                    isSuccess &= scApp.CMDBLL.doCreatTransferCommand(vh.VEHICLE_ID, "", chosenCst.CSTID.Trim(),
                                        E_CMD_TYPE.LoadUnload,
                                        chosenCst.Carrier_LOC,
                                        out_mode_port.PLCPortID, 0, 0,
                                        chosenCst.BOXID.Trim(), chosenCst.LotID,
                                        from_adr, out_mode_port.ADR_ID);
                }
            }
        }

        private ShelfDef FindRandomEmptyShelf(List<CassetteData> cassetteDatas)//A0.01 
        {
            //找出在儲位中的Cassette
            cassetteDatas = cassetteDatas.Where(cst => cst.Carrier_LOC.StartsWith("10") ||
                                                       cst.Carrier_LOC.StartsWith("11") ||
                                                       cst.Carrier_LOC.StartsWith("21") ||
                                                       cst.Carrier_LOC.StartsWith("20")).
                                                       ToList();
            List<string> current_cst_at_shelf_id = cassetteDatas.
                Select(cst => SCUtility.Trim(cst.Carrier_LOC, true)).
                ToList();
            //刪除目前有cst所在的儲位，讓他排除在Cycle Run的列表中
            foreach (var shelf in shelfDefs.ToList())
            {
                if (current_cst_at_shelf_id.Contains(SCUtility.Trim(shelf.ShelfID)))
                {
                    shelfDefs.Remove(shelf);
                }
            }

            //隨機找出一個要放置的shelf
            int task_RandomIndex = rnd_Index.Next(shelfDefs.Count - 1);
            return shelfDefs[task_RandomIndex];
        }


        private void AGVStationWhenTestAGV() //A0.01 
        {
            //1-確認各個AGV Station狀態
            // a-Out put mode
            //   a.1-如果有空box在上面，要將他夾回儲位
            //   a.2-如果沒有Box在上面，則要夾一個實BOX放過去
            // b-In put mode
            //   b.1-如果有實Box在上面，要將他夾回儲位
            //   b.2-如果沒有Box在上面，則要夾一個空BOX過去

            ProcessOurPutModeAndEmptyBoxOnAGVStationScript();


            //確認卡夾在哪些位置上
            List<CassetteData> cassetteDatas = scApp.CassetteDataBLL.loadCassetteData();
            //NTB算是CVport 的一種
            List<AVEHICLE> vhs = scApp.VehicleBLL.cache.loadVhs();
            //是否有要執行且可執行之 NTB to Shelf命令
            foreach (AVEHICLE vh in vhs)
            {
                if (vh.isTcpIpConnect &&
                    vh.MODE_STATUS == ProtocolFormat.OHTMessage.VHModeStatus.AutoRemote &&
                    vh.ACT_STATUS == ProtocolFormat.OHTMessage.VHActionStatus.NoCommand &&
                    vh.HAS_BOX == 0 &&
                    !SCUtility.isEmpty(vh.CUR_ADR_ID) &&
                    !scApp.CMDBLL.isCMD_OHTCExcuteByVh(vh.VEHICLE_ID))
                {
                    GenerateNtbToShelf(vh, cassetteDatas);
                }
            }
            //是否有要執行且可執行之 Shelf to NTB命令
            foreach (AVEHICLE vh in vhs)
            {
                if (vh.isTcpIpConnect &&
                    vh.MODE_STATUS == ProtocolFormat.OHTMessage.VHModeStatus.AutoRemote &&
                    vh.ACT_STATUS == ProtocolFormat.OHTMessage.VHActionStatus.NoCommand &&
                    vh.HAS_BOX == 0 &&
                    !SCUtility.isEmpty(vh.CUR_ADR_ID) &&
                    !scApp.CMDBLL.isCMD_OHTCExcuteByVh(vh.VEHICLE_ID))
                {
                    GenerateShelfToNtb(vh, cassetteDatas);
                }
            }
        }

        private void ProcessOurPutModeAndEmptyBoxOnAGVStationScript()
        {
            List<APORT> AGVStationPorts = scApp.PortBLL.OperateCatch.loadAGVStationPorts();
            AGVStationPorts = AGVStationPorts.Where(port => isOutPutModeAndEmptyBoxOnAGVStation(port)).
                                              ToList();
        }

        private bool isOutPutModeAndEmptyBoxOnAGVStation(APORT port)
        {
            var port_plc_info = scApp.TransferService.GetPLC_PortData(port.PORT_ID);
            bool is_true = port_plc_info.IsOutputMode &&
                           port_plc_info.LoadPosition1 &&
                           !port_plc_info.IsCSTPresence &&
                           port_plc_info.IsReadyToUnload;
            return is_true;
        }
    }
}

