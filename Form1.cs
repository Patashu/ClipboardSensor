using System.Diagnostics;
using System.DirectoryServices;
using System.Media;
using System.Resources;
using System.Runtime.InteropServices;
using ClipboardSensor.Properties;

namespace ClipboardSensor
{
    public partial class ClipboardSensorForm : Form
    {

        #region Hotkeys

        // https://stackoverflow.com/questions/3568513/how-to-create-keyboard-shortcut-in-windows-that-call-function-in-my-app/3569097#3569097

        public class HotKeyManager
        {
            public static event EventHandler<HotKeyEventArgs> HotKeyPressed;

            public static int RegisterHotKey(Keys key, KeyModifiers modifiers)
            {
                int id = System.Threading.Interlocked.Increment(ref _id);
                RegisterHotKey(_wnd.Handle, id, (uint)modifiers, (uint)key);
                return id;
            }

            public static bool UnregisterHotKey(int id)
            {
                return UnregisterHotKey(_wnd.Handle, id);
            }

            protected static void OnHotKeyPressed(HotKeyEventArgs e)
            {
                if (HotKeyManager.HotKeyPressed != null)
                {
                    HotKeyManager.HotKeyPressed(null, e);
                }
            }

            private static MessageWindow _wnd = new MessageWindow();

            private class MessageWindow : Form
            {
                protected override void WndProc(ref Message m)
                {
                    if (m.Msg == WM_HOTKEY)
                    {
                        HotKeyEventArgs e = new HotKeyEventArgs(m.LParam);
                        HotKeyManager.OnHotKeyPressed(e);
                    }

                    base.WndProc(ref m);
                }

                private const int WM_HOTKEY = 0x312;
            }

            [DllImport("user32")]
            private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

            [DllImport("user32")]
            private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

            private static int _id = 0;
        }

        public class HotKeyEventArgs : EventArgs
        {
            public readonly Keys Key;
            public readonly KeyModifiers Modifiers;

            public HotKeyEventArgs(Keys key, KeyModifiers modifiers)
            {
                this.Key = key;
                this.Modifiers = modifiers;
            }

            public HotKeyEventArgs(IntPtr hotKeyParam)
            {
                uint param = (uint)hotKeyParam.ToInt64();
                Key = (Keys)((param & 0xffff0000) >> 16);
                Modifiers = (KeyModifiers)(param & 0x0000ffff);
            }
        }

        [Flags]
        public enum KeyModifiers
        {
            Alt = 1,
            Control = 2,
            Shift = 4,
            Windows = 8,
            NoRepeat = 0x4000
        }

        #endregion

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

        System.Media.SoundPlayer switchwav = new System.Media.SoundPlayer();
        System.Media.SoundPlayer switch2wav = new System.Media.SoundPlayer();
        System.Media.SoundPlayer bumpwav = new System.Media.SoundPlayer();
        System.Media.SoundPlayer undowav = new System.Media.SoundPlayer();
        System.Media.SoundPlayer redowav = new System.Media.SoundPlayer();
        int debounceMs = 100; //in milliseconds
        Stopwatch time = new Stopwatch();
        long lastRead = -9999; //in ElapsedMilliseconds
        long lastWrite = -9999; //in ElapsedMilliseconds
        bool settingDataObject = false;
        List<IDataObject> history = new List<IDataObject>();
        int currentPosition = -1; //zero indexed
        int maxHistorySize = 32; //inclusive
        const int muteAfterWriteMs = 100; //in milliseconds
        List<int> registeredHotkeys = new List<int>();
        
        public ClipboardSensorForm()
        {
            this.Name = "ClipboardSensor";
            InitializeComponent();
            this.Disposed += OnDisposed;
            this.Shown += (o, e) =>
            {
                AddClipboardFormatListener(this.Handle);
                RegisterHotkeys();
                HotKeyManager.HotKeyPressed += OnHotKeyPressed;
            };
            ResourceManager rm = Resources.ResourceManager;
            //not going to fix the potential NREs here because if it does NRE it is correct to crash~
            switchwav.Stream = (Stream)rm.GetObject("switch");
            switch2wav.Stream = (Stream)rm.GetObject("switch2");
            bumpwav.Stream = (Stream)rm.GetObject("bump");
            undowav.Stream = (Stream)rm.GetObject("undo");
            redowav.Stream = (Stream)rm.GetObject("redo");
            DebounceNumericBox.Value = debounceMs;
            time.Start();
        }

        void RegisterHotkeys()
        {
            if (registeredHotkeys.Any())
            {
                return;
            }
            registeredHotkeys.Add(HotKeyManager.RegisterHotKey(Keys.Z, KeyModifiers.Alt));
            registeredHotkeys.Add(HotKeyManager.RegisterHotKey(Keys.X, KeyModifiers.Alt));
        }

        void UnregisterHotkeys()
        {
            foreach (var id in registeredHotkeys)
            {
                HotKeyManager.UnregisterHotKey(id);
            }
            registeredHotkeys.Clear();
        }

        void OnDisposed(object sender, EventArgs e)
        {
            UnregisterHotkeys();
            RemoveClipboardFormatListener(this.Handle);
        }

        const int WM_CLIPBOARDUPDATE = 0x031D;
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_CLIPBOARDUPDATE:
                    HandleClipboard();
                    break;
            }
            base.WndProc(ref m);
        }

        void OnHotKeyPressed(object sender, HotKeyEventArgs e)
        {
            if (e.Key == Keys.Z)
            {
                UndoButton_Click(null, null);
            }
            else if (e.Key == Keys.X)
            {
                RedoButton_Click(null, null);
            }
        }

        void ShrinkHistory()
        {
            //...and while history is greater than maxHistorySize...
            while (history.Count > maxHistorySize)
            {
                //remove entries from the longer side (tiebreaking to the left side)
                //example: curentPosition is 3, Count is 5
                //0 1 2 [3] 4
                //left side is 3 (currentPosition), right side is 1 (Count - 1 - currentPosition)
                //so we truncate one from the left side
                if (currentPosition >= (history.Count - 1 - currentPosition))
                {
                    history.RemoveAt(0);
                    ProgrammaticallySetCurrentPosition(currentPosition - 1);
                }
                else
                {
                    history.RemoveAt(history.Count - 1);
                }
            }
            ProgrammaticallySetMaximumPositionBox(history.Count);
        }

        void PlayIfNotMuted(SoundPlayer speaker, bool userInitiated = false)
        {
            //don't play non-user initiated sounds if we wrote within the last muteAfterWriteMs ms
            if (!userInitiated && time.ElapsedMilliseconds - lastWrite < muteAfterWriteMs)
            {
                return;
            }
            speaker.Play();
        }

        void UpdateCurrentTextBoxFromDataObject(IDataObject dataObject, bool playSound)
        {
            var result = "";
            var formats = dataObject.GetFormats();
            var containsText = false;
            if (formats.Any())
            {
                if (formats.Contains("Text"))
                {
                   result = (string)dataObject.GetData("Text");
                    containsText = true;
                }
                else
                {
                    result = formats.Aggregate((x, y) => x + ", " + y);
                }
            }

            if (playSound)
            {
                if (String.IsNullOrEmpty(result))
                {
                    PlayIfNotMuted(bumpwav);
                }
                else if (result == "Chromium internal source RFH token, Chromium internal source URL")
                {
                    PlayIfNotMuted(bumpwav);
                }
                else if (containsText)
                {
                    PlayIfNotMuted(switchwav);
                }
                else
                {
                    PlayIfNotMuted(switch2wav);
                }
            }
            CurrentTextBox.Text = result;
            lastRead = time.ElapsedMilliseconds;
        }

        DataObject Clone(IDataObject other)
        {
            var clone = new DataObject();
            foreach (var format in other.GetFormats())
            {
                clone.SetData(format, other.GetData(format));
            }
            return clone;
        }

        IDataObject? ClipboardGetDataObjectWrapper()
        {
            //has been seen to be flaky in practice, so try a few times
            IDataObject? result = null;
            for (var i = 0; i < 4; ++i)
            {
                try
                {
                    result = Clipboard.GetDataObject();
                    if (result != null && result.GetFormats().Any())
                    {
                        break;
                    }
                }
                catch (ExternalException)
                {
                }
                Thread.Sleep((int)Math.Pow(10, i-1)); //0, 1, 10, 100
            }
            return result;
        }

        bool ClipboardSetDataObjectWrapper(IDataObject data)
        {
            //has been seen to be flaky in practice, so try a few times
            try
            {
                //don't immediately think a new, external clipboard change came in
                settingDataObject = true;
                lastRead = time.ElapsedMilliseconds;
                lastWrite = time.ElapsedMilliseconds;
                for (var i = 0; i < 4; ++i)
                {
                    try
                    {
                        Clipboard.SetDataObject(data, true, 0, 0);
                        return true;
                    }
                    catch (ExternalException)
                    {
                    }
                    Thread.Sleep((int)Math.Pow(10, i-1)); //0, 1, 10, 100
                }
                //this still can fail even after multiple seconds of retrying every 0.1 seconds.
                //given it can have an arbitrarily unbound length of failure, I can't just make the user wait.
                //I'd rather beep than freeze up.
                //(I wonder if there is any better way to do this that I don't know of, though...)
                //(Like, how does winforms handle this?)
                return false;
            }
            finally
            {
                settingDataObject = false;
            }
        }

        void HandleClipboard()
        {
            //don't try to HandleClipboard automatically while we're doing undo/redo
            if (settingDataObject || (time.ElapsedMilliseconds - lastWrite < muteAfterWriteMs))
            {
                return;
            }

            var dataObject = ClipboardGetDataObjectWrapper();
            if (dataObject != null)
            {
                var clone = Clone(dataObject);
                //if not debounced...
                if (time.ElapsedMilliseconds - lastRead > debounceMs)
                {
                    //add to history
                    history.Insert(currentPosition + 1, clone);
                    ProgrammaticallySetCurrentPosition(currentPosition + 1);
                    ShrinkHistory();
                }
                else
                {
                    //else, rewrite current entry
                    history[currentPosition] = clone;
                }
                UpdateCurrentTextBoxFromDataObject(clone, true);
            }
        }

        void ProgrammaticallySetCurrentPosition(int value)
        {
            currentPosition = value;
            programmaticallySettingCurrentPositionBoxValue = true;
            CurrentPositionBox.Value = value + 1;
            programmaticallySettingCurrentPositionBoxValue = false;
        }

        void ProgrammaticallySetMaximumPositionBox(int value)
        {
            if (userFocusingMaximumPositionBox)
            {
                return;
            }
            MaximumPositionBox.Value = value;
        }

        void GotoCurrentPosition(int value)
        {
            if (value == currentPosition)
            {
                PlayIfNotMuted(bumpwav, true);
                return;
            }
            else if (value < 0)
            {
                PlayIfNotMuted(bumpwav, true);
                ProgrammaticallySetCurrentPosition(currentPosition);
                return;
            }
            else if (value >= history.Count)
            {
                PlayIfNotMuted(bumpwav, true);
                ProgrammaticallySetCurrentPosition(currentPosition);
                return;
            }
            else if (value < currentPosition)
            {
                PlayIfNotMuted(undowav, true);
            }
            else
            {
                PlayIfNotMuted(redowav, true);
            }

            if (ClipboardSetDataObjectWrapper(history[value]))
            {
                //hooray!
            }
            else
            {
                PlayIfNotMuted(bumpwav, true);
                return;
            }
            //now we know it succeeded so continue
            ProgrammaticallySetCurrentPosition(value);
            //crazy COM stack overflow if I just immediately try to read it for some reason, so we'll manually unpack it here
            UpdateCurrentTextBoxFromDataObject(history[currentPosition], false);
        }

        bool programmaticallySettingCurrentPositionBoxValue = false;
        private void CurrentPositionBox_ValueChanged(object sender, EventArgs e)
        {
            if (!programmaticallySettingCurrentPositionBoxValue)
            {
                GotoCurrentPosition((int)CurrentPositionBox.Value - 1);
            }
        }

        private void MaximumPositionBox_ValueChanged(object sender, EventArgs e)
        {
            if (userFocusingMaximumPositionBox)
            {
                maxHistorySize = (int)MaximumPositionBox.Value;
                ShrinkHistory();
            }
        }

        bool userFocusingMaximumPositionBox = false;

        private void MaximumPositionBox_GotFocus(object sender, EventArgs e)
        {
            ProgrammaticallySetMaximumPositionBox(maxHistorySize);
            userFocusingMaximumPositionBox = true;
        }

        private void MaximumPositionBox_LostFocus(object sender, EventArgs e)
        {
            _ = MaximumPositionBox.Value; //forces a parse which makes it realize the value changed
            userFocusingMaximumPositionBox = false; //NOW we can say we're not looking at it anymore
            ProgrammaticallySetMaximumPositionBox(history.Count);
        }

        private void UndoButton_Click(object sender, EventArgs e)
        {
            GotoCurrentPosition(currentPosition - 1);
        }

        private void RedoButton_Click(object sender, EventArgs e)
        {
            GotoCurrentPosition(currentPosition + 1);
        }

        private void DebounceNumericBox_ValueChanged(object sender, EventArgs e)
        {
            debounceMs = (int)DebounceNumericBox.Value;
        }
    }
}
