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
using System.Collections;
using Reportman;
using Reportman.Drawing;
using Reportman.Reporting;
#if REPMAN_DOTNET2
using System.Data;
using System.Data.Common;
#endif

namespace Reportman.Commands
{
	public class PrintReportPDF
	{
		static private Strings GetParams()
		{
			Strings aresult=new Strings();
			aresult.Add("Report Manager Dot Net Printreport");
			aresult.Add("Usage:");
			aresult.Add("printreport.exe [options] reportfile|-stdin");
			aresult.Add("-showdata DATASETNAME Open and show the dataset");
			aresult.Add("-testconnection CONNECTIONNAME Test a connection");
			aresult.Add("-showfields DATASETNAME filename Open and save field info");
			aresult.Add("-showprogress Show progress window");
			aresult.Add("-pdf pdffilename Output to pdf file");
			aresult.Add("-deletereport Delete the report file after completion");
			aresult.Add("-u Save pdf as uncompressed");
			aresult.Add("-eval exp Evaluate expression and show result");
			aresult.Add("-syntax exp Syntax check expression");
			aresult.Add("-syncexecution Do not process de report asynchronously");
			aresult.Add("-throw Throw exception on error, instead of message box");
#if REPMAN_DOTNET2
			aresult.Add("-getproviders filename Get the registered data provider names");
#endif
			return aresult;
		}
		static private void PrintParams()
		{
			Strings astrings=GetParams();
			for (int i=0;i<astrings.Count;i++)
				System.Console.WriteLine(astrings[i]);
		}

		/// <summary>
		/// PrintReport application gets a Report file (.rep) as input, process it
		/// using Reportman library and outputs the result to screen, printer, pdf
		/// and other formats.<br></br>
		/// Usage:
		/// <code>
		/// printreport.exe [options] reportfile|-stdin
		/// </code>
		/// This means options are optional, but a report file or -stdin must be specified.
		/// <list type="table">
		/// <listheader><term>Option</term>
		/// <description>Description</description></listheader>
		/// <item><term><nobr>-showprogress</nobr>
		/// </term><description>While calculating the report, show a progress 
		/// window, that is for the impatients</description>
		/// </item><item><term><nobr>-pdf filename</nobr></term>
		/// <description>Generate a pdf file, file name must be specified
		/// </description></item><item><term><nobr>-u</nobr></term>
		/// <description>The pdf output will be uncompressed, have effect
		/// only used with -pdf option</description></item>
		/// <item><term><nobr>-deletereport</nobr></term>
		/// <description>Delete the report file after execution, handle with
		/// care</description></item><item>
		/// <term><nobr>-syncexecution</nobr></term>
		/// <description>Do not process report asynchronously</description>
		/// </item><item><term><nobr>-throw</nobr></term>
		/// <description>Throw expecion on error, by default a window with 
		/// error message will pop up</description></item>
		/// </list>
		/// </summary>
		/// <remarks>
		/// <ul>
		/// <li>Report files can be designed and tested using
		/// <a href="http://reportman.sourceforge.net">Report Manager Designer</a>.
		/// </li>
		/// <li>The default output is the default printer.</li>
		/// <li>If an error occurs a window is shown to the user,
		/// unless you use -throw option</li>
		/// </ul>
		/// </remarks>
		/// <example>
		/// This command will generate a pdf document executing a report definition file:
		/// <code>
		/// printreport.exe myreport.rep -pdf mydocument.pdf
		/// </code>
		/// </example>
		/// <example>
		/// This command will show the preview window executing a report definition file:
		/// <code>
		/// printreport.exe myreport.rep -pdf mydocument.pdf
		/// </code>
		/// </example>
		/// <param name="args">Main entry point is a serie of strings, the strings are interpreted as command line options</param>
		[STAThread]
		public static void Main(string[] args)
		{
			bool dothrow=false;


			Report rp=new Report();
			try
			{
				bool asyncexecution = true;
				bool stdin=false;
				bool pdf=false;
				bool showprintdialog=false;
				string filename="";
				string pdffilename="";
				string fieldsfilename="";
				bool deletereport=false;
				bool compressedpdf=true;
				bool showfields=false;
				bool testconnection=false;
				string connectionname="";
				bool doprint=true;
				bool evaluatetext=false;
        bool syntaxcheck=false;
				bool doread = true;
        string evaltext="";
				string dataset="";
#if REPMAN_DOTNET2
				bool showproviders = false;
				string providersfilename = "";
#endif
				try
				{

					for (int i=0;i<args.Length;i++)
					{
						if (args[i].Length>0)
						{
							switch (args[i].ToUpper())
							{
								case "-STDIN":
									stdin=true;
									break;
								case "-SYNCEXECUTION":
									asyncexecution = false;
									break;
								case "-U":
									compressedpdf=false;
									break;
								case "-THROW":
									dothrow=true;
									break;
								case "-PDF":
									pdf=true;
                  if (args.GetUpperBound(0)>i)
                  {
                    i++;
                    pdffilename=args[i];
                  }
                  break;
								case "-TESTCONNECTION":
									doprint=false;
									testconnection=true;
									if (args.GetUpperBound(0)>i)
									{
										i++;
										connectionname=args[i];
									}
									break;
								case "-EVAL":
									doprint=false;
									evaluatetext=true;
									if (args.GetUpperBound(0)>i)
									{
										i++;
  									evaltext=args[i];
									}
									break;
								case "-SYNTAX":
									doprint=false;
									syntaxcheck=true;
									if (args.GetUpperBound(0)>i)
									{
										i++;
  									evaltext=args[i];
									}
									break;
								case "-SHOWFIELDS":
									showfields=true;
									doprint=false;
									if (args.GetUpperBound(0)>i)
									{
										i++;
										dataset=args[i];
									}
									if (args.GetUpperBound(0)>i)
									{
										i++;
										fieldsfilename=args[i];
									}
									break;
#if REPMAN_DOTNET2
								case "-GETPROVIDERS":
									showproviders = true;
									doprint = false;
									doread = false;
									if (args.GetUpperBound(0) > i)
									{
										i++;
										providersfilename = args[i];
									}
									break;
#endif
								case "-DELETEREPORT":
									deletereport=true;
									break;
								case "-PRINTDIALOG":
									showprintdialog=true;
									break;
								default:
									if (args[i][0]=='-')
										throw new Exception("Invalid argument:"+args[i]);
									if (filename.Length>0)
									{
										filename=args[i];
									}
									else
										filename=args[i];								
									break;
							}
						}
					}
#if REPMAN_DOTNET2
					if (showproviders)
					{
						int indexp;
						DataTable atable = DbProviderFactories.GetFactoryClasses();
						if (providersfilename.Length == 0)
						{
							string messageproviders = "";
							for (indexp = 0; indexp < atable.Rows.Count ; indexp++)
							{
								if (messageproviders.Length != 0)
									messageproviders = messageproviders + (char)13 + (char)10;
								messageproviders = messageproviders + atable.Rows[indexp][2].ToString();
							}
							System.Console.WriteLine(messageproviders);
						}
						else
						{
							FileStream providersstream = new FileStream(providersfilename, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.None);
							try
							{
								for (indexp = 0; indexp < atable.Rows.Count; indexp++)
								{
									StreamUtil.SWriteLine(providersstream, atable.Rows[indexp][2].ToString());
								}
							}
							finally
							{
								providersstream.Close();
							}
						}
					}
#endif
					if (doread)
					{
						if (stdin)
						{
							Stream astream = System.Console.OpenStandardInput();
							rp.LoadFromStream(astream, 8192);
						}
						else
						{
							rp.LoadFromFile(filename);
						}
					}
					if (testconnection)
					{
						int conindex=rp.DatabaseInfo.IndexOf(connectionname);
						if (conindex<0)
							throw new Exception("Connection name not found:"+connectionname);
						rp.DatabaseInfo[conindex].Connect();
						System.Console.WriteLine("Connexion successfull:"+connectionname);
					}
					if (showfields)
					{
            int index=rp.DataInfo.IndexOf(dataset);
            if (index<0)
						  throw new Exception("Dataset not found:"+dataset);
            rp.DataInfo[index].Connect();
				    FileStream fstream=new FileStream(fieldsfilename,
							System.IO.FileMode.Create,System.IO.FileAccess.Write,
							System.IO.FileShare.None);
				    try
				    {
              rp.DataInfo[index].GetFieldsInfo(fstream);
            }
            finally
            {
              fstream.Close();
            }
					}
					if ((evaluatetext) || (syntaxcheck))
					{
						rp.PrintOnlyIfDataAvailable=false;
						PrintOutPDF printpdf2=new PrintOutPDF();
						rp.BeginPrint(printpdf2);
            if (evaluatetext)
            {
              try
              {
                Variant aresult=rp.Evaluator.EvaluateText(evaltext);
   						  System.Console.WriteLine("Result:"+aresult.ToString());
              }
              catch(EvalException e)
              {
						   System.Console.WriteLine("Error Line: "+e.SourceLine.ToString()+
                " Error position:"+e.SourcePos.ToString()+" - "+e.Message);
              }
              catch(Exception E)
              {
						   System.Console.WriteLine("Error: "+E.Message);
              }
            }
            else
            {
              try
              {
               rp.Evaluator.CheckSyntax(evaltext);
						   System.Console.WriteLine("Syntax check ok");
              }
              catch(Exception E)
              {
						   System.Console.WriteLine("Error: "+E.Message);
              }
            }
          }
					if (doprint)
					{
						if (pdf)
						{
							rp.AsyncExecution=false;
							PrintOutPDF printpdf = new PrintOutPDF();
							printpdf.FileName=pdffilename;
							printpdf.Compressed=compressedpdf;
							printpdf.Print(rp.MetaFile);
						}
						else
						{
							PrintOutNet prw= new PrintOutNet();
							rp.AsyncExecution=false;
							prw.Print(rp.MetaFile);
						}
					}
				}
				finally
				{
					if (deletereport)
						if (filename.Length>0)
							System.IO.File.Delete(filename);
				}

			}
			catch(Exception E)
			{
				if (!dothrow)
				{
					int i;
					string amessage=E.Message+(char)13+(char)10;
					for (i=0;i<args.Length;i++)
					{
						amessage=amessage+(char)13+(char)10+"Arg"+i.ToString()+":"+args[i];
					}
					Strings astrings=GetParams();
					for (i=0;i<astrings.Count;i++)
					{
						amessage=amessage+(char)13+(char)10+astrings[i];
					}

					System.Console.WriteLine(amessage,"Error");
				}
				else
				{
					PrintParams();
					throw;
				}
//				Variant.WriteStringToUTF8Stream(amessage,System.Console.OpenStandardError());
			}
		}
	}
}
