using System.Resources;
using System.Runtime.InteropServices;

namespace ClipboardSensor
{
    public partial class Form1 : Form
    {
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SetClipboardViewer(IntPtr hWndNewViewer);

        System.Media.SoundPlayer switchwav = new System.Media.SoundPlayer("E:\\Users\\tim\\Documents\\Godot Projects\\ClipboardSensor\\switch.wav");
        System.Media.SoundPlayer switch2wav = new System.Media.SoundPlayer("E:\\Users\\tim\\Documents\\Godot Projects\\ClipboardSensor\\switch2.wav");
        System.Media.SoundPlayer bumpwav = new System.Media.SoundPlayer("E:\\Users\\tim\\Documents\\Godot Projects\\ClipboardSensor\\bump.wav");

        public Form1()
        {
            this.Name = "ClipboardSensor";
            InitializeComponent();
            this.Shown += (o, e) =>
            {
                var _ClipboardViewerNext = SetClipboardViewer(this.Handle);
            };
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
            textBox1.Text = "";
            var pureText = false;
            if (Clipboard.ContainsText())
            {
                textBox1.Text = Clipboard.GetText();
                pureText = true;

            }
            else
            {
                textBox1.Text = Clipboard.GetDataObject().GetFormats().Aggregate((x, y) => x + ", " + y);
            }

            if (textBox1.Text.Length > 0)
            {
                if (textBox1.Text == "Chromium internal source RFH token, Chromium internal source URL")
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
    }
}
