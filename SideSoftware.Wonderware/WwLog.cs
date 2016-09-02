using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace SideSoftware.Wonderware.Logger
{
    [ComVisible(false)]
    public class WwLog
    {
        #region Constructor

        static WwLog()
        {
            _loggerDllLoaded = false;
            _mhIdentity = 0;
        }

        #endregion

        #region Private Properties

        private static int _mhIdentity;
        private static bool _loggerDllLoaded;
        private const string LoggerPath = "Software\\ArchestrA\\Framework\\Logger";
        private const string InstallPathKey = "InstallPath";
        private const string LoggerDllName = "LoggerDll.dll";

        #endregion

        #region Public Properties

        /// <summary>
        ///     Retrieves the logger error count
        /// </summary>
        public static int ErrorCount
        {
            get
            {
                // Initialize logger dll
                InitLoggerDll();

                // Set default variables
                var errorCount = 0;
                var warningCount = 0;
                var ftLastError = 0L;
                var ftLastWarning = 0L;

                if (GetLoggerStats("", ref errorCount, ref ftLastError, ref warningCount, ref ftLastWarning) <= 0)
                    return -1;

                return errorCount;
            }
        }

        /// <summary>
        ///     Retrieves the logger warning count
        /// </summary>
        public static int WarningCount
        {
            get
            {
                InitLoggerDll();
                var errorCount = 0;
                var warningCount = 0;
                var ftLastError = 0L;
                var ftLastWarning = 0L;
                if (GetLoggerStats("", ref errorCount, ref ftLastError, ref warningCount, ref ftLastWarning) <= 0)
                    return -1;
                return warningCount;
            }
        }

        #endregion

        #region Interop Functions

        [DllImport("kernel32")]
        private static extern IntPtr LoadLibraryW([MarshalAs(UnmanagedType.LPWStr)] string lpMdoule);

        [DllImport("LoggerDLL.dll", EntryPoint = "REGISTERLOGGERCLIENT", CharSet = CharSet.Unicode)]
        private static extern int RegisterLoggerClient(ref int hIdentity);

        [DllImport("LoggerDLL.dll", EntryPoint = "UNREGISTERLOGGERCLIENT", CharSet = CharSet.Unicode)]
        private static extern int UnregisterLoggerClient(ref int hIdentity);

        [DllImport("LoggerDLL.dll", EntryPoint = "SETIDENTITYNAME", CharSet = CharSet.Unicode)]
        private static extern int SetIdentityName(int hIdentity, string strIdentity);

        [DllImport("LoggerDLL.dll", EntryPoint = "LOGERROR", CharSet = CharSet.Unicode)]
        private static extern void InternalLogError(int hIdentity, [MarshalAs(UnmanagedType.LPWStr)] string message);

        [DllImport("LoggerDLL.dll", EntryPoint = "LOGWARNING", CharSet = CharSet.Unicode)]
        private static extern void InternalLogWarning(int hIdentity, [MarshalAs(UnmanagedType.LPWStr)] string message);

        [DllImport("LoggerDLL.dll", EntryPoint = "LOGINFO", CharSet = CharSet.Unicode)]
        private static extern void InternalLogInfo(int hIdentity, [MarshalAs(UnmanagedType.LPWStr)] string message);

        [DllImport("LoggerDLL.dll", EntryPoint = "LOGTRACE", CharSet = CharSet.Unicode)]
        private static extern void InternalLogTrace(int hIdentity, [MarshalAs(UnmanagedType.LPWStr)] string message);

        [DllImport("LoggerDLL.dll", EntryPoint = "LOGSTARTSTOP", CharSet = CharSet.Unicode)]
        private static extern void InternalLogStartStop(int hIdentity, [MarshalAs(UnmanagedType.LPWStr)] string message);

        [DllImport("LoggerDLL.dll", EntryPoint = "LOGENTRYEXIT", CharSet = CharSet.Unicode)]
        private static extern void InternalLogEntryExit(int hIdentity, [MarshalAs(UnmanagedType.LPWStr)] string message);

        [DllImport("LoggerDLL.dll", EntryPoint = "LOGTHREADSTARTSTOP", CharSet = CharSet.Unicode)]
        private static extern void InternalLogThreadStartStop(int hIdentity,
            [MarshalAs(UnmanagedType.LPWStr)] string message);

        [DllImport("LoggerDLL.dll", EntryPoint = "LOGSQL", CharSet = CharSet.Unicode)]
        private static extern void InternalLogSQL(int hIdentity, [MarshalAs(UnmanagedType.LPWStr)] string message);

        [DllImport("LoggerDLL.dll", EntryPoint = "LOGCONNECTION", CharSet = CharSet.Unicode)]
        private static extern void InternalLogConnection(int hIdentity, [MarshalAs(UnmanagedType.LPWStr)] string message);

        [DllImport("LoggerDLL.dll", EntryPoint = "LOGCTORDTOR", CharSet = CharSet.Unicode)]
        private static extern void InternalLogCtorDtor(int hIdentity, [MarshalAs(UnmanagedType.LPWStr)] string message);

        [DllImport("LoggerDLL.dll", EntryPoint = "LOGREFCOUNT", CharSet = CharSet.Unicode)]
        private static extern void InternalLogRefCount(int hIdentity, [MarshalAs(UnmanagedType.LPWStr)] string message);

        [DllImport("LoggerDLL.dll", EntryPoint = "REGISTERLOGFLAG", CharSet = CharSet.Unicode)]
        private static extern int RegisterLogFlag(int hIdentity, int nCustomFlag,
            [MarshalAs(UnmanagedType.LPWStr)] string strFlag);

        [DllImport("LoggerDLL.dll", EntryPoint = "REGISTERLOGFLAGEX", CharSet = CharSet.Unicode)]
        private static extern int RegisterLogFlagEx(int hIdentity, int nCustomFlag,
            [MarshalAs(UnmanagedType.LPWStr)] string strFlag, int nDefaultVal);

        [DllImport("LoggerDLL.dll", EntryPoint = "LOGCUSTOM2", CharSet = CharSet.Unicode)]
        private static extern void InternalLogCustom(int hIdentity, int nCustomFlag,
            [MarshalAs(UnmanagedType.LPWStr)] string message);

        [DllImport("LoggerDLL.dll", EntryPoint = "GETLOGGERSTATS", CharSet = CharSet.Unicode)]
        private static extern int GetLoggerStats([MarshalAs(UnmanagedType.LPWStr)] string HostName, ref int ErrorCount,
            ref long ftLastError, ref int WarningCount, ref long ftLastWarning);

        #endregion

        #region Private Functions

        /// <summary>
        ///     Used to initialize the dll
        /// </summary>
        /// <returns></returns>
        private static bool Initialize()
        {
            try
            {
                // If identity is set, bail out
                if (_mhIdentity != 0)
                    return true;

                // Initialize the Win32 DLL
                InitLoggerDll();

                // Register the logger client and retrieve the identity
                RegisterLoggerClient(ref _mhIdentity);

                // Make sure we have an identity
                if (_mhIdentity == 0)
                    return false;

                // Set the default identity name
                SetIdentityName(_mhIdentity, Assembly.GetExecutingAssembly().GetName().Name);

                // Return success
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        ///     Initializes the Wonderware Logger dll
        /// </summary>
        private static void InitLoggerDll()
        {
            // Flag indicating if dll has been loaded
            if (_loggerDllLoaded)
                return;

            // Retrieve logger registry key
            var registryKey = Registry.LocalMachine.OpenSubKey(LoggerPath, false);

            // If the key isn't null, proceed
            if (registryKey != null)
            {
                // Retrieve install path
                var path = Convert.ToString(registryKey.GetValue(InstallPathKey, ""));

                // Make sure we found a path
                if (path.Length > 0)
                    LoadLibraryW(Path.Combine(path, LoggerDllName));
            }

            // Mark the logger loaded as true
            _loggerDllLoaded = true;
        }

        #endregion

        #region Public Functions

        /// <summary>
        ///     The function takes a string that specifies the Identity name to be used.
        ///     Otherwise the default identity name will be used.
        /// </summary>
        /// <param name="identityName">Identity name</param>
        public static void LogSetIdentityName(string identityName)
        {
            if (!Initialize())
                return;

            SetIdentityName(_mhIdentity, identityName);
        }

        /// <summary>
        ///     Log an error message.
        ///     Error messages are used to indicate an error condition from which you cannot continue.
        /// </summary>
        /// <param name="message">Message to log</param>
        public static void LogError(string message)
        {
            if (!Initialize())
                return;

            InternalLogError(_mhIdentity, message);
        }

        /// <summary>
        ///     Log an error message.
        ///     This function can not be used from InTouch as InTouch does not
        ///     support object arrays
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="args">Message parameters</param>
        public static void LogErrorFormat(string message, object[] args)
        {
            if (!Initialize())
                return;

            // Build string based upon parameters
            var text = args.Length == 0 ? message : string.Format(message, args);

            InternalLogError(_mhIdentity, text);
        }

        /// <summary>
        ///     Log a warning message.
        ///     Warning messages are used to indicate an error condition from which you can continue, but the output may not be
        ///     what was desired.
        /// </summary>
        /// <param name="message">Message to log</param>
        public static void LogWarning(string message)
        {
            if (!Initialize())
                return;
            InternalLogWarning(_mhIdentity, message);
        }

        /// <summary>
        ///     Log an informational message.
        ///     Info messages simply describe successful completion of large tasks, or other things that may be of casual interest
        ///     to the user.
        /// </summary>
        /// <param name="message">Message to log</param>
        public static void LogInfo(string message)
        {
            if (!Initialize())
                return;
            InternalLogInfo(_mhIdentity, message);
        }

        /// <summary>
        /// </summary>
        /// <param name="message">Message to log</param>
        public static void LogTrace(string message)
        {
            if (!Initialize())
                return;
            InternalLogTrace(_mhIdentity, message);
        }

        /// <summary>
        ///     Log that some component has started or stopped.
        ///     These messages can help in showing when certain processes or objects have been started or shut down.
        /// </summary>
        /// <param name="message">Message to log</param>
        public static void LogStartStop(string message)
        {
            if (!Initialize())
                return;
            InternalLogStartStop(_mhIdentity, message);
        }

        /// <summary>
        ///     Log that a thread has started or stopped.
        /// </summary>
        /// <param name="message">Message to log</param>
        public static void LogThreadStartStop(string message)
        {
            if (!Initialize())
                return;

            InternalLogThreadStartStop(_mhIdentity, message);
        }

        /// <summary>
        ///     Log a connection message.
        /// </summary>
        /// <param name="message">Message to log</param>
        public static void LogConnection(string message)
        {
            if (!Initialize())
                return;
            InternalLogConnection(_mhIdentity, message);
        }

        /// <summary>
        ///     Log object reference counts.
        /// </summary>
        /// <param name="message">Message to log</param>
        public static void LogReferenceCount(string message)
        {
            if (!Initialize())
                return;

            InternalLogRefCount(_mhIdentity, message);
        }

        /// <summary>
        ///     Log a Constructor/Destructor message.
        /// </summary>
        /// <param name="message">Message to log</param>
        public static void LogCtorDtor(string message)
        {
            if (!Initialize())
                return;
            InternalLogCtorDtor(_mhIdentity, message);
        }

        /// <summary>
        ///     Log SQL related messages.
        ///     These messages can be used for things like dumping SQL select strings that are too long to be viewed in the
        ///     Developer Studio debugger.
        /// </summary>
        /// <param name="message">Message to log</param>
        public static void LogSql(string message)
        {
            if (!Initialize())
                return;
            InternalLogSQL(_mhIdentity, message);
        }

        /// <summary>
        ///     Log a function entry/exit message.
        ///     These messages simply flag that functions have been entered and exited.
        /// </summary>
        /// <param name="message">Message to log</param>
        public static void LogEntryExit(string message)
        {
            if (!Initialize())
                return;

            InternalLogEntryExit(_mhIdentity, message);
        }

        /// <summary>
        ///     Define your own custom message type and register it with the Logger.
        ///     In addition to the pre-defined categories of messages, if your component needs to log messages belonging to a
        ///     custom category, it can be done using custom flags
        ///     When a custom flag is registered by using the LogRegisterCustomFlag function,
        ///     the default value of the LogFlag is OFF. This means any message belonging to the custom
        ///     flag will not be logged by the FSLogger, unless it is turned ON externally by the user.
        ///     If a particular custom flag needs to be ON by default, the function
        ///     LogRegisterCustomFlagEx can be used. This function takes an additional parameter,
        ///     which sets the default value of the custom flag. If this parameter is a non-zero value,
        ///     the Custom flag is ON by default. All messages using this flag will then be logged by the
        ///     Logger (provided the flag is not turned OFF externally).
        /// </summary>
        /// <param name="flagName">Name of flag to register</param>
        /// <returns>A cookie, which can then be used to refer to the custom category in other Logger functions.</returns>
        public static int LogRegisterCustomFlag(string flagName)
        {
            return Initialize() ? RegisterLogFlag(_mhIdentity, 11, flagName) : 0;
        }

        /// <summary>
        ///     Registers a Custom flag with default value as ON or OFF
        /// </summary>
        /// <param name="flagName">Custom Flag Name</param>
        /// <param name="defaultValue">Default flag value 1 or 0 (ON/OFF)</param>
        /// <returns></returns>
        public static int LogRegisterCustomFlagEx(string flagName, int defaultValue)
        {
            return Initialize() ? RegisterLogFlagEx(_mhIdentity, 11, flagName, defaultValue) : 0;
        }

        /// <summary>
        ///     Log messages using custom category defined by LogRegisterCustomFlag
        /// </summary>
        /// <param name="cookie">Handle generated from LogRegisterCustom flag or LogRegisterCustomFlagEx</param>
        /// <param name="message">Message to log</param>
        public static void LogCustom(int cookie, string message)
        {
            if (!Initialize())
                return;

            InternalLogCustom(_mhIdentity, cookie, message);
        }

        #endregion
    }
}