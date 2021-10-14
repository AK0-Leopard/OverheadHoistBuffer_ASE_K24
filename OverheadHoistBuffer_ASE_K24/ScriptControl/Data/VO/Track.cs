﻿using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.Data.VO
{
    public class Track : AUNIT
    {
        public TrackDir TrackDir { get; set; }
        public Stopwatch stopwatch { get; private set; } = new Stopwatch();
        public string LastUpdateTime
        {
            get
            {
                return stopwatch.ElapsedMilliseconds.ToString();
            }
        }
        public void setTrackDir(TrackDir _trackDir)
        {
            TrackDir = _trackDir;
            stopwatch.Restart();
        }
    }
}