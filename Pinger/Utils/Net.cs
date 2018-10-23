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
        public static long GetIcmpRtt (string targetHost)
        {
            var pinger = new System.Net.NetworkInformation.Ping();
            var reply = pinger.Send(targetHost);
            return reply.RoundtripTime;
        }

        public static long GetTcpRtt(string targetHost, ushort targetPort, uint timeoutMilliseconds)
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

            return connected ? (long)watch.Elapsed.TotalMilliseconds : 0;
        }
    }
}
