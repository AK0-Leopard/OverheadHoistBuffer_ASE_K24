//*********************************************************************************
//      DefaultValueDefMapAction.cs
//*********************************************************************************
// File Name: PortDefaultValueDefMapAction.cs
// Description: Port Scenario 
//
//(c) Copyright 2013, MIRLE Automation Corporation
//
// Date          Author         Request No.    Tag     Description
// ------------- -------------  -------------  ------  -----------------------------
//**********************************************************************************
using System;
using System.Threading.Tasks;
using com.mirle.ibg3k0.bcf.App;
using com.mirle.ibg3k0.bcf.Common;
using com.mirle.ibg3k0.bcf.Controller;
using com.mirle.ibg3k0.bcf.Data.VO;
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Data.PLC_Functions.EFEM;
using com.mirle.ibg3k0.sc.Data.ValueDefMapAction.Events.EFEM;
using com.mirle.ibg3k0.sc.Data.ValueDefMapAction.Interface;
using com.mirle.ibg3k0.sc.Data.VO;
using NLog;

namespace com.mirle.ibg3k0.sc.Data.ValueDefMapAction
{
    public class EFEMPortStationDefaultValueDefMapAction : IEFEMValueDefMapAction
    {

        protected Logger logger = NLog.LogManager.GetCurrentClassLogger();
        protected EFEM_PORTSTATION port = null;
        private SCApplication scApp = null;
        private BCFApplication bcfApp = null;

        protected ANODE node = null;

        public event EFEMEvents.EFEMEventHandler OnAlarmHappen;
        public event EFEMEvents.EFEMEventHandler OnAlarmClear;

        public string PortName => port == null ? "" : port.PORT_ID;

        public EFEMPortStationDefaultValueDefMapAction()
            : base()
        {
            scApp = SCApplication.getInstance();
            bcfApp = scApp.getBCFApplication();
        }

        public string getIdentityKey()
        {
            return this.GetType().Name;
        }
        public void setContext(BaseEQObject baseEQ)
        {
            this.port = baseEQ as EFEM_PORTSTATION;
        }

        public void unRegisterEvent()
        {
        }

        /// <summary>
        /// Does the share memory initialize.
        /// </summary>
        /// <param name="runLevel">The run level.</param>
        public void doShareMemoryInit(BCFAppConstants.RUN_LEVEL runLevel)
        {
            try
            {
                switch (runLevel)
                {
                    case BCFAppConstants.RUN_LEVEL.ZERO:
                        port.AliveStopwatch.Restart();
                        break;
                    case BCFAppConstants.RUN_LEVEL.ONE:
                        break;
                    case BCFAppConstants.RUN_LEVEL.TWO:
                        break;
                    case BCFAppConstants.RUN_LEVEL.NINE:
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception:");
            }
        }



        public void doInit()
        {
            try
            {
                if (bcfApp.tryGetReadValueEventstring(port.EqptObjectCate, port.PORT_ID, "MGV_TO_OHxC_HEARTBEAT", out ValueRead vr1))
                {
                    vr1.afterValueChange += (_sender, e) => MGV_TO_OHxC_HEARTBEAT(_sender, e);
                }
                if (bcfApp.tryGetReadValueEventstring(port.EqptObjectCate, port.PORT_ID, "MGV_TO_OHxC_PORTALLINFO", out ValueRead vr2))
                {
                    vr2.afterValueChange += (_sender, e) => MGV_TO_OHxC_PORTALLINFO(_sender, e);
                }
                if (bcfApp.tryGetReadValueEventstring(port.EqptObjectCate, port.PORT_ID, "MGV_TO_OHxC_ERRORINDEX", out ValueRead vr3))
                {
                    vr3.afterValueChange += (_sender, e) => MGV_TO_OHxC_ErrorIndexChanged(_sender, e);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception:");
            }
        }

        private void MGV_TO_OHxC_ErrorIndexChanged(object sender, ValueChangedEventArgs e)
        {
            var function = scApp.getFunBaseObj<EFEMPortPLCInfo>(port.PORT_ID) as EFEMPortPLCInfo;

            try
            {
                //1.建立各個Function物件
                function.Read(bcfApp, port.EqptObjectCate, port.PORT_ID);

                //2.read log
                logger.Info(function.ToString());

                var first_alarm_code = function.AlarmCodes[0];

                if (first_alarm_code == 0)
                {
                    OnAlarmClear?.Invoke(this, new EFEMEventArgs(function));
                }
                else
                {
                    OnAlarmHappen?.Invoke(this, new EFEMEventArgs(function));
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<EFEMPortPLCInfo>(function);
            }
        }

        private void MGV_TO_OHxC_PORTALLINFO(object sender, ValueChangedEventArgs e)
        {
            try
            {
                var port_data = scApp.getFunBaseObj<EFEMPortPLCInfo>(port.PORT_ID) as EFEMPortPLCInfo;
                port_data.Read(bcfApp, port.EqptObjectCate, port.PORT_ID);
                logger.Info(port_data.ToString());
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception:");
            }
        }

        private void MGV_TO_OHxC_HEARTBEAT(object sender, ValueChangedEventArgs e)
        {
            try
            {
                port.AliveStopwatch.Restart();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception:");
            }
        }

        public object GetPortState()
        {
            var port_data = scApp.getFunBaseObj<EFEMPortPLCInfo>(port.PORT_ID) as EFEMPortPLCInfo;
            port_data.Read(bcfApp, port.EqptObjectCate, port.PORT_ID);
            return port_data;
        }

        public Task ChangeToInModeAsync(bool isOn)
        {
            return Task.Run(() =>
            {
                var function = scApp.getFunBaseObj<EFEMPortPLCControl>(port.PORT_ID) as EFEMPortPLCControl;
                function.IsChangeToInMode = isOn;
                CommitChange(function);
            });
        }
        public Task SetControllerErrorIndexAsync(int newIndex)
        {
            return Task.Run(() =>
            {
                var function = scApp.getFunBaseObj<EFEMPortPLCControl_ERROR_INDEX>(port.PORT_ID) as EFEMPortPLCControl_ERROR_INDEX;
                try
                {
                    function.OhbcErrorIndex = (UInt16)newIndex;

                    function.Write(bcfApp, port.EqptObjectCate, port.PORT_ID);

                    logger.Info(function.ToString());
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception");
                }
                finally
                {
                    scApp.putFunBaseObj<EFEMPortPLCControl_ERROR_INDEX>(function);
                }
            });
        }


        public Task HeartBeatAsync(bool setOn)
        {
            return Task.Run(() =>
            {
                try
                {
                    var function = scApp.getFunBaseObj<EFEMPortPLCControl_HeartBeat>(port.PORT_ID) as EFEMPortPLCControl_HeartBeat;
                    try
                    {
                        function.IsHeartBeatOn = setOn;
                        function.Write(bcfApp, port.EqptObjectCate, port.PORT_ID);
                        logger.Info(function.ToString());
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "Exception");
                    }
                    finally
                    {
                        scApp.putFunBaseObj<EFEMPortPLCControl_HeartBeat>(function);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception");
                }
            });
        }

        private void CommitChange(EFEMPortPLCControl function)
        {
            try
            {
                function.Write(bcfApp, port.EqptObjectCate, port.PORT_ID);

                logger.Info(function.ToString());
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<EFEMPortPLCControl>(function);
            }
        }

        public Task ChangeToOutModeAsync(bool isOn)
        {
            return Task.CompletedTask;
        }


        public Task ResetAlarmAsync()
        {
            return Task.CompletedTask;
        }

        public Task StopBuzzerAsync()
        {
            return Task.CompletedTask;
        }

        public Task SetRunAsync()
        {
            return Task.CompletedTask;
        }

        public Task SetStopAsync()
        {
            return Task.CompletedTask;
        }

        public Task SetCommandingAsync(bool setOn)
        {
            return Task.CompletedTask;
        }

    }
}
