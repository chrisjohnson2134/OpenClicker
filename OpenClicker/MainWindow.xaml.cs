using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OpenClicker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    ///https://frasergreenroyd.com/c-global-keyboard-listeners-implementation-of-key-hooks/
    ///https://stackoverflow.com/a/11608209/6228010
    ///https://www.iditect.com/program-example/c--how-to-detect-mouse-clicks.html#:~:text=To%20detect%20mouse%20clicks%20in%20a%20C%23%20Windows,you%20would%20need%20to%20use%20global%20mouse%20hooks.
    ///https://stackoverflow.com/questions/1316681/getting-mouse-position-in-c-sharp
    /// https://www.iditect.com/faq/csharp/how-to-simulate-a-mouse-click-at-a-certain-position-on-the-screen-in-c.html#:~:text=First%2C%20we%20set%20the%20x%20and%20y%20variables,event%20%280x04%29%20at%20the%20given%20position%20using%20mouse_event.
    public partial class MainWindow : Window
    {
        Win32.POINT _myPosition;
        int _clickIndex;
        bool _globalKeyPressed;
        bool _repeatClick;
        bool _recording;
        HotKey _globalHotkey;

        public MainWindow()
        {
            InitializeComponent();

            _globalHotkey = new HotKey(Key.F7, KeyModifier.None, StopPlaybackHandler);
            _globalHotkey = new HotKey(Key.F1, KeyModifier.None, StartRecordingHandler);
            _globalHotkey = new HotKey(Key.F2, KeyModifier.None, StopRecordingHandler);
            _globalHotkey = new HotKey(Key.F6, KeyModifier.None, RepeatHandler);
        }



        //HotKeys Subscriptions
        private void StartRecordingHandler(HotKey key)
        {
            StartRecording();
        }

        private void StopRecordingHandler(HotKey key)
        {
            StopRecording();
        }

        private void StopPlaybackHandler(HotKey key)
        {
            _globalKeyPressed = true;
        }

        private void RepeatHandler(HotKey key)
        {
            RepeatClick();
        }

        //Click Buttons
        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            StartRecording();
        }

        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            StopRecording();
        }

        private void PlayBtn_Click(object sender, RoutedEventArgs e)
        {
            _globalKeyPressed = false;

            int loopCounter = 0;
            bool loop = LoopChckBx.IsChecked.Value;
            int delayTime = 100;
            Int32.TryParse(DelayTB.Text, out delayTime);
            if (delayTime <= 0)
                delayTime = 100;

            Task.Run(async () =>
            {
                while ((loop || loopCounter == 0) && !_globalKeyPressed)
                {
                    foreach (var item in MyPoints.Items)
                    {
                        if (item is IndexPoint point)
                        {
                            Win32.SetCursorPos(point.X, point.Y);
                            if (point.HoldTimeNumber == 0)
                                Win32.mouse_event(Win32.MOUSEEVENTF_LEFTDOWN | Win32.MOUSEEVENTF_LEFTUP,
                                    point.X, point.Y, 0, 0);
                            else
                            {
                                Win32.mouse_event(Win32.MOUSEEVENTF_LEFTDOWN,
                                    point.X, point.Y, 0, 0);
                                await Task.Delay(point.HoldTimeNumber);
                                Win32.mouse_event(Win32.MOUSEEVENTF_LEFTUP,
                                    point.X, point.Y, 0, 0);
                            }

                        }

                        await Task.Delay(delayTime);
                    }
                    loopCounter++;
                }
            });

            _globalKeyPressed = false;
        }

        private void ResetBtn_Click(object sender, RoutedEventArgs e)
        {
            PointLabel.Content = "Ready";
            MouseHook.stop();
            MouseHook.MouseAction -= MouseHook_MouseAction;

            MyPoints.Items.Clear();
            _clickIndex = 0;
        }

        //On Application Close Events
        protected override void OnClosed(EventArgs e)
        {
            _globalHotkey.Dispose();
            base.OnClosed(e);
        }


        //Mouse Subscription Event
        private void MouseHook_MouseAction(object? sender, EventArgs e)
        {
            Win32.GetCursorPos(out _myPosition);
            //PointLabel.Content = "Last Click = " + _myPosition.x + " " + _myPosition.y;
            MyPoints.Items.Add(new IndexPoint(_clickIndex, _myPosition.X, _myPosition.Y));
            _clickIndex++;
        }

        //Abstracted Events
        private void StartRecording()
        {
            MouseHook.Start();
            MouseHook.MouseAction += MouseHook_MouseAction;

            _recording = true;
            _clickIndex = 0;
            PointLabel.Content = "Recording Started.";
        }

        private void StopRecording()
        {
            MouseHook.stop();
            MouseHook.MouseAction -= MouseHook_MouseAction;

            if (MyPoints.Items.Count > 0 && _recording)
            {
                MyPoints.Items.RemoveAt(_clickIndex - 1);
                _clickIndex--;
            }

            _recording = false;
            PointLabel.Content = "Recording Stopped.";
        }

        private void AutoClickBtn_Click(object sender, RoutedEventArgs e)
        {
            RepeatClick();
        }

        private void RepeatClick()
        {
            int delayTime = 100;
            Int32.TryParse(DelayTB.Text, out delayTime);
            if (delayTime <= 0)
                delayTime = 100;

            _repeatClick = !_repeatClick;

            Task.Run(async () =>
            {
                while (!_repeatClick)
                {
                    Win32.GetCursorPos(out _myPosition);
                    Win32.mouse_event(Win32.MOUSEEVENTF_LEFTDOWN | Win32.MOUSEEVENTF_LEFTUP,
                                            _myPosition.X, _myPosition.Y, 0, 0);
                    await Task.Delay(delayTime);
                }
            });
        }
    }

    class IndexPoint
    {
        public int _holdTime = 0;

        public IndexPoint(int index, int x, int y)
        {
            Index = index;
            X = x;
            Y = y;
        }

        public int Index { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int HoldTimeNumber => _holdTime;
        public string HoldTime
        {
            get => _holdTime.ToString();
            set
            {
                if (!Int32.TryParse(value, out _holdTime))
                    _holdTime = 100;
            }
        }
    }

}