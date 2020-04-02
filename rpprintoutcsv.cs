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
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Reportman.Drawing
{
    /// <summary>
    /// Report preocessing driver, capable of generate csv files
    /// </summary>
	public static class PrintOutCSV
	{
      public static string ExportToCSV(MetaFile nmeta,
           bool allpages,int frompage,int topage,string separator,char delimiter)
      {
         return ExportToCSV(nmeta,allpages,frompage,topage,separator,delimiter,10);
      }
      public static string ExportToCSV(MetaFile nmeta, 
           bool allpages, int frompage, int topage, string separator, char delimiter, int precision)
      {
         int j,k;
         string[,] pmatrix;
         MetaPage apage;
         SortedList<string, int> columns = new SortedList<string, int>();
         SortedList<string, int> rows = new SortedList<string, int>();
         StringBuilder nbuilder = new StringBuilder();
         int index;
         string topstring;
         string leftstring;

         if (allpages)
         {
            nmeta.RequestPage(MetaFile.MAX_NUMBER_PAGES);
            frompage = 0;
            topage = nmeta.Pages.CurrentCount - 1;
         }
         else
         {
            frompage = frompage - 1;
            topage = topage - 1;
            nmeta.RequestPage(topage);
            if (topage > nmeta.Pages.CurrentCount - 1)
               topage = nmeta.Pages.Count - 1;
         }
         // First distribute in columns
         columns.Clear();
         for (int i = frompage; i <= topage; i++)
         {
            apage = nmeta.Pages[i];
            for (j = 0; j < apage.Objects.Count; j++)
            {
               MetaObject nobject = apage.Objects[j];
               leftstring = (nobject.Left / precision).ToString("0000000000");
               index = columns.IndexOfKey(leftstring);
               if (index < 0)
                  columns.Add(leftstring, 1);
               else
                  columns[leftstring] = columns[leftstring] + 1;
            }
         }

         // Distribute in rows and columns
         for (int i = frompage; i <= topage; i++)
         {
            apage = nmeta.Pages[i];
            rows.Clear();
            for (j = 0; j < apage.Objects.Count; j++)
            {
               MetaObject nobject = apage.Objects[j];
               if (nobject.MetaType == MetaObjectType.Text)
               {
                  topstring = (nobject.Top / precision).ToString("0000000000");
                  index = rows.IndexOfKey(topstring);
                  if (index < 0)
                     rows.Add(topstring, 1);
                  else
                     rows[topstring] = rows[topstring]+1;
               }
            }
            pmatrix = new string[rows.Count,columns.Count];
            for (j = 0; j < apage.Objects.Count; j++)
            {
               MetaObject nobject = apage.Objects[j];
               PrintObject(pmatrix, apage, nobject,
                  precision,rows,columns);
            }
            for (j = 0; j < rows.Count; j++)
            {
               for (k = 0; k < columns.Count; k++)
               {
                  if (k != 0)
                     nbuilder.Append(separator);
                  string nvalue = pmatrix[j, k];
                  if (nvalue == null)
                     nvalue = "";
                  nbuilder.Append(StringUtil.CustomQuoteStr(nvalue,delimiter));
               }
               nbuilder.Append(System.Environment.NewLine);
            }
            // Page separator is new line
            nbuilder.Append(System.Environment.NewLine);
         }
         return nbuilder.ToString();
      }
      static void PrintObject(string[,] pmatrix,MetaPage page,MetaObject obj,int precision,SortedList<string,int> rows,
         SortedList<string,int> columns)
      {
         string topstring = (obj.Top / precision).ToString("0000000000");
         string leftstring = (obj.Left / precision).ToString("0000000000");
         int arow = rows.IndexOfKey(topstring);
         int acolumn = columns.IndexOfKey(leftstring);
         switch (obj.MetaType)
         {
            case MetaObjectType.Text:
               pmatrix[arow,acolumn] = page.GetText((MetaObjectText)obj);
               break;
            default:
               break;
         }
      }
   }
}
