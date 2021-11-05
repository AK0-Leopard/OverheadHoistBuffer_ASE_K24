//*********************************************************************************
//      MESDefaultMapAction.cs
//*********************************************************************************
// File Name: MESDefaultMapAction.cs
// Description: 與EAP通訊的劇本
//
//(c) Copyright 2014, MIRLE Automation Corporation
//
// Date          Author         Request No.    Tag     Description
// ------------- -------------  -------------  ------  -----------------------------
// 2019/07/16    Mark Chou      N/A            M0.01   修正回覆S1F4 SVID305會發生Exception的問題
// 2019/08/26    Kevin Wei      N/A            M0.02   修正原本在只要有From、To命令還是在Wating的狀態時，
//                                                     此時MCS若下達一筆命令則會拒絕，改成只要是From相同，就會拒絕。
//**********************************************************************************

using com.mirle.ibg3k0.bcf.App;
using com.mirle.ibg3k0.bcf.Common;
using com.mirle.ibg3k0.bcf.Data.ValueDefMapAction;
using com.mirle.ibg3k0.bcf.Data.VO;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data.SECSDriver;
using com.mirle.ibg3k0.sc.Data.VO;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using com.mirle.ibg3k0.stc.Common;
using Grpc.Core;
using NLog;
using System;
using System.Collections.Generic;

namespace com.mirle.ibg3k0.sc.Data.ValueDefMapAction.ReelNTBC
{
    public class ReelNTBCDefaultMapActionSend : IValueDefMapAction
    {
        protected static Logger logger = LogManager.GetCurrentClassLogger();
        private readonly string DEVICE_NAME_MCS = "MCS";
        Channel channel = null;
        Mirle.U332MA30.Grpc.OhbcNtbcConnect.OhbcToNtbcService.OhbcToNtbcServiceClient client = null;
        sc.App.SCApplication scApp = null;
        ReelNTB eqpt = null;
        public ReelNTBCDefaultMapActionSend() : base()
        {

        }

        public void doInit()
        {
            scApp = sc.App.SCApplication.getInstance();
            string s_grpc_client_ip = scApp.getString("ReelNTBGrpcClientIP", "999.999.999.999");
            string s_grpc_client_port = scApp.getString("ReelNTBGrpcClientPort", "5004");
            int.TryParse(s_grpc_client_port, out int i_grpc_client_port);
            channel = new Channel(s_grpc_client_ip, i_grpc_client_port, ChannelCredentials.Insecure);
            client = new Mirle.U332MA30.Grpc.OhbcNtbcConnect.OhbcToNtbcService.OhbcToNtbcServiceClient(channel);
        }

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
                logger.Error(ex, "Exection:");
            }
        }

        public string getIdentityKey()
        {
            return this.GetType().Name;
        }

        public void setContext(BaseEQObject baseEQ)
        {
            eqpt = baseEQ as ReelNTB;
        }

        public void unRegisterEvent()
        {
        }

        public bool SendHeartBeatRequest()
        {
            try
            {
                var hb = new Mirle.U332MA30.Grpc.OhbcNtbcConnect.HeartBeatReq();
                LogHelper.RecordHostReportInfo(hb);
                var ask = client.HeartBeatRequest(hb);
                LogHelper.RecordHostReportInfoAsk(ask);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public Mirle.U332MA30.Grpc.OhbcNtbcConnect.OhtPortSignal IoPortSignalQuery(string portName)
        {
            try
            {
                var send = new Mirle.U332MA30.Grpc.OhbcNtbcConnect.OhtPortQuery();
                send.PortName = portName;
                LogHelper.RecordHostReportInfo(send);
                var ask = client.IoPortSignalQuery(send);
                LogHelper.RecordHostReportInfoAsk(ask);
                return ask;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                return null;
            }
        }
        public bool ReelStateUpdate(string cstID, Mirle.U332MA30.Grpc.OhbcNtbcConnect.ReelTransferState state, bool isToNTB, string mcsCmdID)
        {
            try
            {
                var send = new Mirle.U332MA30.Grpc.OhbcNtbcConnect.ReelStateUpdateReq();
                send.CarrierReelId = cstID;
                send.Scenario = state;
                if (isToNTB)
                    send.McsTransferToNtbCommand = mcsCmdID;
                else
                    send.McsTransferToEqPortCommandId = mcsCmdID;

                LogHelper.RecordHostReportInfo(send);
                var ask = client.ReelStateUpdate(send);
                LogHelper.RecordHostReportInfoAsk(ask);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public bool ReelStateUpdate()
        {
            try
            {
                var send = new Mirle.U332MA30.Grpc.OhbcNtbcConnect.ReelStateUpdateReq();
                send.CarrierReelId = "RELLCSTID";
                send.Scenario = Mirle.U332MA30.Grpc.OhbcNtbcConnect.ReelTransferState.ArrivedEqPort;
                send.McsTransferToEqPortCommandId = "12345678";
                send.McsTransferToEqPortCommandId = "87654321";
                LogHelper.RecordHostReportInfo(send);
                var ask = client.ReelStateUpdate(send);
                LogHelper.RecordHostReportInfoAsk(ask);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
