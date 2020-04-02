namespace Reportman.Designer
{
  partial class PropertyFrame
  {
    /// <summary> 
    /// Variable del diseñador requerida.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary> 
    /// Limpiar los recursos que se estén utilizando.
    /// </summary>
    /// <param name="disposing">true si los recursos administrados se deben eliminar; false en caso contrario, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Código generado por el Diseñador de componentes

    /// <summary> 
    /// Método necesario para admitir el Diseñador. No se puede modificar
    /// el contenido del método con el editor de código.
    /// </summary>
    private void InitializeComponent()
    {
        this.comboselection = new System.Windows.Forms.ComboBox();
        this.paneltop = new System.Windows.Forms.Panel();
        this.panelclient = new System.Windows.Forms.Panel();
        this.paneltop.SuspendLayout();
        this.SuspendLayout();
        // 
        // comboselection
        // 
        this.comboselection.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.comboselection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.comboselection.FormattingEnabled = true;
        this.comboselection.Location = new System.Drawing.Point(3, 3);
        this.comboselection.Name = "comboselection";
        this.comboselection.Size = new System.Drawing.Size(240, 21);
        this.comboselection.TabIndex = 0;
        this.comboselection.SelectedIndexChanged += new System.EventHandler(this.comboselection_SelectedIndexChanged);
        this.comboselection.Click += new System.EventHandler(this.comboselection_Click);
        // 
        // paneltop
        // 
        this.paneltop.Controls.Add(this.comboselection);
        this.paneltop.Dock = System.Windows.Forms.DockStyle.Top;
        this.paneltop.Location = new System.Drawing.Point(0, 0);
        this.paneltop.Name = "paneltop";
        this.paneltop.Size = new System.Drawing.Size(246, 27);
        this.paneltop.TabIndex = 0;
        // 
        // panelclient
        // 
        this.panelclient.Dock = System.Windows.Forms.DockStyle.Fill;
        this.panelclient.Location = new System.Drawing.Point(0, 27);
        this.panelclient.Name = "panelclient";
        this.panelclient.Size = new System.Drawing.Size(246, 270);
        this.panelclient.TabIndex = 0;
        // 
        // PropertyFrame
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Controls.Add(this.panelclient);
        this.Controls.Add(this.paneltop);
        this.Name = "PropertyFrame";
        this.Size = new System.Drawing.Size(246, 297);
        this.paneltop.ResumeLayout(false);
        this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.ComboBox comboselection;
      private System.Windows.Forms.Panel paneltop;
      private System.Windows.Forms.Panel panelclient;
  }
}
