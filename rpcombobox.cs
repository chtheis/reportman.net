using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace Reportman.Drawing.Forms
{
    public class ComboBoxAdvanced : ComboBox
    {
        private ImageList imageList;
        public ImageList ImageList
        {
            get { return imageList; }
            set { imageList = value; }
        }

        public ComboBoxAdvanced():base()
        {
            DrawMode = DrawMode.OwnerDrawFixed;
        }
        protected override void OnDrawItem(DrawItemEventArgs ea)
        {
            if (imageList == null)
            {
                base.OnDrawItem(ea);
                return;
            }
            ea.DrawBackground();
            ea.DrawFocusRectangle();

            ComboBoxAdvancedItem item;
            Size imageSize = imageList.ImageSize;
            Rectangle bounds = ea.Bounds;

            try
            {
                item = (ComboBoxAdvancedItem)Items[ea.Index];

                if (item.ImageIndex != -1)
                {
                    imageList.Draw(ea.Graphics, bounds.Left, bounds.Top,
          item.ImageIndex);
                    ea.Graphics.DrawString(item.Text, ea.Font, new
          SolidBrush(ea.ForeColor), bounds.Left + imageSize.Width, bounds.Top);
                }
                else
                {
                    ea.Graphics.DrawString(item.Text, ea.Font, new
          SolidBrush(ea.ForeColor), bounds.Left, bounds.Top);
                }
            }
            catch
            {
                if (ea.Index != -1)
                {
                    ea.Graphics.DrawString(Items[ea.Index].ToString(), ea.Font, new
          SolidBrush(ea.ForeColor), bounds.Left, bounds.Top);
                }
                else
                {
                    ea.Graphics.DrawString(Text, ea.Font, new
          SolidBrush(ea.ForeColor), bounds.Left, bounds.Top);
                }
            }

            base.OnDrawItem(ea);
        }
    }

    public class ComboBoxAdvancedItem
    {
        private string _text;
        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        private int _imageIndex;
        public int ImageIndex
        {
            get { return _imageIndex; }
            set { _imageIndex = value; }
        }

        public ComboBoxAdvancedItem()
            : this("")
        {
        }

        public ComboBoxAdvancedItem(string text)
            : this(text, -1)
        {
        }

        public ComboBoxAdvancedItem(string text, int imageIndex)
        {
            _text = text;
            _imageIndex = imageIndex;
        }

        public override string ToString()
        {
            return _text;
        }
    }
    public class FormUtils
    {
        [DllImport("user32.dll")]
        static extern IntPtr GetFocus();

        public static Control FocusedControl()
        {
            if (System.Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                Control focused = null;
                IntPtr handle = GetFocus();
                if (handle != IntPtr.Zero)
                {
                    focused = Control.FromHandle(handle);
                }
                return focused;
            }
            else
                return null;
        }

        public static bool IsChildFocused(Control parentcontrol)
        {
            Control focused = FocusedControl();
            if (focused==null)
                return false;
            while (focused != null)
            {
                if (focused == parentcontrol)
                    return true;
                else
                    focused = focused.Parent;
            }
            return false;
        }


    }
    public class CustomPaintControl: Control
    {
        public PaintEventHandler OnCustomPaint;
        public CustomPaintControl()
            : base()
        {
            DoubleBuffered = true;
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (OnCustomPaint != null)
                OnCustomPaint(this,e);
        }
        
    }

    public class UnSelectableButton : Button
    {
       private const int WM_MOUSEACTIVATE = 0x0021;
       const int MA_NOACTIVATE = 3;
       public UnSelectableButton()
          : base()
       {
          SetStyle(ControlStyles.Selectable, false);
       }
       protected override void WndProc(ref Message m)
       {
          if (m.Msg == WM_MOUSEACTIVATE)
             m.Result = (IntPtr)MA_NOACTIVATE;
          base.WndProc(ref m);
       }
    }

    public class ToolStripAdvanced : ToolStrip
    {
        Size FNewImageScalingSize;
        public new Size ImageScalingSize
        {
            set
            {
                FNewImageScalingSize = value;
                base.ImageScalingSize = new Size(Convert.ToInt32(Math.Round(Reportman.Drawing.GraphicUtils.DPIScaleX * FNewImageScalingSize.Width)),
                     Convert.ToInt32(Math.Round(Reportman.Drawing.GraphicUtils.DPIScaleY * FNewImageScalingSize.Height)));
            }
            get
            {
                return FNewImageScalingSize;
            }
        }
        Size FNewSize;
        public new Size Size
        {
            set
            {
                FNewSize = value;
                base.Size = new Size(Convert.ToInt32(Math.Round(Reportman.Drawing.GraphicUtils.DPIScaleX * FNewSize.Width)),
                     Convert.ToInt32(Math.Round(Reportman.Drawing.GraphicUtils.DPIScaleY * FNewSize.Height)));
            }
            get
            {
                return FNewSize;
            }
        }
    }


}
