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
 *  Copyright (c) 1994 - 2008 Toni Martir (toni@reportman.es)
 *  All Rights Reserved.
*/
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Reportman.Drawing;
using Reportman.Reporting;
using Reportman.Drawing.Forms;
using Reportman.Reporting.Forms;


namespace Reportman.Designer
{
    public partial class FrameMainDesigner : UserControl
    {
        private string CurrentFilename = "";
        private Report FReport;
        private SubReport CurrentSubReport;
      DesignerInterface CurrentInterface;
        PrintOutReportWinForms nprintdriver;
        private SortedList<SelectedItemPalette, ToolStripButton> PaletteButtons;
        private List<ToolStripItem> EditButtons;
        private List<ToolStripItem> SelectedButtons;
        private List<ToolStripItem> TwoSelectedButtons;
        private List<ToolStripItem> ThreeSelectedButtons;
        public EventHandler OnExitClic;
        public FrameMainDesigner()
        {
            InitializeComponent();

            frameproperties.inspector.OnPropertyChange += new DataRowChangeEventHandler(PropChange);

            frameproperties.inspector.Structure = fstructure;
            frameproperties.inspector.SubReportEdit = subreportedit;

            fstructure.OnReportChange += new EventHandler(StructureChange);
            fstructure.OnSelectionChange += new EventHandler(StructureSelectionChange);

            fdatadef.OnSelectionChange += new EventHandler(DataSelectionChange);

            subreportedit.AfterInsert += new EventHandler(AfterInsertDesign);
            subreportedit.AfterSelect += new EventHandler(AfterSelectDesign);
            EditButtons = new List<ToolStripItem>();
            EditButtons.Add(bsave);
            EditButtons.Add(bprint);
            EditButtons.Add(bexecute);
            EditButtons.Add(bexpression);
            EditButtons.Add(dropdownzoom);
            EditButtons.Add(bgrid);
            EditButtons.Add(barrow);
            EditButtons.Add(blabel);
            EditButtons.Add(bexpression);
            EditButtons.Add(bimage);
            EditButtons.Add(bshape);
            EditButtons.Add(bchart);
            EditButtons.Add(bbarcode);
            EditButtons.Add(bpaste);
            EditButtons.Add(bpagesetup);
            EditButtons.Add(bpreview);

            SelectedButtons = new List<ToolStripItem>();
            SelectedButtons.Add(bdelete);
            SelectedButtons.Add(bcut);
            SelectedButtons.Add(bcopy);

            TwoSelectedButtons = new List<ToolStripItem>();
            TwoSelectedButtons.Add(balignbottom);
            TwoSelectedButtons.Add(baligntop);
            TwoSelectedButtons.Add(balignright);
            TwoSelectedButtons.Add(balignleft);
            
            ThreeSelectedButtons = new List<ToolStripItem>();
            ThreeSelectedButtons.Add(bverticalgap);
            ThreeSelectedButtons.Add(bhorizontalgap);

            PaletteButtons = new SortedList<SelectedItemPalette, ToolStripButton>();
            PaletteButtons.Add(SelectedItemPalette.Arrow, barrow);
            PaletteButtons.Add(SelectedItemPalette.Label, blabel);
            PaletteButtons.Add(SelectedItemPalette.Expression, bexpression);
            PaletteButtons.Add(SelectedItemPalette.Shape, bshape);
            PaletteButtons.Add(SelectedItemPalette.Image, bimage);
            PaletteButtons.Add(SelectedItemPalette.Chart, bchart);
            PaletteButtons.Add(SelectedItemPalette.Barcode, bbarcode);
            FReport = null;
            bnew.Text = Translator.TranslateStr(40);
            bopen.Text = Translator.TranslateStr(42);
            bsave.Text = Translator.TranslateStr(46);
            bpreview.Text = Translator.TranslateStr(54);
            bexecute.Text = Translator.TranslateStr(779);
            madobepdf.Text = Translator.TranslateStr(701);
            mpdffile2.Text = Translator.TranslateStr(702);
            mmetafile.Text = Translator.TranslateStr(703);
            mmetafile2.Text = Translator.TranslateStr(1397);
            mtextfile.Text = Translator.TranslateStr(1049);
            mhtmlfile.Text = Translator.TranslateStr(1221);
            mhtml2.Text = Translator.TranslateStr(1438);
            mtextfile2.Text = Translator.TranslateStr(1260);
            msvgfile.Text = Translator.TranslateStr(1257);
            mimagefile.Text = Translator.TranslateStr(1110);
            openreportdialog.Title = Translator.TranslateStr(43);
            openreportdialog.Filter = Translator.TranslateStr(704) + '|' + "*.rep";
            tabstruc.Text = Translator.TranslateStr(1151);
            tabdata.Text = Translator.TranslateStr(131);
            tabfields.Text = Translator.TranslateStr(1150);
            msave.Text = Translator.TranslateStr(46);
            msave.ToolTipText = Translator.TranslateStr(47);
            msaveas.Text = Translator.TranslateStr(48);
            msaveas.ToolTipText = Translator.TranslateStr(49);
            msavetolibrary.Text = Translator.TranslateStr(1136);
            msavetolibrary.ToolTipText = Translator.TranslateStr(1139);
            msetuplib.Text = Translator.TranslateStr(1134);
            msetuplib.ToolTipText = Translator.TranslateStr(1137);
            msetuplib2.Text = msetuplib.Text;
            msetuplib2.ToolTipText = msetuplib2.ToolTipText;
            mopenfromlib.Text = Translator.TranslateStr(1135);
            mopenfromlib.ToolTipText = Translator.TranslateStr(1138);
            mopen.Text = Translator.TranslateStr(42);
            mopen.ToolTipText = Translator.TranslateStr(43);


            DisableMenus();

        }
        public Report Report
        {
            get
            {
                return FReport;
            }
            set
            {
                DisableMenus();
                if (FReport != null)
                {
                }
                FReport = value;
                fstructure.Report = FReport;
                fdatadef.Report = FReport;
                ffields.Report = FReport;
                CurrentSubReport = FReport.SubReports[0];
                subreportedit.SetSubReport(FReport,CurrentSubReport);
                if (FReport == null)
                    return;
                EnableMenus();
            }
        }
        public void EnableMenus()
        {
            foreach (ToolStripItem aitem in EditButtons)
            {
                aitem.Enabled = true;
            }

            mainsplit.Visible = true;
        }
        public void DisableMenus()
        {
            foreach (ToolStripItem aitem in EditButtons)
            {
                aitem.Enabled = false;
            }
            foreach (ToolStripItem aitem in SelectedButtons)
            {
                aitem.Enabled = false;
            }
            foreach (ToolStripItem aitem in TwoSelectedButtons)
            {
                aitem.Enabled = false;
            }
            foreach (ToolStripItem aitem in ThreeSelectedButtons)
            {
                aitem.Enabled = false;
            }



            mainsplit.Visible = false;
        }
        private void bnew_Click(object sender, EventArgs e)
        {
            if (!CheckSave())
                return;
            Report nrep= new Report();
            nrep.CreateNew();
            Report=nrep;
        }
        public bool CheckSave()
        {
            return true;
        }
        public void Open(string filename)
        {
            if (!CheckSave())
                return;
            Report nrep = new Report();
            nrep.LoadFromFile(filename);
            nrep.ConvertToDotNet();
            Report = nrep;
            CurrentFilename = filename;

        }
        private void bopen_Click(object sender, EventArgs e)
        {
            if (!CheckSave())
                return;
            if (openreportdialog.ShowDialog() != DialogResult.OK)
                return;
            Open(openreportdialog.FileName);
        }

        public static void Test(string filename)
        {
            FrameMainDesigner fm = new FrameMainDesigner();
            Form nform = new Form();
            fm.Parent = nform;
            fm.Dock = DockStyle.Fill;
            Report rp = new Report();
            if (filename.Length > 0)
                rp.LoadFromFile(filename);
            rp.CreateNew();
            fm.Report = rp;
            nform.ShowDialog();
        }
      public void OpenFile(string nfilename)
      {
        if (!CheckSave())
          return;
        Report nrep = new Report();
        nrep.LoadFromFile(nfilename);
        Report = nrep;
      }

      private void bpreview_Click(object sender, EventArgs e)
      {
        if (!Reportman.Reporting.Forms.ParamsForm.ShowParams(Report))
          return;

        Report.MetaFile.Clear();
        if (nprintdriver == null)
        {
          nprintdriver = new PrintOutReportWinForms(Report);
        }
        if (Report != nprintdriver.Report)
        {
          nprintdriver.Dispose();
          nprintdriver = new PrintOutReportWinForms(Report);
        }
        //PrintOutReportWinForms nprintdriver = new PrintOutReportWinForms(Report);
        nprintdriver.Preview = true;
        nprintdriver.WindowMode = PreviewWindowMode.Window;
        nprintdriver.PreviewWindow.Parent = Parent;
        nprintdriver.PreviewWindow.Dock = DockStyle.Fill;
        nprintdriver.ShowEmptyReportMessage = true;
        try
        {
          nprintdriver.Print(Report.MetaFile);
          nprintdriver.PreviewWindow.BringToFront();
        }
        catch
        {
          nprintdriver.PreviewWindow.SendToBack();
          throw;
        }

/*        previewmetafile = new PreviewMetaFile();
        SetReportEvents();
        previewmetafile.OptimizeWMF = OptimizeWMF;
        previewmetafile.MetaFile = meta;
        previewmetafile.SetDriver(this);
        previewwindow.ShowInTaskbar = ShowInTaskbar;
        previewwindow.PreviewReport(previewmetafile);

        PreviewWindow.BringToFront();
        try
        {
          PreviewWindow.PreviewReport(Report.MetaFile);
        }
        catch
        {
          PreviewWindow.SendToBack();
          throw;
        }*/
      }

      private void combozoom_SelectedIndexChanged(object sender, EventArgs e)
      {

      }

      private void mscale100_Click(object sender, EventArgs e)
      {
        ToolStripMenuItem mitem = (ToolStripMenuItem)sender;
        double DrawScale = 1.0;
        string ntext = mitem.Text.Substring(0, mitem.Text.Length - 1);
        DrawScale = System.Convert.ToDouble(ntext);
        DrawScale = DrawScale / 100.0;
        subreportedit.DrawScale = DrawScale;
        dropdownzoom.Text = mitem.Text;
      }

      private void bgrid_Click(object sender, EventArgs e)
      {
          if (GridOptions.AlterGridOptions(Report))
          {
              subreportedit.Redraw();
          }
      }
      private void barrow_Click(object sender, EventArgs e)
      {
        SelectedItemPalette nselitem = (SelectedItemPalette)System.Convert.ToInt32(((ToolStripButton)sender).Tag.ToString());
        foreach (SelectedItemPalette nkey in PaletteButtons.Keys)
        {
          PaletteButtons[nkey].Checked = (nkey == nselitem);
        }
        subreportedit.SelectedPalette = nselitem;        
      }
      private void AfterInsertDesign(object sender, EventArgs e)
      {
        barrow_Click(barrow, null);
      }
      private void AfterSelectDesign(object sender, EventArgs e)
      {
          // for band selection clear all items
          int selectedcount = 0;
          if (subreportedit.SelectedItems.Count == 1)
          {
              if (subreportedit.SelectedItems.Values[0] is PrintPosItem)
                  selectedcount = 1;
          }
          else
              selectedcount = subreportedit.SelectedItems.Count;
          switch (selectedcount)
          {
              case 0:
                foreach (ToolStripItem nitem in SelectedButtons)
                {
                    nitem.Enabled = false;
                }
                foreach (ToolStripItem nitem in TwoSelectedButtons)
                {
                    nitem.Enabled = false;
                }
                foreach (ToolStripItem nitem in ThreeSelectedButtons)
                {
                    nitem.Enabled = false;
                }
                break;
              case 1:
                foreach (ToolStripItem nitem in SelectedButtons)
                {
                    nitem.Enabled = true;
                }
                foreach (ToolStripItem nitem in TwoSelectedButtons)
                {
                    nitem.Enabled = false;
                }
                foreach (ToolStripItem nitem in ThreeSelectedButtons)
                {
                    nitem.Enabled = false;
                }
                // Set Object inspector object                    
                break;
              case 2:
                foreach (ToolStripItem nitem in SelectedButtons)
                {
                    nitem.Enabled = true;
                }
                foreach (ToolStripItem nitem in TwoSelectedButtons)
                {
                    nitem.Enabled = true;
                }
                foreach (ToolStripItem nitem in ThreeSelectedButtons)
                {
                    nitem.Enabled = false;
                }
                break;
              default:
                foreach (ToolStripItem nitem in SelectedButtons)
                {
                    nitem.Enabled = true;
                }
                foreach (ToolStripItem nitem in TwoSelectedButtons)
                {
                    nitem.Enabled = true;
                }
                foreach (ToolStripItem nitem in ThreeSelectedButtons)
                {
                    nitem.Enabled = true;
                }
                break;
          }
          SortedList<int, ReportItem> lselec = new SortedList<int, ReportItem>();
            if (PControl.SelectedIndex == 0)
            {
              foreach (int key in subreportedit.SelectedItems.Keys)
                lselec.Add(key, subreportedit.SelectedItems[key]);
                if (lselec.Count == 0)
                {
                  if (fstructure.FindSelectedNode().Tag is ReportItem)
                  lselec.Add(1, (ReportItem)fstructure.FindSelectedNode().Tag);
                }
            }
            else
            if (PControl.SelectedIndex == 1)
            {
                TreeNode nnode = fdatadef.FindSelectedNode();
                if (nnode!=null)
                    if (fdatadef.FindSelectedNode().Tag is ReportItem)
                    {
                        lselec.Add(1, (ReportItem)fdatadef.FindSelectedNode().Tag);
                    }
            }
          if (lselec.Count>0)
          {
            CurrentInterface = DesignerInterface.GetFromOject(lselec);
            frameproperties.SetObject(CurrentInterface);
          }
      }
      private void StructureChange(object sender, EventArgs args)
      {
          if (FReport.SubReports.IndexOf(CurrentSubReport) < 0)
          {
              CurrentSubReport = FReport.SubReports[0];
          }
          subreportedit.SetSubReport(FReport,CurrentSubReport);
      }
      private void StructureSelectionChange(object sender, EventArgs args)
      {
          SubReport nsubreport = fstructure.FindSelectedSubReport();
          if (subreportedit.SubReport != nsubreport)
            subreportedit.SetSubReport(FReport, nsubreport);
          if ((fstructure.FindSelectedNode().Tag is Section)) 
          {
            Section sec = (Section)fstructure.FindSelectedNode().Tag;
            subreportedit.ClearSelection();
            subreportedit.SelectedItems.Add(sec.SelectionIndex, sec);
            subreportedit.SelectedSection = sec;
            AfterSelectDesign(this, null);
            subreportedit.parentcontrol.Invalidate();
          }
        else
            if ((fstructure.FindSelectedNode().Tag is ReportItem))
            {
              ReportItem sub = (ReportItem)fstructure.FindSelectedNode().Tag;
              subreportedit.ClearSelection();
              AfterSelectDesign(this, null);
            }
      }
      private void DataSelectionChange(object sender, EventArgs args)
      {
        TreeNode nnode = fdatadef.FindSelectedNode();
        if (nnode == null)
          return;
        if ((nnode.Tag is ReportItem))
        {
          ReportItem sub = (ReportItem)nnode.Tag;
          subreportedit.ClearSelection();
          AfterSelectDesign(this, null);
        }
      }

      private void bcopy_Click(object sender, EventArgs e)
      {
        // Copy
        if (subreportedit.SelectedItems.Count == 0)
        {
          MessageBox.Show("No items selected");
          return;
        }
        if (!(subreportedit.SelectedItems.Values[0] is PrintPosItem))
        {
          MessageBox.Show("No items selected");
          return;
        }
        List<PrintPosItem> nlist = new List<PrintPosItem>();
        foreach (BandInfo nband in subreportedit.SelectedItemsBands.Values)
        {
          Section nsec = nband.Section;
          if (nsec != null)
          {
            foreach (PrintPosItem nitem in nsec.Components)
            {
              int index = subreportedit.SelectedItems.IndexOfKey(nitem.SelectionIndex);
              if (index >= 0)
              {
                nlist.Add(nitem);
              }
            }
          }
        }
        string nresult = ReportWriter.WriteComponents(nlist);
        Clipboard.SetText(nresult);
      }

      private void bpaste_Click(object sender, EventArgs e)
      {
        // Paste
        if (subreportedit.SelectedSection == null)
        {
          MessageBox.Show("Select a destination section first");
          return;
        }
        if (!Clipboard.ContainsText())
        {
          MessageBox.Show("Clipboard data not valid");
          return;
        }
        string ntext = Clipboard.GetText().Trim();
        if (ntext.Length < 10)
        {
          MessageBox.Show("Clipboard content not valid");
          return;
        }
        if (ntext.Substring(0,8)!="<SECTION")
        {
          MessageBox.Show("Clipboard content not valid");
          return;
        }
        Section sec = subreportedit.SelectedSection;
        Report nreport = new Report();
        {
          ReportReader rreader = new ReportReader(nreport);
          {
            List<PrintPosItem> nlist = rreader.ReadFromString(ntext);
            foreach (PrintPosItem xitem in nlist)
            {
              // Validate name
              if (FReport.Components.IndexOfKey(xitem.Name) >= 0)
              {
                FReport.GenerateNewName(xitem);
              }
              FReport.Components.Add(xitem.Name, xitem);
              sec.Components.Add(xitem);
              xitem.Section = sec;
            }
            subreportedit.Redraw();
            // Select recently added items
            subreportedit.ClearSelection();
            foreach (PrintPosItem xitem in nlist)
            {
              subreportedit.SelectedItems.Add(xitem.SelectionIndex, xitem);
            }
          }
        }
        BandInfo nband = subreportedit.BandsList[sec.SelectionIndex];
        subreportedit.SelectedItemsBands.Add(sec.SelectionIndex, nband);
        subreportedit.SelectPosItem();
        AfterSelectDesign(this, null);
        subreportedit.parentcontrol.Invalidate();
      }

      private void bdelete_Click(object sender, EventArgs e)
      {
        foreach (PrintItem nitem in subreportedit.SelectedItems.Values)
        {
          if (nitem is PrintPosItem)
          {
            PrintPosItem positem = (PrintPosItem)nitem;
            positem.Section.Components.Remove(positem);
            FReport.RemoveComponent(positem);            
          }
        }
        subreportedit.ClearSelection();
        foreach (BandInfo ninfo in subreportedit.SelectedItemsBands.Values)
          subreportedit.ReDrawBand(ninfo);
        AfterSelectDesign(this, null);
        subreportedit.parentcontrol.Invalidate();
      }

      private void bcut_Click(object sender, EventArgs e)
      {
        bcopy_Click(this, null);
        bdelete_Click(this, null);
      }

      private void balignleft_Click(object sender, EventArgs e)
      {
        //
        int minx = int.MaxValue;
        foreach (PrintPosItem nitem in subreportedit.SelectedItems.Values)
        {
          if (nitem.PosX < minx)
            minx = nitem.PosX;
        }
        foreach (PrintPosItem nitem in subreportedit.SelectedItems.Values)
        {
          nitem.PosX = minx;
        }
        foreach (BandInfo binfo in subreportedit.SelectedItemsBands.Values)
        {
          subreportedit.ReDrawBand(binfo);
        }
        subreportedit.parentcontrol.Invalidate();
      }

      private void balignright_Click(object sender, EventArgs e)
      {
        //
        int maxx = int.MinValue;
        foreach (PrintPosItem nitem in subreportedit.SelectedItems.Values)
        {
          if (nitem.PosX+nitem.Width > maxx)
            maxx = nitem.PosX+nitem.Width;
        }
        foreach (PrintPosItem nitem in subreportedit.SelectedItems.Values)
        {
          nitem.PosX = maxx-nitem.Width;
        }
        foreach (BandInfo binfo in subreportedit.SelectedItemsBands.Values)
        {
          subreportedit.ReDrawBand(binfo);
        }
        subreportedit.parentcontrol.Invalidate();
      }

      private void baligntop_Click(object sender, EventArgs e)
      {
        //
        int minx = int.MaxValue;
        foreach (PrintPosItem nitem in subreportedit.SelectedItems.Values)
        {
          if (nitem.PosY < minx)
            minx = nitem.PosY;
        }
        foreach (PrintPosItem nitem in subreportedit.SelectedItems.Values)
        {
          nitem.PosY = minx;
        }
        foreach (BandInfo binfo in subreportedit.SelectedItemsBands.Values)
        {
          subreportedit.ReDrawBand(binfo);
        }
        subreportedit.parentcontrol.Invalidate();
      }

      private void balignbottom_Click(object sender, EventArgs e)
      {
        //
        int maxx = int.MinValue;
        foreach (PrintPosItem nitem in subreportedit.SelectedItems.Values)
        {
          if (nitem.PosY + nitem.Height > maxx)
            maxx = nitem.PosY + nitem.Height;
        }
        foreach (PrintPosItem nitem in subreportedit.SelectedItems.Values)
        {
          nitem.PosY = maxx - nitem.Height;
        }
        foreach (BandInfo binfo in subreportedit.SelectedItemsBands.Values)
        {
          subreportedit.ReDrawBand(binfo);
        }
        subreportedit.parentcontrol.Invalidate();
      }

      private void bhorizontalgap_Click(object sender, EventArgs e)
      {
        //
        int maxx = int.MinValue;
        int minx = int.MaxValue;
        PrintPosItem firstitem = null;
        PrintPosItem lastitem = null;
        int totalwidth = 0;
        int itemcount = 0;
        SortedList<int, PrintPosItem> sorteditems = new SortedList<int, PrintPosItem>();
        foreach (PrintPosItem nitem in subreportedit.SelectedItems.Values)
        {
          if (nitem.PosX + nitem.Width > maxx)
          {
            maxx = nitem.PosX + nitem.Width;
            lastitem = nitem;
          }
          if (nitem.PosX < minx)
          {
            minx = nitem.PosX;
            firstitem = nitem;
          }
          totalwidth = totalwidth + nitem.Width;
          itemcount++;
        }
        if (firstitem == null)
          return;
        if (lastitem == null)
          return;
        if (firstitem == lastitem)
          return;
        foreach (PrintPosItem nitem in subreportedit.SelectedItems.Values)
        {
          if ((nitem != firstitem) && (nitem != lastitem))
          {
            sorteditems.Add(nitem.PosX, nitem);
          }
        }
        // Calculate free distance
        int firstpos = firstitem.PosX+firstitem.Width;
        int lastpos = lastitem.PosX;
        int freespace = lastpos - firstpos;
        foreach (PrintPosItem nitem in sorteditems.Values)
        {
          freespace = freespace - nitem.Width;
        }
        int dif = freespace / (itemcount - 1);
        foreach (PrintPosItem nitem in sorteditems.Values)
        {
          nitem.PosX = firstpos + dif;
          firstpos = nitem.PosX + nitem.Width;
        }
        foreach (BandInfo binfo in subreportedit.SelectedItemsBands.Values)
        {
          subreportedit.ReDrawBand(binfo);
        }
        subreportedit.parentcontrol.Invalidate();
      }

      private void bverticalgap_Click(object sender, EventArgs e)
      {
        //
        int maxx = int.MinValue;
        int minx = int.MaxValue;
        PrintPosItem firstitem = null;
        PrintPosItem lastitem = null;
        int totalwidth = 0;
        int itemcount = 0;
        SortedList<int, PrintPosItem> sorteditems = new SortedList<int, PrintPosItem>();
        foreach (PrintPosItem nitem in subreportedit.SelectedItems.Values)
        {
          if (nitem.PosY + nitem.Height > maxx)
          {
            maxx = nitem.PosY + nitem.Height;
            lastitem = nitem;
          }
          if (nitem.PosY < minx)
          {
            minx = nitem.PosY;
            firstitem = nitem;
          }
          totalwidth = totalwidth + nitem.Height;
          itemcount++;
        }
        if (firstitem == null)
          return;
        if (lastitem == null)
          return;
        if (firstitem == lastitem)
          return;
        foreach (PrintPosItem nitem in subreportedit.SelectedItems.Values)
        {
          if ((nitem != firstitem) && (nitem != lastitem))
          {
            sorteditems.Add(nitem.PosY, nitem);
          }
        }
        // Calculate free distance
        int firstpos = firstitem.PosY + firstitem.Height;
        int lastpos = lastitem.PosY;
        int freespace = lastpos - firstpos;
        foreach (PrintPosItem nitem in sorteditems.Values)
        {
          freespace = freespace - nitem.Height;
        }
        int dif = freespace / (itemcount - 1);
        foreach (PrintPosItem nitem in sorteditems.Values)
        {
          nitem.PosY = firstpos + dif;
          firstpos = nitem.PosY + nitem.Height;
        }
        foreach (BandInfo binfo in subreportedit.SelectedItemsBands.Values)
        {
          subreportedit.ReDrawBand(binfo);
        }
        subreportedit.parentcontrol.Invalidate();
      }

      private void bpagesetup_Click(object sender, EventArgs e)
      {
          PageSetup.ShowPageSetup(FReport, true);
      }
      public bool HasChanges()
      {
        return false;
      }
      private void bexit_Click(object sender, EventArgs e)
      {
        // Ask for saving changes
        if (OnExitClic != null)
          OnExitClic(this,new EventArgs());
        else
        {
          Parent.Controls.Remove(this);
        }
      }
      private void PropChange(object sender, DataRowChangeEventArgs args)
      {
        if (CurrentInterface == null)
          return;

        if (CurrentInterface.SelectionList.Count == 0)
          return;
        if (CurrentInterface.SelectionList.Values[0] is PrintItem)
        {          
          foreach (ReportItem aitem in CurrentInterface.SelectionList.Values)
          {
            if (aitem is Section)
            {
              subreportedit.Redraw();
            }
            else
            {
              subreportedit.ReDrawBand(subreportedit.BandsList[((PrintPosItem)aitem).Section.SelectionIndex]);
            }
          }
          subreportedit.parentcontrol.Invalidate();
          subreportedit.SelectPosItem();
        }
      }

      private void bsave_Click(object sender, EventArgs e)
      {
          FinishEdit();
          SaveChanges();
      }
      private void FinishEdit()
      {
          frameproperties.inspector.FinishEdit();
      }
      private void SaveChanges()
      {
          if (FReport == null)
              throw new Exception("Report Not readed");
          if (CurrentFilename.Length == 0)
          {
              SaveFileDialog ndiag = new SaveFileDialog();
              ndiag.Filter = Translator.TranslateStr(704) + "|" + "*.rep";
              if (ndiag.ShowDialog() != DialogResult.OK)
                  return;
              string nfilename = ndiag.FileName;
              FReport.SaveToFile(nfilename);
              CurrentFilename = nfilename;
          }
          else
            FReport.SaveToFile(CurrentFilename);
      }
    }
}
