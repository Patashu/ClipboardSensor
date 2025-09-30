using System.Diagnostics;
using System.Media;
using System.Resources;
using System.Runtime.InteropServices;
using ClipboardSensor.Properties;

namespace ClipboardSensor
{
    public partial class ClipboardSensorForm : Form
    {
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SetClipboardViewer(IntPtr hWndNewViewer);

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
        
        public ClipboardSensorForm()
        {
            this.Name = "ClipboardSensor";
            InitializeComponent();
            this.Shown += (o, e) =>
            {
                var _ClipboardViewerNext = SetClipboardViewer(this.Handle);
            };
            ResourceManager rm = Resources.ResourceManager;
            //not going to fix the potential NREs here because if it does NRE it is correct to crash~
            switchwav.Stream = (Stream)rm.GetObject("switch");
            switch2wav.Stream = (Stream)rm.GetObject("switch2");
            bumpwav.Stream = (Stream)rm.GetObject("bump");
            undowav.Stream = (Stream)rm.GetObject("undo");
            redowav.Stream = (Stream)rm.GetObject("redo");
            DebounceNumericBox.Value = debounceMs;
            ProgrammaticallySetMaximumPosition(maxHistorySize);
            time.Start();
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 0x0308: //WM_DRAWCLIPBOARD
                    HandleClipboard();
                    break;
            }
            base.WndProc(ref m);
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

        void HandleClipboardDueToUndoOrRedo(IDataObject dataObject)
        {
            var formats = dataObject.GetFormats();
            if (formats.Any())
            {
                //TODO: text
                if (formats.Contains("Text"))
                {
                    CurrentTextBox.Text = (string)dataObject.GetData("Text");
                }
                else
                {
                    CurrentTextBox.Text = formats.Aggregate((x, y) => x + ", " + y);
                }
            }

            lastRead = time.ElapsedMilliseconds;
        }

        DataObject Clone(IDataObject other)
        {
            var clone = new DataObject();
            foreach (var format in other.GetFormats())
            {
                clone.SetData(format, other.GetData(format));
                //do I also have to do text or is that a kind of 'data'?
            }
            return clone;
        }

        void HandleClipboard()
        {
            //don't try to HandleClipboard automatically while we're doing undo/redo
            if (settingDataObject || (time.ElapsedMilliseconds - lastWrite < muteAfterWriteMs))
            {
                return;
            }

            var dataObject = Clipboard.GetDataObject();
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
            }

            CurrentTextBox.Text = "";
            var pureText = false;
            if (Clipboard.ContainsText())
            {
                CurrentTextBox.Text = Clipboard.GetText();
                pureText = true;

            }
            else
            {
                var formats = Clipboard.GetDataObject()?.GetFormats() ?? Array.Empty<string>();
                if (!formats.Any())
                {
                    PlayIfNotMuted(bumpwav);
                }
                else
                {
                    CurrentTextBox.Text = formats.Aggregate((x, y) => x + ", " + y);
                }
            }

            if (CurrentTextBox.Text.Length > 0)
            {
                if (CurrentTextBox.Text == "Chromium internal source RFH token, Chromium internal source URL")
                {
                    PlayIfNotMuted(bumpwav);
                }
                else if (pureText)
                {
                    PlayIfNotMuted(switchwav);
                }
                else
                {
                    PlayIfNotMuted(switch2wav);
                }
            }
            else
            {
                PlayIfNotMuted(bumpwav);
            }

            lastRead = time.ElapsedMilliseconds;
        }

        void ProgrammaticallySetCurrentPosition(int value)
        {
            currentPosition = value;
            programmaticallySettingCurrentPositionBoxValue = true;
            CurrentPositionBox.Value = value + 1;
            programmaticallySettingCurrentPositionBoxValue = false;
        }

        void ProgrammaticallySetMaximumPosition(int value)
        {
            programmaticallySettingMaximumPositionBoxValue = true;
            MaximumPositionBox.Value = value;
            programmaticallySettingMaximumPositionBoxValue = false;
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
            //have to do this right now in case it succeeds
            lastRead = time.ElapsedMilliseconds;
            lastWrite = time.ElapsedMilliseconds;
            //some arbitrary retry amounts...
            //throws ExternalException if it fails. if it fails I guess we just need to tell the user that.
            try
            {
                settingDataObject = true;
                Clipboard.SetDataObject(history[value]);
            }
            catch (ExternalException)
            {
                PlayIfNotMuted(bumpwav, true);
                return;
            }
            finally
            {
                settingDataObject = false;
            }
            //now we know it succeeded so continue
            ProgrammaticallySetCurrentPosition(value);
            //crazy COM stack overflow if I just immediately try to read it for some reason, so we'll manually unpack it here
            HandleClipboardDueToUndoOrRedo(history[currentPosition]);
        }

        bool programmaticallySettingCurrentPositionBoxValue = false;
        private void CurrentPositionBox_ValueChanged(object sender, EventArgs e)
        {
            if (!programmaticallySettingCurrentPositionBoxValue)
            {
                GotoCurrentPosition((int)CurrentPositionBox.Value - 1);
            }
        }

        bool programmaticallySettingMaximumPositionBoxValue = false;
        private void MaximumPositionBox_ValueChanged(object sender, EventArgs e)
        {
            if (!programmaticallySettingMaximumPositionBoxValue)
            {
                //TODO
            }
        }

        bool userFocusingMaximumPositionBox = false;

        private void MaximumPositionBox_GotFocus(object sender, EventArgs e)
        {
            userFocusingMaximumPositionBox = true;
            //TODO
        }

        private void MaximumPositionBox_LostFocus(object sender, EventArgs e)
        {
            userFocusingMaximumPositionBox = false;
            //TODO
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
