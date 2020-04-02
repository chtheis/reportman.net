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
using System.Text;
using System.IO;
using System.Drawing;
using Reportman.Drawing;
using Reportman.Reporting;

namespace Reportman.Designer
{
    internal class DesignerInterface
    {
      public virtual void SetPropertyMulti(string pname, Variant newvalue)
      {
        if (SelectionList.Count == 1)
        {
          SetProperty(pname, newvalue);
        }
        else
        {
          foreach (ReportItem nitem in SelectionList.Values)
          {
            SetItem(nitem);
            SetProperty(pname, newvalue);
          }
        }
      }

      public Variant GetPropertyMulti(string pname)
      {
        if (SelectionList.Count == 1)
        {
          return GetProperty(pname);
        }
        else
        {
          Variant nvar = new Variant();
          bool firstpass = true;
          foreach (ReportItem nitem in SelectionList.Values)
          {
            SetItem(nitem);
            if (firstpass)
            {
              nvar = GetProperty(pname);
              firstpass = false;
            }
            else
              if (!nvar.Equals(GetProperty(pname)))
              {
                return new Variant();
              }
          }
          return nvar;
        }
      }
      public static DesignerInterface GetFromOject(SortedList<int, ReportItem> litems)
      {
        string baseclass = "";
        foreach (ReportItem nitem in litems.Values)
        {
          if (baseclass.Length==0)
          {
            baseclass = nitem.GetType().ToString();
            baseclass = baseclass.Substring(20, baseclass.Length - 20);
          }
          else
          {
            string atypename = nitem.GetType().ToString();
            atypename = atypename.Substring(20, atypename.Length - 20);
            if (atypename != baseclass)
            {

              switch (atypename)
              {
                case "LabelItem":
                  if ((nitem.ClassName=="ExpressionItem") || (nitem.ClassName == "ChartItem"))
                    baseclass = "PrintItemText";
                  break;
                case "ExpressionItem":
                  if ((nitem.ClassName=="LabelItem") || (nitem.ClassName == "ChartItem"))
                    baseclass = "PrintItemText";
                  break;
              case "ChartItem":
                  if ((nitem.ClassName=="LabelItem") || (nitem.ClassName == "ExpressionItem"))
                    baseclass = "PrintItemText";
                  break;
                case "PrintItemText":
                  if ((nitem.ClassName != "LabelItem") && (nitem.ClassName != "ExpressionItem")
                     && (nitem.ClassName != "ChartItem"))
                    baseclass = "PrintPosItem";
                  break;
                case "SubReport":
                  baseclass = "SubReport";
                  break;
                case "DatabaseInfo":
                  baseclass = "DatabaseInfo";
                  break;
                case "DataInfo":
                  baseclass = "DataInfo";
                  break;
                default:
                  baseclass = "PrintPosItem";
                  break;
              }
            }
          }
          if (baseclass == "PrintPosItem")
            break;
        }
        DesignerInterface nresult = null;
        switch (baseclass)
        {
          case "LabelItem":
            nresult = new DesignerInterfaceText(litems);
            break;
          case "ExpressionItem":
            nresult = new DesignerInterfaceText(litems);
            break;
          case "ChartItem":
            nresult = new DesignerInterfaceText(litems);
            break;
          case "ShapeItem":
            nresult = new DesignerInterfaceShape(litems);
            break;
          case "PrintPosItem":
            nresult = new DesignerInterfaceSizePos(litems);
            break;
          case "BarcodeItem":
            nresult = new DesignerInterfaceSizePos(litems);
            break;
          case "Section":
            nresult = new DesignerInterfaceSection(litems);
            break;
          case "SubReport":
            nresult = new DesignerInterfaceSubReport(litems);
            break;
          case "DatabaseInfo":
            nresult = new DesignerInterfaceDbInfo(litems);
            break;
          case "DataInfo":
            nresult = new DesignerInterfaceDataInfo(litems);
            break;
          case "Param":
            nresult = new DesignerInterfaceParam(litems);
            break;
          case "ImageItem":
            nresult = new DesignerInterfaceImage(litems);
            break;
        }
        nresult.SelectionClassName = baseclass;
        return nresult;
      }
        private SortedList<int,ReportItem> FReportItems;
      public SortedList<int, ReportItem> SelectionList
      {
        get { return FReportItems; }
      }
      public string SelectionClassName;
        private ReportItem FReportItemObject;
        public DesignerInterface(SortedList<int, ReportItem> repitem)
        {
          FReportItems = repitem;
          if (repitem.Count == 1)
              FReportItemObject = repitem.Values[0];
        }
        public ReportItem ReportItemObject { get { return FReportItemObject; } }
        public virtual void GetProperties(Strings lnames,Strings ltypes, Variants lvalues, Strings lhints, Strings lcat)
        {
        }
        public virtual void GetPropertyValues(string pname, Strings lpossiblevalues)
        {
            throw new Exception(Translator.TranslateStr(675)+":"+pname);
        }
        public virtual void SetItem(ReportItem nitem)
        {
        }
        public virtual Variant GetProperty(string pname)
        {
            throw new Exception(Translator.TranslateStr(674) + ":" + pname);
        }
        public virtual MemoryStream GetProperty(string pname, ref MemoryStream memvalue)
        {
            throw new Exception(Translator.TranslateStr(674) + ":" + pname);
        }
        public virtual void SetProperty(string pname, Variant newvalue)
        {
            throw new Exception(Translator.TranslateStr(674) + ":" + pname);
        }
        public virtual void SetProperty(string pname, MemoryStream newvalue)
        {
            throw new Exception(Translator.TranslateStr(642) + ":" + pname);
        }
    }
    internal class DesignerInterfaceParam : DesignerInterface
    {
        private Param FParamItem;
        public DesignerInterfaceParam(SortedList<int, ReportItem> repitem):base (repitem)
        {
            if (repitem.Count == 1)
              FParamItem = (Param)repitem.Values[0];
        }
        public override void SetItem(ReportItem nitem)
        {
            FParamItem = (Param)nitem;
            base.SetItem(nitem);
        }
        public override void GetProperties(Strings lnames, Strings ltypes, Variants lvalues, Strings lhints, Strings lcat)
        {
            lnames.Clear();
            ltypes.Clear();
            lhints.Clear();
            lcat.Clear();
            if (lvalues != null)
                lvalues.Clear();
            // Name
            lnames.Add(Translator.TranslateStr(544));
            ltypes.Add(Translator.TranslateStr(200));
            lhints.Add("refparam.html");
            lcat.Add(Translator.TranslateStr(722));
            if (lvalues != null)
                lvalues.Add(FParamItem.Alias);
            // Param type
            lnames.Add(Translator.TranslateStr(193));
            ltypes.Add(Translator.TranslateStr(569));
            lhints.Add("refparam.html");
            lcat.Add(Translator.TranslateStr(722));
            if (lvalues != null)
                lvalues.Add((int)FParamItem.ParamType);
            // Value
            lnames.Add(Translator.TranslateStr(194));
            ltypes.Add(Translator.TranslateStr(135));
            lhints.Add("refparam.html");
            lcat.Add(Translator.TranslateStr(722));
            if (lvalues != null)
                lvalues.Add(FParamItem.Value);
            // Visible
            lnames.Add(Translator.TranslateStr(183));
            ltypes.Add(Translator.TranslateStr(568));
            lhints.Add("refparam.html");
            lcat.Add(Translator.TranslateStr(722));
            if (lvalues != null)
                lvalues.Add(FParamItem.Visible);
            // Description
            lnames.Add(Translator.TranslateStr(197));
            ltypes.Add(Translator.TranslateStr(200));
            lhints.Add("refparam.html");
            lcat.Add(Translator.TranslateStr(722));
            if (lvalues != null)
                lvalues.Add(FParamItem.Description);
            // Hint
            lnames.Add(Translator.TranslateStr(1382));
            ltypes.Add(Translator.TranslateStr(200));
            lhints.Add("refparam.html");
            lcat.Add(Translator.TranslateStr(722));
            if (lvalues != null)
                lvalues.Add(FParamItem.Hint);
            // Validation
            lnames.Add(Translator.TranslateStr(1401));
            ltypes.Add(Translator.TranslateStr(200));
            lhints.Add("refparam.html");
            lcat.Add(Translator.TranslateStr(722));
            if (lvalues != null)
                lvalues.Add(FParamItem.Validation);
            // Error Message
            lnames.Add(Translator.TranslateStr(1403));
            ltypes.Add(Translator.TranslateStr(200));
            lhints.Add("refparam.html");
            lcat.Add(Translator.TranslateStr(722));
            if (lvalues != null)
                lvalues.Add(FParamItem.ErrorMessage);
            // Datasets
            lnames.Add(Translator.TranslateStr(198));
            ltypes.Add(Translator.TranslateStr(198));
            lhints.Add("refparam.html");
            lcat.Add(Translator.TranslateStr(722));
            if (lvalues != null)
                lvalues.Add(FParamItem.Datasets.ToSemiColon());
        }
        public override void GetPropertyValues(string pname, Strings lpossiblevalues)
        {
            // Alignment
            if (pname == Translator.TranslateStr(193))
            {
                lpossiblevalues.Add(Translator.TranslateStr(557)); // String
                lpossiblevalues.Add(Translator.TranslateStr(559)); // Integer
                lpossiblevalues.Add(Translator.TranslateStr(887)); // Float
                lpossiblevalues.Add(Translator.TranslateStr(556)); // Currency
                lpossiblevalues.Add(Translator.TranslateStr(888)); // Date
                lpossiblevalues.Add(Translator.TranslateStr(889)); // DateTime
                lpossiblevalues.Add(Translator.TranslateStr(890)); // Time
                lpossiblevalues.Add(Translator.TranslateStr(891)); // Bool
                lpossiblevalues.Add(Translator.TranslateStr(942)); // Expression Before open
                lpossiblevalues.Add(Translator.TranslateStr(943)); // Expression After open
                lpossiblevalues.Add(Translator.TranslateStr(951)); // Sustitution parameter
                lpossiblevalues.Add(Translator.TranslateStr(961)); // Param list
                lpossiblevalues.Add(Translator.TranslateStr(1368)); // Param list multiple
                lpossiblevalues.Add(Translator.TranslateStr(1422)); // string sustituion expression
                lpossiblevalues.Add(Translator.TranslateStr(1442)); // string sustitution list
                lpossiblevalues.Add(Translator.TranslateStr(1443)); // Initial expression
                lpossiblevalues.Add(Translator.TranslateStr(575));// Unkonwn
                return;
            }
        }

        public override void SetProperty(string pname, Variant newvalue)
        {
            // Name
            if (pname == Translator.TranslateStr(544))
            {
                FParamItem.Name = newvalue.ToString();
                return;
            }
            // Param type
            if (pname == Translator.TranslateStr(193))
            {
                FParamItem.ParamType = (ParamType)(int)newvalue;
                return;
            }
            // Value
            if (pname == Translator.TranslateStr(194))
            {
                FParamItem.Value = newvalue;
                return;
            }
            // Visible
            if (pname == Translator.TranslateStr(183))
            {
                FParamItem.Visible = (bool)newvalue;
                return;
            }
            // Description
            if (pname == Translator.TranslateStr(197))
            {
                FParamItem.Description = newvalue.ToString();
                return;
            }
            // Hint
            if (pname == Translator.TranslateStr(1382))
            {
                FParamItem.Description = newvalue.ToString();
                return;
            }
            // Validation
            if (pname == Translator.TranslateStr(1401))
            {
                FParamItem.Validation = newvalue.ToString();
                return;
            }
            // Error message
            if (pname == Translator.TranslateStr(1403))
            {
                FParamItem.ErrorMessage = newvalue.ToString();
                return;
            }
            // Datasets
            if (pname == Translator.TranslateStr(198))
            {
                FParamItem.Datasets = Strings.FromSemiColon(newvalue); ;
                return;
            }
            base.SetProperty(pname, newvalue);
        }
        public override Variant GetProperty(string pname)
        {
            // Name
            if (pname == Translator.TranslateStr(544))
            {
                return FParamItem.Name;
            }
            // Param type
            if (pname == Translator.TranslateStr(193))
            {
                return (int)FParamItem.ParamType;
            }
            // Value
            if (pname == Translator.TranslateStr(194))
            {
                return FParamItem.Value;
            }
            // Visible
            if (pname == Translator.TranslateStr(183))
            {
                return FParamItem.Visible;
            }
            // Description
            if (pname == Translator.TranslateStr(197))
            {
                return FParamItem.Description;
            }
            // Hint
            if (pname == Translator.TranslateStr(1382))
            {
                return FParamItem.Hint;
            }
            // Validation
            if (pname == Translator.TranslateStr(1401))
            {
                return FParamItem.Validation;
            }
            // Error message
            if (pname == Translator.TranslateStr(1403))
            {
                return FParamItem.ErrorMessage;
            }
            // Datasets
            if (pname == Translator.TranslateStr(198))
            {
                return FParamItem.Datasets.ToSemiColon();
            }
            return base.GetProperty(pname);
        }
    }
    internal class DesignerInterfaceSize:DesignerInterface
    {
        private PrintItem FPrintItemObject;
        public DesignerInterfaceSize(SortedList<int, ReportItem> repitem):base (repitem)
        {
            if (repitem.Count == 1)
              FPrintItemObject = (PrintItem)repitem.Values[0];
        }
        public PrintItem PrintItemObject { get { return FPrintItemObject; } }
        private int FLeft;
        private int FTop;
        private int FWidth;
        private int FHeight;
        public int Left { get { return FLeft; } }
        public int Top { get { return FTop; } }
        public int Width { get { return FWidth; } }
        public int Height { get { return FHeight; } }
        private double FScale;
        public double Scale { get { return FScale; } set { SetScale(value); } }
        protected virtual void SetScale(double newscale)
        {
            FScale = newscale;
            UpdatePos();
        }
        private bool FSelected;
        protected void SetSelected(bool newvalue)
        {
            FSelected = newvalue;
        }
        public virtual void UpdatePos()
        {
            int newwidth = Twips.TwipsToPixels(FPrintItemObject.Width,FScale);
            int newheight = Twips.TwipsToPixels(FPrintItemObject.Height, FScale);
            SetBounds(Left, newwidth, Top, newheight);
        }
        public void SetBounds(int newleft, int newwidth, int newtop, int newheight)
        {
            FLeft = newleft;
            FWidth = newwidth;
            FTop = newtop;
            FHeight = newheight;
        }
        public override void GetProperties(Strings lnames, Strings ltypes,Variants lvalues, Strings lhints, Strings lcat)
        {
            lnames.Clear();
            ltypes.Clear();
            lhints.Clear();
            lcat.Clear();
            if (lvalues!=null)
                lvalues.Clear();
            // PrintCondition
            lnames.Add(Translator.TranslateStr(614));
            ltypes.Add(Translator.TranslateStr(571));
            lhints.Add("refcommon.html");
            lcat.Add(Translator.TranslateStr(1201));
            if (lvalues!=null)
                lvalues.Add(PrintItemObject.PrintCondition);
            // Before Print
            lnames.Add(Translator.TranslateStr(613));
            ltypes.Add(Translator.TranslateStr(571));
            lhints.Add("refcommon.html");
            lcat.Add(Translator.TranslateStr(1201));
            if (lvalues!=null)
                lvalues.Add(PrintItemObject.DoBeforePrint);
            // After Print
            lnames.Add(Translator.TranslateStr(612));
            ltypes.Add(Translator.TranslateStr(571));
            lhints.Add("refcommon.html");
            lcat.Add(Translator.TranslateStr(1201));
            if (lvalues!=null)
                lvalues.Add(PrintItemObject.DoAfterPrint);

            // Width
            lnames.Add(Translator.TranslateStr(554));
            ltypes.Add(Translator.TranslateStr(556));
            lhints.Add("refcommon.html");
            lcat.Add(Translator.TranslateStr(1201));
            if (lvalues!=null)
                lvalues.Add(PrintItemObject.Width);
            // Height
            lnames.Add(Translator.TranslateStr(555));
            ltypes.Add(Translator.TranslateStr(556));
            lhints.Add("refcommon.html");
            lcat.Add(Translator.TranslateStr(1201));
            if (lvalues!=null)
                lvalues.Add(PrintItemObject.Height);
        }
      public override void SetItem(ReportItem nitem)
      {
        FPrintItemObject = (PrintItem)nitem;
        base.SetItem(nitem);
      }
      public override Variant GetProperty(string pname)
        {
            // Print condition
            if (pname == Translator.TranslateStr(614))
                return PrintItemObject.PrintCondition;
            // Before print
            if (pname == Translator.TranslateStr(613))
                return PrintItemObject.DoBeforePrint;
            // After print
            if (pname == Translator.TranslateStr(612))
                return PrintItemObject.DoAfterPrint;
            // Width
            if (pname == Translator.TranslateStr(554))
                return PrintItemObject.Width;
            // Height
            if (pname == Translator.TranslateStr(555))
                return PrintItemObject.Height;
            // inherited
            return base.GetProperty(pname);
        }
        public override void SetProperty(string pname, Variant newvalue)
        {
            // Print condition
            if (pname == Translator.TranslateStr(614))
            {
                PrintItemObject.PrintCondition=newvalue.ToString();
                return;
            }
            // Before print
            if (pname == Translator.TranslateStr(613))
            {
                PrintItemObject.DoBeforePrint=newvalue.ToString();
                return;
            }
            // After print
            if (pname == Translator.TranslateStr(612))
            {
                PrintItemObject.DoAfterPrint=newvalue.ToString();
                return;
            }
            // Width
            if (pname == Translator.TranslateStr(554))
            {
                PrintItemObject.Width= newvalue;
                UpdatePos();
                return;
            }
            // Height
            if (pname == Translator.TranslateStr(555))
            {
                PrintItemObject.Height = newvalue;
                UpdatePos();
                return;
            }
            // inherited
            base.SetProperty(pname, newvalue);
        }
    }
    internal class DesignerInterfaceSizePos : DesignerInterfaceSize
    {
        private PrintPosItem FPrintPosItemObject;
        public DesignerInterfaceSizePos(SortedList<int, ReportItem> repitem):base (repitem)
        {
            if (repitem.Count == 1)
              FPrintPosItemObject = (PrintPosItem)repitem.Values[0];
        }
        public PrintPosItem PrintPosItemObject { get { return FPrintPosItemObject; } }
        public override void UpdatePos()
        {
            int newwidth = Twips.TwipsToPixels(FPrintPosItemObject.Width, Scale);
            int newheight = Twips.TwipsToPixels(FPrintPosItemObject.Height, Scale);
            int newleft = Twips.TwipsToPixels(FPrintPosItemObject.PosX, Scale);
            int newtop = Twips.TwipsToPixels(FPrintPosItemObject.PosY, Scale);
            SetBounds(newleft, newwidth, newtop, newheight);
        }
        public override void GetProperties(Strings lnames, Strings ltypes, Variants lvalues, Strings lhints, Strings lcat)
        {
            base.GetProperties(lnames, ltypes, lvalues, lhints, lcat);
            // Left
            lnames.Add(Translator.TranslateStr(553));
            ltypes.Add(Translator.TranslateStr(556));
            lhints.Add("refcommon.html");
            lcat.Add(Translator.TranslateStr(1201));
            if (lvalues != null)
                lvalues.Add(PrintPosItemObject.PosX);
            // Top
            lnames.Add(Translator.TranslateStr(552));
            ltypes.Add(Translator.TranslateStr(556));
            lhints.Add("refcommon.html");
            lcat.Add(Translator.TranslateStr(1201));
            if (lvalues != null)
                lvalues.Add(PrintPosItemObject.PosY);
            // Align
            lnames.Add(Translator.TranslateStr(623));
            ltypes.Add(Translator.TranslateStr(569));
            lhints.Add("refcommon.html");
            lcat.Add(Translator.TranslateStr(1201));
            if (lvalues != null)
                lvalues.Add((int)PrintPosItemObject.Align);

        }
        public static PrintItemAlign StringToPrintItemAlign(string text)
        {
            if (text == Translator.TranslateStr(621))
                return PrintItemAlign.Bottom;
            else
                if (text == Translator.TranslateStr(622))
                    return PrintItemAlign.Right;
                else
                    if (text == (Translator.TranslateStr(621) + "/" + Translator.TranslateStr(622)))
                        return PrintItemAlign.BottomRight;
                    else
                        if (text == Translator.TranslateStr(1224))
                            return PrintItemAlign.LeftRight;
                        else
                            if (text == Translator.TranslateStr(1225))
                                return PrintItemAlign.TopBottom;
                            else
                                if (text == Translator.TranslateStr(1226))
                                    return PrintItemAlign.AllClient;
                                else
                                    return PrintItemAlign.None;
        }
        public static string PrintItemAlignToString(PrintItemAlign align)
        {
            string aresult = "";
            switch (align)
            {
                case PrintItemAlign.None:
                    aresult = Translator.TranslateStr(294);
                    break;
                case PrintItemAlign.Bottom:
                    aresult = Translator.TranslateStr(621);
                    break;
                case PrintItemAlign.Right:
                    aresult = Translator.TranslateStr(622);
                    break;
                case PrintItemAlign.BottomRight:
                    aresult = Translator.TranslateStr(621) + "/" + Translator.TranslateStr(622);
                    break;
                case PrintItemAlign.LeftRight:
                    aresult = Translator.TranslateStr(1224);
                    break;
                case PrintItemAlign.TopBottom:
                    aresult = Translator.TranslateStr(1225);
                    break;
                case PrintItemAlign.AllClient:
                    aresult = Translator.TranslateStr(1226);
                    break;
            }
            return aresult;
        }
        public override void GetPropertyValues(string pname, Strings lpossiblevalues)
        {
            // Align
            if (pname == Translator.TranslateStr(623))
            {
                lpossiblevalues.Add(Translator.TranslateStr(294));
                lpossiblevalues.Add(Translator.TranslateStr(621));
                lpossiblevalues.Add(Translator.TranslateStr(622));
                lpossiblevalues.Add(Translator.TranslateStr(621) + "/" + Translator.TranslateStr(622));
                lpossiblevalues.Add(Translator.TranslateStr(1224));
                lpossiblevalues.Add(Translator.TranslateStr(1225));
                lpossiblevalues.Add(Translator.TranslateStr(1226));
                return;
            }
            base.GetPropertyValues(pname, lpossiblevalues);
        }
        public override void SetProperty(string pname, Variant newvalue)
        {
            // Left/PosX
            if (pname == Translator.TranslateStr(553))
            {
                PrintPosItemObject.PosX = newvalue;
                UpdatePos();
                return;
            }
            // Top/PosY
            if (pname == Translator.TranslateStr(552))
            {
                PrintPosItemObject.PosY = newvalue;
                UpdatePos();
                return;
            }
            // Align
            if (pname == Translator.TranslateStr(623))
            {
                PrintPosItemObject.Align = (PrintItemAlign)(int)newvalue;
                return;
            }
            // inherited
            base.SetProperty(pname, newvalue);
        }
        public override void SetItem(ReportItem nitem)
        {
          FPrintPosItemObject = (PrintPosItem)nitem;
          base.SetItem(nitem);
        }
        public override Variant GetProperty(string pname)
        {
            // Left/PosX
            if (pname == Translator.TranslateStr(553))
                return PrintPosItemObject.PosX;
            // Top/PosY
            if (pname == Translator.TranslateStr(552))
                return PrintPosItemObject.PosY;
            // Align
            if (pname == Translator.TranslateStr(623))
                return (int)PrintPosItemObject.Align;
            // inherited
            return base.GetProperty(pname);
        }
    }
    internal class DesignerInterfaceText : DesignerInterfaceSizePos
    {
        private PrintItemText FPrintItemText;
        public DesignerInterfaceText(SortedList<int, ReportItem> repitem)
            : base(repitem)
        {
          if (repitem.Count == 1)
            FPrintItemText = (PrintItemText)repitem.Values[0];
        }
        public PrintItemText PrintItemTextObject { get { return FPrintItemText; } }
        public override void GetProperties(Strings lnames, Strings ltypes, Variants lvalues, Strings lhints, Strings lcat)
        {
            base.GetProperties(lnames, ltypes, lvalues, lhints, lcat);
            // Text H.Alignment
            lnames.Add(Translator.TranslateStr(628));
            ltypes.Add(Translator.TranslateStr(569));
            lhints.Add("refcommontext.html");
            lcat.Add(Translator.TranslateStr(1202));
            if (lvalues != null)
                lvalues.Add((int)FPrintItemText.Alignment);
            // Text V.Alignment
            lnames.Add(Translator.TranslateStr(629));
            ltypes.Add(Translator.TranslateStr(569));
            lhints.Add("refcommontext.html");
            lcat.Add(Translator.TranslateStr(1202));
            if (lvalues != null)
                lvalues.Add((int)FPrintItemText.VAlignment);
            // Font Name
            lnames.Add(Translator.TranslateStr(560));
            ltypes.Add(Translator.TranslateStr(560));
            lhints.Add("refcommontext.html");
            lcat.Add(Translator.TranslateStr(1202));
            if (lvalues != null)
                lvalues.Add(FPrintItemText.WFontName);
            // Linux (postcript) Font Name
            lnames.Add(Translator.TranslateStr(561));
            ltypes.Add(Translator.TranslateStr(561));
            lhints.Add("refcommontext.html");
            lcat.Add(Translator.TranslateStr(1202));
            if (lvalues != null)
                lvalues.Add(FPrintItemText.LFontName);
            // Type1 Font (PDF Font)
            lnames.Add(Translator.TranslateStr(562));
            ltypes.Add(Translator.TranslateStr(569));
            lhints.Add("refcommontext.html");
            lcat.Add(Translator.TranslateStr(1202));
            if (lvalues != null)
                lvalues.Add((int)FPrintItemText.Type1Font);
            // Font Step (dot matrix and esc/p export)
            lnames.Add(Translator.TranslateStr(1039));
            ltypes.Add(Translator.TranslateStr(569));
            lhints.Add("refcommontext.html");
            lcat.Add(Translator.TranslateStr(1202));
            if (lvalues != null)
                lvalues.Add((int)FPrintItemText.PrintStep);
            // Font Size
            lnames.Add(Translator.TranslateStr(563));
            ltypes.Add(Translator.TranslateStr(559));
            lhints.Add("refcommontext.html");
            lcat.Add(Translator.TranslateStr(1202));
            if (lvalues != null)
                lvalues.Add(FPrintItemText.FontSize);
            // Font Color
            lnames.Add(Translator.TranslateStr(564));
            ltypes.Add(Translator.TranslateStr(558));
            lhints.Add("refcommontext.html");
            lcat.Add(Translator.TranslateStr(1202));
            if (lvalues != null)
                lvalues.Add(FPrintItemText.FontColor);
            // Font Style
            lnames.Add(Translator.TranslateStr(566));
            ltypes.Add(Translator.TranslateStr(566));
            lhints.Add("refcommontext.html");
            lcat.Add(Translator.TranslateStr(1202));
            if (lvalues != null)
                lvalues.Add(FPrintItemText.FontStyle);
            // RightToLeft
            lnames.Add(Translator.TranslateStr(954));
            ltypes.Add(Translator.TranslateStr(568));
            lhints.Add("refcommontext.html");
            lcat.Add(Translator.TranslateStr(1202));
            if (lvalues != null)
                lvalues.Add(FPrintItemText.RightToLeft);
            // Back Color
            lnames.Add(Translator.TranslateStr(565));
            ltypes.Add(Translator.TranslateStr(558));
            lhints.Add("refcommontext.html");
            lcat.Add(Translator.TranslateStr(1202));
            if (lvalues != null)
                lvalues.Add(FPrintItemText.BackColor);
            // Transparent
            lnames.Add(Translator.TranslateStr(567));
            ltypes.Add(Translator.TranslateStr(568));
            lhints.Add("refcommontext.html");
            lcat.Add(Translator.TranslateStr(1202));
            if (lvalues != null)
              lvalues.Add(FPrintItemText.Transparent);
            // Cut Text
            lnames.Add(Translator.TranslateStr(625));
            ltypes.Add(Translator.TranslateStr(568));
            lhints.Add("refcommontext.html");
            lcat.Add(Translator.TranslateStr(1202));
            if (lvalues != null)
                lvalues.Add(FPrintItemText.CutText);
            // Word Wrap
            lnames.Add(Translator.TranslateStr(626));
            ltypes.Add(Translator.TranslateStr(568));
            lhints.Add("refcommontext.html");
            lcat.Add(Translator.TranslateStr(1202));
            if (lvalues != null)
                lvalues.Add(FPrintItemText.WordWrap);
            // Word Break
//            lnames.Add(Translator.TranslateStr(626));
//            ltypes.Add(Translator.TranslateStr(568));
//            lhints.Add("refcommontext.html");
//            lcat.Add(Translator.TranslateStr(1202));
 //           if (lvalues != null)
 //               lvalues.Add(FPrintItemText.WordBreak);
            // Single line
            lnames.Add(Translator.TranslateStr(627));
            ltypes.Add(Translator.TranslateStr(568));
            lhints.Add("refcommontext.html");
            lcat.Add(Translator.TranslateStr(1202));
            if (lvalues != null)
                lvalues.Add(FPrintItemText.SingleLine);
            // Font Rotation
            lnames.Add(Translator.TranslateStr(551));
            ltypes.Add(Translator.TranslateStr(559));
            lhints.Add("refcommontext.html");
            lcat.Add(Translator.TranslateStr(1202));
            if (lvalues != null)
                lvalues.Add(FPrintItemText.FontRotation/10);
        }
        public override void GetPropertyValues(string pname, Strings lpossiblevalues)
        {
// if pname=SrpSRightToLeft then
// begin
//  GetBidiDescriptions(lpossiblevalues);
//  exit;
// end;
            // Alignment
            if (pname == Translator.TranslateStr(628))
            {
                //lpossiblevalues.Add(Translator.TranslateStr(630));
                lpossiblevalues.Add(Translator.TranslateStr(631));
                lpossiblevalues.Add(Translator.TranslateStr(632));
                lpossiblevalues.Add(Translator.TranslateStr(633));
                lpossiblevalues.Add(Translator.TranslateStr(1113));
                return;
            }
            // VAlignment
            if (pname == Translator.TranslateStr(629))
            {
                //lpossiblevalues.Add(Translator.TranslateStr(630));
                lpossiblevalues.Add(Translator.TranslateStr(634));
                lpossiblevalues.Add(Translator.TranslateStr(635));
                lpossiblevalues.Add(Translator.TranslateStr(633));
                return;
            }
            // Type1Font
            if (pname == Translator.TranslateStr(562))
            {
                lpossiblevalues.Add("Helvetica");
                lpossiblevalues.Add("Courier");
                lpossiblevalues.Add("Times Roman");
                lpossiblevalues.Add("Symbol");
                lpossiblevalues.Add("ZafDingbats");
                lpossiblevalues.Add("Truetype Link");
                lpossiblevalues.Add("Truetype Embed");
                return;
            }
            // FontStep
            if (pname == Translator.TranslateStr(1039))
            {
                lpossiblevalues.Add(Translator.TranslateStr(1038));
                lpossiblevalues.Add("20 cpi");
                lpossiblevalues.Add("17 cpi");
                lpossiblevalues.Add("15 cpi");
                lpossiblevalues.Add("12 cpi");
                lpossiblevalues.Add("10 cpi");
                lpossiblevalues.Add("6 cpi");
                lpossiblevalues.Add("5 cpi");
                return;
            }
              
            base.GetPropertyValues(pname, lpossiblevalues);
        }
        public static PrintStepType StringToPrintStep(string nvalue)
        {
            PrintStepType aresult = PrintStepType.BySize;
            switch (nvalue)
            {
                case "5 cpi":
                    aresult = PrintStepType.cpi5;
                    break;
                case "6 cpi":
                    aresult = PrintStepType.cpi6;
                    break;
                case "10 cpi":
                    aresult = PrintStepType.cpi10;
                    break;
                case "12 cpi":
                    aresult = PrintStepType.cpi12;
                    break;
                case "15 cpi":
                    aresult = PrintStepType.cpi15;
                    break;
                case "17 cpi":
                    aresult = PrintStepType.cpi17;
                    break;
                case "20 cpi":
                    aresult = PrintStepType.cpi20;
                    break;
            }
            return aresult;
        }
        public static TextAlignType PrintStringToAlignment(string nvalue)
        {
            TextAlignType aresult = TextAlignType.Left;
            if (nvalue == Translator.TranslateStr(630))
                aresult = TextAlignType.Left;
            if (nvalue == Translator.TranslateStr(631))
                aresult = TextAlignType.Right;
            if (nvalue == Translator.TranslateStr(632))
                aresult = TextAlignType.Right;
            if (nvalue == Translator.TranslateStr(633))
                aresult = TextAlignType.Center;
            if (nvalue == Translator.TranslateStr(1113))
                aresult = TextAlignType.Justify;
            return aresult;
        }
        public static TextAlignVerticalType PrintStringToVAlignment(string nvalue)
        {
            TextAlignVerticalType aresult = TextAlignVerticalType.Top;
            if (nvalue == Translator.TranslateStr(630))
                aresult = TextAlignVerticalType.Top;
            if (nvalue == Translator.TranslateStr(634))
                aresult = TextAlignVerticalType.Top;
            if (nvalue == Translator.TranslateStr(633))
                aresult = TextAlignVerticalType.Center;
            if (nvalue == Translator.TranslateStr(635))
                aresult = TextAlignVerticalType.Bottom;
            return aresult;
        }
        public static PDFFontType Type1FontStringToType1Font(string nvalue)
        {
            PDFFontType aresult = PDFFontType.Helvetica;
            return aresult;
        }

        public override void SetProperty(string pname, Variant newvalue)
        {
            // Alignment
            if (pname == Translator.TranslateStr(628))
            {
                FPrintItemText.Alignment = (TextAlignType)(int)newvalue;
                UpdatePos();
                return;
            }
            // V. Alignment
            if (pname == Translator.TranslateStr(629))
            {
                FPrintItemText.VAlignment = (TextAlignVerticalType)(int)newvalue;
                UpdatePos();
                return;
            }
            // Font Name
            if (pname == Translator.TranslateStr(560))
            {
                FPrintItemText.WFontName = newvalue.ToString();
                return;
            }
            // L. Font Name
            if (pname == Translator.TranslateStr(561))
            {
                FPrintItemText.LFontName = newvalue.ToString();
                return;
            }
            // Type1Font
            if (pname == Translator.TranslateStr(562))
            {
                FPrintItemText.Type1Font = Type1FontStringToType1Font(newvalue);
                return;
            }
            // Font Step (dot matrix and esc/p export)
            if (pname == Translator.TranslateStr(1039))
            {
                FPrintItemText.PrintStep = StringToPrintStep(newvalue);
                return;
            }
            // Font Size
            if (pname == Translator.TranslateStr(563))
            {
                FPrintItemText.FontSize = (short)newvalue;
                return;
            }
            // Font Color
            if (pname == Translator.TranslateStr(564))
            {
                FPrintItemText.FontColor = newvalue;
                return;
            }
            // Font Style
            if (pname == Translator.TranslateStr(566))
            {
                FPrintItemText.FontStyle = newvalue;
                return;
            }
            // Back Color
            if (pname == Translator.TranslateStr(565))
            {
                FPrintItemText.BackColor = newvalue;
                return;
            }
            // RightToLeft
            if (pname == Translator.TranslateStr(954))
            {
                FPrintItemText.RightToLeft = newvalue;
                return;
            }
            // Transparent
            if (pname == Translator.TranslateStr(567))
            {
                FPrintItemText.Transparent = newvalue;
                return;
            }
            // Cut Text
            if (pname == Translator.TranslateStr(625))
            {
                FPrintItemText.CutText = newvalue;
                return;
            }
            // Word Wrap
            if (pname == Translator.TranslateStr(626))
            {
                FPrintItemText.WordWrap = newvalue;
                return;
            }
            // Word Break
            //            lnames.Add(Translator.TranslateStr(626));
            //            ltypes.Add(Translator.TranslateStr(568));
            //            lhints.Add("refcommontext.html");
            //            lcat.Add(Translator.TranslateStr(1202));
            //           if (lvalues != null)
            //               lvalues.Add(FPrintItemText.WordBreak);
            // Single line
            // Word Wrap
            if (pname == Translator.TranslateStr(627))
            {
                FPrintItemText.SingleLine = newvalue;
                return;
            }
            // Font Rotation
            // Word Wrap
            if (pname == Translator.TranslateStr(551))
            {
                FPrintItemText.FontRotation = (short)(newvalue*10);
                return;
            }
            // inherited
            base.SetProperty(pname, newvalue);
        }
        public static int StringToFontStyle(string newvalue)
        {
            int aresult = 0;
            return aresult;
        }
        public override void SetItem(ReportItem nitem)
        {
          FPrintItemText = (PrintItemText)nitem;
          base.SetItem(nitem);
        }
        public override Variant GetProperty(string pname)
        {
            // Text H.Alignment
            if (pname == Translator.TranslateStr(628))
                return (int)FPrintItemText.Alignment;
            // Text V.Alignment
            if (pname == Translator.TranslateStr(629))
                return (int)FPrintItemText.VAlignment;
            // Font Name
            if (pname == Translator.TranslateStr(560))
                return FPrintItemText.WFontName;
            // Linux (postcript) Font Name
            if (pname == Translator.TranslateStr(561))
                return FPrintItemText.LFontName;
            // Type1 Font (PDF Font)
            if (pname == Translator.TranslateStr(562))
                return (int)FPrintItemText.Type1Font;
            // Font Step (dot matrix and esc/p export)
            if (pname == Translator.TranslateStr(1039))
                return  (int)FPrintItemText.PrintStep;
            // Font Size
            if (pname == Translator.TranslateStr(563))
                return FPrintItemText.FontSize;
            // Font Color
            if (pname == Translator.TranslateStr(564))
                return FPrintItemText.FontColor;
            // Font Style
            if (pname == Translator.TranslateStr(566))
                return FPrintItemText.FontStyle;
            // RightToLeft
            if (pname == Translator.TranslateStr(954))
                return FPrintItemText.RightToLeft;
            // RightToLeft
            //            lnames.Add(Translator.TranslateStr(954));
            //            ltypes.Add(Translator.TranslateStr(568));
            //            lhints.Add("refcommontext.html");
            //lcat.Add(Translator.TranslateStr(1202));
            //            if (lvalues != null)
            //                lvalues.Add(FPrintItemText.RightToLeft);
            // Back Color
            if (pname == Translator.TranslateStr(565))
                return FPrintItemText.BackColor;
            // Transparent
            if (pname == Translator.TranslateStr(567))
                return FPrintItemText.Transparent;
            // Cut Text
            if (pname == Translator.TranslateStr(625))
                return FPrintItemText.CutText;
            // Word Wrap
            if (pname == Translator.TranslateStr(626))
                return FPrintItemText.WordWrap;
            // Word Break
            //            lnames.Add(Translator.TranslateStr(626));
            //            ltypes.Add(Translator.TranslateStr(568));
            //            lhints.Add("refcommontext.html");
            //            lcat.Add(Translator.TranslateStr(1202));
            //           if (lvalues != null)
            //               lvalues.Add(FPrintItemText.WordBreak);
            // Single line
            if (pname == Translator.TranslateStr(627))
                return FPrintItemText.SingleLine;
            // Font Rotation
            if (pname == Translator.TranslateStr(551))
                return FPrintItemText.FontRotation / 10;
            
            // inherited
            return base.GetProperty(pname);
        }
    }

  internal class DesignerInterfaceSubReport:DesignerInterface
  {
    SubReport fsubreport;
        public DesignerInterfaceSubReport(SortedList<int, ReportItem> repitem):base (repitem)
        {
            if (repitem.Count == 1)
              fsubreport = (SubReport)repitem.Values[0];
        }
    public override void GetProperties(Strings lnames, Strings ltypes, Variants lvalues, Strings lhints, Strings lcat)
    {

      base.GetProperties(lnames, ltypes, lvalues, lhints, lcat);
      fsubreport = (SubReport)ReportItemObject;
      // Main dataset
      lnames.Add(Translator.TranslateStr(275));
      ltypes.Add(Translator.TranslateStr(569));
      lhints.Add("refsubreport.html");
      //lcat.Add(Translator.TranslateStr(280));
      if (lvalues != null)
        lvalues.Add(fsubreport.Alias);
      // Print only if data available
      lnames.Add(Translator.TranslateStr(800));
      ltypes.Add(Translator.TranslateStr(568));
      lhints.Add("refsubreport.html");
      //lcat.Add(Translator.TranslateStr(280));
      if (lvalues != null)
        lvalues.Add(fsubreport.PrintOnlyIfDataAvailable);

    }
    public override void GetPropertyValues(string pname, Strings lpossiblevalues)
    {
      if (pname == Translator.TranslateStr(275))
      {
        fsubreport = (SubReport)ReportItemObject;
        lpossiblevalues.Add("");
        foreach (DataInfo dinfo in fsubreport.Report.DataInfo)
        {
          lpossiblevalues.Add(dinfo.Alias);
        }
        return;
      }
      base.GetPropertyValues(pname, lpossiblevalues);
    }
    public override void SetItem(ReportItem nitem)
    {
      fsubreport = (SubReport)nitem;
      base.SetItem(nitem);
    }
    public override Variant GetProperty(string pname)
    {
      if (pname == Translator.TranslateStr(275))
      {
        return fsubreport.Alias;
      }
      if (pname == Translator.TranslateStr(800))
      {
        return fsubreport.PrintOnlyIfDataAvailable;
      }

      return base.GetProperty(pname);
    }
    public override void SetProperty(string pname, Variant newvalue)
    {
      if (pname == Translator.TranslateStr(275))
      {
        fsubreport.Alias = newvalue;

        return;
      }
      if (pname == Translator.TranslateStr(800))
      {
        fsubreport.PrintOnlyIfDataAvailable = newvalue;
        return;
      }
      base.SetProperty(pname, newvalue);
    }
  }

}
