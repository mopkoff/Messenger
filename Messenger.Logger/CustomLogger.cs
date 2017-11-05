﻿using System;
using System.Diagnostics;
using NLog;

namespace Messenger.Logger
{
    public class CustomLogger : IDisposable
    {
        private const string TimerStartDefault = "TSTART: ";
        private const string TimerStopDefault = "TEND: ";

        private readonly Stopwatch _stopwatch;
        private readonly LogLevel _logLevel;
        private readonly string _formatString;
        private readonly object[] _formatValues;
        private readonly string _timerStopString;

        public CustomLogger(string format, params object[] objects) : this(LogLevel.Debug, format, objects) { }

        public CustomLogger(LogLevel logLevel, string format, params object[] objects)
            : this(TimerStartDefault, TimerStopDefault, logLevel, format, objects) { }

        public CustomLogger(string tStart, string tEnd, LogLevel logLevel, string format, params object[] objects)
        {
            _timerStopString = tEnd;
            _formatString = format;
            _formatValues = objects;
            
            _logLevel = logLevel;
            NLogger.Logger.Log(_logLevel, tStart + format, objects);
            _stopwatch = new Stopwatch();
        }

        public TimeSpan WarnTimeSpan { get; set; } = new TimeSpan(0, 0, 4); // 4 seconds default

        public void Start()
        {
            _stopwatch.Start();
        }

        public TimeSpan ElapsedTime => _stopwatch.Elapsed;

        public void Dispose()
        {
            _stopwatch.Stop();
            NLogger.Logger.Log((_stopwatch.Elapsed.CompareTo(WarnTimeSpan) <= 0 ? _logLevel : LogLevel.Warn), _timerStopString + _formatString, _formatValues);
            NLogger.Logger.Log(_stopwatch.Elapsed.CompareTo(WarnTimeSpan) <= 0 ? _logLevel : LogLevel.Warn,
                "Elapsed: {0}" + (_stopwatch.Elapsed.CompareTo(WarnTimeSpan) <= 0 ? "" : ". JOB TOOK MORE THAN {1:c}"), _stopwatch.Elapsed, WarnTimeSpan);
        }
    }
}