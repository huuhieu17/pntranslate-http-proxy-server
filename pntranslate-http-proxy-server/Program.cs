using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using BenderProxy;
using BenderProxy.Headers;
using BenderProxy.Writers;
using Microsoft.Win32;
using HttpResponseHeader = BenderProxy.Headers.HttpResponseHeader;

namespace pntranslate_http_proxy_server
{
    public class Program
    {
        static void Main(string[] args)
        {
            RegistryKey registry =
                Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings",
                    true);
            registry.SetValue("ProxyEnable", 1);
            registry.SetValue("ProxyServer", "localhost:8081");
            HttpProxyServer httpProxyServer = new HttpProxyServer("localhost", 8081, new HttpProxy()
            {
                OnResponseReceived = context =>
                {
                    Console.WriteLine(context.RequestHeader.RequestURI);
                    switch (context.RequestHeader.RequestURI)
                    {
                        case "http://v1.ninjawars.ru/":
                            SendFile("test.html", context, 250);
                            break;
                        case "http://v1.ninjawars.ru/DM.swf":
                            SendFile("DM.swf", context, 500);
                            registry.SetValue("ProxyEnable", 0);
                            Environment.Exit(0);
                            break;
                    }
                }
            });
            httpProxyServer.Start();
        }

        static void SendFile(String fileName, ProcessingContext context, int timeout)
        {
            var bytesToWrite = File.ReadAllBytes(fileName);
            var responseStream = new MemoryStream(bytesToWrite, false);
            context.ResponseHeader.EntityHeaders.ContentLength = responseStream.Length;
            Console.WriteLine(context.ResponseHeader);
            new HttpResponseWriter(context.ClientStream).Write(context.ResponseHeader, responseStream, responseStream.Length);
            Thread.Sleep(timeout);
        }
    }
}