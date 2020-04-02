using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Reportman.Drawing.Forms
{
	/// <summary>
	/// Preview Window implementation for Windows.Forms
	/// </summary>
    public partial class PreviewWinForms2 : Form
    {
        private bool searchchanged;
        private MetaFileWorkProgress eventprogress;
        private PageDrawnEvent eventdrawn;
        private PreviewMetaFile fmetapr;

        /// <summary>
        /// Constructor for PreviewInForms
        /// </summary>
        public PreviewWinForms2()
        {
            InitializeComponent();
            BDivPageSetup.Visible = false;
            BPageSetup.Visible = false;
            BParameters.Visible = false;
            BMail.Visible = false;
            ModalWindow = true;
            //ActiveControl = EPage;
        }
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
/*				this.BScaleEntire.Pressed = false;
				this.BScaleFull.Checked = false;
				this.BScaleWide.Checked = false;
				switch (fmetapr.AutoScale)
				{
					case AutoScaleType.EntirePage:
						BScaleEntire.Checked = true;
						break;
					case AutoScaleType.Real:
						BScaleFull.Checked= true;
						break;
					case AutoScaleType.Wide:
						BScaleWide.Checked = true;
						break;
				}
*/				switch (fmetapr.MetaFile.PreviewWindow)
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

        private void MScale1_Click(object sender, EventArgs e)
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
            ((ToolStripMenuItem)sender).Checked = true;
            fmetapr.EntirePageCount = System.Convert.ToInt32(((ToolStripMenuItem)sender).Text);
            fmetapr.AutoScale = AutoScaleType.EntirePage;
            BScaleFull.Checked = false;
//            BScaleEntire.Checked = true;
            BScaleWide.Checked = false;
        }

        private void RefreshStatus()
        {
            BarStatusEdit.Text = Translator.TranslateStr(1416) + ": " +
                (fmetapr.Page + 1).ToString("#,##0");
            StatusPage.Text = Translator.TranslateStr(1414) + ": "
            + (fmetapr.MetaFile.Pages.CurrentCount).ToString("#,##0");
            if (fmetapr.PagesDrawn > 1)
                BarStatusEdit.Text = BarStatusEdit.Text +
                    " " + Translator.TranslateStr(1415) + ": " +
                    fmetapr.PagesDrawn.ToString(",##0");
/*            this.BScaleEntire.Checked = false;
            this.BScaleFull.Checked = false;
            this.BScaleWide.Checked = false;
            switch (fmetapr.AutoScale)
            {
                case AutoScaleType.EntirePage:
                    //BScaleEntire.Pushed = true;
                    break;
                case AutoScaleType.Real:
                    BScaleFull.Checked = true;
                    break;
                case AutoScaleType.Wide:
                    BScaleWide.Checked = true;
                    break;
            }
*/
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
                atext = atext + Translator.TranslateStr(684) + ": " +
                records.ToString("##,##0");
            atext = atext + " " + Translator.TranslateStr(1414) + ": " +
                (pagecount).ToString("#,##0");
            StatusPage.Text = atext;
            if (mainstatus.Visible)
                mainstatus.Refresh();
        }
        private void DisableButtons()
        {
            bsfirst.Enabled = false;
            bslast.Enabled = false;
            bsnext.Enabled = false;
            bsprior.Enabled = false;
            BPrint.Enabled = false;
            BMail.Enabled = false;
            BSave.Enabled = false;
            bssearch.Enabled = false;
            EPage.Enabled = false;
            textsearch.Enabled = false;
            BScaleFull.Enabled = false;
            BScaleWide.Enabled = false;
            BScaleEntire.Enabled = false;
            BZoomMinus.Enabled = false;
            BZoomPlus.Enabled = false;
            metapr.Visible = false;
        }
        private void EnableButtons()
        {
            bsfirst.Enabled = true;
            bslast.Enabled = true;
            bsnext.Enabled = true;
            bsprior.Enabled = true;
            BPrint.Enabled = true;
            BMail.Enabled = true;
            BSave.Enabled = true;
            bssearch.Enabled = true;
            EPage.Enabled = true;
            textsearch.Enabled = true;
            BScaleWide.Enabled = true;
            BScaleEntire.Enabled = true;
            BZoomMinus.Enabled = true;
            BZoomPlus.Enabled = true;

            metapr.Visible = true;
        }
        private void bsfirst_Click(object sender, EventArgs e)
        {
            if (sender == BExit)
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
            if (sender == BScaleFull)
            {
                fmetapr.AutoScale = AutoScaleType.Real;
//                BScaleFull.Checked = true;
//                BScaleEntire.Pushed = false;
//                BScaleWide.Checked = false;
                return;
            }
            if (sender == this.BPageSetup)
            {
                try
                {
                    if (fmetapr.OnPageSetup())
                    {
                        if (fmetapr.MetaFile.Empty)
                            DisableButtons();
                        else
                            EnableButtons();
                    }
                }
                catch
                {
                    DisableButtons();
                    throw;
                }
            return;
            }
            if (sender == this.BParameters)
            {
                try
                {
                    if (fmetapr.OnReportParams())
                    {
                        if (fmetapr.MetaFile.Empty)
                            DisableButtons();
                        else
                            EnableButtons();
                    }
                }
                catch
                {
                    DisableButtons();
                    throw;
                }
                return;
            }
            if (sender == BScaleWide)
            {
                fmetapr.AutoScale = AutoScaleType.Wide;
//                BScaleFull.Checked = false;
//                BScaleEntire.Pushed = false;
//                BScaleWide.Checked = true;
                return;
            }
            if (sender == BScaleEntire)
            {
                fmetapr.AutoScale = AutoScaleType.EntirePage;
//                BScaleFull.Checked = false;
//                BScaleEntire.Pushed = true;
//                BScaleWide.Checked = false;
                return;
            }
            if (sender == BZoomMinus)
            {
                fmetapr.PreviewScale = fmetapr.PreviewScale - 0.1F;
//                BScaleFull.Checked = false;
//                BScaleEntire.Pushed = false;
//                BScaleWide.Checked = false;
                return;
            }
            if (sender == BZoomPlus)
            {
                fmetapr.PreviewScale = fmetapr.PreviewScale + 0.1F;
//                BScaleFull.Checked = false;
//                BScaleEntire.Checked = false;
//                BScaleWide.Checked = false;
                return;
            }
            if (sender == BPrint)
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
            if (sender == BSave)
            {
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    switch (saveFileDialog1.FilterIndex)
                    {
                        case 1:
                        case 2:
                        case 3:
                            PrintOutPDF prpdf = new PrintOutPDF();
                            if ((fmetapr.MetaFile.PrinterFonts == PrinterFontsType.Recalculate) ||
                                 (fmetapr.MetaFile.PrinterFonts == PrinterFontsType.Always))
                            {
                                fmetapr.MetaFile.Clear();
                                fmetapr.MetaFile.BeginPrint(prpdf);
                            }
                            prpdf.Compressed = (saveFileDialog1.FilterIndex == 0);
                            if (saveFileDialog1.FilterIndex != 0)
                            {
                                string nfilename = saveFileDialog1.FileName;
                                string nextension = System.IO.Path.GetExtension(nfilename).ToUpper();
                                if (nextension != ".PDF")
                                    nfilename = nfilename + ".pdf";
                                prpdf.FileName = nfilename;
                                prpdf.Print(fmetapr.MetaFile);
                            }
                            else
                            {
                                fmetapr.MetaFile.SaveToFile(saveFileDialog1.FileName,true);
                            }
                            break;
                        case 4:
                        case 5:
                            PrintOutExcel prex = new PrintOutExcel();
                            prex.OneSheet = (saveFileDialog1.FilterIndex==5);
                            prex.FileName = saveFileDialog1.FileName;

                            if ((fmetapr.MetaFile.PrinterFonts == PrinterFontsType.Recalculate) ||
                                 (fmetapr.MetaFile.PrinterFonts == PrinterFontsType.Always))
                            {
                                fmetapr.MetaFile.Clear();
                                fmetapr.MetaFile.BeginPrint(prex);
                            }
                            prex.Print(fmetapr.MetaFile);
                            break;
                    }
                }
                return;
            }
            if (sender == BMail)
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

        private void EPage_Validating(object sender, CancelEventArgs e)
        {
            if (fmetapr != null)
            {
                string pages = EPage.Text.Trim();
                if (pages.Length > 0)
                {
                    bool valid=true;
                    for (int i = 0; i < pages.Length; i++)
                    {
                        if (!Char.IsDigit(pages[i]))
                        {
                            valid = false;
                            break;
                        }
                    }
                    if (valid)
                    {
                        int newpage=System.Convert.ToInt32(EPage.Text) - 1;
                        if (newpage <= 0)
                            newpage = 1;
                        if (fmetapr.Page != newpage)
                        {
                            fmetapr.Page = newpage;
                        }
                    }
                }
            }
        }

        private void EPage_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
                EPage_Validating(EPage, null);
        }

        private void PreviewWinForms2_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.PageDown:
                    if (e.Control)
                        bsfirst_Click(bslast, new EventArgs());
                    else
                        bsfirst_Click(bsnext, new EventArgs());

                    e.Handled = true;
                    break;
                case Keys.PageUp:
                    if (e.Control)
                        bsfirst_Click(bsfirst, new EventArgs());
                    else
                        bsfirst_Click(bsprior, new EventArgs());
                    e.Handled = true;
                    break;
            }
        }

        private void PreviewWinForms2_Load(object sender, EventArgs e)
        {
//            this.BFirst.Text = Translator.TranslateStr(220);
			this.bsfirst.ToolTipText = Translator.TranslateStr(221);
			this.bsprior.ToolTipText = Translator.TranslateStr(223);
			this.bsnext.ToolTipText = Translator.TranslateStr(225);
			this.bslast.ToolTipText = Translator.TranslateStr(227);
            this.bssearch.ToolTipText = Translator.TranslateStr(1435);
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
            this.saveFileDialog1.Filter = Translator.TranslateStr(703)+"|*.rpmf|"+
                Translator.TranslateStr(701)+"|*.pdf|"+
                Translator.TranslateStr(702)+"|*.pdf|"+
                // Excel file
                Translator.TranslateStr(1031)+"|*.xls|"+
                // Excel file one sheet
                Translator.TranslateStr(1342)+"|*.xls";

        }

        private void textsearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
                bsfirst_Click(bssearch, new EventArgs());
        }

        private void EPage_Leave(object sender, EventArgs e)
        {
            EPage_Validating(EPage,new CancelEventArgs());
        }

      private void PreviewWinForms2_FormClosed(object sender, FormClosedEventArgs e)
      {
        if (fmetapr != null)
        {
          try
          {
            fmetapr.Dispose();
          }
          finally
          {
            fmetapr = null;
          }
        }
      }

    }
}