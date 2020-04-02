using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Drawing;
using System.Web.UI.WebControls;
using Reportman.Drawing;
using Reportman.Reporting;

namespace Reportman.Web
{
	[ToolboxBitmap(typeof(PreviewWeb), "previewmetafile.ico")]
	[ToolboxData("<{0}:PreviewWeb runat=server></{0}:PreviewWeb>")]
	public class PreviewWeb : CompositeControl,System.Web.UI.INamingContainer
	{
		public PreviewWeb():base()
		{
			EnableViewState = true;
			FPreviewControl = new PreviewMetaFile();
		}
		protected override void OnInit(EventArgs e)
		{
			Translator.SetDefaultTranslatorPath(Page.Request.PhysicalApplicationPath);
			base.OnInit(e);
			EnsureChildControls();
		}
		private PreviewMetaFile FPreviewControl;
		private ImageButton BFirst;
		private ImageButton BPrevious;
		private ImageButton BNext;
		private ImageButton BLast;
		private ImageButton BScale1;
		private ImageButton BPrint;
		private ImageButton BZoomPlus;
		private ImageButton BZoomMinus;
		private DropDownList BFormat;
		private TextBox EPage;
		private ImageButton BPage;
		private Label LTotalPages;
		[Bindable(true)]
		[Category("Appearance")]
		[Localizable(true)]
		public PreviewMetaFile PreviewControl
		{
			get
			{
				return FPreviewControl;
			}
		}
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(true)]
		[Localizable(true)]
		public bool ShowFormat
		{
			get
			{
				if (ViewState["ShowFormat"]==null)
					return true;
				else
					return (bool)ViewState["ShowFormat"];
			}
			set
			{
				ViewState["ShowFormat"] = value;
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
		protected void Format_Click(object sender, EventArgs e)
		{
			if (sender == BFormat)
			{
				PreviewControl.Format = BFormat.Text;
				return;
			}
		}
		protected void Image_Click(object sender, ImageClickEventArgs e)
		{
			if (sender == BFirst)
			{
				PreviewControl.CurrentPage = 0;
				return;
			}
			if (sender == BNext)
			{
				PreviewControl.CurrentPage = PreviewControl.CurrentPage + 1;
				return;
			}
			if (sender == BLast)
			{
				PreviewControl.CurrentPage = int.MaxValue - 10;
				return;
			}
			if (sender == BPrevious)
			{
				PreviewControl.CurrentPage = PreviewControl.CurrentPage - 1;
				return;
			}
			if (sender == BZoomMinus)
			{
				PreviewControl.PreviewScale = PreviewControl.PreviewScale - 0.1F;
				return;
			}
			if (sender == BZoomPlus)
			{
				PreviewControl.PreviewScale = PreviewControl.PreviewScale + 0.1F;
				return;
			}
			if (sender == BScale1)
			{
				PreviewControl.PreviewScale = 1.0F;
				return;
			}
			if (sender == BPage)
			{
				try
				{					
					PreviewControl.CurrentPage = System.Convert.ToInt32(EPage.Text)-1;
				}
				catch
				{
				}
				return;
			}
			if (sender == BPrint)
			{
				PreviewControl.GetPdf();
				return;
			}
		}
		protected override void CreateChildControls()
		{
			FPreviewControl.AsyncExecution = AsyncExecution;
			FPreviewControl.AsyncPriority = AsyncPriority;
			LiteralControl lt;

			lt=new LiteralControl("<table border=\"1\">\n");
			Controls.Add(lt);

			lt = new LiteralControl("<tr>\n");
			Controls.Add(lt);

			lt = new LiteralControl("<td>\n");
			Controls.Add(lt);

			{
				{
					ImageClickEventHandler clickevent =
						new ImageClickEventHandler(Image_Click);

					lt = new LiteralControl("<table>\n");
					Controls.Add(lt);

					lt = new LiteralControl("<tr>\n");
					Controls.Add(lt);

					lt = new LiteralControl("<td>\n");
					Controls.Add(lt);
					// First
					BFirst =new System.Web.UI.WebControls.ImageButton();
					BFirst.EnableViewState = false;
					BFirst.ImageUrl = Page.ClientScript.GetWebResourceUrl(this.GetType(),
					 "Reportman.Web.first.bmp");
					BFirst.AlternateText = Translator.TranslateStr(221);
					BFirst.Click += clickevent;
					Controls.Add(BFirst);

					lt = new LiteralControl("</td>\n");
					Controls.Add(lt);

					lt = new LiteralControl("<td>\n");
					Controls.Add(lt);

					// Previous
					BPrevious = new System.Web.UI.WebControls.ImageButton();
					BPrevious.EnableViewState = false;
					BPrevious.ImageUrl = Page.ClientScript.
						GetWebResourceUrl(this.GetType(),
					 "Reportman.Web.previous.bmp");
					BPrevious.AlternateText = Translator.TranslateStr(223);
					BPrevious.Click += clickevent;
					Controls.Add(BPrevious);

					lt = new LiteralControl("</td>\n");
					Controls.Add(lt);


					lt = new LiteralControl("<td>\n");
					Controls.Add(lt);

					// Next
					BNext = new System.Web.UI.WebControls.ImageButton();
					BNext.EnableViewState = false;
					BNext.ImageUrl = Page.ClientScript.GetWebResourceUrl(this.GetType(),
					 "Reportman.Web.next.bmp");
					BNext.AlternateText = Translator.TranslateStr(225);
					BNext.Click += clickevent;
					Controls.Add(BNext);

					lt = new LiteralControl("</td>\n");
					Controls.Add(lt);

					lt = new LiteralControl("<td>\n");
					Controls.Add(lt);

					// Last
					BLast = new System.Web.UI.WebControls.ImageButton();
					BLast.EnableViewState = false;
					BLast.ImageUrl = Page.ClientScript.GetWebResourceUrl(this.GetType(),
					 "Reportman.Web.last.bmp");
					BLast.AlternateText = Translator.TranslateStr(227);
					BLast.Click += clickevent;
					Controls.Add(BLast);

					lt = new LiteralControl("</td>\n");
					Controls.Add(lt);

					lt = new LiteralControl("<td>\n");
					Controls.Add(lt);

					// Escale real
					BScale1 = new System.Web.UI.WebControls.ImageButton();
					BScale1.EnableViewState = false;
					BScale1.ImageUrl = Page.ClientScript.GetWebResourceUrl(this.GetType(),
					 "Reportman.Web.scale1.bmp");
					BScale1.AlternateText = Translator.TranslateStr(229);
					BScale1.Click += clickevent;
					Controls.Add(BScale1);

					lt = new LiteralControl("</td>\n");
					Controls.Add(lt);

					lt = new LiteralControl("<td>\n");
					Controls.Add(lt);

					// Print
					BPrint = new System.Web.UI.WebControls.ImageButton();
					BPrint.EnableViewState = false;
					BPrint.ImageUrl = Page.ClientScript.GetWebResourceUrl(this.GetType(),
					 "Reportman.Web.print.bmp");
					BPrint.AlternateText = Translator.TranslateStr(53);
					BPrint.Click += clickevent;
					Controls.Add(BPrint);

					lt = new LiteralControl("</td>\n");
					Controls.Add(lt);

					lt = new LiteralControl("<td>\n");
					Controls.Add(lt);

					// Zoom Minus
					BZoomMinus = new System.Web.UI.WebControls.ImageButton();
					BZoomMinus.EnableViewState = false;
					BZoomMinus.ImageUrl = Page.ClientScript.GetWebResourceUrl(
						this.GetType(), "Reportman.Web.zoom1.bmp");
					BZoomMinus.AlternateText = Translator.TranslateStr(235);
					BZoomMinus.Click += clickevent;
					Controls.Add(BZoomMinus);

					lt = new LiteralControl("</td>\n");
					Controls.Add(lt);

					lt = new LiteralControl("<td>\n");
					Controls.Add(lt);

					// Zoom Plus
					BZoomPlus = new System.Web.UI.WebControls.ImageButton();
					BZoomPlus.EnableViewState = false;
					BZoomPlus.ImageUrl = Page.ClientScript.GetWebResourceUrl(
						this.GetType(), "Reportman.Web.zoom2.bmp");
					BZoomPlus.AlternateText = Translator.TranslateStr(237);
					BZoomPlus.Click += clickevent;
					Controls.Add(BZoomPlus);

					lt = new LiteralControl("</td>\n");
					Controls.Add(lt);

					lt = new LiteralControl("<td>\n");
					Controls.Add(lt);

					// ComboBox Image Format
					BFormat = new System.Web.UI.WebControls.DropDownList();
					BFormat.AutoPostBack = true;
					Strings codecs = GraphicUtils.GetImageCodecs();
					BFormat.Items.Clear();
					foreach (string s in codecs)
					{
						BFormat.Items.Add(new ListItem(s));
					}
					BFormat.SelectedIndex = BFormat.Items.IndexOf(new ListItem(PreviewControl.Format));
					//					BFormat.AlternateText = Translator.TranslateStr(237);
					EventHandler clickeventformat = new EventHandler(Format_Click);
						BFormat.SelectedIndexChanged += clickeventformat;
					Controls.Add(BFormat);

					lt = new LiteralControl("</td>\n");
					Controls.Add(lt);


					lt = new LiteralControl("<td>" +
						HttpUtility.HtmlEncode(Translator.TranslateStr(269)) + "</td>\n");
					Controls.Add(lt);

					lt = new LiteralControl("<td>\n");
					Controls.Add(lt);
					// Go to page
					EPage=new System.Web.UI.WebControls.TextBox();
					EPage.Text = "";
					EPage.MaxLength = 6;
					EPage.Width = 50;
					Controls.Add(EPage);
					lt = new LiteralControl("</td>\n");
					Controls.Add(lt);
					
					lt = new LiteralControl("<td>\n");
					Controls.Add(lt);

					// Update page
					BPage = new System.Web.UI.WebControls.ImageButton();
					BPage.EnableViewState = false;
					BPage.ImageUrl = Page.ClientScript.GetWebResourceUrl(
						this.GetType(), "Reportman.Web.update.bmp");
					BPage.AlternateText = Translator.TranslateStr(1149);
					BPage.Click += clickevent;
					Controls.Add(BPage);

					lt = new LiteralControl("</td>\n");
					Controls.Add(lt);

					lt = new LiteralControl("<td>\n");
					Controls.Add(lt);

					// Label total pages
					LTotalPages = new System.Web.UI.WebControls.Label();
					Controls.Add(LTotalPages);

					lt = new LiteralControl("</td>\n");
					Controls.Add(lt);

					lt = new LiteralControl("</tr>\n");
					Controls.Add(lt);

					lt = new LiteralControl("</table>\n");
					Controls.Add(lt);
				}
			}
			lt = new LiteralControl("</td>\n");
			Controls.Add(lt);

			lt = new LiteralControl("</tr>\n");
			Controls.Add(lt);

			lt = new LiteralControl("<tr>\n");
			Controls.Add(lt);

			lt = new LiteralControl("<td>\n");
			Controls.Add(lt);

			Controls.Add(FPreviewControl);

			lt = new LiteralControl("</td>\n");
			Controls.Add(lt);

			lt = new LiteralControl("</tr>\n");
			Controls.Add(lt);

			lt = new LiteralControl("</table>\n");
			Controls.Add(lt);

			base.CreateChildControls();
			ChildControlsCreated = true;
		}
		public override void RenderBeginTag(HtmlTextWriter writer)
		{
			if (DesignMode)
			{
				base.RenderBeginTag(writer);
				return;
			} 
			EnsureChildControls();
			EPage.Text = "";
			if (FPreviewControl.CurrentPage >= 0)
				EPage.Text = (FPreviewControl.CurrentPage + 1).ToString();
			MetaFile meta = FPreviewControl.GetMetaFile();
			if (meta != null)
			{
				LTotalPages.Visible = meta.Pages.CurrentCount>0;
				if (!meta.Finished)
				{
					if (AsyncExecution)
						LTotalPages.Text = "Processing page: " + meta.Pages.CurrentCount.ToString("##,##0");
					else
						LTotalPages.Text = "";
				}
				else
					LTotalPages.Text = "Total pages: " + meta.Pages.CurrentCount.ToString("##,##0");
			}
			base.RenderBeginTag(writer);
		}
		protected override void Render(HtmlTextWriter output)
		{
			EnsureChildControls();
			base.Render(output);
		}
		protected override void RenderContents(HtmlTextWriter output)
		{
			EnsureChildControls();
			base.RenderContents(output);
		}
	}
}
