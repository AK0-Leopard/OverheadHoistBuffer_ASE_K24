using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data.PLC_Functions.MGV;
using com.mirle.ibg3k0.sc.Data.PLC_Functions.MGV.Enums;
using com.mirle.ibg3k0.sc.Data.ValueDefMapAction.Interface;
using com.mirle.ibg3k0.sc.Service.Interface;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace com.mirle.ibg3k0.sc.Service
{
    public class ManualPortControlService : IManualPortControlService
    {
        public ManualPortControlService()
        {
            WriteLog($"ManualPortControlService Initial");
            HeartBeatStopwatch = new Stopwatch();
        }

        public void Start(IEnumerable<IManualPortValueDefMapAction> ports)
        {
            WriteLog($"ManualPortControlService Start");

            RegisterPort(ports);

            HeartBeatStopwatch.Start();
            WriteLog($"HeartBeat Stopwatch Start");
        }

        private void RegisterPort(IEnumerable<IManualPortValueDefMapAction> ports)
        {
            manualPorts = new ConcurrentDictionary<string, IManualPortValueDefMapAction>();
            comingOutCarrierOfManualPorts = new ConcurrentDictionary<string, string>();
            lastComingOutCarrierOfManualPorts = new ConcurrentDictionary<string, string>();
            readyToWaitOutCarrierOfManualPorts = new ConcurrentDictionary<string, List<string>>();
            lastReadyToWaitOutCarrierOfManualPorts = new ConcurrentDictionary<string, List<string>>();
            stopWatchForCheckCommandingSignal = new ConcurrentDictionary<string, Stopwatch>();
            LogOfCheckCommandingSignal = new ConcurrentDictionary<string, bool>();

            foreach (var port in ports)
            {
                manualPorts.TryAdd(port.PortName, port);
                WriteLog($"Add Manual Port Control Success ({port.PortName})");

                comingOutCarrierOfManualPorts.TryAdd(port.PortName, string.Empty);
                WriteLog($"Add Manual Port Control Coming Out Carrier Of Manual Ports Success ({port.PortName})");

                lastComingOutCarrierOfManualPorts.TryAdd(port.PortName, string.Empty);
                WriteLog($"Add Manual Port Control Last Coming Out Carrier Of Manual Ports Success ({port.PortName})");

                readyToWaitOutCarrierOfManualPorts.TryAdd(port.PortName, new List<string>());
                WriteLog($"Add Manual Port Control Ready To Wait Out Carrier Of Manual Ports Success ({port.PortName})");

                lastReadyToWaitOutCarrierOfManualPorts.TryAdd(port.PortName, new List<string>());
                WriteLog($"Add Manual Port Control Last Ready To Wait Out Carrier Of Manual Ports Success ({port.PortName})");

                stopWatchForCheckCommandingSignal.TryAdd(port.PortName, new Stopwatch());
                WriteLog($"Add Manual Port Control Stopwatch Of Manual Ports For Check Commanding Signal Success ({port.PortName})");

                LogOfCheckCommandingSignal.TryAdd(port.PortName, false);
                WriteLog($"Add Manual Port Control ConcurrentDictionary (Log Of Check Commanding Signal) Success ({port.PortName})");
            }
        }

        private Logger logger = LogManager.GetLogger("ManualPortLogger");

        private string now { get => DateTime.Now.ToString("HH:mm:ss.fff"); }

        private ConcurrentDictionary<string, IManualPortValueDefMapAction> manualPorts { get; set; }

        private ConcurrentDictionary<string, string> comingOutCarrierOfManualPorts { get; set; }

        private ConcurrentDictionary<string, string> lastComingOutCarrierOfManualPorts { get; set; }

        private ConcurrentDictionary<string, List<string>> readyToWaitOutCarrierOfManualPorts { get; set; }

        private ConcurrentDictionary<string, List<string>> lastReadyToWaitOutCarrierOfManualPorts { get; set; }

        private ConcurrentDictionary<string, Stopwatch> stopWatchForCheckCommandingSignal { get; set; }

        private ConcurrentDictionary<string, bool> LogOfCheckCommandingSignal { get; set; }

        private const string NO_CST = " ";

        private const int timeoutElapsedMillisecondsForOffCommandingSignal = 12_000;

        private const int CheckHeartbeatMilliseconds = 4_000;

        private Stopwatch HeartBeatStopwatch;

        #region Log

        private void WriteLog(string message)
        {
            var logMessage = $"[{now}] {message}";
            logger.Info(logMessage);
        }

        #endregion Log

        public void ReflashState()
        {
            HeartBeat();
            var allCommands = ACMD_MCS.MCS_CMD_InfoList;
            ReflashPlcMonitor(allCommands);
            ReflashReadyToWaitOutCarrier();
            ReflashComingOutCarrier();
            CheckCommandingSignal(allCommands);
        }

        private void HeartBeat()
        {
            try
            {
                if (HeartBeatStopwatch.ElapsedMilliseconds < CheckHeartbeatMilliseconds)
                    return;

                foreach (var portItem in manualPorts)
                {
                    var port = portItem.Value;

                    if (port.IsPlcHeartbeatOn)
                        port.HeartBeatAsync(setOn: false);
                    else
                        port.HeartBeatAsync(setOn: true);
                }

                HeartBeatStopwatch.Reset();
                HeartBeatStopwatch.Start();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                WriteLog($"{MethodBase.GetCurrentMethod()}, Exception Happen: (( {ex} ))");
            }
        }

        private void ReflashPlcMonitor(ConcurrentDictionary<string, ACMD_MCS> allCommands)
        {
            try
            {
                foreach (var portItem in manualPorts)
                {
                    var portName = portItem.Key;

                    readyToWaitOutCarrierOfManualPorts[portName].Clear();

                    var commandsOfManualPort = allCommands.Where(c => c.Value.HOSTDESTINATION.Trim() == portName);

                    var manualPortcommands = new List<ACMD_MCS>();

                    foreach (var commandItem in commandsOfManualPort)
                        manualPortcommands.Add(commandItem.Value);

                    DescendingSortByPriority(manualPortcommands);

                    foreach (var command in manualPortcommands)
                    {
                        if (command.TRANSFERSTATE == E_TRAN_STATUS.Queue || string.IsNullOrEmpty(command.RelayStation) == false)
                        {
                            if (readyToWaitOutCarrierOfManualPorts[portName].Count < 2)
                                readyToWaitOutCarrierOfManualPorts[portName].Add(command.BOX_ID);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                WriteLog($"{MethodBase.GetCurrentMethod()}, Exception Happen: (( {ex} ))");
            }
        }

        private void ReflashReadyToWaitOutCarrier()
        {
            try
            {
                foreach (var item in readyToWaitOutCarrierOfManualPorts)
                {
                    if (lastReadyToWaitOutCarrierOfManualPorts[item.Key].Count == item.Value.Count)
                    {
                        if (item.Value.Count == 0)
                            continue;
                        else if (item.Value.Count == 1)
                        {
                            if (lastReadyToWaitOutCarrierOfManualPorts[item.Key].FirstOrDefault() == item.Value.FirstOrDefault())
                                continue;
                        }
                        else if (item.Value.Count == 2)
                        {
                            if (lastReadyToWaitOutCarrierOfManualPorts[item.Key][0] == item.Value[0] &&
                                lastReadyToWaitOutCarrierOfManualPorts[item.Key][1] == item.Value[1])
                                continue;
                        }
                    }

                    lastReadyToWaitOutCarrierOfManualPorts[item.Key].Clear();

                    foreach (var carrierID in item.Value)
                        lastReadyToWaitOutCarrierOfManualPorts[item.Key].Add(carrierID);

                    if (item.Value.Count == 0)
                    {
                        manualPorts[item.Key].ShowReadyToWaitOutCarrierOnMonitorAsync(NO_CST, NO_CST);
                        WriteLog($"{item.Key} Has no carrier that ready to WaitOut. Show PLC Monitor ({NO_CST})({NO_CST}). 沒有「準備出庫」的 ID");
                    }
                    else if (item.Value.Count == 1)
                    {
                        manualPorts[item.Key].ShowReadyToWaitOutCarrierOnMonitorAsync(item.Value[0], NO_CST);
                        WriteLog($"{item.Key} Has one carrier that ready to WaitOut. Show PLC Monitor ({item.Value[0]})(NO_CST).  有一個「準備出庫」的 ID");
                    }
                    else
                    {
                        manualPorts[item.Key].ShowReadyToWaitOutCarrierOnMonitorAsync(item.Value[0], item.Value[1]);
                        WriteLog($"{item.Key} Has two carrier that ready to WaitOut. Show PLC Monitor ({item.Value[0]})({item.Value[1]}).  有兩個「準備出庫」的 ID");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                WriteLog($"{MethodBase.GetCurrentMethod()}, Exception Happen: (( {ex} ))");
            }
        }

        private void ReflashComingOutCarrier()
        {
            try
            {
                var commandsOfOHT = ACMD_OHTC.CMD_OHTC_InfoList;

                foreach (var portItem in manualPorts)
                {
                    var portName = portItem.Key;
                    comingOutCarrierOfManualPorts[portName] = string.Empty;

                    var cmds = commandsOfOHT.Where(c => SCUtility.isMatche(c.Value.DESTINATION.Trim(), portName)).Select(c => c.Value).ToList();

                    if (cmds == null)
                        continue;

                    var cmd = cmds.FirstOrDefault();

                    if (cmd == null)
                        continue;

                    comingOutCarrierOfManualPorts[portName] = cmd.BOX_ID.Trim();
                }

                foreach (var item in comingOutCarrierOfManualPorts)
                {
                    var carrierId = item.Value;

                    if (lastComingOutCarrierOfManualPorts[item.Key] == carrierId)
                        continue;

                    lastComingOutCarrierOfManualPorts[item.Key] = carrierId;

                    if (string.IsNullOrEmpty(item.Value))
                    {
                        manualPorts[item.Key].ShowComingOutCarrierOnMonitorAsync(NO_CST);
                        WriteLog($"{item.Key} Has no carrier coming out. Show PLC Monitor ({NO_CST}).  沒有「正在出庫」的 ID");
                    }
                    else
                    {
                        WriteLog($"{item.Key} Has carrier coming out. Show PLC Monitor ({carrierId}).  有「正在出庫的」的 ID");
                        manualPorts[item.Key].ShowComingOutCarrierOnMonitorAsync(carrierId);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                WriteLog($"{MethodBase.GetCurrentMethod()}, Exception Happen: (( {ex} ))");
            }
        }

        protected void DescendingSortByPriority(List<ACMD_MCS> allCommands)
        {
            allCommands.Sort((x, y) =>
            {
                return -x.PRIORITY_SUM.CompareTo(y.PRIORITY_SUM);
            });
        }

        private void CheckCommandingSignal(ConcurrentDictionary<string, ACMD_MCS> allCommands)
        {
            try
            {
                foreach (var portItem in manualPorts)
                {
                    var portName = portItem.Key;

                    var commandsOfManualPort = allCommands.Where(c => c.Value.HOSTDESTINATION.Trim() == portName || c.Value.HOSTSOURCE.Trim() == portName);

                    if (commandsOfManualPort != null || commandsOfManualPort.Count() > 0)
                    {
                        stopWatchForCheckCommandingSignal[portName].Reset();
                        stopWatchForCheckCommandingSignal[portName].Stop();
                        LogOfCheckCommandingSignal[portName] = false;
                        continue;
                    }

                    if (stopWatchForCheckCommandingSignal[portName].IsRunning)
                    {
                        if (stopWatchForCheckCommandingSignal[portName].ElapsedMilliseconds > timeoutElapsedMillisecondsForOffCommandingSignal)
                        {
                            portItem.Value.SetCommandingAsync(setOn: false);
                            stopWatchForCheckCommandingSignal[portName].Reset();

                            if (LogOfCheckCommandingSignal[portName] == false)
                            {
                                WriteLog($"{portName}, CheckCommandingSignal(), 沒有相關命令且觀察 {timeoutElapsedMillisecondsForOffCommandingSignal} 毫秒後一樣沒命令，因此強制將預約的 Bit OFF，避免殘留。");
                                LogOfCheckCommandingSignal[portName] = true;
                            }
                        }
                    }
                    else
                        stopWatchForCheckCommandingSignal[portName].Start();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                WriteLog($"{MethodBase.GetCurrentMethod()}, Exception Happen: (( {ex} ))");
            }
        }

        public bool GetPortPlcState(string portName, out ManualPortPLCInfo info)
        {
            info = null;

            if (manualPorts.ContainsKey(portName) == false)
            {
                WriteLog($"{MethodBase.GetCurrentMethod().Name}({portName}) => Cannot Find this port");
                return false;
            }

            info = manualPorts[portName].GetPortState() as ManualPortPLCInfo;
            WriteLog($"{MethodBase.GetCurrentMethod().Name}({portName})");
            return true;
        }

        public bool ChangeToInMode(string portName)
        {
            if (manualPorts.ContainsKey(portName) == false)
            {
                WriteLog($"{MethodBase.GetCurrentMethod().Name}({portName}) => Cannot Find this port");
                return false;
            }

            manualPorts[portName].ChangeToInModeAsync(true);
            WriteLog($"{MethodBase.GetCurrentMethod().Name}({portName})");
            return true;
        }

        public bool ChangeToOutMode(string portName)
        {
            if (manualPorts.ContainsKey(portName) == false)
            {
                WriteLog($"{MethodBase.GetCurrentMethod().Name}({portName}) => Cannot Find this port");
                return false;
            }

            manualPorts[portName].ChangeToOutModeAsync(true);
            WriteLog($"{MethodBase.GetCurrentMethod().Name}({portName})");
            return true;
        }

        public bool MoveBack(string portName)
        {
            if (manualPorts.ContainsKey(portName) == false)
            {
                WriteLog($"{MethodBase.GetCurrentMethod().Name}({portName}) => Cannot Find this port");
                return false;
            }

            manualPorts[portName].MoveBackAsync();
            WriteLog($"{MethodBase.GetCurrentMethod().Name}({portName})");
            return true;
        }

        public bool SetMoveBackReason(string portName, MoveBackReasons reason)
        {
            if (manualPorts.ContainsKey(portName) == false)
            {
                WriteLog($"{MethodBase.GetCurrentMethod().Name}({portName}) => Cannot Find this port");
                return false;
            }

            manualPorts[portName].SetMoveBackReasonAsync(reason);
            WriteLog($"{MethodBase.GetCurrentMethod().Name}({portName})");
            return true;
        }

        public bool ResetAlarm(string portName)
        {
            if (manualPorts.ContainsKey(portName) == false)
            {
                WriteLog($"{MethodBase.GetCurrentMethod().Name}({portName}) => Cannot Find this port");
                return false;
            }

            manualPorts[portName].ResetAlarmAsync();
            WriteLog($"{MethodBase.GetCurrentMethod().Name}({portName})");
            return true;
        }

        public bool StopBuzzer(string portName)
        {
            if (manualPorts.ContainsKey(portName) == false)
            {
                WriteLog($"{MethodBase.GetCurrentMethod().Name}({portName}) => Cannot Find this port");
                return false;
            }

            manualPorts[portName].StopBuzzerAsync();
            WriteLog($"{MethodBase.GetCurrentMethod().Name}({portName})");
            return true;
        }

        public bool SetRun(string portName)
        {
            if (manualPorts.ContainsKey(portName) == false)
            {
                WriteLog($"{MethodBase.GetCurrentMethod().Name}({portName}) => Cannot Find this port");
                return false;
            }

            manualPorts[portName].SetRunAsync();
            WriteLog($"{MethodBase.GetCurrentMethod().Name}({portName})");
            return true;
        }

        public bool SetStop(string portName)
        {
            if (manualPorts.ContainsKey(portName) == false)
            {
                WriteLog($"{MethodBase.GetCurrentMethod().Name}({portName}) => Cannot Find this port");
                return false;
            }

            manualPorts[portName].SetStopAsync();
            WriteLog($"{MethodBase.GetCurrentMethod().Name}({portName})");
            return true;
        }

        public bool SetCommanding(string portName, bool setOn)
        {
            if (manualPorts.ContainsKey(portName) == false)
            {
                WriteLog($"{MethodBase.GetCurrentMethod().Name}({portName}) => Cannot Find this port");
                return false;
            }

            manualPorts[portName].SetCommandingAsync(setOn);
            WriteLog($"{MethodBase.GetCurrentMethod().Name}({portName})");
            return true;
        }

        public bool SetControllerErrorIndex(string portName, int newIndex)
        {
            if (manualPorts.ContainsKey(portName) == false)
            {
                WriteLog($"{MethodBase.GetCurrentMethod().Name}({portName}) => Cannot Find this port");
                return false;
            }

            manualPorts[portName].SetControllerErrorIndexAsync(newIndex);
            WriteLog($"{MethodBase.GetCurrentMethod().Name}({portName})");
            return true;
        }
    }
}