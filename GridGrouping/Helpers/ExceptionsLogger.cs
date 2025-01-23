using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
//using log4net;
//using log4net.Config;
using System.Reflection;

namespace Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Helpers
{
    /// <summary>
    /// Use to Log Syncfusiion related errors to the current logging path
    /// </summary>
    public static class ExceptionsLogger
    {
        private static string logFormat;
        private static string path = String.Empty;
        private static string logsDirectory = "Logs";
        private static string fileName = "NewGridLog.txt";
        private static StringBuilder tempLog = new StringBuilder();
        private static readonly string GridLoggerName = "GridLogger";
        //private static ILog gridLogger = null;

        static ExceptionsLogger()
        {
            try
            {
                //gridLogger = LogManager.GetLogger(GridLoggerName);

                // dd/mm/yyyy hh:mm:ss AM/PM ==> Log Message
                logFormat = DateTime.Now.ToShortDateString().ToString() + " " +
                            DateTime.Now.ToLongTimeString().ToString() + " ==> ";

                path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\" +
                       Application.CompanyName + @"\" +
                       Application.ProductName + @"\" + logsDirectory + @"\" + fileName;
            }
            catch
            {
            }
        }

        public static void LogError(string sErrMsg)
        {
            LogError(sErrMsg, LogEntryType.Info, null);
        }

        public static void LogError(string sErrMsg, LogEntryType type, Exception ex)
        {
            try
            {
                string logEntryTimeString = String.Empty;

                tempLog.Clear();

                logEntryTimeString = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,ffff - ");

                string logEntryTypeString = String.Empty;

                if (type == LogEntryType.Error)
                    logEntryTypeString = "ERROR";
                else if (type == LogEntryType.Warning)
                    logEntryTypeString = "WARNING";
                else if (type == LogEntryType.Warning)
                    logEntryTypeString = "Internal";

                if (!String.IsNullOrEmpty(logEntryTypeString))
                {
                    tempLog.Append("[");
                    tempLog.Append(logEntryTypeString);
                    tempLog.Append("] ");
                }

                tempLog.Append(Environment.NewLine);
                tempLog.Append(sErrMsg);

                string messageBody = Environment.NewLine + logEntryTimeString + tempLog.ToString();

                //if (gridLogger != null)
                //{
                //    switch (type)
                //    {
                //        case LogEntryType.Info:
                //            gridLogger.Info(messageBody, ex);
                //            break;
                //        case LogEntryType.Error:
                //            gridLogger.Error(messageBody, ex);
                //            break;
                //        case LogEntryType.Warning:
                //            gridLogger.Warn(messageBody, ex);
                //            break;
                //        default:
                //            gridLogger.Info(messageBody, ex);
                //            break;
                //    }
                //}
                //else
                //{
                //    //
                //    //Use old logging if rolling log is not available
                //    //
                //    File.AppendAllText(path, messageBody + "\n\n");
                //}
            }
            catch
            {
            }

            //try
            //{
            //    if (tempLog.Length > 0)
            //    {
            //        File.AppendAllText(path, tempLog.ToString());
            //        tempLog.Remove(0, tempLog.Length);
            //    }

            //    File.AppendAllText(path, logFormat + sErrMsg + "\n\n" + " ");
            //}
            //catch 
            //{
            //    tempLog.AppendLine(logFormat + sErrMsg + "\n\n" + " ");
            //}
        }

        public static void LogError(Exception exception)
        {
            try
            {
                if (exception != null)
                {
                    string logEntryTimeString = String.Empty;

                    StringBuilder sb = new StringBuilder();

                    sb.Append(exception.ToString() + Environment.NewLine);

                    //if (gridLogger == null)
                    {
                        sb.Append("  " + Environment.NewLine);
                        sb.Append(exception);
                        sb.Append("  " + Environment.NewLine);
                        sb.Append(exception.StackTrace);
                    }

                    if (exception.InnerException != null)
                    {
                        sb.Append(" - Inner - " + Environment.NewLine);
                        sb.Append(exception.InnerException.StackTrace);
                    }
                    string messageBody = sb.ToString();

                    LogError(messageBody, LogEntryType.Error, exception);

                    //LogError(exception.ToString() + Environment.NewLine + exception.StackTrace);
                }
            }
            catch 
            {

            }
        }
    }

    public enum LogEntryType
    {
        Info,
        Warning,
        Error
    }
}
