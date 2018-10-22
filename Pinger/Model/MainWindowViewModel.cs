using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace Pinger.Model
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public IEnumerable<PingProtocol> PingProtocolValues
        {
            get => Enum.GetValues(typeof(PingProtocol)).Cast<PingProtocol>();
        }

        // Private use
        private PingProtocol selectedPingProtocol;
        private string actionButtonText = "Start";

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindowViewModel()
        {
        }

        // Public access
        public int PingInterval { get; private set; } = 1000;
        public string PingIntervalText
        {
            get => $"{PingInterval}ms";
            set
            {
                var text = value;
                var regex = new Regex(@"\d+");
                PingInterval = Convert.ToInt32(regex.Match(text).ToString());
            }
        }
        public PingProtocol SelectedPingProtocol
        {
            get => selectedPingProtocol;
            set
            {
                selectedPingProtocol = value;
                OnPropertyChanged("SelectedPingProtocol");
                OnPropertyChanged("PortInputEnabled");
            }
        }
        public string ActionButtonText
        {
            get => actionButtonText;
            set
            {
                actionButtonText = value;
                OnPropertyChanged("ActionButtonText");
            }
        }

        public bool PortInputEnabled
        {
            get => selectedPingProtocol == PingProtocol.TCP;
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
