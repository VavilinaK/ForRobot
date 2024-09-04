using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using ForRobot.Libr.Client;

namespace JsonRPCTest
{
    [TestClass]
    /// <summary>
    /// Тесты соединения
    /// </summary>
    public class UnitTestConnection
    {
        [TestMethod]
        /// <summary>
        /// Тест открытия соединения
        /// </summary>
        /// <returns></returns>
        public void TestConnection()
        {
            try
            {
                JsonRpcConnection connection = new JsonRpcConnection("169.254.59.82", 3333);
                connection.Open();
                ////while (connection.Client.Connected)
                ////{
                ////    connection.Process_State().Wait();
                ////    this.ProcessState = $"Выбрана программа {Task.Run(async () => await connection.Pro_Name()).Result.Replace("\"", "")}";
                ////}
                bool answer = Task.Run<bool>(async () => await connection.CopyMem2File("D:\\newPrograms\\R1\\edge_0_left_stm.dat", "D:\\newPrograms\\R1\\edge_0_left_stm.dat")).Result;
                Assert.AreEqual(answer, true);
                //Dictionary<String, String> answer = Task.Run<Dictionary<String, String>>(async () => await connection.File_NameList("KRC:\\R1\\Program\\")).Result;
                //bool answer = Task.Run<bool>(async () => await connection.Copy(@"D:\NewProgramm\R1\test.dat", "KRC:\\R1\\Program\\test.dat")).Result;
                //Dictionary<String, String> answer = Task.Run<Dictionary<String, String>>(async () => await connection.File_NameList("KRC:\\")).Result;
            }
            catch (Exception ex)
            {
                string mess = ex.Message;
            }
        }

        private string ProcessState { get; set; }

        [TestMethod]
        /// <summary>
        /// Тест постоянного запроса стасуса
        /// </summary>
        /// <returns></returns>
        public async Task TestState()
        {
            //string message;
            //try
            //{
            //    CancellationTokenSource cancellationToken = new CancellationTokenSource();
            //    JsonRpcConnection connection = new JsonRpcConnection("192.168.92.129", 3333);
            //    connection.Open();
            //    while (connection.Client.Connected)
            //    {
            //        var taskState = connection.Process_State();
            //        //var taskName = connection.Pro_Name();

            //        //await Task.WhenAll(new Task[] { delay, task });
            //        if (await Task.WhenAny(taskState, Task.Delay(5000, cancellationToken.Token)) == taskState)
            //        {
            //            string Pro_State = taskState.Result;
            //            switch (Pro_State)
            //            {
            //                case "#P_FREE":
            //                    this.ProcessState = "Программа не выбрана";
            //                    break;

            //                case "#P_RESET":
            //                    //taskName.Wait();
            //                    this.ProcessState = $"Выбрана программа {Task.Run(async () => await connection.Pro_Name()).Result.Replace("\"", "")}";
            //                    break;

            //                case "#P_ACTIVE":
            //                    this.ProcessState = $"Запущена программа {Task.Run(async () => await connection.Pro_Name()).Result.Replace("\"", "")}";
            //                    break;

            //                case "#P_STOP":
            //                    this.ProcessState = $"Программа {connection.Pro_Name()} остановлена";
            //                    break;

            //                case "#P_END":
            //                    this.ProcessState = $"Программа {connection.Pro_Name()} завершена";
            //                    break;

            //                default:
            //                    this.ProcessState = "Нет соединения";
            //                    break;
            //            }
            //        }
            //        else
            //        {
            //            // timeout/cancellation logic
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    message = ex.Message;
            //}
        }

        [TestMethod]
        /// <summary>
        /// Тест постоянного запроса стасуса
        /// </summary>
        /// <returns></returns>
        public async Task TestState2()
        {
            //string message;
            //try
            //{
            //    CancellationTokenSource cancellationToken = new CancellationTokenSource();
            //    JsonRpcConnection connection = new JsonRpcConnection("192.168.92.129", 3333);
            //    connection.Open();
            //    while (connection.Client.Connected)
            //    {
            //        var task = connection.Process_State();
            //        await Task.WhenAll(new Task[] { Task.Delay(1000, cancellationToken.Token), task });
            //        this.ProcessState = task.Result;
            //    }                
            //}
            //catch (Exception ex)
            //{
            //    message = ex.Message;
            //}
        }


        [TestMethod]
        /// <summary>
        /// Тест перезапуска программы
        /// </summary>
        /// <returns></returns>
        public void TestRepeat()
        {
            //string message;
            //try
            //{
            //    JsonRpcConnection connection = new JsonRpcConnection("169.254.59.82", 3333);
            //    connection.Open();
            //    Dictionary<String, String> answer = Task.Run<Dictionary<String, String>>(async () => await connection.File_NameList("KRC:\\")).Result;

            //    //bool answer = Task.Run(async () => await connection.Run("KRC:\\R1\\Program\\test_io")).Result;
            //}
            //catch (Exception ex)
            //{
            //    message = ex.Message;
            //}
        }
    }

    //public class TestClient
    //{
    //    public static string Call()
    //    {
    //        string ip_addr = "192.168.119.134";
    //        int ip_port = 3333;
    //        Task<string> answer;
    //        Task<string> answer2;
    //        Task<string> answer3;
    //        Task<string> answer4;
    //        Task<string> answer5;
    //        string res = "";

    //        TcpClient client = new TcpClient(ip_addr, ip_port) { SendTimeout=3000, ReceiveTimeout=3000 };

    //        NetworkStream stream = client.GetStream();
    //        using (var message_handler = new NewLineDelimitedMessageHandler(stream, stream, new JsonMessageFormatter()))
    //        {
    //            using (JsonRpc jsonRpc = new JsonRpc(message_handler))
    //            {
    //                jsonRpc.StartListening();
    //                // {'method':'auth','params':['My_example_KEY'],'id':1}
    //                answer = Task.Run(() => jsonRpc.InvokeAsync<string>("auth", "My_example_KEY"));

    //                answer2 = Task.Run(() => jsonRpc.InvokeAsync<string>("Var_ShowVar", "$PRO_STATE"));

    //                answer3 = Task.Run(() => jsonRpc.InvokeAsync<string>("File_Delete", "KRC:\\R1\\Program\\test.src"));

    //                object[] args = { "D:\\generation\\test.src", "KRC:\\R1\\Program\\test.src", 64 };
    //                answer4 = Task.Run(() => jsonRpc.InvokeAsync<string>("File_Copy", args));

    //                // {'method':'Select_Select','params':['KRC:\\R1\\Program\\test3.src'],'id':1}
    //                answer5 = Task.Run(() => jsonRpc.InvokeAsync<string>("Select_Select", "KRC:\\R1\\Program\\test.src"));


    //                res = answer.Result;
    //                res = answer2.Result;
    //                res = answer3.Result;
    //                res = answer4.Result;
    //                res = answer5.Result;
    //                //Console.WriteLine($"Authorization:{answer}");

    //                //// {'method':'Var_ShowVar','params':['$TRAFONAME[]'],'id':1}
    //                //answer = await jsonRpc.InvokeAsync<string>("Var_ShowVar", "$TRAFONAME[]");
    //                //Console.WriteLine($"Robot model:{answer}");

    //                //// {'method':'File_NameList','params':['KRC:\\R1',511,127],'id':1}
    //                //string root_path = "KRC:\\R1";
    //                //Dictionary<string, string> flist = await jsonRpc.InvokeAsync<Dictionary<string, string>>("File_NameList", root_path, 511, 127);
    //                //Console.WriteLine($"List of files in {root_path}:\n");
    //                //foreach (KeyValuePair<string, string> kvp in flist)
    //                //{
    //                //    Console.WriteLine($"{kvp.Key} \t: {kvp.Value}");
    //                //}

    //                //// {'method':'File_CopyFile2Mem','params':['/R1/test2.src'],'id':1}
    //                //string f_name = "/R1/test.dat";
    //                //answer = await jsonRpc.InvokeAsync<string>("File_CopyFile2Mem", f_name);
    //                //Console.WriteLine($"File {f_name} content:\n------------\n{answer}\n-------------\n");
    //             }
    //            stream.Close();
    //            client.Close();
    //        }
    //        return res;
    //    }

    //    public static string CallList()
    //    {
    //        string ip_addr = "192.168.119.134";
    //        int ip_port = 3333;
    //        string res = "";

    //        TcpClient client = new TcpClient(ip_addr, ip_port) { SendTimeout = 3000, ReceiveTimeout = 3000 };

    //        NetworkStream stream = client.GetStream();
    //        using (var message_handler = new NewLineDelimitedMessageHandler(stream, stream, new JsonMessageFormatter()))
    //        {
    //            using (JsonRpc jsonRpc = new JsonRpc(message_handler))
    //            {
    //                jsonRpc.StartListening();
    //                List<Task<string>> taskList = new List<Task<string>>();
    //                // {'method':'auth','params':['My_example_KEY'],'id':1}
    //                taskList.Add(Task.Run(() => jsonRpc.InvokeAsync<string>("auth", "My_example_KEY")));

    //                taskList.Add(Task.Run(async () => await jsonRpc.InvokeAsync<string>("Var_ShowVar", "$PRO_STATE")));

    //                // {'method':'Select_Select','params':['KRC:\\R1\\Program\\test3.src'],'id':1}
    //                taskList.Add(Task.Run(() => jsonRpc.InvokeAsync<string>("Select_Select", "KRC:\\R1\\Program\\test3.src")));

    //                //Task.WaitAll(taskList.ToArray());

    //                foreach(Task<string> task in taskList)
    //                {
    //                    res = task.Result;
    //                }
    //            }
    //            stream.Close();
    //            client.Close();
    //        }
    //        return res;
    //    }
    //}
}
