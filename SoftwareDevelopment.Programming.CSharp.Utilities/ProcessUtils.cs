using System;
using System.IO;
using System.Diagnostics;
using System.Reflection;

namespace SoftwareDevelopment.Programming.CSharp.Utilities
{
    /// <summary>
    /// Util class for common operations related to .NET Process class.
    /// </summary>
    [Obfuscation(ApplyToMembers = true, Exclude = false, StripAfterObfuscation = true)]
    public static class ProcessUtils
	{
        /// <summary>
        /// Run external process with provided parameters.
        /// </summary>
        /// <param name="processName">name of the process to run</param>
        /// <param name="processArgs">process arguments</param>
        /// <param name="redirectStandardOutput">specifies whether to redirect process output</param>
        /// <param name="pathToLogFile">path to log file</param>
        /// <param name="waitForExit">specifies whether to run process in an asynchronous way</param>
		public static void RunProcess(string processName, string processArgs, bool redirectStandardOutput = false, string pathToLogFile = null, bool waitForExit = true)
		{
			LogUtils.Log("Preparing process setup info... ", false);
			ProcessStartInfo processSetupParams = new ProcessStartInfo();
			processSetupParams.WorkingDirectory = Environment.CurrentDirectory;
			processSetupParams.FileName = processName;
			processSetupParams.Arguments = processArgs;
			processSetupParams.CreateNoWindow = true;
			processSetupParams.UseShellExecute = false;
			processSetupParams.WindowStyle = ProcessWindowStyle.Hidden;

            if (redirectStandardOutput && String.IsNullOrEmpty(pathToLogFile))
            {
                throw ExceptionUtils.CreateException("Redirecting process output to log file requires providing log file path. " + ExceptionUtils.ArgumentNullException_MessageFormat, "pathToLogFile");
            }
            processSetupParams.RedirectStandardOutput = redirectStandardOutput;

            LogUtils.Log("done", true);

			LogUtils.Log(string.Format("Executing process for '{0}'... ", processName), false);
			Process process = new Process();
            process.StartInfo = processSetupParams;

            process.Start();

            if (redirectStandardOutput)
            {
                string processOutput = process.StandardOutput.ReadToEnd();
                File.AppendAllText(pathToLogFile, processOutput);
            }

            if(waitForExit)
                process.WaitForExit();

			LogUtils.Log(string.Format("done ('{0}' completed successfully)", processName), true);
		}
	}
}
