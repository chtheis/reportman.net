#region Copyright
/* Code based on Magic Library tab control
 * Crownwood.Magic.Controls.TabControl 
 * 
 * 
 * 
 * 
 * 
 * 
 */
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;

namespace Reportman.Drawing.Forms
{
   public class TabPageAdvanced:PanelAdvanced
   {
              // Enumeration of property change events
        public enum Property
        {
            Title,
            Control,
            ImageIndex,
            ImageList,
            Icon,
            Selected,
        }

        // Declare the property change event signature
        public delegate void PropChangeHandler(TabPageAdvanced page, Property prop, object oldValue);

        // Public events
        public event PropChangeHandler PropertyChanged;

        // Instance fields
        protected string _title;
        protected Control _control;
        protected int _imageIndex;
        protected ImageList _imageList;
        protected Icon _icon;
        protected bool _selected;
		protected Control _startFocus;
		protected bool _shown;

        public TabPageAdvanced()
        {
            InternalConstruct("Page", null, null, -1, null);
        }

        public TabPageAdvanced(string title)
        {
            InternalConstruct(title, null, null, -1, null);
        }

        public TabPageAdvanced(string title, Control control)
        {
            InternalConstruct(title, control, null, -1, null);
        }
			
        public TabPageAdvanced(string title, Control control, int imageIndex)
        {
            InternalConstruct(title, control, null, imageIndex, null);
        }

        public TabPageAdvanced(string title, Control control, ImageList imageList, int imageIndex)
        {
            InternalConstruct(title, control, imageList, imageIndex, null);
        }

        public TabPageAdvanced(string title, Control control, Icon icon)
        {
            InternalConstruct(title, control, null, -1, icon);
        }

        protected void InternalConstruct(string title, 
                                         Control control, 
                                         ImageList imageList, 
                                         int imageIndex,
                                         Icon icon)
        {
            // Assign parameters to internal fields
            _title = title;
            _control = control;
            _imageIndex = imageIndex;
            _imageList = imageList;
            _icon = icon;

            // Appropriate defaults
            _selected = false;
			_startFocus = null;
        }

        [DefaultValue("Page")]
        [Localizable(true)]
        public string Title
        {
            get { return _title; }
		   
            set 
            { 
                if (_title != value)
                {
                    string oldValue = _title;
                    _title = value; 

                    OnPropertyChanged(Property.Title, oldValue);
                }
            }
        }

        [DefaultValue(null)]
        public Control Control
        {
            get { return _control; }

            set 
            { 
                if (_control != value)
                {
                    Control oldValue = _control;
                    _control = value; 

                    OnPropertyChanged(Property.Control, oldValue);
                }
            }
        }

        [DefaultValue(-1)]
        public int ImageIndex
        {
            get { return _imageIndex; }
		
            set 
            { 
                if (_imageIndex != value)
                {
                    int oldValue = _imageIndex;
                    _imageIndex = value; 

                    OnPropertyChanged(Property.ImageIndex, oldValue);
                }
            }
        }

        [DefaultValue(null)]
        public ImageList ImageList
        {
            get { return _imageList; }
		
            set 
            { 
                if (_imageList != value)
                {
                    ImageList oldValue = _imageList;
                    _imageList = value; 

                    OnPropertyChanged(Property.ImageList, oldValue);
                }
            }
        }

        [DefaultValue(null)]
        public Icon Icon
        {
            get { return _icon; }
		
            set 
            { 
                if (_icon != value)
                {
                    Icon oldValue = _icon;
                    _icon = value; 

                    OnPropertyChanged(Property.Icon, oldValue);
                }
            }
        }

        [DefaultValue(true)]
        public bool Selected
        {
            get { return _selected; }

            set
            {
                if (_selected != value)
                {
                    bool oldValue = _selected;
                    _selected = value;

                    OnPropertyChanged(Property.Selected, oldValue);
                }
            }
        }

        [DefaultValue(null)]
        public Control StartFocus
        {
            get { return _startFocus; }
            set { _startFocus = value; }
        }

        public virtual void OnPropertyChanged(Property prop, object oldValue)
        {
            // Any attached event handlers?
            if (PropertyChanged != null)
                PropertyChanged(this, prop, oldValue);
        }
        
        internal bool Shown
        {
            get { return _shown; }
            set { _shown = value; }
        }

   }
}
