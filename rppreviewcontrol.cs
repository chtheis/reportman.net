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
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using Reportman.Drawing;
using Reportman.Drawing.Forms;
using Reportman.Reporting;

namespace Reportman.Reporting.Forms
{
#if REPMAN_COMPACT
#else
	[ToolboxBitmapAttribute(typeof(PreviewControl), "previewcontrol.ico")]
#endif
	public class PreviewControl : PreviewMetaFile
	{
		private Report FReport;
		public Report Report
		{
			get { return FReport; }
			set { SetReport(value); }
		}
		private bool FFinished;
		public bool Finished
		{
			get
			{
				return FFinished;
			}
 		}
		public PreviewControl()
			: base()
		{
			FFinished = false;
		}
		private void SetReport(Report areport)
		{
			FReport = areport;
			if (FReport == null)
				return;
			FReport.BeginPrint(prdriver);
			try
			{
				FFinished = FReport.RequestPage(0);
				Page = 0;
				MetaFile = FReport.MetaFile;
			}
			catch
			{
				FReport.EndPrint();
				throw;
			}
		}
	}
}
