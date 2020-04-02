using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Globalization;

using System.Drawing.Drawing2D;

namespace Reportman.Drawing.Forms
{
    class ToolStripComboBoxAdvanced:ToolStripTextBox
    {
        public delegate List<string> OnSearchHandler(string textsearch);
        public ToolStripComboBoxAdvanced():base()
        {
            const int SEARCH_INTERVAL = 500;
            timer1 = new Timer();
            timer1.Interval = SEARCH_INTERVAL;
            timer1.Tick += new EventHandler(timer1_Tick);
        }
        public OnSearchHandler OnSearch;
        System.Windows.Forms.Timer timer1;

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
        }
        private void UpdateData()
        {
            if (OnSearch != null)
            {
                List<string> searchData = OnSearch(Text);
                UpdateHint(searchData);
            }
        }

        //While timer is running don't start search
        //timer1.Interval = 1500;
        private void RestartTimer()
        {
            timer1.Stop();
            timer1.Start();
        }

        //Update data when timer stops
        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            string newtext = Text;
            if (newtext != oldtext)
                UpdateData();
        }
        protected override void OnLeave(EventArgs e)
        {
            if (currenttooltip != null)
                currenttooltip.Hide(this.Parent);

            base.OnLeave(e);
        }

        public List<string> OnSearchSample(string search_text)
        {
            List<string> source = new List<string>();
            source.Add("LONCH. JAMON CURADO");
            source.Add("L.EMBUTIDO/EMBUCHADO");
            source.Add("EMBUTIDO/CHORIZO/EMB");
            source.Add("LONCHEADOS COCIDOS");
            source.Add("JAMON CURADO PIEZA");
            source.Add("TAPAS");
            source.Add("PIEZAS COCIDAS");
            source.Add("FIAMBRES/MORTADELAS");
            source.Add("LONCHEADOS FIAMBRES");
            source.Add("LONCHEADO COMBINADO");

            return WordSearch(source,search_text);
        }
        string oldtext = "";
        protected override void OnKeyDown(KeyEventArgs e)
        {
            oldtext = Text;
            base.OnKeyDown(e);
        }
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            RestartTimer();
        }
        static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
        public List<string> WordSearch(List<string> source,string search_text)
        {
            search_text = RemoveDiacritics(search_text.Trim().ToUpper());
            List<string> result = new List<string>();
            string[] words = search_text.Trim().ToUpper().Split(' ');
            for (int i = 0; i < words.Length;i++ )
            {
                words[i] = words[i].Trim();
            }
            foreach (string item in source)
            {

                bool match = true;
                string item_upper = RemoveDiacritics(item.Trim().ToUpper());
                foreach (string word in words)
                {
                    if (!item_upper.Contains(word))
                        match = false;
                }
                if (match)
                    result.Add(item);
            }
            return result;
        }
        ToolTipList currenttooltip;
        private void UpdateHint(List<string> source)
        {
            if (currenttooltip == null)
                currenttooltip = new ToolTipList();
            if (source.Count == 0)
                currenttooltip.Hide(this.Parent);
            else
            {
                currenttooltip.MinimumWidth = this.Width;
                currenttooltip.DataSource = source;
                currenttooltip.Position = 0;
                currenttooltip.Show("test", this.Parent,new Point(this.Bounds.Left,this.Bounds.Height+this.Bounds.Top));
            }
        }
    }
    public class ToolTipList:ToolTip
    {
        public int MinimumWidth;
        public int MaximumLines = 20;
        public ToolTipList()
        {
            this.OwnerDraw = true;
            this.Popup += new PopupEventHandler(this.OnPopup);
            this.Draw += new DrawToolTipEventHandler(this.OnDraw);
        }
        private int GetLineHeight(Graphics gr,Font font)
        {
            int LINE_MARGIN = Convert.ToInt32(2 * (float)gr.DpiX / 96);
            SizeF lineitem = gr.MeasureString("Mg", font, 2000);
            int lineheight = Convert.ToInt32(lineitem.Height) + LINE_MARGIN;
            return lineheight;
        }
        private void OnPopup(object sender, PopupEventArgs e) // use this event to set the size of the tool tip
        {
            using (Graphics gr = e.AssociatedControl.CreateGraphics())
            {
                int lines = DataSource.Count;
                if (lines > MaximumLines)
                    lines = MaximumLines;
                int lineheight = GetLineHeight(gr, e.AssociatedControl.Font);
                int height =  lineheight * lines;
                int width = MinimumWidth;
                for (int i = 0; i < lines;i++ )
                {
                    float linwidth = gr.MeasureString(DataSource[i], e.AssociatedControl.Font).Width;
                    if (linwidth > width)
                        width = Convert.ToInt32(linwidth);
                }
                e.ToolTipSize = new Size(width, height);
            }
        }
        SolidBrush BackgroundBrush = new SolidBrush(SystemColors.Info);
        SolidBrush BackgroundBrushHightLight = new SolidBrush(SystemColors.Highlight);
        Color ForegroundColor = SystemColors.InfoText;
        SolidBrush ForegroundBrush = new SolidBrush(SystemColors.InfoText);
        SolidBrush ForegroundBrushHightlight = new SolidBrush(SystemColors.HighlightText);
        Pen ForegroundPen = new Pen(new SolidBrush(SystemColors.InfoText), 1);
        private void OnDraw(object sender, DrawToolTipEventArgs e) // use this event to customise the tool tip
        {
            Graphics g = e.Graphics;
            
 
            //LinearGradientBrush b = new LinearGradientBrush(e.Bounds,
            //    Color.GreenYellow, Color.MintCream, 45f);
 
            g.FillRectangle(BackgroundBrush, e.Bounds);
 
            g.DrawRectangle(ForegroundPen, new Rectangle(e.Bounds.X, e.Bounds.Y,
                e.Bounds.Width - 1, e.Bounds.Height - 1));

            int lineheight = GetLineHeight(g, e.AssociatedControl.Font);

            int posy = 0;
            int idx = 0;
            foreach (string nstring in DataSource)
            {
                //if (idx == Position)
                //{
                //    g.FillRectangle(BackgroundBrushHightLight, new Rectangle(0, posy, e.Bounds.Width, lineheight));
                //    g.DrawString(nstring, e.AssociatedControl.Font, ForegroundBrushHightlight, new Point(0, posy));
                //}
                //else
                    g.DrawString(nstring, e.AssociatedControl.Font, ForegroundBrush, new Point(0, posy));
                posy = posy + lineheight;
                idx++;
            }

 
            //g.DrawString(e.ToolTipText, new Font(e.Font, FontStyle.Bold), Brushes.Silver,
            //    new PointF(e.Bounds.X + 6, e.Bounds.Y + 6)); // shadow layer
            //g.DrawString(e.ToolTipText, new Font(e.Font, FontStyle.Bold), Brushes.Black,
            //    new PointF(e.Bounds.X + 5, e.Bounds.Y + 5)); // top layer
 
        }        
        public List<string> DataSource = new List<string>();
        public int Position;
    }
}
