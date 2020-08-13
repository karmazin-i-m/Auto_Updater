using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace SG_VersionsAutoIncrement
{
    class Program
    {
        private static string _assemblyVersion = @"\[assembly\: AssemblyVersion\(""(\d{1,})\.(\d{1,})\.(\d{1,})\.(\d{1,})""\)\]";
        private static string _assembluFileVersion = @"\[assembly\: AssemblyFileVersion\(""(\d{1,})\.(\d{1,})\.(\d{1,})\.(\d{1,})""\)\]";
        static void Main(string[] args)
        {
            try
            {
                string str = File.ReadAllText(@"..\..\Properties\AssemblyInfo.cs");

            }
            catch (Exception ex)
            {

            }
        }

        private static string GetResult(string pattern, string str, string app)
        {
            int _build;

            Regex regex = new Regex(pattern);

            Match m = regex.Match(str);

            _build = Convert.ToInt32(m.Groups[3].Value) + 1;

            string rz = string.Format("[assembly: {4}(\"{0}.{1}.{2}.{3}\")]", m.Groups[1], m.Groups[2], _build, _revno, app);
            return regex.Replace(str, rz);
        }

        private static int GetBuild()
        {
            //Process gitProcess = new Process();
            //gitProcess.StartInfo.WorkingDirectory = Environment.CurrentDirectory;
            //gitProcess.StartInfo.FileName = Path.Combine(ProgramFilesx86(), @"Git\git-bash.exe");
            //gitProcess.StartInfo.Arguments = @"rev-list master --count";
            //gitProcess.StartInfo.UseShellExecute = false;
            //gitProcess.StartInfo.RedirectStandardOutput = true;
            //gitProcess.StartInfo.RedirectStandardError = true;
            //gitProcess.OutputDataReceived += (sender, e) => 
            //{
            //    if (e.Data == null)
            //        outputWaitHandle.Set();
            //    else
            //        output.AppendLine(e.Data);
            //};


            //gitProcess.Start();

            //StreamWriter sortStreamWriter = gitProcess.StandardInput;
            //gitProcess.BeginOutputReadLine();
            //sortStreamWriter.Close();
            //gitProcess.WaitForExit();
            //return Convert.ToInt32(sortOutput[0].ToString());

            Process process = new Process();
            process.StartInfo.WorkingDirectory = Environment.CurrentDirectory;
            process.StartInfo.FileName = Path.Combine(ProgramFilesx86(), @"Git\git-bash.exe");
            process.StartInfo.Arguments = @"rev-list master --count";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;

            StringBuilder output = new StringBuilder();
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

                    if (process.WaitForExit(timeout) && outputWaitHandle.WaitOne(timeout))
                    {
                        string text = File.ReadAllText(@"..\..\..\" + args[1] + @"\Properties\AssemblyInfo.cs");

                        Match match = new Regex("AssemblyVersion\\(\"(.*?)\"\\)").Match(text);
                        Version ver = new Version(match.Groups[1].Value);
                        int build = args[0] == "Release" ? ver.Build + 1 : ver.Build;
                        Version newVer = new Version(ver.Major, ver.Minor, build, Convert.ToInt16(output.ToString().Trim()));

                        text = Regex.Replace(text, @"AssemblyVersion\((.*?)\)", "AssemblyVersion(\"" + newVer.ToString() + "\")");
                        text = Regex.Replace(text, @"AssemblyFileVersionAttribute\((.*?)\)", "AssemblyFileVersionAttribute(\"" + newVer.ToString() + "\")");
                        text = Regex.Replace(text, @"AssemblyFileVersion\((.*?)\)", "AssemblyFileVersion(\"" + newVer.ToString() + "\")");

                        File.WriteAllText(@"..\..\..\" + args[1] + @"\Properties\AssemblyInfo.cs", text);
                    }
                }
        }

        static string ProgramFilesx86()
        {
            if (8 == IntPtr.Size
                || (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"))))
            {
                return Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            }

            return Environment.GetEnvironmentVariable("ProgramFiles");
        }
    }
}
