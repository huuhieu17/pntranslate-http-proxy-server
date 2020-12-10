using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Microsoft.Win32;

namespace pntranslate_http_proxy_server
{
    public class Program
    {
        
        private static TcpListener _tcpListener;

        static void Main(string[] args)
        {
            try
            {
                _tcpListener = new TcpListener(IPAddress.Loopback, 8081);
                _tcpListener.Start();
                WebClient wc = new WebClient();
                RegistryKey registry =
                    Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings",
                        true);
                registry.SetValue("ProxyEnable", 1);
                registry.SetValue("ProxyServer", "localhost:8081");
                while (true)
                {
                    TcpClient client = _tcpListener.AcceptTcpClient();
                    Stream clientStream = client.GetStream();
                    StreamReader clientStreamReader = new StreamReader(clientStream);
                    String httpCmd = clientStreamReader.ReadLine();
                    String[] splitBuffer = httpCmd.Split(" ", 3);
                    String method = splitBuffer[0];
                    String remoteUri = splitBuffer[1];
                    Console.WriteLine($"{method} {remoteUri}");
                    byte[] responseBytes;
                    if (method.Equals("GET"))
                    {
                        switch (remoteUri)
                        {
                            case "http://v1.ninjawars.ru/":
                                responseBytes = File.ReadAllBytes("test.html");
                                clientStream.Write(responseBytes, 0, responseBytes.Length);
                                clientStream.Close();
                                break;
                            case "http://v1.ninjawars.ru/DM.swf":
                                responseBytes = File.ReadAllBytes("DM.swf");
                                clientStream.Write(responseBytes, 0, responseBytes.Length);
                                clientStream.Close();
                                registry.SetValue("ProxyEnable", 0);
                                Environment.Exit(0);
                                break;
                            default:
                                registry.SetValue("ProxyEnable", 0);
                                responseBytes = wc.DownloadData(remoteUri);
                                clientStream.Write(responseBytes, 0, responseBytes.Length);
                                clientStream.Close();
                                registry.SetValue("ProxyEnable", 1);
                                break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                RegistryKey registry = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", true);
                registry.SetValue("ProxyEnable", 0);
                throw;
            }
        }
    }
}