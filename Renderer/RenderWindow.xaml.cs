using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CefSharp;
using CefSharp.WinForms;
using System.Windows.Forms.Integration;
using System.Runtime.InteropServices;

namespace Renderer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string Mode;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var args = Environment.GetCommandLineArgs();
            try
            {
                Init(args);
            }
            catch (Exception ex)
            {
                ShowWindow(GetConsoleWindow(), 5);
                Console.WriteLine("Invalid arguments, please execute this application from the StreamGround2 main user interface");
                Console.WriteLine("Alternatively, you can specify the arguments here:");
                Init(Console.ReadLine().Split());
            }
        }

        void Init(string[] args)
        {
            Left = int.Parse(args[1]);
            Top = int.Parse(args[2]);
            Width = int.Parse(args[3]);
            Height = int.Parse(args[4]);
            StreamGround2.W32.SetParent(new WindowInteropHelper(this).Handle, new IntPtr(long.Parse(args[5])));
            Mode = args[6];
            switch (Mode)
            {
                case "media":
                    {
                        var player = new MediaElement()
                        {
                            Width = double.NaN,
                            Height = double.NaN,
                            VerticalAlignment = VerticalAlignment.Stretch,
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            Source = new Uri(args[7]),
                            Volume = double.Parse(args[8]),
                            Stretch = (Stretch)int.Parse(args[9])
                        };
                        player.MediaEnded += (s, e2) => { player.Position = TimeSpan.Zero; };
                        Grid.Children.Add(player);
                        Text.Visibility = Visibility.Hidden;
                        break;
                    }
                case "html":
                    {
                        Cef.Initialize();
                        var browser = new ChromiumWebBrowser(args[7]);
                        browser.Dock = System.Windows.Forms.DockStyle.Fill;
                        var host = new WindowsFormsHost { Child = browser };
                        Grid.Children.Add(host);
                        Text.Visibility = Visibility.Hidden;
                        break;
                    }

            }
            ShowWindow(GetConsoleWindow(), 0);
            Console.WriteLine(new WindowInteropHelper(this).Handle.ToInt64().ToString());
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }
        private void Window_Closed(object sender, EventArgs e)
        {
            switch (Mode)
            {
                case "html":
                    Cef.Shutdown();
                    break;
            }
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    }
}
