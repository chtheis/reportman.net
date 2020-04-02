using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Globalization;
using System.Threading;

namespace Reportman.Drawing.Forms
{
    public enum ColumnDataType { Text, Integer, Numeric, Double, Date, DateTime, Time, ComboBox,ComboBoxList,Boolean,Password};
    public enum TextBoxDataType { Text, Integer, Numeric, Double};
    public class TextBoxAdvanced : TextBox
    {
        SortedList<char,char> validnumeric;
        SortedList<char, char> validinteger;
        SortedList<char, char> validdouble;
        char decimalsep;
        public TextBoxAdvanced()
            : base()
        {
            FBarCodeBeginChar = '$';
            FBarCodeEndChar = '%';


            decimalsep = (char)0;
            validnumeric = new SortedList<char, char>();
            validdouble = new SortedList<char, char>();

            string defsep = Thread.CurrentThread.CurrentUICulture.NumberFormat.NumberDecimalSeparator;
            validnumeric.Add('\n','\n');
            validnumeric.Add('0', '0');
            validnumeric.Add('1', '1');
            validnumeric.Add('2','2');
            validnumeric.Add('3','3');
            validnumeric.Add('4','4');
            validnumeric.Add('5','5');
            validnumeric.Add('6','6');
            validnumeric.Add('7','7');
            validnumeric.Add('8','8');
            validnumeric.Add('9','9');
            validnumeric.Add('\b', '\b');
            validnumeric.Add('-', '-');

            validinteger = new SortedList<char, char>();
            foreach (char c1 in validnumeric.Keys)
                validinteger.Add(c1,c1);
            foreach (char c in defsep)
            {
                if (decimalsep == (char)0)
                    decimalsep = c;
                validnumeric.Add(c,c);
            }

            foreach (char c2 in validnumeric.Keys)
                validdouble.Add(c2,c2);
            validdouble.Add('e', 'e');

            // Control-X
            validdouble.Add((char)24, (char)24);
            validnumeric.Add((char)24, (char)24);
            validinteger.Add((char)24, (char)24);

            // Control->Z
            validdouble.Add((char)26, (char)26);
            validnumeric.Add((char)26, (char)26);
            validinteger.Add((char)26, (char)26);

            // Control->C
            validdouble.Add((char)3, (char)3);
            validnumeric.Add((char)3, (char)3);
            validinteger.Add((char)3, (char)3);


        
        }
        private TextBoxDataType FDataType;
        public TextBoxDataType DataType
        {
            get { return FDataType; }
            set
            {
                FDataType = value;
            }
        }
        private bool FReadBarCode;
        public bool ReadBarCode
        {
            get { return FReadBarCode; }
            set
            {
                FReadBarCode = value;
            }
        }
        private char FBarCodeBeginChar;
        public char BarCodeBeginChar
        {
            get { return FBarCodeBeginChar; }
            set
            {
                FBarCodeBeginChar = value;
            }
        }
        protected override void OnTextChanged(EventArgs e)
        {
            if (FReadBarCode)
            {
                if (Text.Length>1)
                {
                    if (Text[0] == FBarCodeBeginChar)
                    {
                        int index = 1;
                        while (index < Text.Length)
                        {
                            if (Text[index] == FBarCodeEndChar)
                            {
                                Text = Text.Substring(1, index - 1);
                                break;
                            }
                            index++;
                        }
                    }
                }
            }
            base.OnTextChanged(e);
        }
        private char FBarCodeEndChar;
        public char BarCodeEndChar
        {
            get { return FBarCodeEndChar; }
            set
            {
                FBarCodeEndChar = value;
            }
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (FDataType)
            {
                case TextBoxDataType.Numeric:
                case TextBoxDataType.Double:
                    if (e.KeyCode == Keys.Decimal)
                    {
                        // No more than one comma admitted
                        string defsep = Thread.CurrentThread.CurrentUICulture.NumberFormat.NumberDecimalSeparator;
                        if (defsep.Length > 0)
                        {
                            if (defsep[0] != '.')
                            {
                                SendKeys.Send(defsep);
                                e.SuppressKeyPress = true;
                            }
                        }
                    }
                    break;

            }
            base.OnKeyDown(e);            
        }
        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
        }
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            int index;
            switch (FDataType)
            {
                case TextBoxDataType.Numeric:
                case TextBoxDataType.Double:
                case TextBoxDataType.Integer:
                    if (e.KeyChar != (char)22)
                    {

                        SortedList<char, char> lvalidate;
                        if (FDataType == TextBoxDataType.Numeric)
                            lvalidate = validnumeric;
                        else
                            if (FDataType == TextBoxDataType.Double)
                                lvalidate = validdouble;
                            else
                                lvalidate = validinteger;
                        index = lvalidate.IndexOfKey(e.KeyChar);
                        if (index < 0)
                            e.KeyChar = (char)0;
                        // only 1 - symbol
                        if (e.KeyChar == '-')
                        {
                            // only at the beginning
                            if (SelectionStart <= 0)
                            {
                                index = Text.IndexOf('-');
                                if (index >= 0)
                                    if (SelectionStart + SelectionLength < index)
                                        e.KeyChar = (char)0;
                            }
                            else
                                e.KeyChar = (char)0;
                        }
                        // only 1 e symbol
                        if (e.KeyChar == 'e')
                        {
                            index = Text.IndexOf('-');
                            if (index >= 0)
                                e.KeyChar = (char)0;
                        }
                        // only 1 decimalsep symbol
                        if (decimalsep != (char)0)
                        {
                            if (e.KeyChar == decimalsep)
                            {
                                index = Text.IndexOf(decimalsep);
                                if (index >= 0)
                                    e.KeyChar = (char)0;
                            }
                        }
                    }
                    break;

            }
            base.OnKeyPress(e);
        }
    }    
    public delegate bool DataColumnButtonClickEvent(DataGridViewColumn ncolumn,ref object value);
    public class DataGridViewColumnAdvanced:DataGridViewColumn
    {
        private ColumnDataType FDataType;
        private int FMaxInputLength;
        private Image FImageButton;
        public DataColumnButtonClickEvent ButtonClick;
        public float ImageButtonScale = 1.0f;
        public ColumnDataType DataType
        {
            get { return FDataType; }
            set
            {
                FDataType = value;
            }
        }
        public int MaxInputLength
        {
            get
            {
                return FMaxInputLength;
            }
            set
            {
                FMaxInputLength = value;
                if (FMaxInputLength < 0)
                    FMaxInputLength = 0;
            }

        }
        public static int ImageWidth
        {
            get
            {
                return Convert.ToInt32(20 * Reportman.Drawing.GraphicUtils.DPIScaleY);
            }
        }
        public Image ImageButton
        {
            get { return FImageButton; }
            set
            {
                FImageButton = value;
            }
        }
        public DataGridViewColumnAdvanced()
            : base(new DataGridViewCellAdvanced())
        {
            FDataType = ColumnDataType.Text;
            
        }

        public override int GetPreferredWidth(DataGridViewAutoSizeColumnMode autoSizeColumnMode, bool fixedHeight)
        {
            int nwidth = base.GetPreferredWidth(autoSizeColumnMode, fixedHeight);;
            return nwidth;
        }
        public override DataGridViewCell CellTemplate
        {
            get
            {
                return base.CellTemplate;
            }
            set
            {
                base.CellTemplate = value;
            }
        }
    }

    public class DataGridViewCellAdvanced : DataGridViewTextBoxCell
    {
        //DataGridViewComboBoxEditingControl ComboBoxPicker;
//        CheckBoxPickerControl CheckBoxPicker;
        //DataGridViewTextBoxEditingControl TextBoxPicker;
        //EllipsisEditingControl EllipsisPicker;
        //ColorPickerControl ColorPickerc;
        //NumericUpDownPickerControl NumericPicker;
        //private EventHandler ButtonClicEvent;
        //private EventHandler ClickFontStyleEvent;
        public DataGridViewCellAdvanced()
            : base()
        {
            //ButtonClicEvent = new EventHandler(ButtonClick);
        }
        private void ButtonClick(object sender, EventArgs args)
        {
/*            DataGridViewColumn col = GetColumn();
            if (!(col is DataGridViewColumnAdvanced))
                return;
            DataGridViewColumnAdvanced ncolumn = (DataGridViewColumnAdvanced)GetColumn();
            if (ncolumn.ButtonClick != null)
                ncolumn.ButtonClick(ncolumn, new DataGridViewColumnEventArgs(ncolumn),);*/
        }
        private DataGridViewColumn GetColumn()
        {
            DataGridViewColumn ncol = null;
            if (DataGridView != null)
            {
                ncol= DataGridView.Columns[ColumnIndex];
            }
            return ncol;
        }
        public override Type EditType
        {
            get
            {
                Type ntype = typeof(AdvancedEditingControl);
                //Return the type of the editing contol that ComboBox uses.
/*                if (DataGridView != null)
                {
                    ObjectInspectorCellType celltype = (ObjectInspectorCellType)GetColumnValue("TYPEENUM");
                    switch (celltype)
                    {
                        case ObjectInspectorCellType.Decimal:
                        case ObjectInspectorCellType.Integer:
                            ntype = typeof(NumericUpDownPickerControl);
                            break;
                        case ObjectInspectorCellType.DropDownList:
                            ntype = typeof(DataGridViewComboBoxEditingControl);
                            break;
                        case ObjectInspectorCellType.Text:
                        case ObjectInspectorCellType.FontStyle:
                            ntype = typeof(DataGridViewTextBoxEditingControl);
                            break;
                        case ObjectInspectorCellType.Color:
                            ntype = typeof(ColorPickerControl);
                            break;
                        case ObjectInspectorCellType.Boolean:
                            ntype = typeof(CheckBoxPickerControl);
                            break;
                        case ObjectInspectorCellType.FontName:
                            ntype = typeof(EllipsisEditingControl);
                            break;
                    }
                }*/
                return ntype;
            }
        }
        protected override void  Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
        {
 	        base.Paint(graphics, clipBounds, cellBounds, rowIndex, cellState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);
        }
        protected override object GetFormattedValue(object value, int rowIndex, ref DataGridViewCellStyle cellStyle, System.ComponentModel.TypeConverter valueTypeConverter, System.ComponentModel.TypeConverter formattedValueTypeConverter, DataGridViewDataErrorContexts context)
        {
            DataGridViewColumnAdvanced ncol = (DataGridViewColumnAdvanced)GetColumn();
            if (ncol.DataType == ColumnDataType.Password)
            {
                if (value == null)
                    return "";
                else
                    if (value == DBNull.Value)
                        return "";
                    else
                        if (value.ToString().Length == 0)
                            return "";
                        else
                            return "" + (char)0x25CF + (char)0x25CF + (char)0x25CF + (char)0x25CF + (char)0x25CF + (char)0x25CF;
            }
            else
                return base.GetFormattedValue(value, rowIndex, ref cellStyle, valueTypeConverter, formattedValueTypeConverter, context);
        }

    }
    public partial class AdvancedEditingControl : UserControl, IDataGridViewEditingControl
    {
        private TextBoxAdvanced textcontrol;
        private DateTimePickerNullable datecontrol1;
        //private DateTimePickerNullable datecontrol2;
        private PictureBox picbo;
        private ColumnDataType controldatatype;
        private ColumnDataType FDataType;
        private bool disabledchange;
        private int FMaxInputLength;

        public void DoKeyDown(object sender, KeyEventArgs args)
        {
            OnKeyDown(args);
        }
        public void DoKeyPress(object sender, KeyPressEventArgs args)
        {
            OnKeyPress(args);
        }
        public void DoKeyUp(object sender, KeyEventArgs args)
        {
            OnKeyUp(args);
        }
        public float ImageButtonScale = 1.0f;

        public Image ImageButton
        {
            get { return picbo.Image; }
            set 
            { 
                picbo.Image = value;
                picbo.Visible = picbo.Image!= null;
            picbo.SizeMode = PictureBoxSizeMode.Zoom;
            picbo.Width = DataGridViewColumnAdvanced.ImageWidth;
                CreateMainControl();
                ResizeControls();
            }

        }
        DataGridView m_dataGridView = null;
        int m_rowIndex = 0;
        bool m_valueChanged = false;
        //string m_prevText = null;
        public Control MainControl;
        object prevvalue;
        public new string Text
        {
            get
            {
                return textcontrol.Text;
            }
            set
            {
                textcontrol.Text = value;
            }
        }
        public AdvancedEditingControl()
        {
            picbo = new PictureBox();
            picbo.Click += new EventHandler(picboxButton_Click);
            picbo.SizeMode = PictureBoxSizeMode.Zoom;
            picbo.Width = DataGridViewColumnAdvanced.ImageWidth;
            picbo.Dock = DockStyle.Right;
            picbo.Visible = false;
            Controls.Add(picbo);
        }
        private void mValueChange(object sender, EventArgs ev)
        {
            NotifyChange();
        }
        public void SaveCurrentValue()
        {
            NotifyChange();
        }
        void mLostFocus(object sender, EventArgs e)
        {
            NotifyChange();
        }
        void mDoubleClick(object sender, EventArgs e)
        {
            if (m_dataGridView is DataGridViewAdvanced)
            {
                DataGridViewAdvanced ngrid = ((DataGridViewAdvanced)m_dataGridView);
                // aceptamos el valor
                ngrid.FinishEdit();
                ngrid.DoDoubleClick(MainControl);
            }
        }


        private void SetValue(object newvalue)
        {
            CreateControl();
            switch (FDataType)
            {
                case ColumnDataType.Double:
                case ColumnDataType.Integer:
                case ColumnDataType.Numeric:
                    textcontrol.Text = newvalue.ToString();
                    break;
                case ColumnDataType.Text:
                case ColumnDataType.Password:
                    textcontrol.Text = newvalue.ToString();
                    break;
                case ColumnDataType.Date:
                    if (newvalue != DBNull.Value)
                        datecontrol1.Value = System.Convert.ToDateTime(newvalue).Date;
                    else
                        datecontrol1.Value = DateTime.MinValue;
                    break;
                case ColumnDataType.Time:
                    if (newvalue != DBNull.Value)
                        datecontrol1.Value = System.Convert.ToDateTime(newvalue);
                    else
                        datecontrol1.Value = DateTime.MinValue;
                    break;
                case ColumnDataType.DateTime:
                    if (newvalue != DBNull.Value)
                        datecontrol1.Value = System.Convert.ToDateTime(newvalue);
                    else
                        datecontrol1.Value = DateTime.MinValue;
                    break;
            }
        }
        public object NewValue
        {
            get { return GetValue(); }
            set
            {
                disabledchange = true;
                try
                {
                    SetValue(value);
                }
                finally
                {
                    disabledchange = false;
                }

                NotifyChange();
            }
        }
        private object GetValue()
        {
            object nresult = null;
            switch (FDataType)
            {
                case ColumnDataType.Text:
                case ColumnDataType.Password:
                    nresult = textcontrol.Text;
                    break;
                case ColumnDataType.Integer:
                case ColumnDataType.Double:
                case ColumnDataType.Numeric:
                    nresult = textcontrol.Text;
                    break;
                case ColumnDataType.Date:
                case ColumnDataType.Time:
                    if (datecontrol1.Value == DateTime.MinValue)
                        nresult = DBNull.Value;
                    else
                        nresult = datecontrol1.Value.Date;
                    break;
                case ColumnDataType.DateTime:
                    if (datecontrol1.Value == DateTime.MinValue)
                        nresult = DBNull.Value;
                    else
                        nresult = datecontrol1.Value;
                    break;
                /*                case ColumnDataType.DateTime:
                                    if (datecontrol1.Value == DateTime.MinValue)
                                        nresult = DBNull.Value;
                                    else
                                        nresult = datecontrol1.Value.Add(datecontrol2.Value - datecontrol2.Value.Date);
                                    break;*/
            }
            return nresult;
        }
        private void SetNewValue(object nval)
        {
            switch (FDataType)
            {
                case ColumnDataType.Text:
                case ColumnDataType.Password:
                    textcontrol.Text = nval.ToString();
                    break;
                case ColumnDataType.Integer:
                case ColumnDataType.Double:
                case ColumnDataType.Numeric:
                    textcontrol.Text = nval.ToString();
                    break;
                case ColumnDataType.Date:
                case ColumnDataType.Time:
                case ColumnDataType.DateTime:
                    if (nval.ToString() != "")                      
                    datecontrol1.Value = (DateTime)nval;
                    break;
                /*case ColumnDataType.DateTime:
                    datecontrol1.Value = ((DateTime)nval).Date;
                    datecontrol2.Value = (DateTime)nval;
                    break;*/
            }
        }
        private void picboxButton_Click(object sender, EventArgs e)
        {
            DataGridViewColumnAdvanced ncol = GetColumn();
            if (ncol == null)
                return;
            if (ncol.ButtonClick != null)
            {
                object nvalue = GetValue();
                bool aresult = ncol.ButtonClick(GetColumn(), ref nvalue);
                if (aresult)
                {
                    //SetValue(nvalue);
                    //NotifyChange();
                }
            }
        }

        private void NotifyChange()
        {
            if (disabledchange)
                return;
            if (!GetValue().Equals(prevvalue))
            {
                m_valueChanged = true;
                m_dataGridView.NotifyCurrentCellDirty(true);
                prevvalue = GetValue();
            }
        }
        public void ApplyCellStyleToEditingControl(DataGridViewCellStyle dataGridViewCellStyle)
        {
            // Do nothing
        }
        public Cursor EditingControlCursor
        {
            get
            {
                return Cursors.IBeam;
            }
        }
        public Cursor EditingPanelCursor
        {
            get
            {
                return Cursors.IBeam;
            }
        }
        public DataGridView EditingControlDataGridView
        {
            get
            {
                return m_dataGridView;
            }
            set
            {
                m_dataGridView = value;
            }
        }
        public object EditingControlFormattedValue
        {
            get
            {
                return GetEditingControlFormattedValue(DataGridViewDataErrorContexts.Display);
            }
            set
            {
                SetNewValue(value);
            }
        }
        public int EditingControlRowIndex
        {
            get
            {
                return m_rowIndex;
            }
            set
            {
                m_rowIndex = value;
            }
        }

        public bool EditingControlValueChanged
        {
            get
            {
                return m_valueChanged;
            }
            set
            {
                m_valueChanged = value;
            }
        }
        public bool EditingControlWantsInputKey(Keys keyData, bool dataGridViewWantsInputKey)
        {
            if (FDataType == ColumnDataType.Date)
            {
                switch (keyData)
                {
                    case Keys.Down:
                        return false;
                    case Keys.Up:
                        return false;
                    default:
                        return true;
                }

            }

            switch (keyData)
            {
                case Keys.Tab:
                    return true;
                case Keys.Home:
                    return true;
                case Keys.End:
                    return true;
                case Keys.Left:
                    if ((this.textcontrol.SelectionLength == 0)
                        && (this.textcontrol.SelectionStart==0))
                        return false;
                    else
                        return true;
                case Keys.Right:
                    if ((this.textcontrol.SelectionLength == 0)
                        && (this.textcontrol.SelectionStart == this.textcontrol.Text.Length))
                        return false;
                    else
                        return true;
                case Keys.Delete:
//                    this.textcontrol.Text = "";
                    return true;
                case Keys.Enter:
                    NotifyChange();
                    return false;
                case Keys.Up:
                    return false;
                case Keys.Down:
                    return false;
                default:
                    if (FDataType == ColumnDataType.Text)
                        return true;
                    else
                        return false;
            }
        }

        public object GetEditingControlFormattedValue(DataGridViewDataErrorContexts context)
        {
            object nvalue = GetValue();
            if (nvalue == null)
                return null;
            string fvalue = "";
            switch (FDataType)
            {
                case ColumnDataType.Text:
                case ColumnDataType.Password:
                    fvalue = nvalue.ToString();
                    break;
                case ColumnDataType.Double:
                case ColumnDataType.Integer:
                case ColumnDataType.Numeric:
                    fvalue = nvalue.ToString();
                    break;
                case ColumnDataType.Date:
                    if (nvalue == DBNull.Value)
                        fvalue = "";
                    else
                        fvalue = ((DateTime)nvalue).ToString("dd/MM/yyyy");
                    break;
                case ColumnDataType.Time:
                    if (nvalue == DBNull.Value)
                        fvalue = "";
                    else
                        fvalue = ((DateTime)nvalue).ToString("HH:mm:ss");
                    break;
                case ColumnDataType.DateTime:
                    if (nvalue == DBNull.Value)
                        fvalue = "";
                    else
                        fvalue = ((DateTime)nvalue).ToString("dd/MM/yyyy HH:mm:ss");
                    break;
            }
            return fvalue;
        }
        public void CreateMainControl()
        {
            if (controldatatype != FDataType)
            {
                MainControl = null;
            }
            else
            {
                if (MainControl is TextBoxAdvanced)
                {
                    if (((TextBoxAdvanced)MainControl).MaxLength != FMaxInputLength)
                        MainControl = null;
                }
            }
            if (MainControl == null)
            {
                controldatatype = FDataType;
                switch (FDataType)
                {
                    case ColumnDataType.Text:
                    case ColumnDataType.Password:
                    case ColumnDataType.Numeric:
                    case ColumnDataType.Integer:
                    case ColumnDataType.Double:
                        if (textcontrol == null)
                        {
                            textcontrol = new TextBoxAdvanced();
                            textcontrol.BorderStyle = BorderStyle.None;
                            textcontrol.Font = this.EditingControlDataGridView.Font;
                            textcontrol.Multiline = true;
                            textcontrol.MinimumSize = new System.Drawing.Size(0,textcontrol.Height );
                            textcontrol.Multiline = false;
                            //textcontrol.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
                            textcontrol.LostFocus += new EventHandler(mLostFocus);
                            textcontrol.TextChanged += new EventHandler(mValueChange);
                            textcontrol.KeyDown += new KeyEventHandler(DoKeyDown);
                            textcontrol.KeyUp += new KeyEventHandler(DoKeyDown);
                            textcontrol.KeyPress += new KeyPressEventHandler(DoKeyPress);
                            textcontrol.DoubleClick += new EventHandler(mDoubleClick);
                            MainControl = textcontrol;
                            textcontrol.DataType = (TextBoxDataType)FDataType;
                            textcontrol.MaxLength = FMaxInputLength;
                            if (FDataType == ColumnDataType.Password)
                                textcontrol.UseSystemPasswordChar = true;
                            else
                                textcontrol.UseSystemPasswordChar = false;
                            Controls.Add(textcontrol);
                        }
                        else
                        {
                            textcontrol.Visible = true;
                            textcontrol.DataType = (TextBoxDataType)FDataType;
                            textcontrol.MaxLength = FMaxInputLength;
                            if (FDataType == ColumnDataType.Password)
                                textcontrol.UseSystemPasswordChar = true;
                            else
                                textcontrol.UseSystemPasswordChar = false;
                            MainControl = textcontrol;
                        }
                        ResizeControls();
                        break;
                    case ColumnDataType.Date:
                    case ColumnDataType.Time:
                    case ColumnDataType.DateTime:
                        if (datecontrol1 == null)
                        {
                            datecontrol1 = new DateTimePickerNullable();
                            datecontrol1.ValueChanged += new EventHandler(datecontrol1_ValueChanged);
                            //datecontrol2 = new DateTimePickerNullable();
                            //datecontrol2.ValueChanged += new EventHandler(datecontrol1_ValueChanged);
                            datecontrol1.Format = DateTimePickerFormat.Custom;
                            if (FDataType == ColumnDataType.DateTime)
                                datecontrol1.CustomFormat = "dd/MM/yyyy HH:mm:ss";
                            else
                                if (FDataType == ColumnDataType.Time)
                                    datecontrol1.CustomFormat = "HH:mm:ss";
                                else
                                    if (FDataType == ColumnDataType.Date)
                                        datecontrol1.CustomFormat = "dd/MM/yyyy";

                            //datecontrol2.Format = DateTimePickerFormat.Custom;
                            //datecontrol2.CustomFormat = "hh:mm:ss";
                            //datecontrol2.KeyDown += new KeyEventHandler(DoKeyDown);
                            //datecontrol2.KeyUp += new KeyEventHandler(DoKeyDown);
                            //datecontrol2.KeyPress += new KeyPressEventHandler(DoKeyPress);
                            datecontrol1.KeyDown += new KeyEventHandler(DoKeyDown);
                            datecontrol1.KeyUp += new KeyEventHandler(DoKeyDown);
                            datecontrol1.KeyPress += new KeyPressEventHandler(DoKeyPress);
                            datecontrol1.LostFocus += new EventHandler(mLostFocus);
                            datecontrol1.ValueChanged += new EventHandler(mValueChange);
                            //datecontrol2.LostFocus += new EventHandler(mLostFocus);
                            //datecontrol2.ValueChanged += new EventHandler(mValueChange);
                            datecontrol1.DoubleClick += new EventHandler(mDoubleClick);
                            //datecontrol2.DoubleClick += new EventHandler(mDoubleClick);
                            Controls.Add(datecontrol1);
                            //Controls.Add(datecontrol2);
                        }
                        //datecontrol1.Visible = ((FDataType == ColumnDataType.Date) || (FDataType == ColumnDataType.DateTime));
                        //datecontrol2.Visible = ((FDataType == ColumnDataType.Time) || (FDataType == ColumnDataType.DateTime));
                        /*if (FDataType == ColumnDataType.Time)
                            MainControl = datecontrol2;
                        else
                            MainControl = datecontrol1;*/                        
                        MainControl = datecontrol1;
                        MainControl.Visible = true;
                        ResizeControls();
                        break;
                }
            }
            else
            {

            }
        }

        void datecontrol1_ValueChanged(object sender, EventArgs e)
        {
            m_dataGridView.NotifyCurrentCellDirty(true);
        }
        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            base.SetBoundsCore(x, y, width, height, specified);
            CreateMainControl();
            ResizeControls();
        }
        private void ResizeControls()
        {
            MainControl.Top = (this.Height - MainControl.Height) / 2;
            int offset=0;
            if (picbo.Visible)
            offset=picbo.Width;
            //if (FDataType == ColumnDataType.DateTime)
            //{
             //   datecontrol1.Width = (this.Width - offset) / 2;
                //datecontrol2.Width = datecontrol1.Width;
                //datecontrol2.Left = datecontrol1.Width;
                //datecontrol2.Top = datecontrol1.Top;
            //}
            //else
            {
                MainControl.Width = this.Width - offset;
            }
        }

        private DataGridViewColumnAdvanced GetColumn()
        {
            DataGridViewColumnAdvanced nresult = null;
            if (m_dataGridView == null)
                return nresult;
            if (m_dataGridView.CurrentCell == null)
                return nresult;
            if (m_dataGridView.CurrentCell.ColumnIndex < 0)
                return nresult;
            return (DataGridViewColumnAdvanced)m_dataGridView.Columns[m_dataGridView.CurrentCell.ColumnIndex];
        }

        public void PrepareEditingControlForEdit(bool selectAll)
        {
            DataGridViewColumnAdvanced ncol = GetColumn();
            if (FDataType != ncol.DataType)
            {
                FDataType = ncol.DataType;
                if (MainControl!=null)
                {
                    MainControl.Visible=false;
                    MainControl = null;
                }
            }
            FMaxInputLength = ncol.MaxInputLength;

            CreateMainControl();
            disabledchange = true;
            try
            {
                prevvalue = DBNull.Value;
                if (this.m_dataGridView.CurrentCell.Value != null)
                    prevvalue = this.m_dataGridView.CurrentCell.Value;
                SetValue(prevvalue);
                prevvalue = GetValue();
            }
            finally
            {
                disabledchange = false;
            }
            if (this.textcontrol!=null)
                if (selectAll)
                    this.textcontrol.SelectAll();
            if (MainControl != null)
                if (MainControl.Visible)
                    MainControl.Focus();
            ImageButtonScale = ncol.ImageButtonScale;
            ImageButton = ncol.ImageButton;
        }
        public bool RepositionEditingControlOnValueChange
        {
            get
            {
                return false;
            }
        }
        public override bool Focused
        {
            get
            {
                if (MainControl != null)
                    return MainControl.Focused;
                else
                    return base.Focused;
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (textcontrol!=null)
                textcontrol.Dispose();
            if (datecontrol1!=null)
                datecontrol1.Dispose();
//            if (datecontrol2!=null)
//                datecontrol2.Dispose();
            if (picbo!=null)
                picbo.Dispose();

            base.Dispose(disposing);
        }
    }


}
