using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Reportman.Drawing;

namespace Reportman.Designer
{
    /// <summary>
    /// Color picker is a class to allow the user to pick colors
    /// </summary>
    [DefaultProperty("Color"),DefaultEvent("ColorChanged")]
    public partial class ColorPicker : CheckBox
    {
        private bool FTextDisplayed;
        public event EventHandler ColorChanged;
//        private EditorService FEditorService;
        private const string DefaultColorName="Black";
        private void Init(Color c)
        {
            //InitializeComponent();
            FTextDisplayed = true;
            Appearance = Appearance.Button;
//          Dock = DockStyle.Fill;
            TextAlign = ContentAlignment.MiddleCenter;
            CheckStateChanged+=new EventHandler(OnCheckStateChanged);
            SetColor(c);
//            Controls.Add(FCheckBox);
///            FEditorService = new EditorService(this);
        }
        public ColorPicker(Color c):base()
        {
            Init(c);
        }
        public ColorPicker():base()
        {
            Init(System.Drawing.Color.FromName(DefaultColorName));
        }
        [Description("The currently selected color."), Category("Appearance"),
            DefaultValue(typeof(Color),DefaultColorName)]
        public Color Color
        {
            get
            {
                return BackColor;
            }
            set
            {
                if (BackColor != value)
                {
                    SetColor(value);
                    if (ColorChanged!=null)
                        ColorChanged(this, new EventArgs());
                }
            }
        }
        [Description("True meanse the control displays the currently selected color's name, False otherwise."),
            Category("Appearance"), DefaultValue(true)]
        public bool TextDisplayed
        {
            get { return FTextDisplayed; }
            set
            {
                FTextDisplayed = value;
                SetColor(this.Color);
            }
        }
        private void SetColor(Color c)
        {
            BackColor=c;
            ForeColor = GraphicUtils.GetInvertedBlackWhite(c);
            if (FTextDisplayed)
                Text=c.Name;
            else
                Text="";
        }
        private void OnCheckStateChanged(object sender,EventArgs e)
        {
//            if (CheckState==CheckState.Checked)
                ShowDropDown();
//            else
//                CloseDropDown();
        }
        private void ShowDropDown()
        {
            ColorDialog ndialog = new ColorDialog();
            ndialog.Color = Color;
            if (ndialog.ShowDialog() == DialogResult.OK)
                Color = ndialog.Color;
/*            System.Drawing.Design.ColorEditor Editor = new System.Drawing.Design.ColorEditor();
            Color c=Color;
            object newvalue=Editor.EditValue(FEditorService,c);
            if ((newvalue!=null) && (!FEditorService.Canceled))
                Color=(System.Drawing.Color)newvalue;            
            CheckState=CheckState.Unchecked;*/
        }
        private void CloseDropDown()
        {
  //          FEditorService.CloseDropDown();
        }
    }
    class DropDownForm:Form
    {
        private bool FCanceled ;
        private bool FCloseDropDownCalled;
        private Panel FParentPanel;
        public DropDownForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            KeyPreview = true;
            StartPosition = FormStartPosition.Manual;
            Panel p=new Panel();
            FParentPanel=p;
            p.BorderStyle = BorderStyle.FixedSingle;
            p.Dock = DockStyle.Fill;
            this.Controls.Add(p);
        } 
        public void SetControl(Control ctl)
        {
            FParentPanel.Controls.Add(ctl);
        }
        public bool Canceled
        {
            get
            {
                return FCanceled;
            }
        }
        public void CloseDropDown()
        {
            FCloseDropDownCalled=true;
            Hide();
        }
        protected override void  OnKeyDown(KeyEventArgs e)
        {
 	        base.OnKeyDown(e);
            if ((e.Modifiers==0) && (e.KeyCode==Keys.Escape))
                Hide();
        }
        protected override void  OnDeactivate(EventArgs e)
        {
//            Owner=null;
     	    base.OnDeactivate(e);
//            if (!FCloseDropDownCalled)
//                FCanceled=true;
//            Hide();
        }
    }

    class EditorService : IWindowsFormsEditorService, IServiceProvider
    {
        private ColorPicker FColorPicker;
        private DropDownForm FDropDownForm;
        private bool FCanceled;
        public EditorService(ColorPicker owner)
            : base()
        {
            FColorPicker = owner;
        }
        public bool Canceled
        {
            get { return FCanceled; }
        }
        public void CloseDropDown()
        {
            FDropDownForm.CloseDropDown();
        }
        public void DropDownControl(Control ct)
        {
            if (FDropDownForm != null)
            {
                FDropDownForm.Dispose();
                FDropDownForm = null;
            }
            FCanceled = false;
            FDropDownForm = new DropDownForm();
            FDropDownForm.Bounds = ct.Bounds;
            FDropDownForm.SetControl(ct);
            // Lookup a parent form for the Picker control and make the dropdown form to be owned
            // by it. This prevents to show the dropdown form icon when the user presses the At+Tab system 
            // key while the dropdown form is displayed.
            Control PickerForm = this.GetParentForm(FColorPicker);
            if ((PickerForm != null) && (PickerForm is Form))
                FDropDownForm.Owner = (Form)PickerForm;
            this.PositionDropDownForm();
//            FDropDownForm.Show();
//            this.DoModalLoop();
            FDropDownForm.ShowDialog();
            FCanceled = FDropDownForm.Canceled;
        }
        public DialogResult ShowDialog(Form dialog)
        {
            throw new NotSupportedException();
        }
        public object GetService(System.Type servicetype)
        {
            if (servicetype==typeof(IWindowsFormsEditorService))
                return this;
            else
                return null;
        }
        private void PositionDropDownForm()
        {
            Point loc=FColorPicker.Parent.PointToScreen(FColorPicker.Location);
            Rectangle screenrect=Screen.PrimaryScreen.WorkingArea;
            if (loc.X<screenrect.X)
                loc.X=screenrect.X;
            else
                if ((loc.X+FDropDownForm.Width)>screenrect.Right)
                    loc.X=screenrect.Right-FDropDownForm.Width;
            if ((loc.Y+FColorPicker.Height+FDropDownForm.Height)>screenrect.Bottom)
                loc.Offset(0,-FDropDownForm.Height);
            else
                loc.Offset(0,FColorPicker.Height);
            FDropDownForm.Location=loc;
        }
        private Control GetParentForm(Control ctl)
        {
            Control aresult = ctl;
            while (true)
            {
                if (ctl.Parent == null)
                {
                    aresult = ctl;
                    break;
                }
                else
                    ctl = ctl.Parent;
            }
            return aresult;
        }

/*        private void DoModalLoop()
        {
            while (FDropDownHolder.Visible)
            {
                Application.DoEvents();
                // The sollowing is the undocumented User32 call. For more information
                // see the accompanying article at http://www.vbinfozine.com/a_colorpicker.shtml
                this.MsgWaitForMultipleObjects(1, IntPtr.Zero, 1, 5, 255);
            }
        }*/
    }
}
