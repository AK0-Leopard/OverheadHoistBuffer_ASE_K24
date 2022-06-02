//*********************************************************************************
//      AlarmBLL.cs
//*********************************************************************************
// File Name: AlarmBLL.cs
// Description: 業務邏輯：Alarm
//
//(c) Copyright 2014, MIRLE Automation Corporation
//
// Date          Author         Request No.    Tag     Description
// ------------- -------------  -------------  ------  -----------------------------
//**********************************************************************************
using System;

namespace com.mirle.ibg3k0.sc.BLL.Interface
{
    public interface IAlarmRemarkFun
    {
        bool setAlarmRemarkInfo(string eqID, DateTime dateTime, string errorCode, string updateUser,int updateClassification,string remark);

    }
}