#region Copyright
/*
 *  Report Manager:  Database Reporting tool for .Net and Mono
 *
 *     The contents of this file are subject to the MPL License
 *     with optional use of GPL or LGPL licenses.
 *     You may not use this file except in compliance with the
 *     Licenses. You may obtain copies of the Licenses at:
 *     http://reportman.sourceforge.net/license
 *
 *     Software is distributed on an "AS IS" basis,
 *     WITHOUT WARRANTY OF ANY KIND, either
 *     express or implied.  See the License for the specific
 *     language governing rights and limitations.
 *
 *  Copyright (c) 1994 - 2006 Toni Martir (toni@reportman.es)
 *  All Rights Reserved.
*/
#endregion

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Reportman.Drawing;

namespace Reportman.Drawing.Forms
{
	/// <summary>
	/// Preview Window implementation for Windows.Forms
	/// </summary>
	public class PreviewWinForms : System.Windows.Forms.Form
	{
		private System.Windows.Forms.ImageList imalist;
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.StatusBar BStatus;
		private System.Windows.Forms.StatusBarPanel StatusPage;
		private System.Windows.Forms.ContextMenu ScaleEntireMenu;
		private System.Windows.Forms.MenuItem MScale1;
		private System.Windows.Forms.MenuItem MScale2;
		private System.Windows.Forms.MenuItem MScale4;
		private System.Windows.Forms.MenuItem MScale8;
		private System.Windows.Forms.MenuItem MScale16;
		private System.Windows.Forms.MenuItem MTopDown;
		private System.Windows.Forms.MenuItem MScale32;
		private System.Windows.Forms.MenuItem MScale64;
		private System.Windows.Forms.MenuItem MScale3;
		private System.Windows.Forms.MenuItem MScale5;
		private System.Windows.Forms.MenuItem MScale6;
		private System.Windows.Forms.MenuItem MScale9;
		private System.Windows.Forms.MenuItem MScale12;
		private System.Windows.Forms.MenuItem MScale14;
		private System.Windows.Forms.MenuItem MScale15;
		private System.Windows.Forms.MenuItem MScale18;
		private PreviewMetaFile fmetapr;
		private System.Windows.Forms.StatusBarPanel BarStatusEdit;
		private System.Windows.Forms.Panel PanelTop;
		private System.Windows.Forms.Panel PanelEPage;
        private System.Windows.Forms.ToolBar BToolBar;
		private System.Windows.Forms.ToolBarButton BDivider1;
		private System.Windows.Forms.ToolBarButton BPrint;
		private System.Windows.Forms.ToolBarButton BSave;
		private System.Windows.Forms.ToolBarButton BMail;
		private System.Windows.Forms.ToolBarButton BDivider2;
		private System.Windows.Forms.ToolBarButton BScaleFull;
		private System.Windows.Forms.ToolBarButton BScaleWide;
		private System.Windows.Forms.ToolBarButton BScaleEntire;
		private System.Windows.Forms.ToolBarButton BDivider3;
		private System.Windows.Forms.ToolBarButton BZoomMinus;
		private System.Windows.Forms.ToolBarButton BZoomPlus;
		private System.Windows.Forms.ToolBarButton BDivider4;
		private System.Windows.Forms.ToolBarButton BExit;
		private System.Windows.Forms.Panel PParent;
		private MetaFileWorkProgress eventprogress;
		private PageDrawnEvent eventdrawn;
		private System.Windows.Forms.TextBox EPage;
		private System.Windows.Forms.SaveFileDialog saveFileDialog1;
		private System.Windows.Forms.ToolBarButton BParameters;
		private System.Windows.Forms.ToolBarButton BPageSetup;
		private System.Windows.Forms.ToolBarButton BDivPageSetup;
        private Button bsprior;
        private Button bsfirst;
        private Button bssearch;
        private TextBox textsearch;
        private Button bslast;
        private Button bsnext;
        private bool searchchanged;

        /// <summary>
        /// By default, the window will be modal (dialog)
        /// </summary>
		public bool ModalWindow;
		private PreviewMetaFile metapr
		{
			get
			{
				return fmetapr;
			}
			set
			{
				fmetapr = value;
				if (fmetapr == null)
					return;
				MTopDown.Checked = !fmetapr.EntireToDown;
				this.BScaleEntire.Pushed = false;
				this.BScaleFull.Pushed = false;
				this.BScaleWide.Pushed = false;
				switch (fmetapr.AutoScale)
				{
					case AutoScaleType.EntirePage:
						BScaleEntire.Pushed = true;
						break;
					case AutoScaleType.Real:
						BScaleFull.Pushed = true;
						break;
					case AutoScaleType.Wide:
						BScaleWide.Pushed = true;
						break;
				}
				switch (fmetapr.MetaFile.PreviewWindow)
				{
					case PreviewWindowStyleType.Maximized:
						WindowState = FormWindowState.Maximized;
						break;
				}
				if (metapr.OnPageSetup!=null)
				{
					BDivPageSetup.Visible=true;
					BPageSetup.Visible=true;
				}
				if (metapr.OnReportParams!=null)
				{
					BDivPageSetup.Visible=true;
					BParameters.Visible=true;
				}
                if (metapr.OnMail != null)
                {
                    BMail.Visible = true;
                }
                eventprogress = new MetaFileWorkProgress(WorkProgress);
				metapr.OnWorkProgress += eventprogress;
				eventdrawn = new PageDrawnEvent(PageDrawn);
				metapr.OnPageDrawn += eventdrawn;
				bool docancel = false;
				WorkProgress(-1, metapr.MetaFile.Pages.CurrentCount, ref docancel);
			}
		}
        /// <summary>
        /// Constructor for PreviewInForms
        /// </summary>
		public PreviewWinForms()
		{
			InitializeComponent();
            BDivPageSetup.Visible = false;
			BPageSetup.Visible=false;
			BParameters.Visible=false;
            BMail.Visible = false;
			ModalWindow = true;
			ActiveControl= EPage;
		}
        /// <summary>
        /// Preview a MetaFile, it will show the form on screen
        /// </summary>
		public void PreviewReport(PreviewMetaFile metapreview)
		{
			metapr = metapreview;
			metapr.Parent = PParent;
			metapr.BackColor = System.Drawing.SystemColors.AppWorkspace;
			metapr.Dock = System.Windows.Forms.DockStyle.Fill;
			if (ModalWindow)
				this.ShowDialog();
			else
				this.Show();
		}
        /// <summary>
        /// Release resources
        /// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#region Generated code by Desginer
		/// <summary>
		/// Necessary method for the designer.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(PreviewWinForms));
			this.imalist = new System.Windows.Forms.ImageList(this.components);
			this.ScaleEntireMenu = new System.Windows.Forms.ContextMenu();
			this.MScale1 = new System.Windows.Forms.MenuItem();
			this.MScale2 = new System.Windows.Forms.MenuItem();
			this.MScale3 = new System.Windows.Forms.MenuItem();
			this.MScale4 = new System.Windows.Forms.MenuItem();
			this.MScale5 = new System.Windows.Forms.MenuItem();
			this.MScale6 = new System.Windows.Forms.MenuItem();
			this.MScale8 = new System.Windows.Forms.MenuItem();
			this.MScale9 = new System.Windows.Forms.MenuItem();
			this.MScale12 = new System.Windows.Forms.MenuItem();
			this.MScale14 = new System.Windows.Forms.MenuItem();
			this.MScale15 = new System.Windows.Forms.MenuItem();
			this.MScale16 = new System.Windows.Forms.MenuItem();
			this.MScale18 = new System.Windows.Forms.MenuItem();
			this.MScale32 = new System.Windows.Forms.MenuItem();
			this.MScale64 = new System.Windows.Forms.MenuItem();
			this.MTopDown = new System.Windows.Forms.MenuItem();
			this.BStatus = new System.Windows.Forms.StatusBar();
			this.StatusPage = new System.Windows.Forms.StatusBarPanel();
			this.BarStatusEdit = new System.Windows.Forms.StatusBarPanel();
			this.PanelTop = new System.Windows.Forms.Panel();
			this.BToolBar = new System.Windows.Forms.ToolBar();
			this.BDivPageSetup = new System.Windows.Forms.ToolBarButton();
			this.BPageSetup = new System.Windows.Forms.ToolBarButton();
			this.BParameters = new System.Windows.Forms.ToolBarButton();
			this.BDivider1 = new System.Windows.Forms.ToolBarButton();
			this.BPrint = new System.Windows.Forms.ToolBarButton();
			this.BSave = new System.Windows.Forms.ToolBarButton();
			this.BMail = new System.Windows.Forms.ToolBarButton();
			this.BDivider2 = new System.Windows.Forms.ToolBarButton();
			this.BScaleFull = new System.Windows.Forms.ToolBarButton();
			this.BScaleWide = new System.Windows.Forms.ToolBarButton();
			this.BScaleEntire = new System.Windows.Forms.ToolBarButton();
			this.BDivider3 = new System.Windows.Forms.ToolBarButton();
			this.BZoomMinus = new System.Windows.Forms.ToolBarButton();
			this.BZoomPlus = new System.Windows.Forms.ToolBarButton();
			this.BDivider4 = new System.Windows.Forms.ToolBarButton();
			this.BExit = new System.Windows.Forms.ToolBarButton();
			this.PanelEPage = new System.Windows.Forms.Panel();
			this.bssearch = new System.Windows.Forms.Button();
			this.textsearch = new System.Windows.Forms.TextBox();
			this.bslast = new System.Windows.Forms.Button();
			this.bsnext = new System.Windows.Forms.Button();
			this.bsprior = new System.Windows.Forms.Button();
			this.bsfirst = new System.Windows.Forms.Button();
			this.EPage = new System.Windows.Forms.TextBox();
			this.PParent = new System.Windows.Forms.Panel();
			this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
			((System.ComponentModel.ISupportInitialize)(this.StatusPage)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BarStatusEdit)).BeginInit();
			this.PanelTop.SuspendLayout();
			this.PanelEPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// imalist
			// 
			this.imalist.ImageSize = new System.Drawing.Size(16, 16);
			this.imalist.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imalist.ImageStream")));
			this.imalist.TransparentColor = System.Drawing.Color.Fuchsia;
			// 
			// ScaleEntireMenu
			// 
			this.ScaleEntireMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																							this.MScale1,
																							this.MScale2,
																							this.MScale3,
																							this.MScale4,
																							this.MScale5,
																							this.MScale6,
																							this.MScale8,
																							this.MScale9,
																							this.MScale12,
																							this.MScale14,
																							this.MScale15,
																							this.MScale16,
																							this.MScale18,
																							this.MScale32,
																							this.MScale64,
																							this.MTopDown});
			this.ScaleEntireMenu.Popup += new System.EventHandler(this.ScaleEntireMenu_Popup);
			// 
			// MScale1
			// 
			this.MScale1.Checked = true;
			this.MScale1.Index = 0;
			this.MScale1.Text = "1";
			this.MScale1.Click += new System.EventHandler(this.MScale1_Click);
			// 
			// MScale2
			// 
			this.MScale2.Index = 1;
			this.MScale2.Text = "2";
			this.MScale2.Click += new System.EventHandler(this.MScale1_Click);
			// 
			// MScale3
			// 
			this.MScale3.Index = 2;
			this.MScale3.Text = "3";
			this.MScale3.Click += new System.EventHandler(this.MScale1_Click);
			// 
			// MScale4
			// 
			this.MScale4.Index = 3;
			this.MScale4.Text = "4";
			this.MScale4.Click += new System.EventHandler(this.MScale1_Click);
			// 
			// MScale5
			// 
			this.MScale5.Index = 4;
			this.MScale5.Text = "5";
			this.MScale5.Click += new System.EventHandler(this.MScale1_Click);
			// 
			// MScale6
			// 
			this.MScale6.Index = 5;
			this.MScale6.Text = "6";
			this.MScale6.Click += new System.EventHandler(this.MScale1_Click);
			// 
			// MScale8
			// 
			this.MScale8.Index = 6;
			this.MScale8.Text = "8";
			this.MScale8.Click += new System.EventHandler(this.MScale1_Click);
			// 
			// MScale9
			// 
			this.MScale9.Index = 7;
			this.MScale9.Text = "9";
			this.MScale9.Click += new System.EventHandler(this.MScale1_Click);
			// 
			// MScale12
			// 
			this.MScale12.Index = 8;
			this.MScale12.Text = "12";
			this.MScale12.Click += new System.EventHandler(this.MScale1_Click);
			// 
			// MScale14
			// 
			this.MScale14.Index = 9;
			this.MScale14.Text = "14";
			this.MScale14.Click += new System.EventHandler(this.MScale1_Click);
			// 
			// MScale15
			// 
			this.MScale15.Index = 10;
			this.MScale15.Text = "15";
			this.MScale15.Click += new System.EventHandler(this.MScale1_Click);
			// 
			// MScale16
			// 
			this.MScale16.Index = 11;
			this.MScale16.Text = "16";
			this.MScale16.Click += new System.EventHandler(this.MScale1_Click);
			// 
			// MScale18
			// 
			this.MScale18.Index = 12;
			this.MScale18.Text = "18";
			this.MScale18.Click += new System.EventHandler(this.MScale1_Click);
			// 
			// MScale32
			// 
			this.MScale32.Index = 13;
			this.MScale32.Text = "32";
			this.MScale32.Click += new System.EventHandler(this.MScale1_Click);
			// 
			// MScale64
			// 
			this.MScale64.Index = 14;
			this.MScale64.Text = "64";
			this.MScale64.Click += new System.EventHandler(this.MScale1_Click);
			// 
			// MTopDown
			// 
			this.MTopDown.Checked = true;
			this.MTopDown.Index = 15;
			this.MTopDown.Text = "->";
			this.MTopDown.Click += new System.EventHandler(this.MTopDown_Click);
			// 
			// BStatus
			// 
			this.BStatus.Location = new System.Drawing.Point(0, 380);
			this.BStatus.Name = "BStatus";
			this.BStatus.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
																					   this.StatusPage,
																					   this.BarStatusEdit});
			this.BStatus.ShowPanels = true;
			this.BStatus.Size = new System.Drawing.Size(824, 16);
			this.BStatus.TabIndex = 1;
			this.BStatus.PanelClick += new System.Windows.Forms.StatusBarPanelClickEventHandler(this.BStatus_PanelClick);
			// 
			// StatusPage
			// 
			this.StatusPage.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Contents;
			this.StatusPage.Width = 10;
			// 
			// BarStatusEdit
			// 
			this.BarStatusEdit.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Contents;
			this.BarStatusEdit.Width = 10;
			// 
			// PanelTop
			// 
			this.PanelTop.Controls.Add(this.BToolBar);
			this.PanelTop.Controls.Add(this.PanelEPage);
			this.PanelTop.Dock = System.Windows.Forms.DockStyle.Top;
			this.PanelTop.Location = new System.Drawing.Point(0, 0);
			this.PanelTop.Name = "PanelTop";
			this.PanelTop.Size = new System.Drawing.Size(824, 31);
			this.PanelTop.TabIndex = 3;
			// 
			// BToolBar
			// 
			this.BToolBar.AutoSize = false;
			this.BToolBar.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
																						this.BDivPageSetup,
																						this.BPageSetup,
																						this.BParameters,
																						this.BDivider1,
																						this.BPrint,
																						this.BSave,
																						this.BMail,
																						this.BDivider2,
																						this.BScaleFull,
																						this.BScaleWide,
																						this.BScaleEntire,
																						this.BDivider3,
																						this.BZoomMinus,
																						this.BZoomPlus,
																						this.BDivider4,
																						this.BExit});
			this.BToolBar.ButtonSize = new System.Drawing.Size(25, 25);
			this.BToolBar.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BToolBar.DropDownArrows = true;
			this.BToolBar.ImageList = this.imalist;
			this.BToolBar.Location = new System.Drawing.Point(280, 0);
			this.BToolBar.Name = "BToolBar";
			this.BToolBar.ShowToolTips = true;
			this.BToolBar.Size = new System.Drawing.Size(544, 30);
			this.BToolBar.TabIndex = 3;
			this.BToolBar.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.BToolBar_ButtonClick);
			// 
			// BDivPageSetup
			// 
			this.BDivPageSetup.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			// 
			// BPageSetup
			// 
			this.BPageSetup.ImageIndex = 7;
			// 
			// BParameters
			// 
			this.BParameters.ImageIndex = 8;
			// 
			// BDivider1
			// 
			this.BDivider1.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			// 
			// BPrint
			// 
			this.BPrint.ImageIndex = 4;
			// 
			// BSave
			// 
			this.BSave.ImageIndex = 5;
			// 
			// BMail
			// 
			this.BMail.ImageIndex = 6;
			// 
			// BDivider2
			// 
			this.BDivider2.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			// 
			// BScaleFull
			// 
			this.BScaleFull.ImageIndex = 9;
			this.BScaleFull.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
			// 
			// BScaleWide
			// 
			this.BScaleWide.ImageIndex = 10;
			this.BScaleWide.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
			// 
			// BScaleEntire
			// 
			this.BScaleEntire.DropDownMenu = this.ScaleEntireMenu;
			this.BScaleEntire.ImageIndex = 11;
			this.BScaleEntire.Style = System.Windows.Forms.ToolBarButtonStyle.DropDownButton;
			// 
			// BDivider3
			// 
			this.BDivider3.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			// 
			// BZoomMinus
			// 
			this.BZoomMinus.ImageIndex = 12;
			// 
			// BZoomPlus
			// 
			this.BZoomPlus.ImageIndex = 13;
			// 
			// BDivider4
			// 
			this.BDivider4.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			// 
			// BExit
			// 
			this.BExit.ImageIndex = 14;
			// 
			// PanelEPage
			// 
			this.PanelEPage.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.PanelEPage.Controls.Add(this.bssearch);
			this.PanelEPage.Controls.Add(this.textsearch);
			this.PanelEPage.Controls.Add(this.bslast);
			this.PanelEPage.Controls.Add(this.bsnext);
			this.PanelEPage.Controls.Add(this.bsprior);
			this.PanelEPage.Controls.Add(this.bsfirst);
			this.PanelEPage.Controls.Add(this.EPage);
			this.PanelEPage.Dock = System.Windows.Forms.DockStyle.Left;
			this.PanelEPage.Location = new System.Drawing.Point(0, 0);
			this.PanelEPage.Name = "PanelEPage";
			this.PanelEPage.Size = new System.Drawing.Size(280, 31);
			this.PanelEPage.TabIndex = 0;
			// 
			// bssearch
			// 
			this.bssearch.ImageIndex = 15;
			this.bssearch.ImageList = this.imalist;
			this.bssearch.Location = new System.Drawing.Point(246, 1);
			this.bssearch.Name = "bssearch";
			this.bssearch.Size = new System.Drawing.Size(26, 26);
			this.bssearch.TabIndex = 8;
			this.bssearch.Click += new System.EventHandler(this.bssearch_Click);
			// 
			// textsearch
			// 
			this.textsearch.Location = new System.Drawing.Point(160, 3);
			this.textsearch.Name = "textsearch";
			this.textsearch.Size = new System.Drawing.Size(82, 20);
			this.textsearch.TabIndex = 7;
			this.textsearch.Text = "";
			this.textsearch.TextChanged += new System.EventHandler(this.textsearch_TextChanged);
			// 
			// bslast
			// 
			this.bslast.ImageIndex = 3;
			this.bslast.ImageList = this.imalist;
			this.bslast.Location = new System.Drawing.Point(131, 1);
			this.bslast.Name = "bslast";
			this.bslast.Size = new System.Drawing.Size(26, 26);
			this.bslast.TabIndex = 6;
			this.bslast.Click += new System.EventHandler(this.bslast_Click);
			// 
			// bsnext
			// 
			this.bsnext.ImageIndex = 2;
			this.bsnext.ImageList = this.imalist;
			this.bsnext.Location = new System.Drawing.Point(104, 1);
			this.bsnext.Name = "bsnext";
			this.bsnext.Size = new System.Drawing.Size(26, 26);
			this.bsnext.TabIndex = 4;
			this.bsnext.Click += new System.EventHandler(this.bsnext_Click);
			// 
			// bsprior
			// 
			this.bsprior.ImageIndex = 1;
			this.bsprior.ImageList = this.imalist;
			this.bsprior.Location = new System.Drawing.Point(28, 1);
			this.bsprior.Name = "bsprior";
			this.bsprior.Size = new System.Drawing.Size(26, 26);
			this.bsprior.TabIndex = 2;
			this.bsprior.Click += new System.EventHandler(this.bsprior_Click);
			// 
			// bsfirst
			// 
			this.bsfirst.ImageIndex = 0;
			this.bsfirst.ImageList = this.imalist;
			this.bsfirst.Location = new System.Drawing.Point(1, 1);
			this.bsfirst.Name = "bsfirst";
			this.bsfirst.Size = new System.Drawing.Size(26, 26);
			this.bsfirst.TabIndex = 1;
			this.bsfirst.Click += new System.EventHandler(this.bsfirst_Click);
			// 
			// EPage
			// 
			this.EPage.Location = new System.Drawing.Point(55, 3);
			this.EPage.Name = "EPage";
			this.EPage.Size = new System.Drawing.Size(46, 20);
			this.EPage.TabIndex = 3;
			this.EPage.Text = "";
			this.EPage.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.EPage.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.EPage_KeyPress);
			this.EPage.Validating += new System.ComponentModel.CancelEventHandler(this.EPage_Validating);
			// 
			// PParent
			// 
			this.PParent.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PParent.Location = new System.Drawing.Point(0, 31);
			this.PParent.Name = "PParent";
			this.PParent.Size = new System.Drawing.Size(824, 349);
			this.PParent.TabIndex = 4;
			this.PParent.Paint += new System.Windows.Forms.PaintEventHandler(this.PParent_Paint);
			// 
			// saveFileDialog1
			// 
			this.saveFileDialog1.Filter = "PDF File|*.pdf|PDF File (Uncompressed)|*.pdf";
			// 
			// PreviewWinForms
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(824, 396);
			this.Controls.Add(this.PParent);
			this.Controls.Add(this.PanelTop);
			this.Controls.Add(this.BStatus);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.KeyPreview = true;
			this.Name = "PreviewWinForms";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PreviewWinForms_KeyDown);
			this.Load += new System.EventHandler(this.PreviewWinForms_Load);
			this.Layout += new System.Windows.Forms.LayoutEventHandler(this.PreviewWinForms_Layout);
			this.Closed += new System.EventHandler(this.PreviewWinForms_Closed);
			((System.ComponentModel.ISupportInitialize)(this.StatusPage)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BarStatusEdit)).EndInit();
			this.PanelTop.ResumeLayout(false);
			this.PanelEPage.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void BToolBar_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			if (e.Button == BExit)
			{
				Close();
			}
			if (sender == bsfirst)
			{
				fmetapr.Page = 0;
				return;
			}
            if (sender == bsprior)
			{
				fmetapr.PriorPage();
				return;
			}
            if (sender == bsnext)
			{
				fmetapr.NextPage();
				return;
			}
            if (sender == bslast)
			{
				fmetapr.LastPage();
				return;
			}
            if (sender == bssearch)
            {
                FindNext();
                return;
            }
            if (e.Button == BScaleFull)
			{
				fmetapr.AutoScale = AutoScaleType.Real;
				BScaleFull.Pushed = true;
				BScaleEntire.Pushed = false;
				BScaleWide.Pushed = false;
				return;
			}
			if (e.Button == this.BPageSetup)
			{
				if (fmetapr.OnPageSetup())
				{

				}
				return;
			}
			if (e.Button == this.BParameters)
			{
				if (fmetapr.OnReportParams())
				{

				}
				return;
			}
			if (e.Button == BScaleWide)
			{
				fmetapr.AutoScale = AutoScaleType.Wide;
				BScaleFull.Pushed = false;
				BScaleEntire.Pushed = false;
				BScaleWide.Pushed = true;
				return;
			}
			if (e.Button == BScaleEntire)
			{
				fmetapr.AutoScale = AutoScaleType.EntirePage;
				BScaleFull.Pushed = false;
				BScaleEntire.Pushed = true;
				BScaleWide.Pushed = false;
				return;
			}
			if (e.Button == BZoomMinus)
			{
				fmetapr.PreviewScale = fmetapr.PreviewScale - 0.1F;
				BScaleFull.Pushed = false;
				BScaleEntire.Pushed = false;
				BScaleWide.Pushed = false;
				return;
			}
			if (e.Button == BZoomPlus)
			{
				fmetapr.PreviewScale = fmetapr.PreviewScale + 0.1F;
				BScaleFull.Pushed = false;
				BScaleEntire.Pushed = false;
				BScaleWide.Pushed = false;
				return;
			}
			if (e.Button == BPrint)
			{
				PrintOutWinForms prw = new PrintOutWinForms();
				//				prw.Preview=true;
                if ((fmetapr.MetaFile.PrinterFonts == PrinterFontsType.Recalculate) ||
                     (fmetapr.MetaFile.PrinterFonts == PrinterFontsType.Always))
                {
                    fmetapr.MetaFile.Clear();
                    fmetapr.MetaFile.BeginPrint(prw);
                }
				prw.ShowPrintDialog = true;
				prw.Print(fmetapr.MetaFile);
				return;
			}
			if (e.Button == BSave)
			{
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
				{
					PrintOutPDF prpdf = new PrintOutPDF();
                    if ((fmetapr.MetaFile.PrinterFonts == PrinterFontsType.Recalculate) ||
                         (fmetapr.MetaFile.PrinterFonts == PrinterFontsType.Always))
                    {
                        fmetapr.MetaFile.Clear();
                        fmetapr.MetaFile.BeginPrint(prpdf);
                    }
                    prpdf.Compressed = (saveFileDialog1.FilterIndex == 0);
					prpdf.FileName = saveFileDialog1.FileName;
					prpdf.Print(fmetapr.MetaFile);
				}
				return;
			}
            if (e.Button == BMail)
            {
                // Update mail params
                if (fmetapr.OnMail != null)
                {

//                    string file_name;
                    string filename;
                    PrintOutPDF prpdf = new PrintOutPDF();
                    if ((fmetapr.MetaFile.PrinterFonts == PrinterFontsType.Recalculate) ||
                         (fmetapr.MetaFile.PrinterFonts == PrinterFontsType.Always))
                    {
                        fmetapr.MetaFile.Clear();
                        fmetapr.MetaFile.BeginPrint(prpdf);
                    }
                    filename = System.IO.Path.GetTempFileName();
                    try
                    {
                        prpdf.Compressed = true;
                        prpdf.FileName = filename;
                        prpdf.Print(fmetapr.MetaFile);
                        fmetapr.OnMail(filename);
                    }
                    finally
                    {
                        System.IO.File.Delete(prpdf.FileName);
                    }
                }
                return;
            }
		}
		private void RefreshStatus()
		{
			BStatus.Panels[1].Text = Translator.TranslateStr(1416)+": " + 
				(fmetapr.Page + 1).ToString("#,##0");
			BStatus.Panels[0].Text = Translator.TranslateStr(1414)+": " 
			+ (fmetapr.MetaFile.Pages.CurrentCount).ToString("#,##0");
			if (fmetapr.PagesDrawn > 1)
				BStatus.Panels[1].Text = BStatus.Panels[1].Text + 
					" "+Translator.TranslateStr(1415)+": "+
					fmetapr.PagesDrawn.ToString(",##0");
			this.BScaleEntire.Pushed = false;
			this.BScaleFull.Pushed = false;
			this.BScaleWide.Pushed = false;
			switch (fmetapr.AutoScale)
			{
				case AutoScaleType.EntirePage:
					BScaleEntire.Pushed = true;
					break;
				case AutoScaleType.Real:
					BScaleFull.Pushed = true;
					break;
				case AutoScaleType.Wide:
					BScaleWide.Pushed = true;
					break;
			}
		}
		private void RefreshPage()
		{
			if (EPage.Text != (fmetapr.Page + 1).ToString())
				EPage.Text = (fmetapr.Page + 1).ToString();
//			if (fmetapr.MetaFile.Finished)
//				EPage.Maximum = fmetapr.MetaFile.Pages.CurrentCount;
			if (fmetapr.CanFocus)
				if (!fmetapr.Focused)
					fmetapr.Focus();
			RefreshStatus();
		}

		private void PreviewWinForms_Closed(object sender, System.EventArgs e)
		{
			if (fmetapr != null)
			{
				if (fmetapr.MetaFile != null)
				{
					fmetapr.MetaFile.OnWorkProgress -= eventprogress;
					fmetapr.OnPageDrawn -= eventdrawn;
					fmetapr.MetaFile.StopWork();
				}
			}
			fmetapr = null;
		}

		private void BStatus_PanelClick(object sender, System.Windows.Forms.StatusBarPanelClickEventArgs e)
		{

		}

		private void ScaleEntireMenu_Popup(object sender, System.EventArgs e)
		{

		}
		private void MScale1_Click(object sender, System.EventArgs e)
		{
			MScale1.Checked = false;
			MScale2.Checked = false;
			MScale3.Checked = false;
			MScale4.Checked = false;
			MScale5.Checked = false;
			MScale6.Checked = false;
			MScale8.Checked = false;
			MScale9.Checked = false;
			MScale12.Checked = false;
			MScale14.Checked = false;
			MScale15.Checked = false;
			MScale16.Checked = false;
			MScale18.Checked = false;
			MScale32.Checked = false;
			MScale64.Checked = false;
			((MenuItem)sender).Checked = true;
			fmetapr.EntirePageCount = System.Convert.ToInt32(((MenuItem)sender).Text);
			fmetapr.AutoScale = AutoScaleType.EntirePage;
			BScaleFull.Pushed = false;
			BScaleEntire.Pushed = true;
			BScaleWide.Pushed = false;
		}

		private void MTopDown_Click(object sender, System.EventArgs e)
		{
			MTopDown.Checked = !MTopDown.Checked;
			fmetapr.EntireToDown = !MTopDown.Checked;
		}
		private void PageDrawn(PreviewMetaFile prm)
		{
			RefreshPage();
		}
		private void WorkProgress(int records, int pagecount, ref bool docancel)
		{
//			if (fmetapr.MetaFile.Finished)
//				EPage.Maximum = fmetapr.MetaFile.Pages.CurrentCount;
			string atext = "";
			if (records > 0)
				atext = atext + Translator.TranslateStr(684)+": " + 
				records.ToString("##,##0");
			atext = atext + " "+Translator.TranslateStr(1414)+": " + 
				(pagecount).ToString("#,##0");
			BStatus.Panels[0].Text = atext;
			BStatus.Refresh();
		}

		private void PreviewWinForms_Load(object sender, System.EventArgs e)
		{
#if REPMAN_NORESOURCES
			SuspendLayout();
			try
			{
				this.BToolBar.AutoSize=true;
				this.BToolBar.Wrappable=true;
				this.BToolBar.TextAlign=ToolBarTextAlign.Right;
				this.bsfirst.Text="First";
				this.bsprior.Text="Prior";
				this.bsnext.Text="Next";
				this.bslast.Text="Last";
				this.bssearch.Text="Find";
				this.BScaleEntire.Text="Scale Auto";
				this.BScaleWide.Text="Scale Wide";
				this.BScaleFull.Text="Scale Real";
				this.BPrint.Text="Print";
				this.BSave.Text="Save";
				this.BMail.Text="Mail";
				this.BZoomMinus.Text="Zoom -";
				this.BZoomPlus.Text="Zoom +";
				this.BParameters.Text="Parameters";
				this.BPageSetup.Text="Page Setup";
			}
			finally
			{
				ResumeLayout();
			}
#else
            //			this.BFirst.Text = Translator.TranslateStr(220);
//			this.bsfirst.ToolTipText = Translator.TranslateStr(221);
//			this.bsprior.ToolTipText = Translator.TranslateStr(223);
//			this.bsnext.ToolTipText = Translator.TranslateStr(225);
//			this.bslast.ToolTipText = Translator.TranslateStr(227);
//            this.bssearch.ToolTipText = Translator.TranslateStr(1435);
            this.BScaleEntire.ToolTipText = Translator.TranslateStr(233);
			this.BScaleWide.ToolTipText = Translator.TranslateStr(231);
			this.BScaleFull.ToolTipText = Translator.TranslateStr(229);
			this.BPrint.ToolTipText = Translator.TranslateStr(53);
			this.BSave.ToolTipText = Translator.TranslateStr(216);
			this.BMail.ToolTipText = Translator.TranslateStr(1231);
			this.BZoomMinus.ToolTipText = Translator.TranslateStr(235);
			this.BZoomPlus.ToolTipText = Translator.TranslateStr(237);
			this.BExit.ToolTipText = Translator.TranslateStr(212);
            this.BParameters.ToolTipText = Translator.TranslateStr(136);
            this.BPageSetup.ToolTipText = Translator.TranslateStr(51);
#endif
            this.saveFileDialog1.Filter = Translator.TranslateStr(703)+"|*.rpmf|"+
                Translator.TranslateStr(701)+"|*.pdf|"+
                Translator.TranslateStr(702)+"|*.pdf";
		}

		private void PreviewWinForms_KeyDown(object sender, 
			System.Windows.Forms.KeyEventArgs e)
		{
			switch (e.KeyCode)
			{
				case Keys.PageDown:
					if (e.Control)
						BToolBar_ButtonClick(bslast, new ToolBarButtonClickEventArgs(BPageSetup));
					else
                        BToolBar_ButtonClick(bsnext, new ToolBarButtonClickEventArgs(BPageSetup));

					e.Handled = true;
					break;
				case Keys.PageUp:
					if (e.Control)
						BToolBar_ButtonClick(bsfirst,
                            new ToolBarButtonClickEventArgs(BPageSetup));
					else
						BToolBar_ButtonClick(bsprior,
                            new ToolBarButtonClickEventArgs(BPageSetup));
					e.Handled = true;
					break;
			}
		}

		private void PreviewWinForms_Layout(object sender, 
			System.Windows.Forms.LayoutEventArgs e)
		{
			if (this.WindowState == FormWindowState.Maximized)
				BStatus.SizingGrip = false;
			else
				BStatus.SizingGrip = true;
#if REPMAN_NORESOURCES
			this.PanelTop.Height=BToolBar.Height;
#endif
		}

		private void EPage_Validating(object sender, 
			System.ComponentModel.CancelEventArgs e)
		{
			if (fmetapr != null)
			{
				if (fmetapr.Page != (System.Convert.ToInt32(EPage.Text) - 1))
				{
					fmetapr.Page = System.Convert.ToInt32(EPage.Text.ToString()) - 1;
				}
			}		
		}

		private void EPage_KeyPress(object sender, 
			System.Windows.Forms.KeyPressEventArgs e)
		{
			if (e.KeyChar==(char)13)
				EPage_Validating(EPage,null);
		}


        private void textsearch_TextChanged(object sender, EventArgs e)
        {
            searchchanged = true;
        }
        private void FindNext()
        {
            int pageindex;
            if (searchchanged)
            {
                fmetapr.MetaFile.DoSearch(textsearch.Text);
                pageindex = fmetapr.MetaFile.NextPageFound(-1);
                searchchanged = false;
            }
            else
            {
                pageindex = fmetapr.MetaFile.NextPageFound(fmetapr.Page + fmetapr.PagesDrawn - 1);
            }
            if (pageindex == fmetapr.Page)
                fmetapr.RefreshPage();
            else
                fmetapr.Page = pageindex;
        }

        private void bsfirst_Click(object sender, EventArgs e)
        {
            BToolBar_ButtonClick(bsfirst, new ToolBarButtonClickEventArgs(BPageSetup));
        }
        private void bsprior_Click(object sender, EventArgs e)
        {
            BToolBar_ButtonClick(bsprior, new ToolBarButtonClickEventArgs(BPageSetup));
        }

        private void bsnext_Click(object sender, EventArgs e)
        {
            BToolBar_ButtonClick(bsnext, new ToolBarButtonClickEventArgs(BPageSetup));
        }

        private void bslast_Click(object sender, EventArgs e)
        {
            BToolBar_ButtonClick(bslast, new ToolBarButtonClickEventArgs(BPageSetup));
        }

        private void bssearch_Click(object sender, EventArgs e)
        {
            BToolBar_ButtonClick(bssearch, new ToolBarButtonClickEventArgs(BPageSetup));
        }

        private void PreviewWinForms_Shown(object sender, EventArgs e)
        {

        }

		private void PParent_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
		
		}
	}
}
