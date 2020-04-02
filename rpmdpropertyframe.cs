using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Reportman.Designer
{
  internal partial class PropertyFrame : UserControl
  {
    public ObjectInspector inspector;
    public PropertyFrame()
    {
      InitializeComponent();

      // Add Object inspector grid
      inspector = new ObjectInspector();
      inspector.ComboSelection = this.comboselection;
      inspector.Initialize();
      inspector.Dock = DockStyle.Fill;

      panelclient.Controls.Add(inspector);
    }
    public void SetObject(DesignerInterface nobj)
    {
      inspector.SetObject(nobj);
    }

      private void comboselection_Click(object sender, EventArgs e)
      {
      }

      private void comboselection_SelectedIndexChanged(object sender, EventArgs e)
      {
          inspector.SetObjectFromCombo();
      }
  }
}
