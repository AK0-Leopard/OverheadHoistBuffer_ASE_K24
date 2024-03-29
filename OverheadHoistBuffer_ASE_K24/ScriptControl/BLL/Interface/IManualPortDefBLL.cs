﻿namespace com.mirle.ibg3k0.sc.BLL.Interface
{
    public interface IManualPortDefBLL
    {
        /// <summary>
        /// 修改 Port Def 資料表內的 E_PortType 為 InMode
        /// </summary>
        /// <param name="portName"></param>
        /// <returns></returns>
        bool ChangeDirectionToInMode(string portName);

        /// <summary>
        /// 修改 Port Def 資料表內的 E_PortType 為 OutMode
        /// </summary>
        /// <param name="portName"></param>
        /// <returns></returns>
        bool ChangeDirectionToOutMode(string portName);

        bool GetPortDef(string portName, out PortDef portDef);
        void UpdataPortService(string portID, E_PORT_STATUS service);
    }
}