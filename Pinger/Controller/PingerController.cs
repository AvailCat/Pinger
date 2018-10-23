using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Pinger.Model;

namespace Pinger.Controller
{
    class PingerController
    {
        public PingerStatus Status = PingerStatus.Stopped;
        public PingProtocol PingProtocol { get; private set; }
        public uint Interval;
        public uint PoolLength;

        private Thread pingThread;

        private string _targetHost;
        private ushort? _targetPort;
        private uint? _timeout;
        private ObservableCollection<double> _rttPool;

        public PingerController (uint interval, uint poolLength)
        {
            Interval = interval;
            PoolLength = poolLength;
        }
        public void Setup (string targetHost, uint interval)
        {
            _targetPort= null;
            _timeout = null;

            PingProtocol = PingProtocol.ICMP;
            _targetHost = targetHost;
            Interval = interval;
        }
        public void Setup (string targetHost, ushort targetPort, uint timeout, uint interval)
        {
            PingProtocol = PingProtocol.TCP;
            _targetHost = targetHost;
            _targetPort = targetPort;
            _timeout = timeout;
            Interval = interval;
        }
        public void Start (ref ObservableCollection<double> rttPool)
        {
            _rttPool = rttPool;
            CheckOldThread();

            switch (PingProtocol)
            {
                case PingProtocol.ICMP:
                    pingThread = new Thread(new ThreadStart(IcmpPing));
                    pingThread.Start();
                    break;
                case PingProtocol.TCP:
                    pingThread = new Thread(new ThreadStart(TcpPing));
                    pingThread.Start();
                    break;
                default:
                    throw new Exception("Unknown protocol");
            }

             Status = PingerStatus.Started;
        }
        public void Stop ()
        {
            pingThread.Abort();
            pingThread = null;
            Status = PingerStatus.Stopped;
        }

        private void CheckOldThread ()
        {
            if (pingThread != null)
            {
                pingThread.Abort();
                pingThread = null;
            }
        }
        private void PoolMaintainer (ref ObservableCollection<double> rttPool)
        {
            while (rttPool.Count >= PoolLength)
            {
                rttPool.RemoveAt(rttPool.Count - 1);
            }
        }
        private void IcmpPing()
        {
            while (true)
            {
                PoolMaintainer(ref _rttPool);
                double rtt = 0;
                try { rtt = Utils.Ping.GetIcmpRtt(_targetHost); } catch { }
                _rttPool.Insert(0, rtt);
                Thread.Sleep((int)Interval);
            }
        }
        private void TcpPing()
        {
            while (true)
            {
                PoolMaintainer(ref _rttPool);
                _rttPool.Insert(0, Utils.Ping.GetTcpRtt(_targetHost, (ushort)_targetPort, (uint)_timeout));
                Thread.Sleep((int)Interval);
            }
        }
    }
}
