using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Pinger.Utils
{
    class Ping
    {
        public static double GetIcmpRtt (string targetHost)
        {
            var pinger = new System.Net.NetworkInformation.Ping();
            var watch = new Stopwatch();
            watch.Start();
            var reply = pinger.Send(targetHost);
            watch.Stop();
            return reply.Status == System.Net.NetworkInformation.IPStatus.Success ? watch.Elapsed.TotalMilliseconds : 0;
        }

        public static double GetTcpRtt(string targetHost, ushort targetPort, uint timeoutMilliseconds)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                Blocking = true
            };

            var watch = new Stopwatch();

            watch.Start();
            var result = socket.BeginConnect(targetHost, targetPort, null, null);
            var connected = result.AsyncWaitHandle.WaitOne((int)timeoutMilliseconds, true);
            watch.Stop();
            socket.Close();

            return connected ? watch.Elapsed.TotalMilliseconds : 0;
        }
    }
}
