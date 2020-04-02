namespace Reportman.Designer
{
    partial class FrameStructure
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrameStructure));
            this.maincontainer = new System.Windows.Forms.ToolStripContainer();
            this.topbar = new System.Windows.Forms.ToolStrip();
            this.baddstruc = new System.Windows.Forms.ToolStripSplitButton();
            this.mstrucaddpheader = new System.Windows.Forms.ToolStripMenuItem();
            this.mstrucaddpfooter = new System.Windows.Forms.ToolStripMenuItem();
            this.mstrucaddgroup = new System.Windows.Forms.ToolStripMenuItem();
            this.mstrucaddsubreport = new System.Windows.Forms.ToolStripMenuItem();
            this.mstrucadddetail = new System.Windows.Forms.ToolStripMenuItem();
            this.maincontainer.TopToolStripPanel.SuspendLayout();
            this.maincontainer.SuspendLayout();
            this.topbar.SuspendLayout();
            this.SuspendLayout();
            // 
            // maincontainer
            // 
            // 
            // maincontainer.ContentPanel
            // 
            this.maincontainer.ContentPanel.Size = new System.Drawing.Size(336, 277);
            this.maincontainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.maincontainer.Location = new System.Drawing.Point(0, 0);
            this.maincontainer.Name = "maincontainer";
            this.maincontainer.Size = new System.Drawing.Size(336, 302);
            this.maincontainer.TabIndex = 0;
            this.maincontainer.Text = "toolStripContainer1";
            // 
            // maincontainer.TopToolStripPanel
            // 
            this.maincontainer.TopToolStripPanel.Controls.Add(this.topbar);
            // 
            // topbar
            // 
            this.topbar.Dock = System.Windows.Forms.DockStyle.None;
            this.topbar.ImageScalingSize = new System.Drawing.Size(19, 19);
            this.topbar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.baddstruc});
            this.topbar.Location = new System.Drawing.Point(3, 0);
            this.topbar.Name = "topbar";
            this.topbar.Size = new System.Drawing.Size(45, 25);
            this.topbar.TabIndex = 0;
            // 
            // baddstruc
            // 
            this.baddstruc.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.baddstruc.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mstrucaddpheader,
            this.mstrucaddpfooter,
            this.mstrucaddgroup,
            this.mstrucaddsubreport,
            this.mstrucadddetail});
            this.baddstruc.Image = ((System.Drawing.Image)(resources.GetObject("baddstruc.Image")));
            this.baddstruc.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.baddstruc.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.baddstruc.Name = "baddstruc";
            this.baddstruc.Size = new System.Drawing.Size(35, 22);
            this.baddstruc.Text = "toolStripSplitButton1";
            // 
            // mstrucaddpheader
            // 
            this.mstrucaddpheader.Name = "mstrucaddpheader";
            this.mstrucaddpheader.Size = new System.Drawing.Size(205, 22);
            this.mstrucaddpheader.Text = "Page header";
            // 
            // mstrucaddpfooter
            // 
            this.mstrucaddpfooter.Name = "mstrucaddpfooter";
            this.mstrucaddpfooter.Size = new System.Drawing.Size(205, 22);
            this.mstrucaddpfooter.Text = "Page footer";
            // 
            // mstrucaddgroup
            // 
            this.mstrucaddgroup.Name = "mstrucaddgroup";
            this.mstrucaddgroup.Size = new System.Drawing.Size(205, 22);
            this.mstrucaddgroup.Text = "Group header and footer";
            // 
            // mstrucaddsubreport
            // 
            this.mstrucaddsubreport.Name = "mstrucaddsubreport";
            this.mstrucaddsubreport.Size = new System.Drawing.Size(205, 22);
            this.mstrucaddsubreport.Text = "Subreport";
            // 
            // mstrucadddetail
            // 
            this.mstrucadddetail.Name = "mstrucadddetail";
            this.mstrucadddetail.Size = new System.Drawing.Size(205, 22);
            this.mstrucadddetail.Text = "Detail";
            // 
            // FrameStructure
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.maincontainer);
            this.Name = "FrameStructure";
            this.Size = new System.Drawing.Size(336, 302);
            this.maincontainer.TopToolStripPanel.ResumeLayout(false);
            this.maincontainer.TopToolStripPanel.PerformLayout();
            this.maincontainer.ResumeLayout(false);
            this.maincontainer.PerformLayout();
            this.topbar.ResumeLayout(false);
            this.topbar.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStripContainer maincontainer;
        private System.Windows.Forms.ToolStrip topbar;
        private System.Windows.Forms.ToolStripSplitButton baddstruc;
        private System.Windows.Forms.ToolStripMenuItem mstrucaddpheader;
        private System.Windows.Forms.ToolStripMenuItem mstrucaddpfooter;
        private System.Windows.Forms.ToolStripMenuItem mstrucaddgroup;
        private System.Windows.Forms.ToolStripMenuItem mstrucaddsubreport;
        private System.Windows.Forms.ToolStripMenuItem mstrucadddetail;
    }
}
