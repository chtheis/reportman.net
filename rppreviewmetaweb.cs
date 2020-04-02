using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.IO;
using System.Web.UI.WebControls;
using Reportman.Drawing;
using System.Drawing;
using System.Threading;
using System.Drawing.Imaging;
using Reportman.Reporting;
using System.Web.Caching;

namespace Reportman.Web
{
	[ToolboxBitmap(typeof(PreviewMetaFile), "previewmetafile.ico")]
	[ToolboxData("<{0}:PreviewMetaFile runat=server></{0}:PreviewMetaFile>")]
	public class PreviewMetaFile : System.Web.UI.WebControls.Image
	{
		const int MAXENTIREPAGES = 128;

		[Browsable(false)]
		public override string ImageUrl
		{
			get { return base.ImageUrl; }
			set { base.ImageUrl = value; }
		}
		private string GetCacheId()
		{
			return Page.Session.SessionID + Page.UniqueID + UniqueID;
//			return  Page.UniqueID + UniqueID;
		}
		
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(false)]
		[Localizable(true)]
		public bool UseHttpHandler
		{
			get
			{
				if (ViewState["UseHttpHandler"] == null)
					return false;
				else
					return (bool)ViewState["UseHttpHandler"];
			}
			set
			{
				ViewState["UseHttpHandler"] = value;
			}
		}
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue("WMF")]
		[Localizable(true)]
		public string Format
		{
			get
			{
				if (ViewState["Format"] == null)
					return "WMF";
				else
					return (string)ViewState["Format"];
			}
			set
			{
				ViewState["Format"] = value;
			}
		}
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(0)]
		[Localizable(true)]
		public int CurrentPage
		{
			get
			{
				if (ViewState["CurrentPage"] == null)
					return 0;
				else
				{
					string cacheid=GetCacheId();
					int nvalue = (int)ViewState["CurrentPage"];
					if (Page.Cache[cacheid + "LastPage"] != null)
					{
						nvalue=(int)Page.Cache[cacheid+"LastPage"];
						ViewState["CurrentPage"] = nvalue;
						Page.Cache.Remove(cacheid+"LastPage");
					}
					return nvalue;
				}
			}
			set
			{
				int nvalue=value;
				if (nvalue<0)
					nvalue=0;
				MetaFile meta = GetMetaFile();
				if (meta != null)
				{
					meta.RequestPage(nvalue);
					if (meta.Pages.CurrentCount <= nvalue)
						nvalue = meta.Pages.CurrentCount - 1;
				}
				ViewState["CurrentPage"] = nvalue;
			}
		}
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(AutoScaleType.Real)]
		[Localizable(true)]
		public float PreviewScale
		{
			get
			{
				if (ViewState["PreviewScale"] == null)
					return 1.0F;
				else
					return (float)ViewState["PreviewScale"];
			}
			set
			{
				ViewState["PreviewScale"] = value;
			}			
		}
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(true)]
		[Localizable(true)]
		public bool AsyncExecution
		{
			get
			{
				if (ViewState["AsyncExecution"] == null)
					return true;
				else
					return (bool)ViewState["AsyncExecution"];
			}
			set
			{
				ViewState["AsyncExecution"] = value;
			}
		}
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(System.Threading.ThreadPriority.Normal)]
		[Localizable(true)]
		public System.Threading.ThreadPriority AsyncPriority
		{
			get
			{
				if (ViewState["AsyncPriority"] == null)
					return System.Threading.ThreadPriority.Normal;
				else
					return (System.Threading.ThreadPriority)ViewState["AsyncPriority"];
			}
			set
			{
				ViewState["AsyncPriority"] = value;
			}
		}
		private void ClearCache()
		{
			if (DesignMode)
				return;
			if (Page == null)
				return;
			if (Page.Cache == null)
				throw new UnNamedException("Application cache not enabled,"+
					" enable it ant web.config");
			string cacheid = GetCacheId();
			Page.Cache.Remove(cacheid + "Report");
			Page.Cache.Remove(cacheid + "MetaFile");
			Page.Cache.Remove(cacheid + "PrintOut");
		}
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(AutoScaleType.Real)]
		[Localizable(true)]
		public string ReportFileName
		{
			get
			{
				if (ViewState["ReportFileName"] == null)
					return "";
				else
					return (string)ViewState["ReportFileName"];
			}
			set
			{
				string oldvalue = ReportFileName;
				string newvalue = value;
				if (value == null)
					newvalue="";
				if (oldvalue != newvalue)
				{
					ClearCache();
				}
				ViewState["ReportFileName"] = newvalue;
			}
		}
		protected override void OnLoad(EventArgs e)
		{
			if (DesignMode)
			{
				base.ImageUrl = Page.ClientScript.GetWebResourceUrl(this.GetType(),
				 "Reportman.Web.reportsmall.jpg");
				base.OnLoad(e);
				return;
			}
			string cacheid = GetCacheId();
			if (!UseHttpHandler)
			{				
				string url = Page.Request.FilePath.Substring(
													 Page.Request.FilePath.LastIndexOf("/") + 1);
				url += "?renderImg=" + cacheid;
				base.ImageUrl = url;
			}

			base.OnLoad(e);
		}
		protected override void OnInit(EventArgs e)
		{
			if (DesignMode)
			{
				base.OnInit(e);
				return;
			}
			Translator.SetDefaultTranslatorPath(Page.Request.PhysicalApplicationPath);
			bool processed = false;
			MemoryStream mems;
			string cacheid = GetCacheId();
			if (!UseHttpHandler)
			{
				if ((Page.Request.QueryString["renderImg"] != null) &&
						(Page.Request.QueryString["renderImg"].ToString() ==
															 cacheid))
				{
					long memssize = -1;
					if (Page.Cache[cacheid + "mimetype"]!=null)
					{
						string mimetype=(string)Page.Cache[cacheid + "mimetype"];
						if (Page.Cache[cacheid + "memstream"]!=null)
						{
							mems = (MemoryStream)Page.Cache[cacheid + "memstream"];
							try
							{
								memssize = mems.Length;
								Page.Response.ContentType = mimetype;
								mems.Seek(0, SeekOrigin.Begin);
								Page.Response.BinaryWrite(mems.ToArray());
								Page.Response.End();
								processed=true;
							}
							finally
							{
								mems.Dispose();
								Page.Cache.Remove(cacheid + "memstream");
								Page.Cache.Remove(cacheid + "mimetype");
							}
						}
					}
					if (!processed)
					{
						mems = new MemoryStream();
						using (mems)
						{
							string amessage = "No cached imaged to show";
							if (memssize >= 0)
								amessage = amessage + " ImageSize: " + memssize.ToString("##,##0") + " bytes";
							Bitmap abitmap = GraphicUtils.TextToBitmap(500,
								"No cached imaged to show", "Arial", 10);
							abitmap.Save(mems, ImageFormat.Gif);
							mems.Seek(0, SeekOrigin.Begin);
							Page.Response.ContentType = "image/gif";
							Page.Response.BinaryWrite(mems.ToArray());
							Page.Response.End();
						}
					}
				}
			}
			base.OnInit(e);
		}
		public override void RenderBeginTag(HtmlTextWriter writer)
		{
			if (DesignMode)
			{
				base.ImageUrl = Page.ClientScript.GetWebResourceUrl(this.GetType(),
				 "Reportman.Web.reportsmall.jpg");
			}
			base.RenderBeginTag(writer);
		}
		public MetaFile GetMetaFile()
		{
			// Execute the report and save the memory stream and
			// the mimetype on cache
			string cacheid = GetCacheId();
			// Check if there is a metafile
			MetaFile meta = (MetaFile)Page.Cache[cacheid + "MetaFile"];
			PrintOutNet prdriver = (PrintOutNet)Page.Cache[cacheid + "PrintOut"];
			if (prdriver == null)
				meta = null;
			if (meta == null)
			{
				Report rp = (Report)Page.Cache[cacheid + "Report"];
				if (rp == null)
				{
					string filename = ReportFileName;
					if (filename != "")
					{
						rp = new Report();
						// Async execution
						rp.AsyncExecution = AsyncExecution;
						rp.AsyncPriority = AsyncPriority;
						ReportReader reader = new ReportReader(rp);
						reader.LoadFromFile(Page.Server.MapPath(filename));
						meta = rp.MetaFile;
						meta.ForwardOnly = false;
						prdriver = new PrintOutNet();
						prdriver.OptimizeWMF = WMFOptimization.Gdiplus;
						Page.Cache.Insert(cacheid + "Report", rp, null,
							DateTime.Now.AddMinutes(30), TimeSpan.Zero);
						Page.Cache.Insert(cacheid + "MetaFile", meta, null,
							DateTime.Now.AddMinutes(30), TimeSpan.Zero);
						Page.Cache.Insert(cacheid + "PrintOut", prdriver, null,
							DateTime.Now.AddMinutes(30), TimeSpan.Zero);
						// Assign parameters to the report here
						rp.BeginPrint(prdriver);
					}
				}
			}
			return meta;
		}
		public void GetPdf()
		{
				MetaFile meta=GetMetaFile();
				PrintOutPDF prpdf=new PrintOutPDF();
				prpdf.Print(meta);
				prpdf.PDFStream.Seek(0,SeekOrigin.Begin);
				Page.Response.ContentType= "application/pdf";
				Page.Response.BinaryWrite(prpdf.PDFStream.ToArray());
				Page.Response.End();
		}
		protected override void RenderContents(HtmlTextWriter output)
		{
			if (DesignMode)
			{
				base.RenderContents(output);
				return;
			}
			Bitmap bm;
			// Execute the report and save the memory stream and
			// the mimetype on cache
			string cacheid = GetCacheId();
			// Check if there is a metafile
			MetaFile meta = GetMetaFile();
			Monitor.Enter(meta);
			try
			{
					PrintOutNet prdriver = (PrintOutNet)Page.Cache[cacheid + "PrintOut"];
				if (prdriver == null)
					prdriver = new PrintOutNet();

				if (meta != null)
				{
					int pagenum = CurrentPage;
					meta.RequestPage(pagenum);
					if (meta.Pages.CurrentCount <= pagenum)
					{
						pagenum = meta.Pages.CurrentCount - 1;
						Page.Cache.Insert(cacheid + "LastPage", pagenum, null,
							DateTime.Now.AddMinutes(30), TimeSpan.Zero);
					}
					else
						Page.Cache.Remove(cacheid + "LastPage");
					if (pagenum < meta.Pages.CurrentCount)
					{
						System.Drawing.Imaging.Metafile metaf;
						MetaPage metap = meta.Pages[pagenum];
						if (metap.WindowsMetafile == null)
						{
							bm = new Bitmap(1, 1, PixelFormat.Format32bppArgb);
							prdriver.Output = bm;
							prdriver.DrawPage(meta, metap);
							metaf = metap.WindowsMetafile;
							if (metaf == null)
								throw new Exception("No se pudo crear el metafile");
						}
						else
							metaf = metap.WindowsMetafile;
						string mimetype;
						MemoryStream mems = new MemoryStream();
						try
						{
							switch (Format)
							{
								case "WMF":
									GraphicUtils.WriteWindowsMetaFile(metaf,
										mems, PreviewScale / metap.WindowsMetafileScale);
									mimetype = "application/x-msMetafile";
									break;
								default:
									GraphicUtils.WriteWindowsMetaFileCodec(metaf,
										mems, PreviewScale / metap.WindowsMetafileScale,
										Format, out mimetype);
									break;
							}
							Page.Cache.Insert(cacheid + "mimetype", mimetype, null,
								DateTime.Now.AddMinutes(10), TimeSpan.Zero);
							Page.Cache.Insert(cacheid + "memstream", mems, null,
								DateTime.Now.AddMinutes(10), TimeSpan.Zero);
						}
						catch
						{
							mems.Dispose();
							throw;
						}
					}
				}
			}
			finally
			{
				Monitor.Exit(meta);
			}
			base.RenderContents(output);
		}
	}
}
