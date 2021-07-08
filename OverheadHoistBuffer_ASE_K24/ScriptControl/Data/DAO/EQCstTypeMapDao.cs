// ***********************************************************************
// Assembly         : ScriptControl
// Author           : 
// Created          : 03-31-2016
//
// Last Modified By : 
// Last Modified On : 03-24-2016
// ***********************************************************************
// <copyright file="AlarmMapDao.cs" company="">
//     Copyright ©  2014
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.mirle.ibg3k0.bcf.Data;
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Data.VO;
using NLog;
using com.mirle.ibg3k0.bcf.Common;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;

namespace com.mirle.ibg3k0.sc.Data.DAO
{
    /// <summary>
    /// Class AlarmMapDao.
    /// </summary>
    /// <seealso cref="com.mirle.ibg3k0.bcf.Data.DaoBase" />
    public class EQCstTypeMapDao : DaoBase
    {
        /// <summary>
        /// The logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Gets the alarm map.
        /// </summary>
        /// <param name="object_id">The eqpt_real_id.</param>
        /// <param name="vhID">The alarm_id.</param>
        /// <returns>AlarmMap.</returns>
        public EQCstTypeMap getEqCstTypeInfo(SCApplication app, string eqID)
        {
            try
            {
                DataTable dt = app.OHxCConfig.Tables["EQCSTTYPEMAP"];
                var query = from c in dt.AsEnumerable()
                            where c.Field<string>("EQ_ID").Trim() == eqID.Trim()
                            select new EQCstTypeMap
                            {
                                EQ_ID = c.Field<string>("EQ_ID"),
                                CST_TYPE = c.Field<string>("CST_TYPE")
                            };
                return query.SingleOrDefault();
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"get cst type fail,eq id:{eqID}");
                throw;
            }
        }


    }
}
