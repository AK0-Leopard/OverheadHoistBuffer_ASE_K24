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
    public class BlockTrackMapDao : DaoBase
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
        public BlockTrackMap getBlockTrackInfo(SCApplication app, string entrySecID)
        {
            try
            {
                DataTable dt = app.OHxCConfig.Tables["BLOCKTRACKMAP"];
                var query = from c in dt.AsEnumerable()
                            where c.Field<string>("ENTRY_SEC_ID").Trim() == entrySecID.Trim()
                            select new BlockTrackMap
                            {
                                ENTRY_SEC_ID = c.Field<string>("ENTRY_SEC_ID"),
                                TRACKS_ID = stringToStringArray(c.Field<string>("TRACKS_ID"))
                            };
                return query.SingleOrDefault();
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"get block track info fail,track id:{entrySecID}");
                throw;
            }
        }
        public List<BlockTrackMap> loadBlockTrackInfoByTackID(SCApplication app, string trackID)
        {
            try
            {
                DataTable dt = app.OHxCConfig.Tables["BLOCKTRACKMAP"];
                var query1 = from c in dt.AsEnumerable()
                             select new BlockTrackMap
                             {
                                 ENTRY_SEC_ID = c.Field<string>("ENTRY_SEC_ID"),
                                 TRACKS_ID = stringToStringArray(c.Field<string>("TRACKS_ID"))
                             };
                var query = query1.Where(t => t.TRACKS_ID.Contains(trackID));
                return query.ToList();
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"get block track info fail,track id:{trackID}");
                throw;
            }
        }
        List<string> stringToStringArray(string value)
        {
            if (value.Contains("-"))
            {
                return value.Split('-').ToList();
            }
            else
            {
                return new List<string>() { value };
            }
        }


    }
}
