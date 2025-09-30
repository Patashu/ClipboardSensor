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

        void HandleClipboard()
        {
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
                    bumpwav.Play();
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
                    bumpwav.Play();
                }
                else if (pureText)
                {
                    switchwav.Play();
                }
                else
                {
                    switch2wav.Play();
                }
            }
            else
            {
                bumpwav.Play();
            }
        }

        private void CurrentPositionBox_ValueChanged(object sender, EventArgs e)
        {

        }

        private void MaximumPositionBox_ValueChanged(object sender, EventArgs e)
        {

        }

        private void MaximumPositionBox_GotFocus(object sender, EventArgs e)
        {

        }

        private void MaximumPositionBox_LostFocus(object sender, EventArgs e)
        {

        }

        private void UndoButton_Click(object sender, EventArgs e)
        {

        }

        private void RedoButton_Click(object sender, EventArgs e)
        {

        }

        private void DebounceNumericBox_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}
