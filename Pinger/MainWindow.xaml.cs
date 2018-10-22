using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Pinger.Model;

namespace Pinger
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        // MainWindow
        private MainWindowViewModel ctx;
        private int barCount = 30;

        private string targetHost;
        private int targetPort;
        private PingProtocol pingProtocol;

        private Timer timer;
        private List<long> rttResults = new List<long>();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
            ctx = (MainWindowViewModel)DataContext;
            UpdateCanvas();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (timer != null)
            {
                timer.Dispose();
                timer = null;
                ctx.ActionButtonText = "Start";
                return;
            } else
            {
                rttResults.Clear();
                ctx.ActionButtonText = "Stop";
            }

            targetHost = HostInput.Text;
            targetPort = Convert.ToInt32(PortInput.Text);
            pingProtocol = ctx.SelectedPingProtocol;

            timer = new Timer(ctx.PingInterval);
            timer.Elapsed += IntervalPing;
            timer.Enabled = true;
        }

        private void IntervalPing(Object source, ElapsedEventArgs e)
        {
            // Prevent bar chart out of screen
            if (rttResults.Count >= barCount)
            {
                while (rttResults.Count > barCount - 1)
                {
                    rttResults.RemoveAt(rttResults.Count - 1);
                }
            }

            if (pingProtocol == PingProtocol.ICMP)
            {
                var pinger = new Ping();
                var reply = pinger.Send(targetHost);
                rttResults.Insert(0, reply.RoundtripTime);
            } else
            {
                var sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    Blocking = true
                };

                var stopwatch = new Stopwatch();

                stopwatch.Start();
                sock.Connect(targetHost, targetPort);
                stopwatch.Stop();
                sock.Close();

                var time = (long)stopwatch.Elapsed.TotalMilliseconds;
                rttResults.Insert(0, time);
            }
            
            UpdateCanvas();
        }

        private void UpdateCanvas()
        {
            Dispatcher.Invoke(() =>
            {
                var c = ResultCanvas;
                c.Children.Clear();

                // Set title bar
                c.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF9F9F9"));
                var text = new TextBlock
                {
                    Text = $"Latency graph ({barCount}, {barCount * (ctx.PingInterval / 1000f)}s)",
                    Foreground = Brushes.Gray
                };
                Canvas.SetTop(text, 0);
                Canvas.SetLeft(text, 0);
                c.Children.Add(text);

                // Draw bar
                var cHeight = c.ActualHeight - 30;
                var cWidth = c.ActualWidth;
                var barWidth = cWidth / barCount;
                for (var i = 0; i < rttResults.Count; i++)
                {
                    var currentRtt = rttResults[i];
                    var barHeight = ((float)currentRtt / Math.Max(rttResults.Max(), 1)) * cHeight;
                    var barRight = i * barWidth;

                    var bar = new Rectangle
                    {
                        Fill = Brushes.Pink,
                        Height = barHeight,
                        Width = barWidth - 5
                    };
                    Canvas.SetBottom(bar, 0);
                    Canvas.SetRight(bar, barRight);

                    // Latency text on per bar
                    var rttText = new TextBlock
                    {
                        Text = currentRtt.ToString(),
                        FontWeight = FontWeights.Bold,
                        FontSize = barWidth / currentRtt.ToString().Length,
                        Width = barWidth - 5,
                        TextAlignment = TextAlignment.Center
                    };
                    Canvas.SetBottom(rttText, barHeight + 5);
                    Canvas.SetRight(rttText, barRight);
                    c.Children.Add(rttText);
                    c.Children.Add(bar);
                }

            });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (timer != null)
            {
                timer.Dispose();
            }
        }

        private void PreviewTextInput_NumberOnly(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void HostInput_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"[^(0-9|a-z|A-Z|\.)]");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
