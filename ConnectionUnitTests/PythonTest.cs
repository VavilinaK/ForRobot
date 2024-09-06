using System;
using System.IO;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using IronPython.Modules;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;

namespace ConnectionUnitTests
{
    [TestClass]
    public class PythonTest
    {
        [TestMethod]
        public void RunTest()
        {
            //            ScriptEngine engine = Python.CreateEngine();
            //            ScriptScope scope = engine.CreateScope();          
            //            scope.SetVariable("p", @"D:\newPrograms\main_gen.json");
            //            scope.SetVariable("o", @"D:\newPrograms");
            //            scope.SetVariable("n", "main_gen.src");
            //            //engine.ImportModule("numpy");
            ////            var libs = new[] {
            ////    "C:\\Program Files (x86)\\Microsoft Visual Studio 14.0\\Common7\\IDE\\Extensions\\Microsoft\\Python Tools for Visual Studio\\2.2",
            ////    "C:\\Program Files (x86)\\IronPython 2.7\\Lib",
            ////    "C:\\Program Files (x86)\\IronPython 2.7\\DLLs",
            ////    "C:\\Program Files (x86)\\IronPython 2.7",
            ////    "C:\\Program Files (x86)\\IronPython 2.7\\lib\\site-packages"
            ////};

            ////            engine.SetSearchPaths(libs);
            //            engine.ExecuteFile("genPlita.py", scope);

            string error = string.Empty;

            string[] args = { @"-p D:\newPrograms\main_gen.json", "-o \"D:\\newPrograms\"", "-n \"main_gen.src\"" };

            Process process = new Process()
            {               
                StartInfo = new ProcessStartInfo()
                {
                    UseShellExecute = false,
                    RedirectStandardInput = false,
                    RedirectStandardOutput = false,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = new FileInfo(Path.GetFullPath("Scripts/test_weld_gen.py")).DirectoryName,
                    FileName = "python.exe",
                    Arguments = Path.GetFullPath("Scripts/test_weld_gen.py") + " " + string.Join(" ", args)
                }
            };
            process.ErrorDataReceived += new DataReceivedEventHandler((sender, e) => { error += e.Data + "\n"; });
            process.Start();
            //process.BeginErrorReadLine();
            string errorStr = process.StandardError.ReadToEnd();
            //string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            //if (process.ExitCode == 0) // Перенести
            //{

            //}
            //else
            //{

            //}
        }
    }
}
