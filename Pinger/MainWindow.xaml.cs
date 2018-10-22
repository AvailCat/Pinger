using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Pinger
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private Timer timer;
        private string host;
        private List<long> rttResults = new List<long>();
        private int barCount = 30;

        public MainWindow()
        {
            InitializeComponent();
            UpdateCanvas();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (timer != null)
            {
                timer.Dispose();
            }

            host = HostInput.Text;
            timer = new Timer(Convert.ToInt32(IntervalInput.Text));
            timer.Elapsed += IntervalPing;
            timer.Enabled = true;
        }

        private void IntervalPing(Object source, ElapsedEventArgs e)
        {
            var pinger = new Ping();
            var reply = pinger.Send(host);
            if (rttResults.Count >= barCount)
            {
                rttResults.RemoveAt(rttResults.Count - 1);
            }
            rttResults.Insert(0, reply.RoundtripTime > 0 ? reply.RoundtripTime : 1);
            UpdateCanvas();
        }

        private void UpdateCanvas ()
        {
            Dispatcher.Invoke(() =>
            {
                var c = ResultCanvas;
                c.Children.Clear();

                // Set title bar
                c.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF9F9F9"));
                var text = new TextBlock
                {
                    Text = $"Latency graph ({barCount})",
                    Foreground = Brushes.Gray
                };
                Canvas.SetTop(text, 0);
                Canvas.SetLeft(text, 0);
                c.Children.Add(text);

                // Draw bar
                var cHeight = c.ActualHeight - 50;
                var cWidth = c.ActualWidth;
                var barWidth = cWidth / barCount;
                for (var i = 0; i < rttResults.Count; i++)
                {
                    var currentRtt = rttResults[i];
                    var barHeight = ((float)currentRtt / rttResults.Max()) * cHeight;
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

        private void IntervalInput_PreviewTextInput(object sender, TextCompositionEventArgs e)
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
