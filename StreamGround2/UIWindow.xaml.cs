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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;

namespace StreamGround2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static Brush buttonColor1 = new SolidColorBrush(Color.FromArgb(0xFF, 0x04, 0x04, 0x04));
        static Brush buttonColor2 = new SolidColorBrush(Color.FromArgb(0xFF, 0x0E, 0x0E, 0x0E));
        static Brush buttonColor3 = new SolidColorBrush(Color.FromArgb(0xFF, 0x11, 0x00, 0x00));
        static Brush buttonColor4 = new SolidColorBrush(Color.FromArgb(0xFF, 0x38, 0x00, 0x00));

        Screen[] Screens = Screen.AllScreens;
        List<Area> Areas;
        NotifyIcon NotifyIcon = new NotifyIcon();
        OpenFileDialog MediaFileDialog = new OpenFileDialog();
        OpenFileDialog HtmlFileDialog = new OpenFileDialog();
        public static IntPtr Workerw;

        double SubdivideAmount = 2;

        int CurrentAreaIndex
        {
            get => AreaSelectBox.SelectedIndex;
            set => AreaSelectBox.SelectedIndex = value;
        }
        Area CurrentArea
        {
            get => Areas[CurrentAreaIndex];
            set => Areas[CurrentAreaIndex] = value;
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            NotifyIcon.Icon = new System.Drawing.Icon(@"F:\Documents\code\C#\StreamGround2\StreamGround2\right-double-chevron.ico");
            NotifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu();
            NotifyIcon.Text = "StreamGround";
            NotifyIcon.ContextMenu.MenuItems.Add("Quit");
            NotifyIcon.ContextMenu.MenuItems.Add("Show");
            NotifyIcon.ContextMenu.MenuItems[0].Click += (s, ee) => { Close(); };
            NotifyIcon.ContextMenu.MenuItems[1].Click += (s, ee) => { Show(); };
            NotifyIcon.DoubleClick += (s, ee) => Show();
            NotifyIcon.Visible = true;

            ResetAreas();

            W32.SendMessageTimeout(W32.FindWindow("Progman", null),
                       0x052C,
                       new IntPtr(0),
                       IntPtr.Zero,
                       W32.SendMessageTimeoutFlags.SMTO_NORMAL,
                       1000,
                       out IntPtr result);

            Workerw = IntPtr.Zero;

            // We enumerate all Windows, until we find one, that has the SHELLDLL_DefView 
            // as a child. 
            // If we found that window, we take its next sibling and assign it to workerw.
            W32.EnumWindows(new W32.EnumWindowsProc((tophandle, topparamhandle) =>
            {
                IntPtr p = W32.FindWindowEx(tophandle,
                                            IntPtr.Zero,
                                            "SHELLDLL_DefView",
                                            IntPtr.Zero);

                if (p != IntPtr.Zero)
                {
                    // Gets the WorkerW Window after the current one.
                    Workerw = W32.FindWindowEx(IntPtr.Zero,
                                               tophandle,
                                               "WorkerW",
                                               IntPtr.Zero);
                }

                return true;
            }), IntPtr.Zero);

            SelectBox.SelectedIndex = 0;
        }

        void ResetAreas()
        {
            Areas = new List<Area>(Screens.Length);
            for (int i = 0; i < Screens.Length; i++)
            {
                Areas.Add(new Area(Screens[i].Bounds.X, Screens[i].Bounds.Y, Screens[i].Bounds.Width, Screens[i].Bounds.Height, "Display " + (i + 1).ToString() + ", " + Screens[i].Bounds.Width + " x " + Screens[i].Bounds.Height));
            }
            UpdateAreaSelectBox();
            CurrentAreaIndex = 0;
        }

        void UpdateAreaSelectBox()
        {
            AreaSelectBox.Items.Clear();
            for (int i = 0; i < Areas.Count; i++)
            {
                AreaSelectBox.Items.Add(Areas[i].Name);
            }
        }

        private void DragBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void AreaSelectBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CurrentAreaIndex == -1) return;
            AreaNameBox.Text = CurrentArea.Name;
            DimensionXBox.Text = CurrentArea.X.ToString();
            DimensionYBox.Text = CurrentArea.Y.ToString();
            DimensionWidthBox.Text = CurrentArea.Width.ToString();
            DimensionHeightBox.Text = CurrentArea.Height.ToString();
        }

        private void _Button_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _Button.Background = buttonColor2;
        }

        private void _Button_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _Button.Background = buttonColor1;
        }

        private void XButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            XButton.Background = buttonColor4;
        }

        private void XButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            XButton.Background = buttonColor3;
        }

        private void XButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            for (int i = 0; i < Areas.Count; i++)
            {
                Areas[i].Stop(false);
            }
            Area.Clear();
            NotifyIcon.Visible = false;
        }

        private void _Button_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) Hide();
        }

        private void SelectBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NoneTextBlock.Visibility = Visibility.Hidden;
            DeleteTextBlock.Visibility = Visibility.Hidden;
            MediaGrid.Visibility = Visibility.Hidden;
            InternetGrid.Visibility = Visibility.Hidden;
            SubdivideGrid.Visibility = Visibility.Hidden;
            MergeGrid.Visibility = Visibility.Hidden;
            DimensionsGrid.Visibility = Visibility.Hidden;
            switch (SelectBox.SelectedIndex)
            {
                case 0: NoneTextBlock.Visibility = Visibility.Visible; break;
                case 1: MediaGrid.Visibility = Visibility.Visible; break;
                case 2: InternetGrid.Visibility = Visibility.Visible; break;
                case 3: SubdivideGrid.Visibility = Visibility.Visible; break;
                case 4: MergeGrid.Visibility = Visibility.Visible; break;
                case 5: DimensionsGrid.Visibility = Visibility.Visible; break;
                case 6: DeleteTextBlock.Visibility = Visibility.Visible; break;
            }
        }

        private void SelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            MediaFileDialog.ShowDialog();
            
        }

        private void MediaVolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MediaVolumeTextBlock.Text = (int)MediaVolumeSlider.Value + "%";
        }

        private void ApplyButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ApplyButton.Focus();
            if (e.ChangedButton != MouseButton.Left) return;
            CurrentArea.Stop(true);
            switch (SelectBox.SelectedIndex)
            {
                case 1:
                    if(MediaFileDialog.FileName != string.Empty) CurrentArea.Start($"media \"{MediaFileDialog.FileName}\" {MediaVolumeSlider.Value / 100} {MediaStretchComboBox.SelectedIndex}");
                    break;
                case 2:
                    if (InternetAddressBox.Text != string.Empty) CurrentArea.Start("html \"" + InternetAddressBox.Text + "\"");
                    break;
                case 3:
                    {
                        DivideAmountBox_LostFocus(null, null);
                        if (SingleDivideButton.IsChecked.Value || (int)SubdivideAmount == 2)
                        {

                            if (HorizontalDivideRadioButton.IsChecked.Value)
                            {
                                int a = CurrentArea.Height;
                                CurrentArea.Height = (int)(a / SubdivideAmount);
                                Areas.Add(new Area(CurrentArea.X, CurrentArea.Y + CurrentArea.Height, CurrentArea.Width, a - CurrentArea.Height, CurrentArea.Name + ", Bottom"));
                                CurrentArea.Name += ", Top";
                            }
                            else if (VerticalDivideRadioButton.IsChecked.Value)
                            {
                                int a = CurrentArea.Width;
                                CurrentArea.Width = (int)(a / SubdivideAmount);
                                Areas.Add(new Area(CurrentArea.X + CurrentArea.Width, CurrentArea.Y, a - CurrentArea.Width, CurrentArea.Height, CurrentArea.Name + ", Right"));
                                CurrentArea.Name += ", Left";
                            }
                            else
                            {
                                int ow = CurrentArea.Width;
                                int oh = CurrentArea.Height;
                                CurrentArea.Width = (int)(ow / SubdivideAmount);
                                CurrentArea.Height = (int)(oh / SubdivideAmount);
                                Areas.Add(new Area(CurrentArea.X + CurrentArea.Width, CurrentArea.Y, ow - CurrentArea.Width, CurrentArea.Height, CurrentArea.Name + ", Part 2"));
                                Areas.Add(new Area(CurrentArea.X, CurrentArea.Y + CurrentArea.Height, CurrentArea.Width, oh - CurrentArea.Height, CurrentArea.Name + ", Part 3"));
                                Areas.Add(new Area(CurrentArea.X + CurrentArea.Width, CurrentArea.Y + CurrentArea.Height, ow - CurrentArea.Width, oh - CurrentArea.Height, CurrentArea.Name + ", Part 4"));
                                CurrentArea.Name += ", Part 1";
                            }
                        }
                        else
                        {
                            if (HorizontalDivideRadioButton.IsChecked.Value)
                            {
                                int divs = (int)SubdivideAmount;
                                CurrentArea.Height /= divs;
                                int a = CurrentArea.Y;
                                for (int i = 1; i < divs; i++)
                                {
                                    a += CurrentArea.Height;
                                    Areas.Add(new Area(CurrentArea.X, a, CurrentArea.Width, CurrentArea.Height, CurrentArea.Name + ", Part " + (i + 1).ToString()));
                                }
                                CurrentArea.Name += ", Part 1";
                            }
                            else if (VerticalDivideRadioButton.IsChecked.Value)
                            {
                                int divs = (int)SubdivideAmount;
                                CurrentArea.Width /= divs;
                                int a = CurrentArea.X;
                                for (int i = 1; i < divs; i++)
                                {
                                    a += CurrentArea.Width;
                                    Areas.Add(new Area(a, CurrentArea.Y, CurrentArea.Width, CurrentArea.Height, CurrentArea.Name + ", Part " + (i + 1).ToString()));
                                }
                                CurrentArea.Name += ", Part 1";
                            }
                            else
                            {
                                int divs = (int)SubdivideAmount;
                                int width = CurrentArea.Width / divs;
                                int heigth = CurrentArea.Height / divs;
                                int originalx = CurrentArea.X;
                                int x = originalx;
                                int y = CurrentArea.Y;
                                string name = CurrentArea.Name;
                                Areas.RemoveAt(CurrentAreaIndex);
                                for (int i1 = 0; i1 < divs; i1++)
                                {
                                    for (int i2 = 0; i2 < divs; i2++)
                                    {
                                        Areas.Add(new Area(x, y, width, heigth, name + ", Row " + (i1 + 1).ToString() + ", Column " + (i2 + 1).ToString()));
                                        x += width;
                                    }
                                    x = originalx;
                                    y += heigth;
                                }
                                CurrentAreaIndex += 1;
                            }
                        }
                        int fooi = CurrentAreaIndex;
                        UpdateAreaSelectBox();
                        CurrentAreaIndex = fooi;
                        break;
                    }
                case 4:
                    {
                        bool foo = false;
                        if (RightMergeBox.IsChecked.Value)
                        {
                            foo = true;
                            int y = CurrentArea.Y;
                            int w = CurrentArea.Width;
                            int h = CurrentArea.Height;
                            int maxy = CurrentArea.Y + CurrentArea.Height;
                            for (int i = 0; i < Areas.Count; i++)
                            {
                                if (i == CurrentAreaIndex) continue;
                                int maxy2 = Areas[i].Y + Areas[i].Height;
                                if(CurrentArea.Y < maxy2 || maxy < Areas[i].Y)
                                {
                                    y = Math.Min(CurrentArea.Y, Areas[i].Y);
                                    w = Math.Max(w, CurrentArea.Width + Areas[i].Width);
                                    h = Math.Max(maxy, maxy2) - y;
                                }
                                CurrentArea.Name += " & " + Areas[i].Name;
                                if (CurrentAreaIndex > i) CurrentAreaIndex--;
                                Areas.RemoveAt(i);
                            }
                            CurrentArea.Y = y;
                            CurrentArea.Width = w;
                            CurrentArea.Height = h;
                        }
                        if (BottomMergeBox.IsChecked.Value)
                        {
                            foo = true;
                            int x = CurrentArea.X;
                            int w = CurrentArea.Width;
                            int h = CurrentArea.Height;
                            int maxx = CurrentArea.X + CurrentArea.Width;
                            for (int i = 0; i < Areas.Count; i++)
                            {
                                if (i == CurrentAreaIndex) continue;
                                int maxx2 = Areas[i].X + Areas[i].Width;
                                if (CurrentArea.X < maxx2 || maxx < Areas[i].X)
                                {
                                    x = Math.Min(CurrentArea.X, Areas[i].X);
                                    h = Math.Max(w, CurrentArea.Height + Areas[i].Height);
                                    w = Math.Max(maxx, maxx2) - x;
                                }
                                if (CurrentAreaIndex > i) CurrentAreaIndex--;
                                Areas.RemoveAt(i);
                            }
                            CurrentArea.X = x;
                            CurrentArea.Width = w;
                            CurrentArea.Height = h;
                        }
                        if (foo)
                        {
                            CurrentArea.Name = "Merged, " + CurrentArea.Width + " x " + CurrentArea.Height;
                            int fooi = CurrentAreaIndex;
                            UpdateAreaSelectBox();
                            CurrentAreaIndex = fooi;
                        }
                        break;
                    }
                case 5:
                    UpdateDimensionBoxes(true);
                    CurrentArea.Name = "Custom, " + CurrentArea.Width + " x " + CurrentArea.Height;
                    UpdateCurrentAreaName();
                    break;
                case 6:
                    if (Areas.Count == 1) ResetAreas();
                    else
                    {
                        Areas.RemoveAt(CurrentAreaIndex);
                        UpdateAreaSelectBox();
                        CurrentAreaIndex = 0;
                    }
                    break;
            }
        }

        private void ApplyButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ApplyButton.Background = buttonColor1;
        }

        private void ApplyButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ApplyButton.Background = buttonColor2;
        }

        private void DivideAmountBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(DivideAmountBox.Text, out var x) && x > 1d) SubdivideAmount = x;
            else DivideAmountBox.Text = SubdivideAmount.ToString();
        }

        private void UpdateDimensionBoxes(object sender, RoutedEventArgs e)
        {
            UpdateDimensionBoxes(false);
        }
        private void UpdateDimensionBoxes(bool apply)
        {
            if (!int.TryParse(DimensionXBox.Text, out int x)) DimensionXBox.Text = CurrentArea.X.ToString();
            else if (apply) CurrentArea.X = x;
            if (!int.TryParse(DimensionYBox.Text, out int y)) DimensionYBox.Text = CurrentArea.Y.ToString();
            else if (apply) CurrentArea.Y = y;
            if (!int.TryParse(DimensionWidthBox.Text, out int w)) DimensionWidthBox.Text = CurrentArea.Width.ToString();
            else if (apply) CurrentArea.Width = w;
            if (!int.TryParse(DimensionHeightBox.Text, out int h)) DimensionHeightBox.Text = CurrentArea.Height.ToString();
            else if (apply) CurrentArea.Height = h;
        }

        private void AreaSelectBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(e.ChangedButton == MouseButton.Left)
            {
                AreaSelectBox.Visibility = Visibility.Hidden;
                AreaRenameBox.Text = CurrentArea.Name;
                AreaRenameBox.Visibility = Visibility.Visible;
                AreaRenameBox.Focus();
            }
        }

        void RenameArea()
        {
            CurrentArea.Name = AreaRenameBox.Text;
            UpdateCurrentAreaName();
            AreaRenameBox.Visibility = Visibility.Hidden;
            AreaSelectBox.Visibility = Visibility.Visible;
        }

        private void AreaRenameBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter) RenameArea();
        }

        private void AreaRenameBox_LostFocus(object sender, RoutedEventArgs e)
        {
            RenameArea();
        }

        void UpdateCurrentAreaName()
        {
            int fooi = CurrentAreaIndex;
            AreaSelectBox.Items[fooi] = CurrentArea.Name;
            CurrentAreaIndex = fooi;
        }

        private void HtmlSelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (HtmlFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) InternetAddressBox.Text = HtmlFileDialog.FileName;
        }
    }

    public class Area
    {
        public Process Process;
        public int X, Y, Width, Height;
        public string Name;

        public string WallpaperKey { get; private set; }

        public Area(int x, int y, int width, int height, string name)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Name = name;
        }

        public void Start(string args)
        {
            Process = new Process
            { StartInfo = new ProcessStartInfo(@"F:\Documents\code\C#\StreamGround2\Renderer\bin\x64\Release\Renderer.exe", $"{X} {Y} {Width} {Height} {MainWindow.Workerw.ToInt64()} {args}")
            { RedirectStandardOutput = true, UseShellExecute = false } };
            Process.Start();
        }
        public void Stop(bool clear)
        {
            if (Process != null)
            {
                string s = Process.StandardOutput.ReadLine();
                IntPtr window = new IntPtr(long.Parse(s));
                SendMessage(window, 0x0010, IntPtr.Zero, IntPtr.Zero);
                Process.WaitForExit();
                Process = null;
                if (clear) Clear();
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        public static void Clear()
        {
            keybd_event(0x5b, 0, 0, 0);
            keybd_event(0x09, 0, 0, 0);
            keybd_event(0x09, 0, 2, 0);
            keybd_event(0x09, 0, 0, 0);
            keybd_event(0x09, 0, 2, 0);
            keybd_event(0x5b, 0, 2, 0);
        }

        [DllImport("user32.dll")]
        static extern int keybd_event(Byte bVk, Byte bScan, long dwFlags, long dwExtraInfo);
    }
}
