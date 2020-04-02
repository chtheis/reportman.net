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
using System.IO;
using System.Drawing;
using Reportman.Drawing;
using System.Collections.Generic;

namespace Reportman.Reporting
{

	class VariableGraph : IdenVariable
	{
		public BaseReport Report;
        public ChartItem NewChart;
		public VariableGraph(Evaluator eval, BaseReport rp)
			: base(eval)
		{
			Report = rp;
		}
        public void Clear()
        {
            NewChart.Clear();
        }
        public void NewValue(double newvalue, bool seriechange, string valuecaption, string seriecaption)
        {
            NewChart.NewValue(newvalue, seriechange, valuecaption, seriecaption);
        }
        public void GraphicColor(int newcolor)
        {
            NewChart.GraphicColor(newcolor);
        }
        public void GraphicSerieColor(int newcolor)
        {
            NewChart.GraphicSerieColor(newcolor);
        }

	}
	public class ChartItem : PrintItemText
	{
		public int[] SeriesColors ={0xFF0000,0xFFDDFF,0x00FF00,0x0000FF,
			0xFFFF00,0xFF00FF,0x00FFFF,0xAAAAAA,0xBB0000,0x00BB00,0x0000BB,0xBBBB00,
			0xBB00BB,0x00BBBB,0x777777,0x773333,0x337733,0x333377,0x777700,
			0x770077,0x007777};
		private Series FSeries;
		private const int DEF_DRAWWIDTH = 500;
		private VariableGraph FIdenChart;
		public string ChangeSerieExpression;
		public string ClearExpression;
		public string GetValueCondition;
		public string ValueExpression;
		public string CaptionExpression;
		public string SerieCaption;
		public string ColorExpression;
		public string SerieColorExpression;
		public bool ChangeSerieBool;
		public ChartType ChartStyle;
		public string Identifier;
		public bool ClearExpressionBool;
		public ChartDriver Driver;
		public bool View3d, View3dWalls;
		public int Perspective, Elevation, Rotation, Zoom;
		public int HorzOffset, VertOffset, Tilt;
		public bool Orthogonal;
		public BarType MultiBar;
		public int Resolution;
		public bool ShowLegend, ShowHint;
		public int MarkStyle;
		public int HorzFontSize, VertFontSize, HorzFontRotation, VertFontRotation;
		internal VariableGraph IdenChart
		{
			get
			{
				return FIdenChart;
			}
		}
		public ChartItem(BaseReport rp)
			: base(rp)
		{
			Height = DEF_DRAWWIDTH;
			Width = Height;
			ShowHint = true;
			FSeries = new Series();
			FSeries.AutoRangeL = true;
			FSeries.AutoRangeH = true;
			FSeries.Logaritmic = false;
			FIdenChart = new VariableGraph(Report.Evaluator, Report);
            FIdenChart.NewChart = this;
			//      FIdenChart.OnClear:=OnClear;
			//      FIdenChart.OnNewValue:=OnNewValue;
			//      FIdenChart.OnSerieColor:=OnSerieColor;
			//      FIdenChart.OnValueColor:=OnValueColor;
			//      FIdenChart.OnBounds:=OnBoundsValue;
			View3d = true;
			Perspective = 15;
			Elevation = 345;
			Rotation = 345;
			Resolution = 100;
			Zoom = 100;
			Orthogonal = true;
			MultiBar = BarType.Side;
			MarkStyle = 0;
			HorzFontSize = 10;
			VertFontSize = 10;
			ChangeSerieExpression = "";
			ClearExpression = "";
			GetValueCondition = "";
			ValueExpression = "";
			CaptionExpression = "";
			SerieCaption = "";
			ColorExpression = "";
			SerieColorExpression = "";
			Identifier = "";
		}
        protected override string GetClassName()
        {
            return "TRPCHART";
        }
        public void Clear()
        {
            this.FSeries.Clear();
        }
        public override void SubReportChanged(SubReportEvent newstate, string newgroup)
        {
            switch (newstate)
            {
                case SubReportEvent.Start:
                    FClearValue = new Variant();
                    FSerieValue = new Variant();
                    Clear();
                    break;
                case SubReportEvent.DataChange:
                    // Gets a value if the condition is true
                    if (CheckValueCondition())
                        GetNewValue();
                    break;
            }
            base.SubReportChanged(newstate, newgroup);
        }
        private void GetNewValue()
        {
            if (ValueExpression.Length == 0)
                return;
            EvaluateClearExpression();
            bool changeserie = EvaluateChangeSerieExpression();
            string seriecaption = "";
            if (changeserie)
                seriecaption = EvaluateSerieCaption().ToString();

            string newcaption = "";
            if (this.CaptionExpression.Length > 0)
            {
                newcaption = EvaluateText(CaptionExpression);
                SeriesItem nitem = FSeries.SeriesItems[FSeries.SeriesItems.Count - 1];
                nitem.ValueCaptions[nitem.ValueCaptions.Count - 1] = newcaption;
            }
            NewValue(EvaluateText(ValueExpression), changeserie, newcaption, seriecaption);

            if (SerieColorExpression.Length > 0)
            {
                int newcolor = EvaluateText(SerieColorExpression);
                if (newcolor != 0)
                    FSeries.SeriesItems[FSeries.SeriesItems.Count - 1].Color = newcolor;
            }
            if (this.ColorExpression.Length > 0)
            {
                int newcolor = EvaluateText(ColorExpression);
                if (newcolor != 0)
                {
                    SeriesItem nitem = FSeries.SeriesItems[FSeries.SeriesItems.Count - 1];
                    nitem.Colors[nitem.Colors.Count-1] = newcolor;
                }
            }
        }
        private Variant EvaluateText(string ntext)
        {
            Evaluator fevaluator;
            fevaluator = Report.Evaluator;
            fevaluator.Expression = ntext;
            Variant aresult;
            fevaluator.Evaluate();
            aresult = fevaluator.Result;
            return aresult;
        }
        Variant FClearValue;
        Variant FSerieValue;
        public void EvaluateClearExpression()
        {
            if (this.ClearExpression.Length == 0)
                return;
            Evaluator fevaluator;
            fevaluator = Report.Evaluator;
            Variant aresult;
            try
            {
                fevaluator.Expression = ClearExpression;
                fevaluator.Evaluate();
                aresult = fevaluator.Result;
                if (ClearExpressionBool)
                {
                    if (aresult)
                        FSeries.SeriesItems.Clear();
                }
                else
                {
                    if (FClearValue != aresult)
                    {
                        FSeries.SeriesItems.Clear();
                        FClearValue = aresult;
                    }
                }

            }
            catch (Exception E)
            {
                throw new ReportException(E.Message + ":" + Name + " Prop:CheckValueCondition " + PrintCondition, this, "GetValueCondition");
            }
        }
        public bool EvaluateChangeSerieExpression()
        {
            if (this.ChangeSerieExpression.Length == 0)
                return false;
            bool nresult = false;
            Evaluator fevaluator;
            fevaluator = Report.Evaluator;
            Variant aresult;
            try
            {
                fevaluator.Expression = ChangeSerieExpression;
                fevaluator.Evaluate();
                aresult = fevaluator.Result;
                if (ChangeSerieBool)
                {
                    if (aresult)
                        nresult = true ;
                }
                else
                {
                    if (FSerieValue != aresult)
                    {
                        FSerieValue = aresult;
                        nresult = true;
                    }
                }

            }
            catch (Exception E)
            {
                throw new ReportException(E.Message + ":" + Name + " Prop:CheckValueCondition " + PrintCondition, this, "GetValueCondition");
            }
            return nresult;
        }
        public Variant EvaluateSerieCaption()
        {
            if (this.SerieCaption.Length == 0)
                return "";
            Evaluator fevaluator;
            fevaluator = Report.Evaluator;
            Variant aresult;
            try
            {
                fevaluator.Expression = SerieCaption;
                fevaluator.Evaluate();
                aresult = fevaluator.Result;
            }
            catch (Exception E)
            {
                throw new ReportException(E.Message + ":" + Name + " Prop:SerieCaption " + PrintCondition, this, "SerieCaption");
            }
            return aresult;
        }
        public bool CheckValueCondition()
        {
            if (this.GetValueCondition.Length == 0)
                return true;
            Evaluator fevaluator;
            bool nresult = false;
            fevaluator = Report.Evaluator;
            Variant aresult;
            try
            {
                fevaluator.Expression = GetValueCondition;
                fevaluator.Evaluate();
                aresult = fevaluator.Result;
                nresult = aresult;
            }
            catch (Exception E)
            {
                throw new ReportException(E.Message + ":" + Name + " Prop:CheckValueCondition " + PrintCondition, this, "GetValueCondition");
            }
            return nresult;
        }


        public void NewValue(double newvalue, bool seriechange, string valuecaption, string seriecaption)
        {
            bool firstserie = false;
            SeriesItem aserie = null;
            if (FSeries.SeriesItems.Count<1)
            {
                aserie = new SeriesItem();
                aserie.ChartStyle = this.ChartStyle;
                aserie.Caption = seriecaption;
                firstserie = true;
                FSeries.FontSize = FontSize;
                FSeries.Resolution = Resolution;
                FSeries.ShowLegend = ShowLegend;
                FSeries.MultiBar = MultiBar;
                FSeries.MarkStyle = MarkStyle;
                FSeries.HorzFontRotation = HorzFontRotation;
                FSeries.VertFontRotation = VertFontRotation;

                FSeries.ShowHint = ShowHint;
                FSeries.Effect3D = this.View3d;

                FSeries.SeriesItems.Add(aserie);
            }
            else
                aserie = FSeries.SeriesItems[FSeries.SeriesItems.Count-1];

            if (seriechange)
            {
                if (!firstserie)
                {
                    aserie = new SeriesItem();
                    aserie.ChartStyle = this.ChartStyle;
                    aserie.Caption = seriecaption;
                    FSeries.SeriesItems.Add(aserie);
                }
            }
            aserie.Values.Add(newvalue);
            aserie.ValueCaptions.Add(valuecaption);
            aserie.Colors.Add(aserie.Color);
        }
        public void GraphicColor(int newcolor)
        {
            if (FSeries.SeriesItems.Count == 0)
                return;
            FSeries.SeriesItems[FSeries.SeriesItems.Count - 1].SetLastValueColor(newcolor);
        }
        public void GraphicSerieColor(int newcolor)
        {
            if (FSeries.SeriesItems.Count == 0)
                return;
            FSeries.SeriesItems[FSeries.SeriesItems.Count - 1].Color = newcolor; ;
        }
        protected override void DoPrint(PrintOut adriver, int aposx, int aposy, int newwidth, int newheight, MetaFile metafile, Point MaxExtent, ref bool PartialPrint)
        {
            base.DoPrint(adriver, aposx, aposy, newwidth, newheight, metafile, MaxExtent, ref PartialPrint);
            FSeries.PrintWidth = PrintWidth;
            FSeries.PrintHeight = PrintHeight;

            if (FSeries.SeriesItems.Count == 0)
                return;
            if (Report.ChartingDriver!= null)
                Report.ChartingDriver.DrawChart(FSeries, metafile, aposx, aposy, this);
            else
                adriver.DrawChart(FSeries, metafile, aposx, aposy, this);
        }
    }

}
