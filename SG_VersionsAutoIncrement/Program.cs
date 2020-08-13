using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;

namespace SG_VersionsAutoIncrement
{
    class Program
    {
        private static string _assemblyVersion = @"\[assembly\: AssemblyVersion\(""(\d{1,})\.(\d{1,})\.(\d{1,})\.(\d{1,})""\)\]";
        private static string _assembluFileVersion = @"\[assembly\: AssemblyFileVersion\(""(\d{1,})\.(\d{1,})\.(\d{1,})\.(\d{1,})""\)\]";
        private static int? _buildNumber = null;

        static void Main(string[] args)
        {
            try
            {
                _buildNumber = GetBuild() + 1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "GIT WARNING!", MessageBoxButton.OK, MessageBoxImage.Warning);
                _buildNumber = null;
            }


            try
            {
                string assemblyInfoText = File.ReadAllText(@"..\..\..\" + args[1] + @"\Properties\AssemblyInfo.cs");
                string result = GetResult(_assemblyVersion, assemblyInfoText, "AssemblyVersion");
                result = GetResult(_assembluFileVersion, result, "AssemblyFileVersion");

                File.Delete(@"..\..\Properties\AssemblyInfo.~cs");
                File.Copy(@"..\..\Properties\AssemblyInfo.cs", @"..\..\Properties\AssemblyInfo.~cs");
                File.WriteAllText(@"..\..\Properties\AssemblyInfo.cs", result);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "WARNING!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private static string GetResult(string pattern, string assemblyInfoText, string app)
        {
            int _revision;

            Regex regex = new Regex(pattern);

            Match m = regex.Match(assemblyInfoText);

            String buildNumber = _buildNumber?.ToString() ??  m.Groups[3].Value;

            _revision = Convert.ToInt32(m.Groups[4].Value) + 1;

            string result = string.Format("[assembly: {4}(\"{0}.{1}.{2}.{3}\")]", m.Groups[1], m.Groups[2], buildNumber, _revision, app);
            return regex.Replace(assemblyInfoText, result);
        }

        private static int GetBuild()
        {
            Process process = new Process();
            process.StartInfo.WorkingDirectory = Environment.CurrentDirectory;
            process.StartInfo.FileName = Path.Combine(ProgramFilesx86(), @"Git\cmd\git.exe");
            process.StartInfo.Arguments = @"rev-list master --count";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;

            StringBuilder output = new StringBuilder();
            Version result = new Version();
            int timeout = 10000;

            using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
            using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
            {
                process.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data == null)
                        outputWaitHandle.Set();
                    else
                        output.AppendLine(e.Data);
                };

                process.Start();
                process.BeginOutputReadLine();

                if (!(process.WaitForExit(timeout) && outputWaitHandle.WaitOne(timeout)))
                {
                    throw new Exception("Failed to load build version from git. This parameter has not been changed!");
                }
                return Convert.ToInt32(output.ToString().Trim());
            }
        }

        private static string ProgramFilesx86()
        {
            if (4 == IntPtr.Size)
            {
                return Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            }

            return Environment.GetEnvironmentVariable("ProgramFiles");
        }
    }
}
