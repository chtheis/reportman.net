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
using System.Data.Common;
using System.Data;
using System.Drawing;
using Reportman.Drawing;
using Reportman.Reporting;

namespace Reportman.Designer
{
    internal class DesignerInterfaceDbInfo : DesignerInterface
    {
        private DatabaseInfo FDbInfo;
        static SortedList<IsolationLevel, string> Isolations;
        
        public DesignerInterfaceDbInfo(SortedList<int, ReportItem> repitem)
            : base(repitem)
        {
            if (repitem.Count == 1)
              FDbInfo = (DatabaseInfo)repitem.Values[0];
        }
        static DesignerInterfaceDbInfo()
        {
            Isolations = new SortedList<IsolationLevel, string>();
            Isolations.Add(IsolationLevel.Chaos, "Chaos");
            Isolations.Add(IsolationLevel.ReadCommitted, "Read Committed");
            Isolations.Add(IsolationLevel.ReadUncommitted, "Read Uncommited");
            Isolations.Add(IsolationLevel.RepeatableRead, "Repeatable Read");
            Isolations.Add(IsolationLevel.Serializable, "Serializable");
            Isolations.Add(IsolationLevel.Snapshot, "Snapshot");
            Isolations.Add(IsolationLevel.Unspecified, "Unspecified");
        }
        public DatabaseInfo DbInfo { get { return FDbInfo; } }
        public static string DriverTypeToString(DriverType driver)
        {
            string aresult = "";
            switch (driver)
            {
                case DriverType.DBExpress:
                    aresult = "Borland DBExpress";
                    break;
                case DriverType.BDE:
                    aresult = "Borland Database Engine";
                    break;
                case DriverType.IBX:
                    aresult = "Interbase Express";
                    break;
                case DriverType.ADO:
                    aresult = "Microsoft DAO";
                    break;
                case DriverType.IBO:
                    aresult = "Interbase Objects";
                    break;
                case DriverType.Mybase:
                    aresult = "B.MyBase and text files";
                    break;
                case DriverType.ZEOS:
                    aresult = "Zeos Database Objects";
                    break;
                case DriverType.DotNet:
                    aresult = "Dot Net Connection";
                    break;
                case DriverType.DotNet2:
                    aresult = "Dot Net 2 Connection";
                    break;
            }
            return aresult;
        }
        public static DriverType StringToDriverType(string astring)
        {
            DriverType aresult = DriverType.DotNet2;
            switch (astring)
            {
                case "Borland DBExpress":
                    aresult = DriverType.DBExpress;
                    break;
                case "Borland Database Engine":
                    aresult = DriverType.BDE;
                    break;
                case "Interbase Express":
                    aresult = DriverType.IBX;
                    break;
                case "Microsoft DAO":
                    aresult = DriverType.ADO;
                    break;
                case "Interbase Objects":
                    aresult = DriverType.IBO;
                    break;
                case "B.MyBase and text files":
                    aresult = DriverType.Mybase;
                    break;
                case "Zeos Database Objects":
                    aresult = DriverType.ZEOS;
                    break;
                case "Dot Net Connection":
                    aresult = DriverType.DotNet;
                    break;
                case "Dot Net 2 Connection":
                    aresult = DriverType.DotNet2;
                    break;
            }
            return aresult;
        }
        public static Strings GetDriverStrings()
        {
            Strings aresult = new Strings();
            aresult.Add("Borland DBExpress");
            aresult.Add("B.MyBase and text files");
            aresult.Add("Interbase Express");
            aresult.Add("Borland Database Engine");
            aresult.Add("Microsoft DAO");
            aresult.Add("Interbase Objects");
            aresult.Add("Zeos Database Objects");
            aresult.Add("Dot Net Connection");
            aresult.Add("Dot Net 2 Connection");
            return aresult;
        }
      public void GetIsolations(Strings lpossiblevalues)
      {
          foreach (string nstring in Isolations.Values)
              lpossiblevalues.Add(nstring);
      }
      public override void GetPropertyValues(string pname, Strings lpossiblevalues)
      {
        if (pname == Translator.TranslateStr(67))
        {
          Strings nstrings = GetDriverStrings();
          foreach (string ns in nstrings)
            lpossiblevalues.Add(ns);
          return;
        }
        if (pname == Translator.TranslateStr(1394))
        {
          bool firebirdfound = false;
          DataTable ntable = DbProviderFactories.GetFactoryClasses();
          foreach(DataRow nrow in ntable.Rows)
          {
              string newvalue = nrow["InvariantName"].ToString();
            lpossiblevalues.Add(newvalue);
            if (newvalue == "FirebirdSql.Data.Firebird")
                firebirdfound = true;
          }
          if (!firebirdfound)
              lpossiblevalues.Add("FirebirdSql.Data.Firebird");
          return;
        }
        if (pname == "Transaction Isolation")
        {
          GetIsolations(lpossiblevalues);
          return;
        }
        base.GetPropertyValues(pname, lpossiblevalues);
      }
        public override void GetProperties(Strings lnames, Strings ltypes, Variants lvalues, Strings lhints, Strings lcat)
        {
            lnames.Clear();
            ltypes.Clear();
            lhints.Clear();
            lcat.Clear();
            if (lvalues != null)
                lvalues.Clear();
            // Connection Name
            lnames.Add(Translator.TranslateStr(400));
            ltypes.Add(Translator.TranslateStr(557));
            lhints.Add("refdatabaseinfo.html");
            lcat.Add(Translator.TranslateStr(1201));
            if (lvalues != null)
                lvalues.Add(FDbInfo.Alias);
            // Driver
            lnames.Add(Translator.TranslateStr(67));
            ltypes.Add(Translator.TranslateStr(569));
            lhints.Add("refdatabaseinfo.html");
            lcat.Add(Translator.TranslateStr(1201));
            if (lvalues != null)
                lvalues.Add(DriverTypeToString(FDbInfo.Driver));
            // Provider Factory (Dot net driver)
            lnames.Add(Translator.TranslateStr(1394));
            ltypes.Add(Translator.TranslateStr(961));
            lhints.Add("refdatabaseinfo.html");
            lcat.Add(Translator.TranslateStr(1201));
            if (lvalues != null)
                lvalues.Add(FDbInfo.ProviderFactory);
            // Connection String
            lnames.Add(Translator.TranslateStr(1099));
            ltypes.Add(Translator.TranslateStr(1099));
            lhints.Add("refdatabaseinfo.html");
            lcat.Add(Translator.TranslateStr(1201));
            if (lvalues != null)
                lvalues.Add(FDbInfo.ConnectionString);
            // Transaction isolation
            lnames.Add("Transaction Isolation");
            ltypes.Add(Translator.TranslateStr(569));
            lhints.Add("refdatabaseinfo.html");
            lcat.Add(Translator.TranslateStr(1201));
            if (lvalues != null)
                lvalues.Add(Isolations[FDbInfo.TransIsolation]);
        }
        public override Variant GetProperty(string pname)
        {
            // Connection Name
            if (pname == Translator.TranslateStr(400))
                return FDbInfo.Alias;
            // Driver
            if (pname == Translator.TranslateStr(67))
                return (int)FDbInfo.Driver;
            // Provider factory
            if (pname == Translator.TranslateStr(1394))
                return FDbInfo.ProviderFactory;
            // Connection string
            if (pname == Translator.TranslateStr(1099))
                return FDbInfo.ConnectionString;
            // Transaction isolation
              if (pname == "Transaction Isolation")
                return Isolations[FDbInfo.TransIsolation];
            return base.GetProperty(pname);
        }
        public override void SetProperty(string pname, Variant newvalue)
        {
            // Connection Name
            if (pname == Translator.TranslateStr(400))
            {
                FDbInfo.Alias = newvalue.ToString();
                return;
            }
            // Driver
            if (pname == Translator.TranslateStr(67))
            {
              Strings nstrings = GetDriverStrings();
              int nindx = nstrings.IndexOf(newvalue);
              if (nindx >= 0)
                FDbInfo.Driver = (DriverType)nindx;
              return;
            }
            // Provider factory
            if (pname == Translator.TranslateStr(1394))
            {
              FDbInfo.ProviderFactory = newvalue;
              return;
            }
            // Connection string
            if (pname == Translator.TranslateStr(1099))
            {
              FDbInfo.ConnectionString = newvalue;
              return;
            }
            // Transaction isolation
            if (pname == "Transaction Isolation")
            {
                int ix = Isolations.IndexOfValue(newvalue);
                if (ix >= 0)
                {
                    FDbInfo.TransIsolation = Isolations.Keys[ix];
                }
              return;
            }
            // inherited
            base.SetProperty(pname, newvalue);
        }
    }
  internal class DesignerInterfaceDataInfo : DesignerInterface
  {
    private DataInfo FDataInfo;
    public DesignerInterfaceDataInfo(SortedList<int, ReportItem> repitem)
      : base(repitem)
    {
      if (repitem.Count == 1)
        FDataInfo = (DataInfo)repitem.Values[0];
    }
    public DataInfo DataInfo { get { return FDataInfo; } }
    public Strings GetAvaliableConnections()
    {
        Strings lpossiblevalues = new Strings();
        foreach (DatabaseInfo dbinfo in this.ReportItemObject.Report.DatabaseInfo)
        {
            lpossiblevalues.Add(dbinfo.Alias);
        }
        return lpossiblevalues;
    }
      public Strings GetAvaliableMasterDataSets()
      {
          Strings lpossiblevalues = new Strings();
          lpossiblevalues.Add("");
          foreach (DataInfo dinfo in this.ReportItemObject.Report.DataInfo)
          {
              if (dinfo.Alias!=FDataInfo.Alias)
                lpossiblevalues.Add(dinfo.Alias);
          }
          return lpossiblevalues;
      }

    public override void GetPropertyValues(string pname, Strings lpossiblevalues)
    {
        if (pname == Translator.TranslateStr(154))
        {
            Strings lconnections = GetAvaliableConnections();
            lpossiblevalues.Clear();
            lpossiblevalues.Add("");
            foreach (string nstring in lconnections)
                lpossiblevalues.Add(nstring);
            return;
        }
        if (pname == Translator.TranslateStr(155))
        {
            Strings ldatasets = GetAvaliableMasterDataSets();
            lpossiblevalues.Clear();
            lpossiblevalues.Add("");
            foreach (string nstring in ldatasets)
                lpossiblevalues.Add(nstring);
            return;
            return;
        }
        base.GetPropertyValues(pname, lpossiblevalues);
    }
    public override void GetProperties(Strings lnames, Strings ltypes, Variants lvalues, Strings lhints, Strings lcat)
    {
        lnames.Clear();
        ltypes.Clear();
        lhints.Clear();
        lcat.Clear();
        if (lvalues != null)
            lvalues.Clear();
        // DataSet Name
        lnames.Add(Translator.TranslateStr(518));
        ltypes.Add(Translator.TranslateStr(557));
        lhints.Add("refdatainfo.html");
        lcat.Add(Translator.TranslateStr(1201));
        if (lvalues != null)
            lvalues.Add(FDataInfo.Alias);
        // Connection
        lnames.Add(Translator.TranslateStr(154));
        ltypes.Add(Translator.TranslateStr(569));
        lhints.Add("refdatainfo.html");
        lcat.Add(Translator.TranslateStr(1201));
        if (lvalues != null)
            lvalues.Add(FDataInfo.DatabaseAlias);
        // Master dataset
        lnames.Add(Translator.TranslateStr(155));
        ltypes.Add(Translator.TranslateStr(569));
        lhints.Add("refdatainfo.html");
        lcat.Add(Translator.TranslateStr(1201));
        if (lvalues != null)
            lvalues.Add(FDataInfo.DataSource);
        // SQL
        lnames.Add("SQL");
        ltypes.Add("SQL");
        lhints.Add("refdatainfo.html");
        lcat.Add(Translator.TranslateStr(1201));
        if (lvalues != null)
            lvalues.Add(FDataInfo.SQL);
    }
    public override Variant GetProperty(string pname)
    {
        // Alias Name
        if (pname == Translator.TranslateStr(518))
            return FDataInfo.Alias;
        // Connection name
        if (pname == Translator.TranslateStr(154))
            return FDataInfo.DatabaseAlias;
        // Master dataset
        if (pname == Translator.TranslateStr(155))
            return FDataInfo.DataSource;
        // SQL
        if (pname == "SQL")
            return FDataInfo.SQL;
        return base.GetProperty(pname);
    }
    public override void SetProperty(string pname, Variant newvalue)
    {
        // DataSet Name
        if (pname == Translator.TranslateStr(518))
        {
            FDataInfo.Alias = newvalue.ToString();
            return;
        }
        // Connection name
        if (pname == Translator.TranslateStr(154))
        {
            Strings nstrings = GetAvaliableConnections();
            int nindx = nstrings.IndexOf(newvalue.ToString());
            if (nindx >= 0)
                FDataInfo.DatabaseAlias = newvalue.ToString();
            return;
        }
        // Master DataSet
        if (pname == Translator.TranslateStr(155))
        {
            Strings nstrings = GetAvaliableMasterDataSets();
            int nindx = nstrings.IndexOf(newvalue.ToString());
            if (nindx >= 0)
                FDataInfo.DataSource = newvalue.ToString();
            return;
        }
        // SQL
        if (pname == "SQL")
        {
            FDataInfo.SQL = newvalue.ToString();
            return;
        }
        // inherited
        base.SetProperty(pname, newvalue);
    }
 }
}
