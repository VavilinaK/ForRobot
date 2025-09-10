using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Sockets;

using ForRobot.Libr.Client;

namespace ConnectionUnitTests
{
    [TestClass]
    public class UnitTestsJsonRpcConnection
    {
        //private JsonRpcConnection connection = new JsonRpcConnection("192.168.92.185", 3333);
        string Host = "192.168.92.185";
        int Port = 3333;

        [TestMethod]
        /// <summary>
        /// Копирование файла на Пк
        /// </summary>
        public void TestCopyMem2File()
        {
            try
            {
                JsonRpcConnection connection = new JsonRpcConnection("192.168.92.128", 3333);
                //connection.Open();
                //bool answer = Task.Run(async () => await connection.CopyMem2File("D:\\newPrograms\\R1\\edge_0_left_stm.dat", "D:\\newPrograms\\R1\\edge_0_left_stm.dat")).Result;
                //Assert.AreEqual(answer, true);
            }
            catch(Exception ex)
            {
                string mess = ex.Message;
            }
        }

        [TestMethod]
        /// <summary>
        /// Запуск уже выбранной программы
        /// </summary>
        public void TestStart()
        {
            try
            {
                JsonRpcConnection connection = new JsonRpcConnection("192.168.92.128", 3333);
                //connection.Open();
                //bool answer = Task.Run(async () => await connection.Start()).Result;
                //Assert.AreEqual(answer, true);
            }
            catch (Exception ex)
            {
                string mess = ex.Message;
            }
        }

        [TestMethod]
        /// <summary>
        /// Проверки отправки во временную папку
        /// </summary>
        public void TestTemp()
        {
            try
            {
                //_connection.Open();
                string filePath = @"D:\newPrograms\R1\main_gen.src";
                //string tempPath = @"C:\Users\KukaUser\AppData\Local\Temp";
                //string tempPath = $@"C:\Windows\Temp\{ResourceAssembly.GetName().Name}";
                string tempPath = $@"C:\Windows\Temp";

                string file = Path.Combine(tempPath, Path.GetFileName(filePath));

                string robotPath = Path.Combine(@"KRC:\R1", Path.GetFileName(filePath));

                //string finishPath = @"KRC:\R1\Program\Generation";
                //System.Collections.Generic.Dictionary<string, string> answer = Task.Run(async () => await connection.File_NameList(tempPath)).Result;

                //if (!Task.Run<bool>(async () => await _connection.CopyMem2File(filePath, file)).Result)
                //    return;

                //if (!Task.Run<bool>(async () => await _connection.Copy(file, robotPath)).Result)
                //    return;

                //Assert.AreNotEqual(answer, null);
            }
            catch (Exception ex)
            {
                string mess = ex.Message;
            }
        }

        [TestMethod]
        /// <summary>
        /// Удаление файла при запущенном процессе
        /// </summary>
        public void TestDelete()
        {
            try
            {
                //JsonRpcConnection connection = new JsonRpcConnection("192.168.92.143", 3333);
                //_connection.Open();
                string filePath = @"KRC:\R1\Program\Generation\main_gen.src";

                //if (!Task.Run<bool>(async () => await _connection.Delet(filePath)).Result)
                //    return;

                //Assert.AreNotEqual(answer, null);
            }
            catch (Exception ex)
            {
                string mess = ex.Message;
            }
        }

        [TestMethod]
        public void TestPeriodTask()
        {
            try
            {
                CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
                //connection.Open();
                JsonRpcConnection connection = new JsonRpcConnection(Host, Port);

                string s = Task.Run(() => connection.Process_StateAsync(), cancelTokenSource.Token).Result;

                Thread.Sleep(10000);

                cancelTokenSource.Cancel();
                connection.Close();
                //bool answer = Task.Run(async () => await connection.CopyMem2File("D:\\newPrograms\\R1\\edge_0_left_stm.dat", "D:\\newPrograms\\R1\\edge_0_left_stm.dat")).Result;
                //Assert.AreEqual(answer, true);
            }
            catch (Exception ex)
            {
                string mess = ex.Message;
            }
        }

        [TestMethod]
        public void TestDownlade()
        {
            try
            {
                string path = "D:\\ForRobot\\Test";
                string downladePath = "KRC:\\R1\\Program\\gen\\edge_0_left_ste.dat";
                JsonRpcConnection connection = new JsonRpcConnection(Host, Port);
                connection.Open(1000);
                string result = Task.Run(async () => await connection.CopyFile2MemAsync(downladePath)).Result;
                //Assert.AreEqual(answer, true);
                connection.Close();

                string newPath = Path.Combine(path, Path.GetFileName(downladePath));

                //File.Create(newPath);
                using (var sw = new StreamWriter(newPath, true))
                {
                    sw.WriteLine(result);
                }
            }
            catch(Exception ex)
            {
                string mess = ex.Message;
            }
        }
    }
}
