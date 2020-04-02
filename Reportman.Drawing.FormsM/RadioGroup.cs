using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Reportman.Drawing.Forms
{
    public partial class RadioGroup : Reportman.Drawing.Forms.GroupBoxAdvanced
    {
        public bool FHorizontal;
        private bool FAutoSize;
        private int FSelectedIndex;
        List<RadioButton> buttons;
        public int SelectedIndex
        {
            get
            {
                return FSelectedIndex;
            }
            set
            {
                if (FSelectedIndex < 0)
                {
                    foreach (RadioButton bt in buttons)
                    {
                        bt.Checked = false;
                    }
                }
                else
                if (FSelectedIndex < buttons.Count)
                {
                    buttons[FSelectedIndex].Checked = true; ;
                    FSelectedIndex = value;
                }
            }
        }
        public bool AutoAdjustSize
        {
            get { return FAutoSize; }
            set 
            { 
                FAutoSize = value;
                DoResize();            
            }
        }
        public bool Horizontal
        {
            get { return FHorizontal; }
            set
            {
                FHorizontal = value;
                DoResize();
            }
        }
        public void SetCaptions(Strings captions)
        {
            foreach (RadioButton rbold in buttons)
            {
                Controls.Remove(rbold);
                rbold.Dispose();
            }
            foreach (string captionname in captions)
            {
                RadioButton nradio = new RadioButton();
                nradio.Text = captionname;
                nradio.CheckedChanged += new EventHandler(radiochange);
                Controls.Add(nradio);
                buttons.Add(nradio);
            }
        }
        private void radiochange(object sender, EventArgs nargs)
        {
            int idx=0;
            FSelectedIndex = -1;
            foreach (RadioButton nbutton in buttons)
            {
                if (nbutton.Checked)
                {
                    FSelectedIndex = idx;
                    break;
                }
            }
        }
        public RadioGroup()
        {
            InitializeComponent();
            buttons = new List<RadioButton>();
            FSelectedIndex = -1;
        }
        private void DoResize()
        {
            const int TOP_GAP = 3;
            const int BOTTOM_GAP = 3;
            const int LEFT_GAP = 10;
            int INTER_GAP = 0;
            int INTER_GAP_VERTICAL =0;
            SizeF maxsize = new SizeF(0.0f,0.0f);
            using (Graphics gr = this.CreateGraphics())
            {
                foreach (RadioButton rb in buttons)
                {
                    SizeF newsize = gr.MeasureString(rb.Text, Font);
                    if (newsize.Height < 17)
                        newsize.Height = 17;
                    if (Horizontal)
                        newsize.Width = newsize.Width + 20;
                    if (newsize.Width > maxsize.Width)
                        maxsize = new SizeF(newsize.Width, maxsize.Height);
                    if (newsize.Height> maxsize.Height)
                        maxsize = new SizeF(maxsize.Width, newsize.Height);
                }
                // Measure the caption
                SizeF newsize2 = gr.MeasureString(Text, Font);
                if (!Horizontal)
                    if (newsize2.Width > maxsize.Width)
                        maxsize = new SizeF(newsize2.Width, maxsize.Height);
                if (newsize2.Height > maxsize.Height)
                    maxsize = new SizeF(maxsize.Width, newsize2.Height);
            }
            Size imaxsize = new Size(System.Convert.ToInt32(maxsize.Width),System.Convert.ToInt32(maxsize.Height));
            INTER_GAP = 5;
            INTER_GAP_VERTICAL = imaxsize.Height/6;
            int TopPos = imaxsize.Height+TOP_GAP;
            int LeftPos = LEFT_GAP;
            if (FAutoSize)
            {
                foreach (RadioButton radio in buttons)
                {
                    radio.SetBounds(LeftPos, TopPos, imaxsize.Width, imaxsize.Height);
                    if (Horizontal)
                    {
                        LeftPos = LeftPos + imaxsize.Width + INTER_GAP;
                    }
                    else
                    {
                        TopPos = TopPos + imaxsize.Height + INTER_GAP_VERTICAL;
                    }
                }
                if (Horizontal)
                    SetBounds(Left, Top, LeftPos + LEFT_GAP * 2, TOP_GAP + imaxsize.Height * 2 + BOTTOM_GAP * 2);
                else
                    SetBounds(Left, Top, imaxsize.Width + LEFT_GAP * 2, TopPos + BOTTOM_GAP);
            }
            else
            {
                if (buttons.Count == 0)
                    return;
                INTER_GAP = (Width - LEFT_GAP * 2) / buttons.Count;
                INTER_GAP_VERTICAL = (Height - TOP_GAP - imaxsize.Height - BOTTOM_GAP) / buttons.Count;
                TopPos = (Height - imaxsize.Height*2) / 2;
                foreach (RadioButton radio in buttons)
                {
                    if (Horizontal)
                    {
                        radio.SetBounds(LeftPos, TopPos, imaxsize.Width, imaxsize.Height);
                        LeftPos = LeftPos + INTER_GAP;
                    }
                    else
                    {
                        radio.SetBounds(LeftPos, TopPos, imaxsize.Width, imaxsize.Height);
                        TopPos = TopPos + INTER_GAP_VERTICAL;
                    }
                }
            }
        }
        protected override void OnFontChanged(EventArgs e)
        {
            DoResize();
            // Resize the control
            base.OnFontChanged(e);
        }
        protected override void OnParentChanged(EventArgs e)
        {
            DoResize();
            base.OnParentChanged(e);
        }
        protected override void OnResize(EventArgs e)
        {
            if (!FAutoSize)
                base.OnResize(e);
        }
    }
}
