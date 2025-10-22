namespace ClipboardSensor
{
    partial class ClipboardSensorForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ClipboardSensorForm));
            CurrentTextBox = new TextBox();
            UndoButton = new Button();
            CurrentPositionBox = new NumericUpDown();
            label1 = new Label();
            MaximumPositionBox = new NumericUpDown();
            RedoButton = new Button();
            label2 = new Label();
            DebounceNumericBox = new NumericUpDown();
            label3 = new Label();
            TrimTextButton = new Button();
            HotkeysCheckBox = new CheckBox();
            ((System.ComponentModel.ISupportInitialize)CurrentPositionBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)MaximumPositionBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)DebounceNumericBox).BeginInit();
            SuspendLayout();
            // 
            // CurrentTextBox
            // 
            CurrentTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            CurrentTextBox.Location = new Point(0, 60);
            CurrentTextBox.MaxLength = 65535;
            CurrentTextBox.Multiline = true;
            CurrentTextBox.Name = "CurrentTextBox";
            CurrentTextBox.ScrollBars = ScrollBars.Both;
            CurrentTextBox.Size = new Size(445, 68);
            CurrentTextBox.TabIndex = 0;
            CurrentTextBox.WordWrap = false;
            // 
            // UndoButton
            // 
            UndoButton.Location = new Point(116, 2);
            UndoButton.Name = "UndoButton";
            UndoButton.Size = new Size(86, 23);
            UndoButton.TabIndex = 1;
            UndoButton.Text = "Undo (Alt+Z)";
            UndoButton.UseVisualStyleBackColor = true;
            UndoButton.Click += UndoButton_Click;
            // 
            // CurrentPositionBox
            // 
            CurrentPositionBox.Location = new Point(2, 2);
            CurrentPositionBox.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            CurrentPositionBox.Name = "CurrentPositionBox";
            CurrentPositionBox.Size = new Size(44, 23);
            CurrentPositionBox.TabIndex = 2;
            CurrentPositionBox.ValueChanged += CurrentPositionBox_ValueChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(50, 4);
            label1.Name = "label1";
            label1.Size = new Size(12, 15);
            label1.TabIndex = 3;
            label1.Text = "/";
            // 
            // MaximumPositionBox
            // 
            MaximumPositionBox.Location = new Point(68, 2);
            MaximumPositionBox.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            MaximumPositionBox.Name = "MaximumPositionBox";
            MaximumPositionBox.Size = new Size(44, 23);
            MaximumPositionBox.TabIndex = 4;
            MaximumPositionBox.ValueChanged += MaximumPositionBox_ValueChanged;
            MaximumPositionBox.GotFocus += MaximumPositionBox_GotFocus;
            MaximumPositionBox.LostFocus += MaximumPositionBox_LostFocus;
            // 
            // RedoButton
            // 
            RedoButton.Location = new Point(208, 2);
            RedoButton.Name = "RedoButton";
            RedoButton.Size = new Size(86, 23);
            RedoButton.TabIndex = 5;
            RedoButton.Text = "Redo (Alt+X)";
            RedoButton.UseVisualStyleBackColor = true;
            RedoButton.Click += RedoButton_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(300, 6);
            label2.Name = "label2";
            label2.Size = new Size(64, 15);
            label2.TabIndex = 6;
            label2.Text = "Debounce:";
            // 
            // DebounceNumericBox
            // 
            DebounceNumericBox.Location = new Point(370, 4);
            DebounceNumericBox.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            DebounceNumericBox.Minimum = new decimal(new int[] { 1, 0, 0, int.MinValue });
            DebounceNumericBox.Name = "DebounceNumericBox";
            DebounceNumericBox.Size = new Size(45, 23);
            DebounceNumericBox.TabIndex = 7;
            DebounceNumericBox.Value = new decimal(new int[] { 100, 0, 0, 0 });
            DebounceNumericBox.ValueChanged += DebounceNumericBox_ValueChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(421, 6);
            label3.Name = "label3";
            label3.Size = new Size(23, 15);
            label3.TabIndex = 8;
            label3.Text = "ms";
            // 
            // TrimTextButton
            // 
            TrimTextButton.Location = new Point(2, 31);
            TrimTextButton.Name = "TrimTextButton";
            TrimTextButton.Size = new Size(110, 23);
            TrimTextButton.TabIndex = 9;
            TrimTextButton.Text = "Trim Text (Alt+C)";
            TrimTextButton.UseVisualStyleBackColor = true;
            TrimTextButton.Click += TrimTextButton_Click;
            // 
            // HotkeysCheckBox
            // 
            HotkeysCheckBox.AutoSize = true;
            HotkeysCheckBox.Checked = true;
            HotkeysCheckBox.CheckState = CheckState.Checked;
            HotkeysCheckBox.Location = new Point(116, 35);
            HotkeysCheckBox.Name = "HotkeysCheckBox";
            HotkeysCheckBox.Size = new Size(101, 19);
            HotkeysCheckBox.TabIndex = 10;
            HotkeysCheckBox.Text = "Hook Hotkeys";
            HotkeysCheckBox.UseVisualStyleBackColor = true;
            HotkeysCheckBox.CheckedChanged += HotkeysCheckBox_CheckedChanged;
            // 
            // ClipboardSensorForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(445, 128);
            Controls.Add(HotkeysCheckBox);
            Controls.Add(TrimTextButton);
            Controls.Add(label3);
            Controls.Add(DebounceNumericBox);
            Controls.Add(label2);
            Controls.Add(RedoButton);
            Controls.Add(MaximumPositionBox);
            Controls.Add(label1);
            Controls.Add(CurrentPositionBox);
            Controls.Add(UndoButton);
            Controls.Add(CurrentTextBox);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "ClipboardSensorForm";
            Text = "ClipboardSensor v1.3";
            ((System.ComponentModel.ISupportInitialize)CurrentPositionBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)MaximumPositionBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)DebounceNumericBox).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox CurrentTextBox;
        private Button UndoButton;
        private NumericUpDown CurrentPositionBox;
        private Label label1;
        private NumericUpDown MaximumPositionBox;
        private Button RedoButton;
        private Label label2;
        private NumericUpDown DebounceNumericBox;
        private Label label3;
        private Button TrimTextButton;
        private CheckBox HotkeysCheckBox;
    }
}
