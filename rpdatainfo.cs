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
using Reportman.Drawing;
using System.Data;
using System.Collections;
#if REPMAN_MONO
using Mono.Data;
#endif
#if REPMAN_DOTNET2
using System.Data.Common;
using System.Collections.Generic;
#else
#if REPMAN_OLEDB
using System.Data.OleDb;
#endif
#if REPMAN_SQL
using System.Data.SqlClient;
#endif
#if REPMAN_SQLCE
using System.Data.SqlServerCe;
#endif
#if REPMAN_ORACLE
using System.Data.OracleClient;
#endif
#if REPMAN_ODBC
using System.Data.Odbc;
#endif
#if REPMAN_FIREBIRD
using FirebirdSql.Data.Firebird;
#endif
#if REPMAN_POSTGRESQL
using Npgsql;
#endif
#if REPMAN_SQLITE
using Mono.Data.SqliteClient;
#endif
#if REPMAN_DB2
using IBM.Data.DB2;
#endif
#if REPMAN_SYBASE
using Mono.Data.SybaseClient;
#endif
#if REPMAN_MYSQL
using MySql.Data.MySqlClient;
#endif
#endif
namespace Reportman.Reporting
{
    /// <summary>
    /// A dataset definition could represent a table inside a database or
    /// a sql query, in .Net version, it's always a Query. <see cref="Variant">DataInfo</see>
    /// </summary>
    public enum DatasetType {
        /// <summary>The DataInfo will open (execute) a query</summary>
        Query,
        /// <summary>The DataInfo will open a table inside a database</summary>
        Table
    };
    /// <summary>
    /// The DriverType is part of a report database information definition, indicating the technology to be used
    /// to connect to the database, in .Net version you can choose DotNet or DotNet2 depending on the version you 
    /// are using. <see cref="Variant">DatabaseInfo</see>
    /// </summary>
    public enum DriverType
	{
        /// <summary>Borland database driver optimized for using disconnected DataSets. Not available in .Net</summary>
        DBExpress,
        /// <summary>In memory database driver, it can be used to read DataSets into memory for further processing</summary>
        /// <remarks>When used in .Net it's capable of reading DataSets</remarks>
        Mybase,
        /// <summary>Borland Interbase Express database driver, can access also Firebird databases.
        /// Not available in .Net</summary>
        IBX,
        /// <summary>Borland Database Engine database driver, can access Paradox,Dbase, and some SQL Databases in native mode.
        /// Not available in .Net</summary>
        BDE,
        /// <summary>Borland Interface to Microsoft DAO, can access any OleDB (including Microsoft Jet) or ODBC databases.
        /// Not available in .Net</summary>
        ADO,
        /// <summary>Database access objects to access Interbase or 4rd databases known as Interbase Objects.
        /// Not available in .Net</summary>
        IBO,
        /// <summary>Opensource Database Objects to access multiple sql database technologies (zeoslib).
        /// Not available in .Net</summary>
        ZEOS,
        /// <summary>Driver used to connect to databases in .Net 1.x, because no abstraction is provided in .Net 1.x, 
        /// you must select alse the driver (object instances) to be used. <see cref="Variant">DotNetDriverType</see>
        /// Available only in .Net 1.x</summary>
        DotNet,
        /// <summary>Driver used to connect to databases in .Net 2.x, it's used with ProviderFactory property inside
        /// DatabaseInfo to perform the connection<see cref="Variant">DatabaseInfo</see>
        /// Available only in .Net 2.x</summary>
        DotNet2
	};
    /// <summary>
    /// The DotNetDriverType indicates the database provider to be used when you select DriverType.DotNet, that
    /// is .Net 1.x, in this versions of .Net there were no abstraction for Database access, so you must select
    /// and pre-link into your executable references to the database client library. 
    /// This type is not used in .Net 2.x because the functionality of database providers, 
    /// configured in machine.config or application.config<see cref="Variant">DatabaseInfo</see>
    /// </summary>
    public enum DotNetDriverType
	{
        /// <summary>OleDb .Net 1.x Data Provider</summary>
		OleDb,
        /// <summary>Odbc .Net 1.x Data Provider</summary>
        Odbc,
        /// <summary>Firebird for .Net 1.x Data Provider</summary>
        Firebird,
        /// <summary>SqlServer .Net 1.x Data Provider</summary>
        Sql,
        /// <summary>PostgreSql .Net 1.x Data Provider</summary>
        PostgreSql,
        /// <summary>MySql .Net 1.x Data Provider</summary>
        MySql,
        /// <summary>SQLite .Net 1.x Data Provider</summary>
        SQLite,
        /// <summary>Oracle .Net 1.x Data Provider</summary>
        Oracle,
        /// <summary>DB2 .Net 1.x Data Provider</summary>
        DB2,
        /// <summary>Sybase .Net 1.x Data Provider</summary>
        Sybase,
        /// <summary>SqlCE .Net 1.x Data Provider</summary>
        SqlCE
	};
    /// <summary>
    /// DatabaseInfo stores information about database connectivity, a Report have a collection
    /// of connection definitions. Each connection can use diferent connectivity technology 
    /// (database providers). <see cref="Variant">Report</see><see cref="Variant">DatabaseInfos</see>
    /// </summary>
	public class DatabaseInfo : ReportItem,ICloneable
	{
        public static string FIREBIRD_PROVIDER = "Firebird.Data.FirebirdClient";
        public static string FIREBIRD_PROVIDER2 = "FirebirdSql.Data.Firebird";
        public static string MYSQL_PROVIDER = "MySql.Data.MySqlClient";
        public static string SQLITE_PROVIDER = "System.Data.SQLite";
        private System.Data.IDbConnection FConnection;
        private System.Data.IDbConnection FExternalConnection;
        private System.Data.IDbTransaction IntTransaction;
        /// <summary>
        /// Obtain current active transaction to execute querys
        /// </summary>
        public System.Data.IDbTransaction CurrentTransaction
        {
            get
            {
                if (Transaction != null)
                    return Transaction;
                else
                    return IntTransaction;
            }
        }
        /// <summary>
        /// You can assign a transaction, so this transaction will be used instead a default created one
        /// </summary>
        public System.Data.IDbTransaction Transaction;
        /// <summary>
        /// You can assign a Connection, so this connection will be used, if you provide a connection, 
        /// all the settings inside DatabaseInfo related to perform connection will be ignored and all
        /// querys will be executed against the provided connection
        /// </summary>
        public System.Data.IDbConnection Connection
        {
            get
            {
                if (FExternalConnection != null)
                    return FExternalConnection;
                else
                    return FConnection;
            }
            set
            {
                if (value == null)
                {
                    FConnection = null;
                    FExternalConnection = null;
                }
                else
                {
                    if (FConnection != value)
                    {
                        FExternalConnection = value;
                    }
                }
            }
        }
        public IDbCommandExecuter SqlExecuter;
        protected override string GetClassName()
        {
            return "TRPDATABASEINFOITEM";
        }
        /// <summary>
        /// Clone the DatabaseInfo item
        /// </summary>
        /// <returns>A new DatabaseInfo item with same data as the original</returns>
		public object Clone()
		{
			DatabaseInfo ninfo=new DatabaseInfo(Report);
			ninfo.Alias=Alias;
			ninfo.Driver=Driver;
			ninfo.ProviderFactory=ProviderFactory;
			ninfo.ReportTable=ReportTable;
			ninfo.ReportSearchField=ReportSearchField;
			ninfo.Name=Name;
			ninfo.ReportField=ReportField;
			ninfo.ReportGroupsTable=ReportGroupsTable;
			ninfo.ConnectionString=ConnectionString;
            ninfo.FExternalConnection = FExternalConnection;
            ninfo.Transaction = Transaction;
            ninfo.DotNetDriver = DotNetDriver;
            ninfo.TransIsolation = TransIsolation;
			return ninfo;
		}			
        /// <summary>
        /// Clone the DatabaseInfo item
        /// </summary>
        /// <param name="areport">Report to assign to the item</param>
        /// <returns>A new DatabaseInfo item with same data as the original</returns>
        public DatabaseInfo Clone(Report areport)
		{
			DatabaseInfo ninfo=(DatabaseInfo)Clone();
			ninfo.Report=areport;
			return ninfo;
		}			
        /// <summary>DatabaseInfo item name</summary>
		public string Alias;
        /// <summary>DatabaseInfo driver type</summary>
        public DriverType Driver;
        /// <summary>Provider factory, only for .Net 2.x driver</summary>
        public string ProviderFactory;
        /// <summary>Report table name when loading report items from a connection</summary>
        public string ReportTable;
        /// <summary>Report search field when loading report items from a connection</summary>
        public string ReportSearchField;
        /// <summary>Report field when loading report items from a connection</summary>
        public string ReportField;
        /// <summary>Report groups table when loading report items from a connection</summary>
        public string ReportGroupsTable;
        /// <summary>Connection string, for ADO and .Net drivers</summary>
        public string ConnectionString;
        /// <summary>DotNet driver type</summary>
        public DotNetDriverType DotNetDriver;
        /// <summary>
        /// Default isolation level if a transaction have not assigned, all the querys related
        /// to this connection will run inside the same transaction
        /// </summary>
        public System.Data.IsolationLevel TransIsolation;
        /// <summary>
        /// Constructor
        /// </summary>
		public DatabaseInfo(BaseReport rp)
			: base(rp)
		{
#if REPMAN_DOTNET1
			Driver = DriverType.DotNet;
#else
			Driver = DriverType.DotNet2;
#endif
			ReportTable = "REPMAN_REPORTS";
			ReportGroupsTable = "REPMAN_GROUPS";
			ReportSearchField = "REPORT_NAME";
			ReportField = "REPORT";
			Alias = "";
			this.ProviderFactory = "";
			ReportTable = ""; ReportSearchField = ""; ReportField = ""; ReportGroupsTable = "";
			ConnectionString = "";
			TransIsolation = System.Data.IsolationLevel.ReadCommitted;
//           	TransIsolation = System.Data.IsolationLevel.RepeatableRead;
        }
        /// <summary>
        /// Disconnect from database, also dispose any transaction
        /// </summary>
		public void DisConnect()
		{
            if (DriverType.Mybase == Driver)
                return;
            if (Connection != null)
			{
				if (IntTransaction != null)
				{
					IntTransaction.Commit();
                    IntTransaction.Dispose();
					IntTransaction = null;
				}
                if (FConnection != null)
                {
                    FConnection.Close();
                    FConnection.Dispose();
                    FConnection = null;
                }
			}
		}
#if REPMAN_COMPACT
#else
        public static SortedList<string, DbProviderFactory> CustomProviderFactories = new SortedList<string, DbProviderFactory>();
#endif
        /// <summary>
        /// Connect to the database
        /// </summary>
        public void Connect()
		{
            if (DriverType.Mybase == Driver)
                return;
            if (SqlExecuter != null)
                return;
                
            if (Connection != null)
            {
                if (Transaction == null)
                    if (IntTransaction==null)
                        IntTransaction = Connection.BeginTransaction(TransIsolation);
                return;
            }
			string UsedConnectionString=ConnectionString;
			int index=Report.Params.IndexOf("ADOCONNECTIONSTRING");
			if (index>0)
				UsedConnectionString=Report.Params[index].Value.ToString();
            index = Report.Params.IndexOf(Alias+"_ADOCONNECTIONSTRING");
            if (index > 0)
                UsedConnectionString = Report.Params[index].Value.ToString();
			
#if REPMAN_DOTNET2
            if (Driver == DriverType.IBX)
                this.ProviderFactory = FIREBIRD_PROVIDER;
			if (this.ProviderFactory.Length == 0)
				throw new UnNamedException("Provider factory not supplied");
/*#if REPMAN_MONO
			IDbConnection aconnection=null;
			Provider provider = Mono.Data.ProviderFactory.Providers[this.ProviderFactory];
			if (provider!=null)
				aconnection=provider.CreateConnection();
			if (aconnection==null)
				throw new NamedException("Mono.Data.ProviderFactory provider not found:" + this.ProviderFactory.ToString(), DotNetDriver.ToString());
			aconnection.ConnectionString = UsedConnectionString;
			aconnection.Open();
			FConnection = aconnection;
#else*/
#if REPMAN_COMPACT
#else
            DbProviderFactory afactory = null;
            if (CustomProviderFactories.IndexOfKey(ProviderFactory) >= 0)
                afactory = CustomProviderFactories[ProviderFactory];
            if (afactory == null)
			    afactory = DbProviderFactories.GetFactory(this.ProviderFactory);
			if (afactory==null)
				throw new NamedException("System.Data.Common.DbProviderFactories Factory not found:" + this.ProviderFactory.ToString(), DotNetDriver.ToString());
			DbConnection aconnection=null;
			aconnection = afactory.CreateConnection();
			aconnection.ConnectionString = UsedConnectionString;
			aconnection.Open();
			FConnection = aconnection;
            if (Transaction==null)
                IntTransaction = FConnection.BeginTransaction(TransIsolation);
#endif
//#endif
#else
			switch (DotNetDriver)
			{
#if REPMAN_OLEDB
				case DotNetDriverType.OleDb:
					System.Data.OleDb.OleDbConnection OleDbConn;
					OleDbConn = new OleDbConnection(UsedConnectionString);
					OleDbConn.Open();
					FConnection = OleDbConn;
					break;
#endif
#if REPMAN_ODBC
				case DotNetDriverType.Odbc:
					System.Data.Odbc.OdbcConnection OdbcConn;
					OdbcConn = new OdbcConnection(UsedConnectionString);
					OdbcConn.Open();
					FConnection = OdbcConn;
					break;
#endif
#if REPMAN_FIREBIRD
				case DotNetDriverType.Firebird:
					FbConnection fbconn = new FbConnection(UsedConnectionString);
					fbconn.Open();
					FConnection = fbconn;
					break;
#endif
#if REPMAN_ORACLE
				case DotNetDriverType.Oracle:
					OracleConnection oraconn = new OracleConnection(UsedConnectionString);
					oraconn.Open();
					FConnection = oraconn;
					break;
#endif
#if REPMAN_POSTGRESQL
				case DotNetDriverType.PostgreSql:
					Npgsql.NpgsqlConnection pgsqlconn=new Npgsql.NpgsqlConnection(UsedConnectionString);
					pgsqlconn.Open();
					FConnection=pgsqlconn;
					break;
#endif
#if REPMAN_SQLITE
				case DotNetDriverType.SQLite:
					Mono.Data.SqliteClient.SqliteConnection sqliconn=new Mono.Data.SqliteClient.SqliteConnection(UsedConnectionString);
					sqliconn.Open();
					FConnection=sqliconn;
					break;
#endif
#if REPMAN_SYBASE
				case DotNetDriverType.Sybase:
					Mono.Data.SybaseClient.SybaseConnection sybaseconn=new Mono.Data.SybaseClient.SybaseConnection(UsedConnectionString);
					sybaseconn.Open();
					FConnection=sybaseconn;
					break;
#endif
#if REPMAN_DB2
				case DotNetDriverType.DB2:
					IBM.Data.DB2.DB2Connection db2conn=new IBM.Data.DB2.DB2Connection(UsedConnectionString);
					db2conn.Open();
					FConnection=db2conn;
					break;
#endif
#if REPMAN_MYSQL
				case DotNetDriverType.MySql:
					MySql.Data.MySqlClient.MySqlConnection myconn=new MySql.Data.MySqlClient.MySqlConnection(UsedConnectionString);
					myconn.Open();
					FConnection=myconn;
					break;
#endif
#if REPMAN_SQL
				case DotNetDriverType.Sql:
					SqlConnection sqconn = new SqlConnection(UsedConnectionString);
					sqconn.Open();
					FConnection = sqconn;
					break;
#endif
#if REPMAN_SQLCE
				case DotNetDriverType.SqlCE:
					SqlCeConnection sqceconn=new SqlCeConnection(UsedConnectionString);
					sqceconn.Open();
					FConnection=sqceconn;
					break;
#endif
				default:
					throw new NamedException("Database driver not supported:" + DotNetDriver.ToString(),DotNetDriver.ToString());
			}
#if REPMAN_COMPACT
#else
			if (Connection != null)
				Transaction = Connection.BeginTransaction(TransIsolation);
#endif
#endif
		}
        /// <summary>
        /// Obrains a IDataReader from a sql sentence
        /// </summary>
        /// <param name="sqlsentence">A valid sql sentence</param>
        /// <param name="dataalias">Parameters related to this alias will be used</param>
        /// <param name="aparams">Report parameters</param>
        /// <param name="onlyexec">Execute only, not open the query</param>
        /// <returns>A valid IDataReader</returns>
		public IDataReader GetDataReaderFromSQL(string sqlsentence, string dataalias,
			Params aparams, bool onlyexec)
		{
			IDataReader adatareader = null;
			Connect();
			IDbCommand Command = Connection.CreateCommand();
			Command.Transaction = Transaction;

			// Assign parameters, string substitution only
			for (int i = 0; i < aparams.Count; i++)
			{
				int index;
				Param aparam = aparams[i];
				switch (aparam.ParamType)
				{
					case ParamType.Subst:
					case ParamType.Multiple:
						index = aparam.Datasets.IndexOf(dataalias);
						if (index >= 0)
							sqlsentence = sqlsentence.Replace(aparam.Search, aparam.Value);
						break;
					case ParamType.SubstExpre:
                    case ParamType.SubsExpreList:
                        int index2 = aparam.Datasets.IndexOf(Alias);
                        if (index2 >= 0)
                        {
                            using (Evaluator eval = new Evaluator())
                            {
                                string nvalue = aparam.Value.ToString();
                                eval.Expression = nvalue;
                                nvalue = eval.Evaluate().ToString();

                                sqlsentence = sqlsentence.Replace(aparam.Search, nvalue);
                            }
                        } 
                        break;                        
				}
			}
			Command.CommandText = sqlsentence;
			// Assign parameters
			for (int i = 0; i < aparams.Count; i++)
			{
				Param aparam = aparams[i];
				int index = aparam.Datasets.IndexOf(dataalias);
				if (index >= 0)
				{
					if ((aparam.ParamType != ParamType.Subst) &&
					 (aparam.ParamType != ParamType.Multiple) &&
                     (aparam.ParamType != ParamType.SubstExpre) && (aparam.ParamType != ParamType.SubsExpreList))
					{
						System.Data.IDataParameter dbparam = Command.CreateParameter();
						dbparam.ParameterName = "@" + aparam.Alias;
						dbparam.Direction = ParameterDirection.Input;
						dbparam.DbType = aparam.Value.GetDbType();
						dbparam.Value = aparam.Value;
						Command.Parameters.Add(dbparam);
					}
				}
			}
			adatareader = Command.ExecuteReader();
			return adatareader;
		}
	}
    /// <summary>
    /// DatabaInfo stores information about a dataset, a Report have a collection
    /// of dataset definitions. Each dataset is related to one connection (DatabaseInfo)<see cref="Variant">Report</see><see cref="Variant">DataInfos</see>
    /// </summary>
    public class DataInfo : ReportItem, IDisposable, ICloneable
	{
        Strings masterfieldslist;
        Strings sharedfieldslist;
        object[] mastervalues;
        
        private Evaluator internalevaluator;
        private Evaluator GetEvaluator()
        {
            if (internalevaluator == null)
            {
                internalevaluator = new Evaluator();
            }
            return internalevaluator;
        }
        private string OldSQLUsed;
		private bool connecting;
        /// <summary>DataInfo name (alias)</summary>
		public string Alias;
        /// <summary>DatabaseInfo alias related to this dataset</summary>
		public string DatabaseAlias;
        /// <summary>Sql sentence, for parameters precede them by double quotes in native drivers or by @ symbol in .Net drivers</summary>
		public string SQL;
        /// <summary>A master dataset can be assigned so the query is executed each time the parameters of the
        /// query change, the parameters with the same name as master dataset fields will be checked</summary>
		public string DataSource;
        /// <summary>Filename to loadwhen MyBase driver is selected</summary>
		public string MyBaseFilename;
        /// <summary>Field definition file when Mybase driver is selected</summary>
        public string MyBaseFields;
        /// <summary>Index fields when MyBase driver is selected</summary>
        public string MyBaseIndexFields;
        /// <summary>Master fields when MyBase driver is selected</summary>
        public string MyBaseMasterFields;
        /// <summary>Index fields when BDE driver is selected</summary>
        public string BDEIndexFields;
        /// <summary>Index name when BDE driver is selected</summary>
        public string BDEIndexName;
        /// <summary>Index table name when BDE driver is selected and BDEType is Table</summary>
        public string BDETable;
        /// <summary>BDE dataset type (table,query) when BDE driver is selected</summary>
        public DatasetType BDEType;
        /// <summary>BDE filter BDE driver is selected</summary>
        public string BDEFilter;
        /// <summary>BDE master fields BDE driver is selected</summary>
        public string BDEMasterFields;
        /// <summary>BDE first range filter BDE driver is selected</summary>
        public string BDEFirstRange;
        /// <summary>BDE last range filter BDE driver is selected</summary>
        public string BDELastRange;
        /// <summary>When Mybase driver is selected, you can fill the dataset (in memory dataset) with other datasets</summary>
        public Strings DataUnions;
        /// <summary>When Mybase driver is selected, you can fill the dataset (in memory dataset) with other datasets, with grouping option</summary>
        public bool GroupUnion;
        /// <summary>When Mybase driver is selected, and union is performed, 
        /// </summary>
        public bool ParallelUnion;
        /// <summary>By default all datasets are open on start, set this property to false when you want to open the dataset at runtime manually</summary>
        public bool OpenOnStart;
        /// <summary>Current IDataReader</summary>
		public IDataReader DataReader;
        /// <summary>Current in memory two record buffer table</summary>
        public ReportDataset Data;
        /// <summary>Current IDbCommand</summary>
		public System.Data.IDbCommand Command;
        /// <summary>Set this property to override the sql sentence at runtime, set is before executing the report</summary>
        public string SQLOverride;
	public DataView DataViewOverride;
        /// <summary>
        /// Free resources
        /// </summary>
		override public void Dispose()
		{
            base.Dispose();
			if (Data!=null)
			{
				Data.Dispose();
				Data=null;
			}
			// TODO: Check for assigned connections and dispose them
			// That is check if they implements IDisposable and call Dispose()
		}
        protected override string GetClassName()
        {
            return "TRPDATAINFOITEM";
        }
        /// <summary>
        /// Clone the DataInfo item
        /// </summary>
        /// <param name="areport">The new owner of the DataInfo</param>
        /// <returns>A new DataInfo item</returns>
		public DataInfo Clone(Report areport)
		{
			DataInfo ninfo=(DataInfo)Clone();
			ninfo.Report=areport;
			return ninfo;
		}
        /// <summary>
        /// Clone the DataInfo item
        /// </summary>
        /// <returns>A new DataInfo item</returns>
        public object Clone()
		{
			DataInfo ninfo=new DataInfo(Report);
			ninfo.Alias=Alias;
			ninfo.BDEFilter=BDEFilter;
			ninfo.BDEFirstRange=this.BDEFirstRange;
			ninfo.BDEIndexFields=this.BDEIndexFields;
			ninfo.BDEIndexName=this.BDEIndexName;
			ninfo.BDELastRange=this.BDELastRange;
			ninfo.BDEMasterFields=this.BDEMasterFields;
			ninfo.BDETable=this.BDETable;
			ninfo.BDEType=this.BDEType;
			ninfo.DatabaseAlias=this.DatabaseAlias;
			ninfo.DataSource=this.DataSource;
			ninfo.DataUnions=(Strings)this.DataUnions.Clone();
			ninfo.GroupUnion=this.GroupUnion;
			ninfo.MyBaseFields=this.MyBaseFields;
			ninfo.MyBaseFilename=this.MyBaseFilename;
			ninfo.MyBaseIndexFields=this.MyBaseIndexFields;
			ninfo.MyBaseMasterFields=this.MyBaseMasterFields;
			ninfo.Name=Name;
			ninfo.OpenOnStart=this.OpenOnStart;
			ninfo.Report=Report;
			ninfo.SQL=this.SQL;
			return ninfo;
		}			
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rp">The owner</param>
		public DataInfo(BaseReport rp)
			: base(rp)
		{
			OpenOnStart = true;
			DataUnions=new Strings();
			Alias = ""; DatabaseAlias = ""; SQL = ""; DataSource = ""; SQLOverride = "";
			MyBaseFilename = ""; MyBaseFields = ""; MyBaseIndexFields = ""; MyBaseMasterFields = "";
			BDEIndexFields = ""; BDEIndexName = ""; BDETable = "";
			BDEFilter = ""; BDEMasterFields = ""; BDEFirstRange = ""; BDELastRange = "";
			Data = new ReportDataset();
		}
        /// <summary>
        /// Obtain the related DatabaseInfoItem, searching by DatabaseAlias
        /// </summary>
        /// <returns>The DatabaseInfo or throws an exception</returns>
		public DatabaseInfo GetDbItem()
		{
			int index = -1;
			DatabaseInfos infos = Report.DatabaseInfo;
			for (int i = 0; i < infos.Count; i++)
			{
				if (infos[i].Alias == DatabaseAlias)
				{
					index = i;
					break;
				}
			}
			if (index >= 0)
				return infos[index];
			else
				throw new NamedException("Dabase Alias not found: " + DatabaseAlias,DatabaseAlias);
		}
        /// <summary>
        /// Obtain field information from the dataset, useful for the designer
        /// </summary>
        /// <param name="astream">Stream to fill with the information</param>
		public void GetFieldsInfo(Stream astream)
		{
			int i;
			for (i = 0; i < Data.Columns.Count; i++)
			{
				StreamUtil.SWriteLine(astream, Data.Columns[i].ColumnName);
				StreamUtil.SWriteLine(astream, Data.Columns[i].DataType.ToString());
				StreamUtil.SWriteLine(astream, Data.Columns[i].MaxLength.ToString());
			}
		}
        public void GoFirstMem()
        {
            if (Data.CurrentView == null)
                return;
            Data.First();
        }

        /// <summary>
        /// Open the dataset and read the first record into the data variable
        /// </summary>
		public void Connect()
		{
			if (connecting)
				throw new NamedException("Circular datalinks not allowed:" + Alias,Alias);
            if ((Data.CurrentReader != null) || (Data.CurrentView != null))
				return;
			connecting = true;
			try
			{
				DatabaseInfo dbitem = GetDbItem();
				dbitem.Connect();
                DataReader = null;
                Data.CurrentReader = null;
                Data.CurrentView = null;
                Data.UpdateColumns = true;

				Params aparams = Report.Params;


                DataInfo mastersource = null;
                // Open first the master source
                if (DataSource.Length > 0)
                {
                    int index = Report.DataInfo.IndexOf(DataSource);
                    if (index < 0)
                        throw new NamedException("Master source not found: "
                         + DataSource + " in " + Alias, DataSource);
                    mastersource = Report.DataInfo[index];
                    Report.UpdateParamsBeforeOpen(index, true);
                    mastersource.Connect();
                }

                if (dbitem.Driver == DriverType.Mybase)
                {
#if REPMAN_COMPACT
                    throw new Exception("Driver Mybase not supported in Compact framework");
#else
                    if (DataViewOverride!=null)
                    {
                        Data.CurrentView = DataViewOverride;

                        if (mastersource != null)
                        {
                            UpdateParams(mastersource.Data, Command);
                            Data.ViewFilter = mastervalues;

                            mastersource.Data.OnDataChange += new DataChange(OnDataChange);
                        }
                    }
        		    else
                    {
                    Strings primarycolumns = new Strings();
                    masterfieldslist = Strings.FromSemiColon(MyBaseMasterFields.ToUpper());                    
                    masterfieldslist.RemoveBlanks();
                    mastervalues = new object[masterfieldslist.Count];

                    for (int ixvalue = 0; ixvalue < masterfieldslist.Count; ixvalue++)
                        mastervalues[ixvalue] = DBNull.Value;
                    DataSet intdataset = dbitem.Report.DatabaseInfo.MemoryDataSet;
                    DataTable inttable = null;
                    int indextable = intdataset.Tables.IndexOf(Alias);
                    if (indextable>=0)
                    {
                        inttable = intdataset.Tables[indextable];
                        intdataset.Tables.Remove(inttable);
                        inttable.Dispose();
                        inttable = null;
                    }
                    if (this.DataUnions.Count>0)
                    {
                        DataView primview = null;

                        SortedList<string, string> fieldsunion=null;
                        if (ParallelUnion)
                            fieldsunion = new SortedList<string, string>();
                        int idxunion = 0;
                        foreach (string ntable in DataUnions)
                        {
                            string field_prefix = "";
                            if (idxunion > 0)
                                field_prefix = "Q" + (idxunion + 1).ToString("00") + "_";
                            if (ParallelUnion)
                                fieldsunion.Clear();
                            // Create structure
                            Strings nnames = Strings.FromSeparator('-',ntable);
                            nnames.RemoveBlanks();
                            string tablename = nnames[0];
                            if (nnames.Count < 2)
                                sharedfieldslist = new Strings();
                            else
                            {
                                sharedfieldslist = Strings.FromSemiColon(nnames[1].ToUpper());
                                sharedfieldslist.RemoveBlanks();
                            }
                            if (Report.DataInfo[tablename].DataReader == null)
                                Report.DataInfo[tablename].Connect();
                            IDataReader FCurrentReader = Report.DataInfo[tablename].DataReader;
                            DataView FCurrentDataView = null;
                            if (FCurrentReader == null)
                            {
                                FCurrentDataView = Report.DataInfo[tablename].Data.CurrentView;
                                if (FCurrentDataView == null)
                                    throw new Exception("No datareader in table: " + tablename);
                            }
                            bool isempty = Report.DataInfo[tablename].Data.Eof;
                            if ((idxunion == 0) || ParallelUnion)
                            {
                                if (idxunion == 0)
                                    inttable = new DataTable();
                                DataTable adatatable = null;
                                if (FCurrentReader != null)
                                    adatatable = FCurrentReader.GetSchemaTable();
                                else
                                {
                                    adatatable = new DataTable();
                                    adatatable.Columns.Add("ColumnName", System.Type.GetType("System.String"));
                                    adatatable.Columns.Add("DataType", System.Type.GetType("System.Type"));
                                    object[] values = new object[2];
                                    foreach (DataColumn ncol in FCurrentDataView.Table.Columns)
                                    {
                                        values[0] = ncol.ColumnName;
                                        values[1] = ncol.DataType;
                                        adatatable.Rows.Add(values);
                                    }
                                }
                                DataColumn col;

                                for (int i = 0; i < adatatable.Rows.Count; i++)
                                {
                                    string acolname;
                                    string originalcolname;
                                    DataRow nrow = adatatable.Rows[i];
                                    acolname = nrow["ColumnName"].ToString().ToUpper();
                                    if (acolname.Length < 1)
                                        acolname = "Column" + i.ToString();
                                    originalcolname = acolname;
                                    if (sharedfieldslist.IndexOf(acolname) < 0)
                                        acolname = field_prefix + acolname;
                                    if (ParallelUnion)
                                        fieldsunion.Add(originalcolname, acolname);
                                    if (inttable.Columns.IndexOf(acolname) < 0)
                                    {
                                        col = inttable.Columns.Add(acolname, (Type)nrow["DataType"]);
                                        if ((col.DataType.ToString() == "System.String") && (FCurrentReader != null))
                                        {
                                            int maxlength = (int)nrow["ColumnSize"];
                                            col.MaxLength = maxlength;
                                        }
                                        col.Caption = acolname;
                                    }
                                }
                            }
                            // Add rows
                            SortedList<int, int> fieldintunion = null;
                            int colcount = 0;
                            if (FCurrentReader != null)
                                colcount = FCurrentReader.FieldCount;
                            else
                                colcount = FCurrentDataView.Table.Columns.Count;
                            object[] nobject = new object[colcount];

                            if (ParallelUnion)
                            {
                                // Create primary key for share union
                                if (sharedfieldslist.Count > 0)
                                {
                                    if (idxunion == 0)
                                    {
                                        DataColumn[] primcols = new DataColumn[sharedfieldslist.Count];
                                        int idxcol = 0;
                                        foreach (string primcolname in sharedfieldslist)
                                        {
                                            primcols[idxcol] = inttable.Columns[primcolname];
                                            primarycolumns.Add(primcolname);
                                            idxcol++;
                                        }
                                        primview = new DataView(inttable, "", sharedfieldslist.ToCharSeparated(','), DataViewRowState.CurrentRows);
                                        //inttable.Constraints.Add("PRIM" + inttable.TableName, primcols,true);
                                    }
                                }
                                fieldintunion = new SortedList<int, int>();


                                if (nobject.Length != inttable.Columns.Count)
                                    nobject = new object[inttable.Columns.Count];
                                if (FCurrentReader != null)
                                {
                                    for (int idxreader = 0; idxreader < FCurrentReader.FieldCount; idxreader++)
                                    {
                                        fieldintunion.Add(idxreader, inttable.Columns.IndexOf(fieldsunion[FCurrentReader.GetName(idxreader)]));
                                    }
                                }
                                else
                                {
                                    for (int idxreader = 0; idxreader < FCurrentDataView.Table.Columns.Count; idxreader++)
                                    {
                                        fieldintunion.Add(idxreader, inttable.Columns.IndexOf(fieldsunion[FCurrentDataView.Table.Columns[idxreader].ColumnName]));
                                    }
                                }
                            }
                            object[] primkeys = null;
                            if (primarycolumns.Count > 0)
                                primkeys = new object[primarycolumns.Count];
                            bool addrow = !isempty;
                            int secuentialindex = 0;
                            int dataviewindex = 0;
                            while (addrow)
                            {
                                int xcol = 0;
                                if (ParallelUnion)
                                {
                                    int idxcolprim = 0;
                                    foreach (string colnameprim in sharedfieldslist)
                                    {
                                        if (FCurrentReader != null)
                                        {
                                            primkeys[idxcolprim] = FCurrentReader[colnameprim];
                                        }
                                        else
                                            primkeys[idxcolprim] = FCurrentDataView[dataviewindex][colnameprim];

                                        idxcolprim++;
                                    }
                                    //DataRow foundrow = inttable.Rows.Find(primkeys);
                                    DataRow foundrow = null;
                                    if (primview != null)
                                    {
                                        int index = primview.Find(primkeys);
                                        if (index >= 0)
                                            foundrow = primview[index].Row;
                                    }
                                    else
                                    {
                                        if (secuentialindex < inttable.Rows.Count)
                                            foundrow = inttable.Rows[secuentialindex];
                                    }

                                    if (foundrow == null)
                                    {
                                        if (FCurrentReader != null)
                                        {
                                            foreach (int xindex in fieldintunion.Keys)
                                            {
                                                nobject[fieldintunion[xindex]] = FCurrentReader[xindex];
                                            }
                                        }
                                        else
                                        {
                                            foreach (int xindex in fieldintunion.Keys)
                                            {
                                                nobject[fieldintunion[xindex]] = FCurrentDataView[dataviewindex][xindex];
                                            }
                                        }
                                        inttable.Rows.Add(nobject);
                                    }
                                    else
                                    {
                                        foundrow.BeginEdit();
                                        if (FCurrentReader != null)
                                        {
                                            foreach (int xindex in fieldintunion.Keys)
                                            {
                                                foundrow[fieldintunion[xindex]] = FCurrentReader[xindex];
                                            }
                                        }
                                        else
                                        {
                                            foreach (int xindex in fieldintunion.Keys)
                                            {
                                                foundrow[fieldintunion[xindex]] = FCurrentDataView[dataviewindex][xindex];
                                            }
                                        }
                                        foundrow.EndEdit();
                                    }
                                }
                                else
                                {
                                    if (FCurrentReader != null)
                                    {
                                        while (xcol < FCurrentReader.FieldCount)
                                        {
                                            nobject[xcol] = FCurrentReader[xcol];
                                            xcol++;
                                        }
                                    }
                                    else
                                    {
                                        while (xcol < colcount)
                                        {
                                            nobject[xcol] = FCurrentDataView[dataviewindex][xcol];
                                            xcol++;
                                        }
                                    }
                                    inttable.Rows.Add(nobject);
                                }
                                secuentialindex++;
                                dataviewindex++;
                                if (FCurrentReader != null)
                                    addrow = FCurrentReader.Read();
                                else
                                    addrow = dataviewindex < FCurrentDataView.Count;
                            }
                            
                            idxunion++;
                        }
                        if (inttable != null)
                        {
                            Strings lsorting = Strings.FromSemiColon(this.MyBaseIndexFields);
                            Strings lnew = new Strings();
                            Strings nmaster = Strings.FromSemiColon(this.MyBaseMasterFields);
                            for (int ix=0;ix<nmaster.Count;ix++)
                                lnew.Add(lsorting[ix]);
                            string sortingstring = lnew.ToCharSeparated(',');
                            DataView intview = null;

                            // Resort table for master/child relations, correctly sorted
                            if (MyBaseIndexFields.Length > 0)
                            {
                                Strings nsort = Strings.FromSemiColon(this.MyBaseIndexFields);
                                string nsorting = nsort.ToCharSeparated(',');
                                intview = new DataView(inttable, "", nsorting, DataViewRowState.CurrentRows);
                                DataTable oldtable = inttable;
                                inttable = oldtable.Clone();
                                object[] nobj = new object[oldtable.Columns.Count];
                                foreach (DataRowView xv in intview)
                                {
                                    for (int idx = 0; idx < nobj.Length; idx++)
                                    {
                                        nobj[idx] = xv[idx];
                                    }
                                    inttable.Rows.Add(nobj);
                                }
                                intview.Dispose();
                                oldtable.Dispose();
                                intview = new DataView(inttable, "", sortingstring, DataViewRowState.CurrentRows);
                            }
                            else
                                intview = new DataView(inttable, "", sortingstring, DataViewRowState.CurrentRows);

                            
                            intdataset.Tables.Add(inttable);
                            Data.CurrentView = intview;

                            if (mastersource != null)
                            {
                                UpdateParams(mastersource.Data, Command);
                                Data.ViewFilter = mastervalues;

                                mastersource.Data.OnDataChange += new DataChange(OnDataChange);
                            }

                        }
			}
                    }
#endif
                }
                else
                {
                    IDataReader areader;
                    if (dbitem.SqlExecuter == null)
                        Command = dbitem.Connection.CreateCommand();
                    else
                        Command = new System.Data.SqlClient.SqlCommand();
                    Command.Transaction = dbitem.CurrentTransaction;
				    string sqlsentence;
				    if (SQLOverride.Length != 0)
					    sqlsentence = SQLOverride;
				    else
					    sqlsentence = SQL;
				    // Assign parameters, string substitution only
				    for (int i = 0; i < aparams.Count; i++)
				    {
					    Param aparam = aparams[i];
					    switch (aparam.ParamType)
					    {
						    case ParamType.Subst:
                            case ParamType.Multiple:
							    int index = aparam.Datasets.IndexOf(Alias);
							    if (index >= 0)
								    sqlsentence = sqlsentence.Replace(aparam.Search, aparam.Value);
							    break;
                            case ParamType.SubstExpre:
                            case ParamType.SubsExpreList:
                                int index2 = aparam.Datasets.IndexOf(Alias);
                                if (index2 >= 0)
                                {

                                    string nvalue = aparam.LastValue.ToString();
                            //        GetEvaluator().Expression = nvalue;
                              //      nvalue = GetEvaluator().Evaluate().ToString();
                                    
                                    sqlsentence = sqlsentence.Replace(aparam.Search, nvalue);
                                }
                                break;
                        }
				    }
				    
				    Command.CommandText = sqlsentence;
				    OldSQLUsed = sqlsentence;
				    // Assign parameters
				    for (int i = 0; i < aparams.Count; i++)
				    {
					    Param aparam = aparams[i];
					    int index = aparam.Datasets.IndexOf(Alias);
					    if (index >= 0)
					    {
						    if ((aparam.ParamType != ParamType.Subst) &&
                             (aparam.ParamType != ParamType.Multiple)&& (aparam.ParamType != ParamType.SubsExpreList)
                                 && (aparam.ParamType != ParamType.SubstExpre))
						    {
							    System.Data.IDataParameter dbparam = Command.CreateParameter();
							    //dbparam.ParameterName = "@" + aparam.Alias;
                                // SQL Server does not like "@" prefix
                                dbparam.ParameterName = aparam.Alias;
                                dbparam.Direction = ParameterDirection.Input;
							    dbparam.DbType = aparam.Value.GetDbType();
							    dbparam.Value = aparam.LastValue.AsObject();
							    Command.Parameters.Add(dbparam);
						    }
					    }
				    }
				    if (mastersource != null)
				    {
					    UpdateParams(mastersource.Data, Command);
					    mastersource.Data.OnDataChange += new DataChange(OnDataChange);
				    }
                    if (dbitem.SqlExecuter != null)
                    {
                        Data.CurrentView = new DataView(dbitem.SqlExecuter.Open(Command));
                    }
                    else
                    {
                        areader = Command.ExecuteReader();
                        DataReader = areader;
                        Data.CurrentReader = DataReader;
                    }
                    
                }

			}
			finally
			{
				connecting = false;
			}
		}
		private void OnDataChange(ReportDataset master)
		{
			if (UpdateParams(master, Command))
			{
                if (Data.CurrentView != null)
                {
                    Data.ViewFilter = mastervalues;
                }
                else
                {
                    if (DataReader != null)
                    {
                        DataReader.Close();
                        DataReader = null;
                    }
                    IDataReader areader = Command.ExecuteReader();
                    DataReader = areader;
                    Data.UpdateColumns = (OldSQLUsed != Command.CommandText);
                    Data.CurrentReader = areader;
                }
			}
		}
        /// <summary>
        /// Update the parameters inside the command, the master dataset will be used to fill the parameters
        /// with matching names
        /// </summary>
        /// <param name="master">Master dataset</param>
        /// <param name="ncommand">Command</param>
        /// <returns></returns>
		public bool UpdateParams(ReportDataset master, IDbCommand ncommand)
		{
            if (Data.CurrentView != null)
            {
                bool nresult = false;
                int idx = 0;
                foreach (string nmasterf in masterfieldslist)
                {
                    object nvalue = DBNull.Value;
                    if (!master.Eof)
                    {
                        nvalue = master.CurrentRow[nmasterf];
                    }
                    if (!nvalue.Equals(mastervalues[idx]))
                    {
                        nresult = true;
                        mastervalues[idx] = nvalue;
                    }
                    idx++;
                }
                return nresult;
            }
            else
            {
                bool aresult = false;
                for (int i = 0; i < ncommand.Parameters.Count; i++)
                {
                    IDbDataParameter param = (IDbDataParameter)ncommand.Parameters[i];
                    string paramname = param.ParameterName;
                    if (paramname.Length > 0)
                    {
                        if (paramname[0] == '@')
                            paramname = param.ParameterName.Substring(1, param.ParameterName.Length - 1);
                        int index = master.Columns.IndexOf(paramname);
                        if (index >= 0)
                        {
                            if (master.Eof)
                            {
                                if (param.Value != null)
                                    aresult = true;
                                param.Value = null;
                            }
                            else
                                if (!master.CurrentRow[paramname].Equals(param.Value))
                                {
                                    aresult = true;
                                    param.Value = master.CurrentRow[paramname];
                                }
                        }
                    }
                }
			    return aresult;
            }
        }
        /// <summary>
        /// Close the dataset, and free the datareader
        /// </summary>
		public void DisConnect()
		{
			if (DataReader != null)
			{
				DataReader.Close();
				Command = null;
				DataReader = null;
                Data.CurrentReader = null;
            }
		}
        public void DisConnectMem()
        {
            
            if (Data != null)
            {
                if (Data.CurrentView!=null)
                    Data.CurrentView = null;
            }
        }
	}
    /// <summary>
    /// Collection of DatabaseInfo items
    /// </summary>
#if REPMAN_DOTNET1
    public class DatabaseInfos:ICloneable,IEnumerable
	{
		DatabaseInfo[] FItems;
		const int FIRST_ALLOCATION_OBJECTS = 10;
		int FCount;
        /// <summary>
        /// Constructor
        /// </summary>
		public DatabaseInfos()
		{
			FCount = 0;
			FItems = new DatabaseInfo[FIRST_ALLOCATION_OBJECTS];
            MemoryDataSet = new DataSet();
		}
        /// <summary>
        /// Clear the collection
        /// </summary>
		public void Clear()
		{
			for (int i = 0; i < FCount; i++)
				FItems[i] = null;
			FCount = 0;
		}
		private void CheckRange(int index)
		{
			if ((index < 0) || (index >= FCount))
				throw new UnNamedException("Index out of range on Sections collection");
		}
		public DatabaseInfo this[int index]
		{
			get { CheckRange(index); return FItems[index]; }
			set { CheckRange(index); FItems[index] = value; }
		}
        public int Count { get { return FCount; } }
		public void Add(DatabaseInfo obj)
		{
			if (FCount > (FItems.Length - 2))
			{
				DatabaseInfo[] nobjects = new DatabaseInfo[FCount];
				System.Array.Copy(FItems, 0, nobjects, 0, FCount);
				FItems = new DatabaseInfo[FItems.Length * 2];
				System.Array.Copy(nobjects, 0, FItems, 0, FCount);
			}
			FItems[FCount] = obj;
			FCount++;
		}
		// IEnumerable Interface Implementation:
		//   Declaration of the GetEnumerator() method 
		//   required by IEnumerable
		public IEnumerator GetEnumerator()
		{
			return new DatabaseInfoEnumerator(this);
		}
		// Inner class implements IEnumerator interface:
		public class DatabaseInfoEnumerator : IEnumerator
		{
			private int position = -1;
			private DatabaseInfos t;

			public DatabaseInfoEnumerator(DatabaseInfos t)
			{
				this.t = t;
			}

			// Declare the MoveNext method required by IEnumerator:
			public bool MoveNext()
			{
				if (position < t.Count - 1)
				{
					position++;
					return true;
				}
				else
				{
					return false;
				}
			}

			// Declare the Reset method required by IEnumerator:
			public void Reset()
			{
				position = -1;
			}

			// Declare the Current property required by IEnumerator:
			public object Current
			{
				get
				{
					return t[position];
				}
			}
		}
#else
    public class DatabaseInfos : System.Collections.Generic.List<DatabaseInfo>,ICloneable
    {
#endif
        public DataSet MemoryDataSet;
        public DatabaseInfos()
		{
            MemoryDataSet = new DataSet();
		}
        /// <summary>
        /// Get item by name
        /// </summary>
        /// <param name="dbname">Database info name or alias</param>
        /// <returns></returns>
        public DatabaseInfo this[string dbname]
        {
            get
            {
                int index = IndexOf(dbname);
                if (index >= 0)
                    return this[index];
                else
                    return null;
            }
        }
        /// <summary>
        /// Returns index by name (alias)
        /// </summary>
        /// <param name="avalue">Alias to search for</param>
        /// <returns>Index or -1 when not found</returns>
        public int IndexOf(string avalue)
        {
            int aresult = -1;
            for (int i = 0; i < Count; i++)
            {
                if (this[i].Alias == avalue)
                {
                    aresult = i;
                    break;
                }
            }
            return aresult;
        }
        /// <summary>
        /// Clone the DatabaseInfos collection
        /// </summary>
        /// <returns>A new DatabaseInfos collection</returns>
        public object Clone()
        {
            DatabaseInfos ninfo = new DatabaseInfos();
            foreach (DatabaseInfo ainfo in this)
            {
                ninfo.Add((DatabaseInfo)ainfo.Clone());
            }
            return ninfo;
        }
        /// <summary>
        /// Clone the DatabaseInfos collection
        /// </summary>
        /// <param name="areport">New owner</param>
        /// <returns>A new DatabaseInfos collection</returns>
        public DatabaseInfos Clone(Report areport)
        {
            DatabaseInfos ninfo = new DatabaseInfos();
            foreach (DatabaseInfo ainfo in this)
            {
                ninfo.Add(ainfo.Clone(areport));
            }
            return ninfo;
        }
    }
    /// <summary>
    /// Collection of DataInfo items
    /// </summary>
#if REPMAN_DOTNET1
	public class DataInfos:ICloneable,IEnumerable
	{
		DataInfo[] FItems;
		const int FIRST_ALLOCATION_OBJECTS = 10;
		int FCount;
		public DataInfos()
		{
			FCount = 0;
			FItems = new DataInfo[FIRST_ALLOCATION_OBJECTS];
		}
		public void Clear()
		{
			for (int i = 0; i < FCount; i++)
				FItems[i] = null;
			FCount = 0;
		}
		private void CheckRange(int index)
		{
			if ((index < 0) || (index >= FCount))
				throw new UnNamedException("Index out of range on Sections collection");
		}
		public DataInfo this[int index]
		{
			get { CheckRange(index); return FItems[index]; }
			set { CheckRange(index); FItems[index] = value; }
		}
        public int Count { get { return FCount; } }
		public void Add(DataInfo obj)
		{
			if (FCount > (FItems.Length - 2))
			{
				DataInfo[] nobjects = new DataInfo[FCount];
				System.Array.Copy(FItems, 0, nobjects, 0, FCount);
				FItems = new DataInfo[FItems.Length * 2];
				System.Array.Copy(nobjects, 0, FItems, 0, FCount);
			}
			FItems[FCount] = obj;
			FCount++;
		}
		// IEnumerable Interface Implementation:
		//   Declaration of the GetEnumerator() method 
		//   required by IEnumerable
		public IEnumerator GetEnumerator()
		{
			return new DataInfoEnumerator(this);
		}
		// Inner class implements IEnumerator interface:
		public class DataInfoEnumerator : IEnumerator
		{
			private int position = -1;
			private DataInfos t;

			public DataInfoEnumerator(DataInfos t)
			{
				this.t = t;
			}

			// Declare the MoveNext method required by IEnumerator:
			public bool MoveNext()
			{
				if (position < t.Count - 1)
				{
					position++;
					return true;
				}
				else
				{
					return false;
				}
			}

			// Declare the Reset method required by IEnumerator:
			public void Reset()
			{
				position = -1;
			}

			// Declare the Current property required by IEnumerator:
			public object Current
			{
				get
				{
					return t[position];
				}
			}
		}
#else
    public class DataInfos : System.Collections.Generic.List<DataInfo>, ICloneable
    {
#endif
        /// <summary>
        /// Get item by name
        /// </summary>
        /// <param name="dname">Dataset name or alias</param>
        /// <returns></returns>
        public DataInfo this[string dname]
        {
            get
            {
                int index = IndexOf(dname);
                if (index >= 0)
                    return this[index];
                else
                    return null;
            }
        }
        /// <summary>
        /// Returns index by name (alias)
        /// </summary>
        /// <param name="avalue">Alias to search for</param>
        /// <returns>Index or -1 when not found</returns>
        public int IndexOf(string avalue)
        {
            int aresult = -1;
            for (int i = 0; i < Count; i++)
            {
                if (this[i].Alias == avalue)
                {
                    aresult = i;
                    break;
                }
            }
            return aresult;
        }
        /// <summary>
        /// Clone the DataInfo collection
        /// </summary>
        /// <returns>A new DataInfo collection</returns>
        public object Clone()
        {
            DataInfos ninfo = new DataInfos();
            foreach (DataInfo ainfo in this)
            {
                ninfo.Add((DataInfo)ainfo.Clone());
            }
            return ninfo;
        }
        /// <summary>
        /// Clone the DataInfo collection
        /// </summary>
        /// <param name="areport">New owner</param>
        /// <returns>A new DataInfo collection</returns>
        public DataInfos Clone(Report areport)
        {
            DataInfos ninfo = new DataInfos();
            foreach (DataInfo ainfo in this)
            {
                ninfo.Add(ainfo.Clone(areport));
            }
            return ninfo;
        }
    }

}
