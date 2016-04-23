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
        /// <param name="processName"></param>
        /// <param name="processArgs"></param>
        /// <param name="redirectStandardOutput"></param>
        /// <param name="pathToLogFile"></param>
		public static void RunProcess(string processName, string processArgs, bool redirectStandardOutput = false, string pathToLogFile = null)
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
                throw ExceptionUtils.CreateException(ExceptionUtils.ArgumentNullException_MessageFormat, "pathToLogFile");
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

            process.WaitForExit();
			LogUtils.Log(string.Format("done ('{0}' completed successfully)", processName), true);
		}
	}
}
