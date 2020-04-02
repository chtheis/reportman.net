using System;
using System.Drawing;
using System.Collections;
using System.IO;
using System.ComponentModel;
using System.Windows.Forms;
using System.Reflection;
using Reportman.Drawing;

namespace Reportman.Compact
{
	/// <summary>
	/// Preview form for Compact framework
	/// </summary>
	public class PreviewCompact : System.Windows.Forms.Form
	{
		private static Assembly ThisAssembly = System.Reflection.Assembly.GetExecutingAssembly();

		private System.Windows.Forms.ToolBar BToolBar;
		private System.Windows.Forms.ToolBarButton BFirst;
		private System.Windows.Forms.ToolBarButton BPrior;
		private System.Windows.Forms.ToolBarButton BNext;
		private System.Windows.Forms.ToolBarButton BLast;
		private System.Windows.Forms.ToolBarButton BPrint;
		private System.Windows.Forms.ToolBarButton BScaleFull;
		private System.Windows.Forms.ToolBarButton BScaleWide;
		private System.Windows.Forms.ToolBarButton BScaleEntire;
		private System.Windows.Forms.ContextMenu MScale;
		private System.Windows.Forms.MenuItem MScale16;
		private System.Windows.Forms.MenuItem MScale8;
		private System.Windows.Forms.MenuItem MScale6;
		private System.Windows.Forms.MenuItem MScale5;
		private System.Windows.Forms.MenuItem MScale4;
		private System.Windows.Forms.MenuItem MScale3;
		private System.Windows.Forms.MenuItem MScale2;
		private System.Windows.Forms.MenuItem MScale1;
		private System.Windows.Forms.MenuItem MTopDown;
		private System.Windows.Forms.MenuItem MScale32;
		private System.Windows.Forms.MenuItem MScale64;
		private System.Windows.Forms.MenuItem MScale128;
		private System.Windows.Forms.ImageList imalist;
		private PreviewMetaFile fmetapr;
		private PageDrawnEvent eventdrawn;
		private	EventHandler eventprogress;
		private PreviewMetaFile metapr
		{
			get
			{
				return fmetapr;
			}
			set
			{
				fmetapr=value;
				if (fmetapr==null)
					return;
				MTopDown.Checked=!fmetapr.EntireToDown;
				this.BScaleEntire.Pushed=false;
				this.BScaleFull.Pushed=false;
				this.BScaleWide.Pushed=false;
				switch (fmetapr.AutoScale)
				{
					case AutoScaleType.EntirePage:
						BScaleEntire.Pushed=true;
						break;
					case AutoScaleType.Real:
						BScaleFull.Pushed=true;
						break;
					case AutoScaleType.Wide:
						BScaleWide.Pushed=true;
						break;
				}
				switch (fmetapr.metafile.PreviewWindow)
				{
					case PreviewWindowStyleType.Maximized:
						WindowState=FormWindowState.Maximized;
						break;
				}
				eventprogress=new EventHandler(WorkProgress);
				metapr.OnWorkProgress+=eventprogress;
				eventdrawn=new PageDrawnEvent(PageDrawn);
				metapr.OnPageDrawn+=eventdrawn;
			}
		}
		private void WorkProgress(object Sender,System.EventArgs e)
		{
//			if (fmetapr.metafile.Finished)
//				EPage.Maximum=fmetapr.metafile.Pages.CurrentCount;
			string atext="Page:"+(fmetapr.Page+1);
			if (metapr.progress_records>0)
				atext=atext+"R:"+metapr.progress_records.ToString("##,##0");
			atext=atext+" P:"+(metapr.progress_pagecount).ToString("#,##0");
			// cancel
			// metapar.progress_cancel=?
			Text=atext;
		}
		private void PageDrawn(PreviewMetaFile prm)
		{
			RefreshPage();
		}
		private void RefreshPage()
		{
//			if (EPage.Value!=fmetapr.Page+1)
//				EPage.Value=fmetapr.Page+1;
//			if (fmetapr.metafile.Finished)
//				EPage.Maximum=fmetapr.metafile.Pages.CurrentCount;
			RefreshStatus();
		}
		private void RefreshStatus()
		{
			Text="Page:"+(fmetapr.Page+1).ToString("#,##0")
			 +" Count:"+(fmetapr.metafile.Pages.CurrentCount).ToString("#,##0");
			if (fmetapr.PagesDrawn>1)
				Text=Text+" Drawn: "+
					fmetapr.PagesDrawn.ToString(",##0");
			BScaleEntire.Pushed = false;
			BScaleFull.Pushed = false;
			BScaleWide.Pushed = false;
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

		public PreviewCompact()
		{
			//
			// Needed by Designer
			//
			InitializeComponent();

		}

		/// <summary>
		/// Clean up resources
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			base.Dispose( disposing );
		}

		#region Generated code by Designer
		/// <summary>
		/// Needed method by designer
		/// </summary>
		private void InitializeComponent()
		{
			this.BToolBar = new System.Windows.Forms.ToolBar();
			this.BFirst = new System.Windows.Forms.ToolBarButton();
			this.BPrior = new System.Windows.Forms.ToolBarButton();
			this.BNext = new System.Windows.Forms.ToolBarButton();
			this.BLast = new System.Windows.Forms.ToolBarButton();
			this.BPrint = new System.Windows.Forms.ToolBarButton();
			this.BScaleFull = new System.Windows.Forms.ToolBarButton();
			this.BScaleWide = new System.Windows.Forms.ToolBarButton();
			this.BScaleEntire = new System.Windows.Forms.ToolBarButton();
			this.MScale = new System.Windows.Forms.ContextMenu();
			this.MScale16 = new System.Windows.Forms.MenuItem();
			this.MScale8 = new System.Windows.Forms.MenuItem();
			this.MScale6 = new System.Windows.Forms.MenuItem();
			this.MScale5 = new System.Windows.Forms.MenuItem();
			this.MScale4 = new System.Windows.Forms.MenuItem();
			this.MScale3 = new System.Windows.Forms.MenuItem();
			this.MScale2 = new System.Windows.Forms.MenuItem();
			this.MScale1 = new System.Windows.Forms.MenuItem();
			this.MTopDown = new System.Windows.Forms.MenuItem();
			this.MScale32 = new System.Windows.Forms.MenuItem();
			this.MScale64 = new System.Windows.Forms.MenuItem();
			this.MScale128 = new System.Windows.Forms.MenuItem();
			this.imalist = new System.Windows.Forms.ImageList();
			// 
			// BToolBar
			// 
			this.BToolBar.Buttons.Add(this.BFirst);
			this.BToolBar.Buttons.Add(this.BPrior);
			this.BToolBar.Buttons.Add(this.BNext);
			this.BToolBar.Buttons.Add(this.BLast);
			this.BToolBar.Buttons.Add(this.BPrint);
			this.BToolBar.Buttons.Add(this.BScaleFull);
			this.BToolBar.Buttons.Add(this.BScaleWide);
			this.BToolBar.Buttons.Add(this.BScaleEntire);
			this.BToolBar.ImageList = this.imalist;
			this.BToolBar.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.BToolBar_ButtonClick_1);
			// 
			// BFirst
			// 
			this.BFirst.ImageIndex = 0;
			// 
			// BPrior
			// 
			this.BPrior.ImageIndex = 1;
			// 
			// BNext
			// 
			this.BNext.ImageIndex = 2;
			// 
			// BLast
			// 
			this.BLast.ImageIndex = 3;
			// 
			// BPrint
			// 
			this.BPrint.ImageIndex = 4;
			// 
			// BScaleFull
			// 
			this.BScaleFull.ImageIndex = 6;
			// 
			// BScaleWide
			// 
			this.BScaleWide.ImageIndex = 6;
			// 
			// BScaleEntire
			// 
			this.BScaleEntire.DropDownMenu = this.MScale;
			this.BScaleEntire.ImageIndex = 7;
			this.BScaleEntire.Style = System.Windows.Forms.ToolBarButtonStyle.DropDownButton;
			// 
			// MScale
			// 
			this.MScale.MenuItems.Add(this.MTopDown);
			this.MScale.MenuItems.Add(this.MScale128);
			this.MScale.MenuItems.Add(this.MScale64);
			this.MScale.MenuItems.Add(this.MScale32);
			this.MScale.MenuItems.Add(this.MScale16);
			this.MScale.MenuItems.Add(this.MScale8);
			this.MScale.MenuItems.Add(this.MScale6);
			this.MScale.MenuItems.Add(this.MScale5);
			this.MScale.MenuItems.Add(this.MScale4);
			this.MScale.MenuItems.Add(this.MScale3);
			this.MScale.MenuItems.Add(this.MScale2);
			this.MScale.MenuItems.Add(this.MScale1);
			// 
			// MScale16
			// 
			this.MScale16.Text = "16";
			this.MScale16.Click += new System.EventHandler(this.MScale1_Click);
			// 
			// MScale8
			// 
			this.MScale8.Text = "8";
			this.MScale8.Click += new System.EventHandler(this.MScale1_Click);
			// 
			// MScale6
			// 
			this.MScale6.Text = "6";
			this.MScale6.Click += new System.EventHandler(this.MScale1_Click);
			// 
			// MScale5
			// 
			this.MScale5.Text = "5";
			this.MScale5.Click += new System.EventHandler(this.MScale1_Click);
			// 
			// MScale4
			// 
			this.MScale4.Text = "4";
			this.MScale4.Click += new System.EventHandler(this.MScale1_Click);
			// 
			// MScale3
			// 
			this.MScale3.Text = "3";
			this.MScale3.Click += new System.EventHandler(this.MScale1_Click);
			// 
			// MScale2
			// 
			this.MScale2.Text = "2";
			this.MScale2.Click += new System.EventHandler(this.MScale1_Click);
			// 
			// MScale1
			// 
			this.MScale1.Text = "1";
			this.MScale1.Click += new System.EventHandler(this.MScale1_Click);
			// 
			// MTopDown
			// 
			this.MTopDown.Text = "->";
			this.MTopDown.Click += new System.EventHandler(this.MTopDown_Click);
			// 
			// MScale32
			// 
			this.MScale32.Text = "32";
			this.MScale32.Click += new System.EventHandler(this.MScale1_Click);
			// 
			// MScale64
			// 
			this.MScale64.Text = "64";
			this.MScale64.Click += new System.EventHandler(this.MScale1_Click);
			// 
			// MScale128
			// 
			this.MScale128.Text = "128";
			this.MScale128.Click += new System.EventHandler(this.MScale1_Click);
			// 
			// imalist
			// 
			this.imalist.ImageSize = new System.Drawing.Size(16, 16);
			// 
			// PreviewCompact
			// 
			this.ClientSize = new System.Drawing.Size(282, 264);
			this.Controls.Add(this.BToolBar);
			this.Load += new System.EventHandler(this.PreviewCompact_Load);

		}
		#endregion

		public void PreviewReport(PreviewMetaFile metapreview)
		{
			metapr=metapreview;
			metapr.Width=Width;
			metapr.Height=Height;
			metapr.Parent=this;
			this.ShowDialog();
		}


		private void BToolBar_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
		}

		private void PreviewWinForms_Closed(object sender, System.EventArgs e)
		{
			fmetapr=null;
		}

		private void MScale1_Click(object sender, System.EventArgs e)
		{
			MScale1.Checked=false;
			MScale2.Checked=false;
			MScale3.Checked=false;
			MScale4.Checked=false;
			MScale5.Checked=false;
			MScale6.Checked=false;
			MScale8.Checked=false;
			MScale16.Checked=false;
			MScale32.Checked=false;
			MScale64.Checked=false;
			MScale128.Checked=false;
			((MenuItem)sender).Checked=true;
			fmetapr.EntirePageCount=System.Convert.ToInt32(((MenuItem)sender).Text);
			fmetapr.AutoScale=AutoScaleType.EntirePage;
			BScaleFull.Pushed=false;
			BScaleEntire.Pushed=true;
			BScaleWide.Pushed=false;
		}

		private void MTopDown_Click(object sender, System.EventArgs e)
		{
			MTopDown.Checked=!MTopDown.Checked;
			fmetapr.EntireToDown=!MTopDown.Checked;
		}
		private Icon GetIcon(string iconname)
		{
			if (Path.GetExtension(iconname)==string.Empty)
				iconname=iconname+".ico";
			string resname=ThisAssembly.GetName().Name + "." + iconname;
			Stream iconStream = ThisAssembly.GetManifestResourceStream(resname);
			Icon nicon=new Icon(iconStream);
			return nicon;
		}
		private void PreviewCompact_Load(object sender, System.EventArgs e)
		{
			imalist.Images.Add(GetIcon("first"));		
			imalist.Images.Add(GetIcon("prior"));		
			imalist.Images.Add(GetIcon("next"));		
			imalist.Images.Add(GetIcon("last"));		
			imalist.Images.Add(GetIcon("print"));		
			imalist.Images.Add(GetIcon("scalef"));		
			imalist.Images.Add(GetIcon("scalew"));		
			imalist.Images.Add(GetIcon("scalee"));		
		}

		private void BToolBar_ButtonClick_1(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			if (e.Button==BFirst)
			{
				fmetapr.Page=0;
				return;
			}
			if (e.Button==BPrior)
			{
				fmetapr.PriorPage();
				return;
			}
			if (e.Button==BNext)
			{
				fmetapr.NextPage();
				return;
			}
			if (e.Button==BLast)
			{
				fmetapr.LastPage();
				return;
			}
			if (e.Button==BScaleFull)
			{	
				fmetapr.AutoScale=AutoScaleType.Real;
				BScaleFull.Pushed=true;
				BScaleEntire.Pushed=false;
				BScaleWide.Pushed=false;
				return;
			}
			if (e.Button==BScaleWide)
			{	
				fmetapr.AutoScale=AutoScaleType.Wide;
				BScaleFull.Pushed=false;
				BScaleEntire.Pushed=false;
				BScaleWide.Pushed=true;
				return;
			}
			if (e.Button==BScaleEntire)
			{	
				fmetapr.AutoScale=AutoScaleType.EntirePage;
				BScaleFull.Pushed=false;
				BScaleEntire.Pushed=true;
				BScaleWide.Pushed=false;
				return;
			}
			/*			if (e.Button==BZoomMinus)
						{
							fmetapr.PreviewScale=fmetapr.PreviewScale-0.1F;
							BScaleFull.Pushed=false;
							BScaleEntire.Pushed=false;
							BScaleWide.Pushed=false;
							return;
						}
						if (e.Button==BZoomPlus)
						{
							fmetapr.PreviewScale=fmetapr.PreviewScale+0.1F;
							BScaleFull.Pushed=false;
							BScaleEntire.Pushed=false;
							BScaleWide.Pushed=false;
							return;
						}*/		
		}

	}
}
