using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using ForRobot.Libr.Client;

namespace ConnectionUnitTests
{
    [TestClass]
    public class UnitTestsJsonRpcConnection
    {
        private JsonRpcConnection _connection = new JsonRpcConnection("192.168.92.143", 3333);

        [TestMethod]
        /// <summary>
        /// Копирование файла на Пк
        /// </summary>
        public void TestCopyMem2File()
        {
            try
            {
                JsonRpcConnection connection = new JsonRpcConnection("192.168.92.128", 3333);
                connection.Open();
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
                connection.Open();
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
                _connection.Open();
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
                _connection.Open();
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
                JsonRpcConnection connection = new JsonRpcConnection("192.168.92.167", 3333);
                connection.Open();

                string s = Task.Run(() => _connection.Process_StateAsync(), cancelTokenSource.Token).Result;

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
    }
}
