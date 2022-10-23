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
using com.mirle.ibg3k0.bcf.Data.VO;
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Data.PLC_Functions.EFEM;
using com.mirle.ibg3k0.sc.Data.ValueDefMapAction.Interface;
using com.mirle.ibg3k0.sc.Data.VO;
using NLog;

namespace com.mirle.ibg3k0.sc.Data.ValueDefMapAction
{
    public class EFMEPortStationDefaultValueDefMapAction : ICommonPortInfoValueDefMapAction
    {

        protected Logger logger = NLog.LogManager.GetCurrentClassLogger();
        protected EFEM_PORTSTATION port = null;
        private SCApplication scApp = null;
        private BCFApplication bcfApp = null;

        protected ANODE node = null;
        public EFMEPortStationDefaultValueDefMapAction()
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
            throw new NotImplementedException();
        }


        public Task ResetAlarmAsync()
        {
            throw new NotImplementedException();
        }

        public Task StopBuzzerAsync()
        {
            throw new NotImplementedException();
        }

        public Task SetRunAsync()
        {
            throw new NotImplementedException();
        }

        public Task SetStopAsync()
        {
            throw new NotImplementedException();
        }

        public Task SetCommandingAsync(bool setOn)
        {
            throw new NotImplementedException();
        }

        public Task SetControllerErrorIndexAsync(int newIndex)
        {
            throw new NotImplementedException();
        }
    }
}
