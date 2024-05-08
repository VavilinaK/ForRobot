using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Net;
using System.Linq;
using System.Windows;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Concurrency;

//using Renci.SshNet;
//using Renci.SshNet.Common;

//using Newtonsoft.Json;

//using AustinHarris.JsonRpc;
//using AustinHarris.JsonRpc.Client;

//using JsonRPCTest.Classes;

using System.Net.Sockets;
using StreamJsonRpc;

namespace JsonRPCTest
{
    [TestClass]
    public class UnitTest1
    {
        /// <summary>
        /// Рабочий тест
        /// </summary>
        [TestMethod]
        public void TestJsonClient()
        {
            //string message;
            //try
            //{
                string answer = TestClient.Call();
            //}
            //catch (Exception ex)
            //{
            //    message = ex.Message;
            //}
        }

        [TestMethod]
        public void TestJsonClient2()
        {
            //string message;
            //try
            //{
            string answer = TestClient.CallList();
            //}
            //catch (Exception ex)
            //{
            //    message = ex.Message;
            //}
        }
    }

    public class TestClient
    {
        public static string Call()
        {
            string ip_addr = "192.168.119.134";
            int ip_port = 3333;
            Task<string> answer;
            Task<string> answer2;
            Task<string> answer3;
            Task<string> answer4;
            Task<string> answer5;
            string res = "";

            TcpClient client = new TcpClient(ip_addr, ip_port) { SendTimeout=3000, ReceiveTimeout=3000 };

            NetworkStream stream = client.GetStream();
            using (var message_handler = new NewLineDelimitedMessageHandler(stream, stream, new JsonMessageFormatter()))
            {
                using (JsonRpc jsonRpc = new JsonRpc(message_handler))
                {
                    jsonRpc.StartListening();
                    // {'method':'auth','params':['My_example_KEY'],'id':1}
                    answer = Task.Run(() => jsonRpc.InvokeAsync<string>("auth", "My_example_KEY"));

                    answer2 = Task.Run(() => jsonRpc.InvokeAsync<string>("Var_ShowVar", "$PRO_STATE"));

                    answer3 = Task.Run(() => jsonRpc.InvokeAsync<string>("File_Delete", "KRC:\\R1\\Program\\test.src"));

                    object[] args = { "D:\\generation\\test.src", "KRC:\\R1\\Program\\test.src", 64 };
                    answer4 = Task.Run(() => jsonRpc.InvokeAsync<string>("File_Copy", args));

                    // {'method':'Select_Select','params':['KRC:\\R1\\Program\\test3.src'],'id':1}
                    answer5 = Task.Run(() => jsonRpc.InvokeAsync<string>("Select_Select", "KRC:\\R1\\Program\\test.src"));


                    res = answer.Result;
                    res = answer2.Result;
                    res = answer3.Result;
                    res = answer4.Result;
                    res = answer5.Result;
                    //Console.WriteLine($"Authorization:{answer}");

                    //// {'method':'Var_ShowVar','params':['$TRAFONAME[]'],'id':1}
                    //answer = await jsonRpc.InvokeAsync<string>("Var_ShowVar", "$TRAFONAME[]");
                    //Console.WriteLine($"Robot model:{answer}");

                    //// {'method':'File_NameList','params':['KRC:\\R1',511,127],'id':1}
                    //string root_path = "KRC:\\R1";
                    //Dictionary<string, string> flist = await jsonRpc.InvokeAsync<Dictionary<string, string>>("File_NameList", root_path, 511, 127);
                    //Console.WriteLine($"List of files in {root_path}:\n");
                    //foreach (KeyValuePair<string, string> kvp in flist)
                    //{
                    //    Console.WriteLine($"{kvp.Key} \t: {kvp.Value}");
                    //}

                    //// {'method':'File_CopyFile2Mem','params':['/R1/test2.src'],'id':1}
                    //string f_name = "/R1/test.dat";
                    //answer = await jsonRpc.InvokeAsync<string>("File_CopyFile2Mem", f_name);
                    //Console.WriteLine($"File {f_name} content:\n------------\n{answer}\n-------------\n");
                 }
                stream.Close();
                client.Close();
            }
            return res;
        }

        public static string CallList()
        {
            string ip_addr = "192.168.119.134";
            int ip_port = 3333;
            string res = "";

            TcpClient client = new TcpClient(ip_addr, ip_port) { SendTimeout = 3000, ReceiveTimeout = 3000 };

            NetworkStream stream = client.GetStream();
            using (var message_handler = new NewLineDelimitedMessageHandler(stream, stream, new JsonMessageFormatter()))
            {
                using (JsonRpc jsonRpc = new JsonRpc(message_handler))
                {
                    jsonRpc.StartListening();
                    List<Task<string>> taskList = new List<Task<string>>();
                    // {'method':'auth','params':['My_example_KEY'],'id':1}
                    taskList.Add(Task.Run(() => jsonRpc.InvokeAsync<string>("auth", "My_example_KEY")));

                    taskList.Add(Task.Run(async () => await jsonRpc.InvokeAsync<string>("Var_ShowVar", "$PRO_STATE")));

                    // {'method':'Select_Select','params':['KRC:\\R1\\Program\\test3.src'],'id':1}
                    taskList.Add(Task.Run(() => jsonRpc.InvokeAsync<string>("Select_Select", "KRC:\\R1\\Program\\test3.src")));

                    //Task.WaitAll(taskList.ToArray());

                    foreach(Task<string> task in taskList)
                    {
                        res = task.Result;
                    }
                }
                stream.Close();
                client.Close();
            }
            return qqqq;
        }
    }
}
