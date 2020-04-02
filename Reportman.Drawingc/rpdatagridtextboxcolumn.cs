using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Reportman.Drawing
{
    public delegate void CellFormatEventHandler(object sender, DataGridFormatEventArgs e);
    public class DataGridTextBoxColumnAdvanced : DataGridTextBoxColumn
    {

        public CellFormatEventHandler CellFormat;
        


        protected override void Paint(Graphics g, Rectangle Bounds, CurrencyManager Source, int RowNum, Brush BackBrush, Brush ForeBrush, bool AlignToRight)
        {
            //check if our event handler is not null for this cell and then signal the event
            //the returning result in 'e' will contain a true or false boolean value
            //the event can be 'receieved' anywhere in our program and the logic can be anything
            // we choose.  The method called from the event simply has to match our 'CheckCellEventHandler'
            //delegate.
            if (CellFormat != null)
            {
                Color BackColor = ((SolidBrush)BackBrush).Color;
                Color ForeColor = ((SolidBrush)ForeBrush).Color;
                DataGridFormatEventArgs e = new DataGridFormatEventArgs(RowNum, this, BackColor,
                    ForeColor);
                CellFormat(this, e);
                if (e.BackColor != BackColor)
                {
                    BackBrush = new SolidBrush(e.BackColor);
                }
                if (e.ForeColor != ForeColor)
                {
                    ForeBrush = new SolidBrush(e.ForeColor);
                }
            }
            base.Paint(g, Bounds, Source, RowNum, BackBrush, ForeBrush, AlignToRight);

            //out third event handler , we are going to use this draw a rectangle around our
            //cell so we call it after we have called the base class paint method
            /*if (CheckCellEquals != null)
            {
                DataGridEnableEventArgs e = new DataGridEnableEventArgs(RowNum, _col, enabled);
                CheckCellEquals(this, e);
                if (e.MeetsCriteria)
                    g.DrawRectangle(new Pen(Color.Red, 2), Bounds.X + 1, Bounds.Y + 1, Bounds.Width - 2, Bounds.Height - 2);
            }*/
        }

    }

    public class DataGridFormatEventArgs : EventArgs
    {
        public DataGridFormatEventArgs(int row, DataGridTextBoxColumn col, Color backcolor,Color forecolor)
        {
            this.Row = row;
            this.Column = col;
            this.BackColor = backcolor;
            this.ForeColor = forecolor;
        }

        public DataGridTextBoxColumn Column;
        public int Row;
        public Color BackColor;
        public Color ForeColor;
    }
}
