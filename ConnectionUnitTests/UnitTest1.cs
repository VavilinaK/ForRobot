using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using ForRobot.Libr.Client;

namespace ConnectionUnitTests
{
    [TestClass]
    public class UnitTestJsonRpcConnectionMethods
    {
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
                bool answer = Task.Run(async () => await connection.CopyMem2File("D:\\newPrograms\\R1\\edge_0_left_stm.dat", "D:\\newPrograms\\R1\\edge_0_left_stm.dat")).Result;
                Assert.AreEqual(answer, true);
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
                bool answer = Task.Run(async () => await connection.Start()).Result;
                Assert.AreEqual(answer, true);
            }
            catch (Exception ex)
            {
                string mess = ex.Message;
            }
        }
    }
}
