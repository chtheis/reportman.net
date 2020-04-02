using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Reportman.Web
{
	[DefaultProperty("Text")]
	[ToolboxData("<{0}:PreviewMetafile runat=server></{0}:PreviewMetafile>")]
	public class PreviewMetafile : WebControl
	{
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue("")]
		[Localizable(true)]
		public string Text
		{
			get
			{
				String s = (String)ViewState["Text"];
				return ((s == null) ? String.Empty : s);
			}

			set
			{
				ViewState["Text"] = value;
			}
		}

		protected override void RenderContents(HtmlTextWriter output)
		{
			output.Write(Text);
		}
	}
}
