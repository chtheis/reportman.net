COMPILE="mcs"
COMPILE2="dmcs"
#COMPILE2="gmcs"
RESGEN="resgen"
RESGEN2="resgen2"
# Sybase has broken, -define:REPMAN_SYBASE removed
DEFINES=-define:REPMAN_DOTNET1 -define:REPMAN_ZLIB -define:REPMAN_NRESOURCES -define:REPMAN_OLEDB -define:REPMAN_ODBC -define:NOREPMAN_MYSQL -define:REPMAN_SQLITE -define:REPMAN_SQL -define:REPMAN_DB2_NO -define:REPMAN_POSTGRESQL -define:REPMAN_ORACLE -define:REPMAN_FIREBIRD -define:REPMAN_MONO
DEFINES2=-define:REPMAN_DOTNET2 -define:REPMAN_ZLIB -define:REPMAN_NRESOURCES -define:REPMAN_MONO
DATALIBS=-r:FirebirdSql.Data.Firebird.dll -r:output/Npgsql.dll -r:output/Mono.Data.SqliteClient.dll -r:output/Mono.Data.SybaseClient.dll -r:System.Data.OracleClient
BASICLIBS=-r:output/ICSharpCode.SharpZipLib.dll -r:System.Drawing
BASICLIBS2=-r:System.Drawing -r:ICSharpCode.SharpZipLib.dll

all: clean reportman reportman2

clean:
	-rm output/*.exe
	-rm output/*.exe
	-rm output/*.pdb
	-rm output2/*.pdb
	-rm output/Reportman*.*
	-rm output2/Reportman*.*
	-rm -Rf /AH *.sbm
	-rm -Rf  printreport/obj/debug/*.*
	-rm -Rf  printreport2/obj/debug/*.*
	-rm -Rf output/documentation/*.*
	-rm -Rf output2/documentation/*.*
	-rm -R output/documentation/ndoc_msdn_temp
	-rm -R output2/documentation/ndoc_msdn_temp
	-rm -R printreport/obj
	-rm -R printreport2/obj

reportman: lib rpprintreport rpprintreportpdf

reportman2: lib2 rpprintreport2 rpprintreportpdf2

lib:
	$(COMPILE) -target:library -out:output/Reportman.Drawing.dll $(DEFINES) rpgraphics.cs rpprintout.cs rpcollections.cs rpprintoutprint.cs rpprintoutpdf.cs rptranslator.cs rpmetafile.cs rppdffile.cs rpinfoprovgdi.cs rpinfoprovid.cs  rptypes.cs rpinifile.cs -resource:reportman.Drawing2/Resources/reportmanres.en $(BASICLIBS)
	$(COMPILE) -target:library -out:output/Reportman.Reporting.dll $(DEFINES) rpdatatable.cs rpvariant.cs rpreport.cs rpxmlstream.cs rpdatainfo.cs rpsubreport.cs rpsection.cs rpchartitem.cs rpbarcodeitem.cs rpdrawitem.cs rpdataset.cs rpalias.cs rptypeval.cs rpparser.cs rpeval.cs rpevalfunc.cs rpparams.cs rpbasereport.cs rplabelitem.cs rpprintitem.cs $(DATALIBS) $(BASICLIBS) -r:System.Data -r:output/Reportman.Drawing.dll
	$(RESGEN) rppreviewwinform.resx Reportman.Drawing.Forms.PreviewWinForms.resources
	$(COMPILE) -target:library -out:output/Reportman.Drawing.Forms.dll $(DEFINES) rppreviewmeta.cs rppreviewwinform.cs rpreportprogress.cs rpprintraw.cs rpprintwinforms.cs $(BASICLIBS) -r:output/Reportman.Drawing.dll -r:System.Windows.Forms -resource:Reportman.Drawing.Forms.PreviewWinForms.resources
	$(COMPILE) -target:library -out:output/Reportman.Reporting.Forms.dll $(DEFINES)  rpsearchdata.cs rpcollectionforms.cs rpreportwinforms.cs rpparamscontrol.cs rpparamsform.cs rpdatashow.cs rppagesetup.cs $(BASICLIBS) -r:output/Reportman.Drawing.dll -r:output/Reportman.Drawing.Forms.dll -r:output/Reportman.Reporting.dll -r:System.Windows.Forms -r:System.Data

lib2:
	sed 's/Resources\\/reportman\.Drawing2\/Resources\//' ./reportman.Drawing2/resource.resx > ./reportman.Drawing2/resource.linux.resx
	$(RESGEN2) ./reportman.Drawing2/resource.linux.resx Reportman.Drawing.resource.resources
	$(COMPILE2) -target:library -out:output2/Reportman.Drawing.dll $(DEFINES2) rpprintoutexcel.cs rppdftruetype.cs rpfreetype.cs rpinfoprovft.cs rpinifile.cs rpprintouttext.cs rpgraphics.cs rpprintout.cs rpcollections.cs rpprintoutprint.cs rpprintoutpdf.cs rptranslator.cs rpmetafile.cs rppdffile.cs rpinfoprovgdi.cs rpinfoprovid.cs  rpprintoutcsv.cs rptypes.cs ./reportman.Drawing2/resource.Designer.cs -resource:Reportman.Drawing.resource.resources $(BASICLIBS2) -r:System.Data
	$(COMPILE2) -target:library -out:output2/Reportman.Reporting.dll $(DEFINES2) rpdatatable.cs rpvariant.cs rpreport.cs rpxmlstream.cs rpdatainfo.cs rpsubreport.cs rpsection.cs rpchartitem.cs rpbarcodeitem.cs rpdrawitem.cs rpdataset.cs rpalias.cs rptypeval.cs rpparser.cs rpeval.cs rpevalfunc.cs rpparams.cs rpbasereport.cs rplabelitem.cs rpprintitem.cs rpqrcodeencoder.cs $(BASICLIBS2) -r:System.Data -r:output2/Reportman.Drawing.dll 
	$(RESGEN2) rppreviewformscontrol.resx Reportman.Drawing.Forms.PreviewWinFormsControl.resources
#	$(COMPILE2) -target:library -out:output2/Reportman.Drawing.Forms.dll $(DEFINES2) rpdatagridview.cs rpdatagridviewcolumnadvanced.cs rpdatagridviewnumericcolumn.cs rppreviewmeta.cs rppreviewwinforms2.cs rppreviewwinforms2.Designer.cs rpreportprogress.cs rpprintraw.cs rpprintwinforms.cs $(BASICLIBS2) -r:output2/Reportman.Drawing.dll -r:System.Windows.Forms -resource:Reportman.Drawing.Forms.PreviewWinForms2.resources -r:System.Data
	$(COMPILE2) -target:library -out:output2/Reportman.Drawing.Forms.dll $(DEFINES2) rpprintwinforms.cs rpprinterconfig.cs rpprinterconfig.Designer.cs rpruler.cs rptreegrid.cs rpdatetimepicker.cs rpdatagridview.cs rpdatagridviewcolumnadvanced.cs rpdatagrivviewnumericcolumn.cs rppreviewmeta.cs rpreportprogress.cs rpprintraw.cs rppreviewformscontrol.cs rppreviewformscontrol.designer.cs $(BASICLIBS2) -r:output2/Reportman.Drawing.dll -r:System.Windows.Forms -r:output2/FirebirdSql.Data.FirebirdClient.dll -resource:Reportman.Drawing.Forms.PreviewWinFormsControl.resources -r:System.Data
	$(COMPILE2) -target:library -out:output2/Reportman.Reporting.Forms.dll $(DEFINES2)  rpsearchdata.cs rpcollectionforms.cs rpreportwinforms.cs rpparamscontrol.cs rpparamsform.cs rpdatashow.cs rppagesetup.cs $(BASICLIBS2) -r:System.Data -r:output2/Reportman.Drawing.dll -r:output2/Reportman.Reporting.dll -r:output2/Reportman.Drawing.Forms.dll -r:System.Windows.Forms -r:System.Data -r:output2/FirebirdSql.Data.FirebirdClient.dll


rpprintreport:
	$(COMPILE) printreport/rpprintreport.cs $(DEFINES) -out:output/printreport.exe -resource:rppreviewwinform.resx $(BASICLIBS) $(DATALIBS) -r:System.Windows.Forms -r:System.Data -r:output/Reportman.Drawing.dll -r:output/Reportman.Reporting.dll -r:output/Reportman.Drawing.Forms -r:output/Reportman.Reporting.Forms

rpprintreportpdf:
	$(COMPILE) printreportpdf/rpprintreport.cs $(DEFINES) -out:output/printreportpdf.exe $(BASICLIBS) -r:output/Reportman.Drawing.dll -r:output/Reportman.Reporting.dll

rpprintreport2:
	$(COMPILE2) $(DEFINES2) printreport/rpprintreport.cs -out:output2/printreport.exe -resource:rppreviewwinform.resx -r:output2/Reportman.Drawing.dll -r:output2/Reportman.Reporting.dll -r:output2/Reportman.Reporting.Forms.dll -r:output2/Reportman.Drawing.Forms.dll -r:System.Windows.Forms -r:System.Data -r:output2/FirebirdSql.Data.FirebirdClient.dll -r:output2/MySql.Data.dll -r:Mono.Data.Sqlite

rpprintreportpdf2:
	$(COMPILE2) $(DEFINES2) printreportpdf/rpprintreport.cs -out:output2/printreportpdf.exe -r:output2/Reportman.Drawing -r:output2/Reportman.Reporting.dll -r:System.Data

