MONOPATH=c:\archivos de programa\Mono-1.2.5\bin
#MONOPATH=c:\archivos de programa\mono\bin
COMPILE="$(MONOPATH)\mcs"
COMPILE2="$(MONOPATH)\gmcs"
MONORES="$(MONOPATH)\resgen"
//MONORES2="$(MONOPATH)\resgen2"
MONORES2="C:\Archivos de programa\Microsoft Visual Studio 8\SDK\v2.0\Bin\resgen"
NDOCPATH="C:\Archivos de programa\NDoc 1.3\bin\mono\1.0\NDocConsole.exe"
#NDOCPATH="H:\Archivos de programa\NDoc 1.3\bin\mono\1.0\NDocConsole.exe"
DEFINES=-define:REPMAN_DOTNET1 -define:REPMAN_ZLIB -define:REPMAN_NORESOURCESXX -define:REPMAN_OLEDB -define:REPMAN_ODBC -define:REPMAN_MYSQL -define:REPMAN_SQLITE -define:REPMAN_SQL -define:REPMAN_DB2_NO -define:REPMAN_SYBASE -define:REPMAN_POSTGRESQL -define:REPMAN_ORACLE -define:REPMAN_FIREBIRD
DEFINES2=-define:REPMAN_DOTNET2 -define:REPMAN_ZLIB -define:REPMAN_NORESOURCESXX
DATALIBS=-r:output\FirebirdSql.Data.Firebird.dll -r:output\Npgsql.dll -r:output\Mono.Data.SqliteClient.dll  -r:output\Mono.Data.SybaseClient.dll -r:System.Data.OracleClient -r:output\MySql.Data.dll
BASICLIBS=-r:output\ICSharpCode.SharpZipLib.dll -r:System.Drawing
BASICLIBS2=-r:System.Drawing -r:output2\ICSharpCode.SharpZipLib.dll

all: clean reportman reportman2

clean:
	-del output\*.exe
	-del output2\*.exe
	-del output\*.pdb
	-del output2\*.pdb
	-del output\Reportman*.*
	-del output2\Reportman*.*
	-del /s /AH *.sbm
	-attrib -S -H printreport\printreport.suo
	-del printreport\printreport.csproj.user
	-del /s /q printreport\obj\debug\*.*
	-del /s /q printreport2\obj\debug\*.*
	-del /s /q output\documentation\*.*
	-del /s /q output2\documentation\*.*
	-rmdir /q /s output\documentation\ndoc_msdn_temp
	-rmdir /q /s output2\documentation\ndoc_msdn_temp
	-rmdir /s /q printreport\obj
	-rmdir /s /q printreport2\obj

reportman: lib printreport doc

reportman2: lib2 printreport2

lib:
	$(COMPILE) -target:library -out:output\Reportman.Drawing.dll $(DEFINES) rpgraphics.cs rpprintout.cs rpcollections.cs rpprintoutprint.cs rpprintoutpdf.cs rptranslator.cs rpmetafile.cs rppdffile.cs rpinfoprovgdi.cs rpinfoprovid.cs  rptypes.cs $(BASICLIBS)
	$(COMPILE) -target:library -out:output\Reportman.Reporting.dll $(DEFINES) rpvariant.cs rpreport.cs rpxmlstream.cs rpdatainfo.cs rpsubreport.cs rpsection.cs rpchartitem.cs rpbarcodeitem.cs rpdrawitem.cs rpdataset.cs rpalias.cs rptypeval.cs rpparser.cs rpeval.cs rpevalfunc.cs rpparams.cs rpbasereport.cs rplabelitem.cs rpprintitem.cs $(DATALIBS) $(BASICLIBS) -r:System.Data -r:output\Reportman.Drawing.dll
	$(COMPILE) -target:library -out:output\Reportman.Drawing.Forms.dll $(DEFINES) rppreviewmeta.cs rppreviewwinform.cs rpreportprogress.cs rpprintraw.cs rpprintwinforms.cs $(BASICLIBS) -r:output\Reportman.Drawing.dll -r:System.Windows.Forms
	$(COMPILE) -target:library -out:output\Reportman.Reporting.Forms.dll $(DEFINES) rpdatashow.cs $(BASICLIBS) -r:output\Reportman.Drawing.dll -r:output\Reportman.Drawing.Forms.dll -r:output\Reportman.Reporting.dll -r:System.Windows.Forms -r:System.Data

lib2:
	$(COMPILE2) -target:library -out:output2\Reportman.Drawing.dll $(DEFINES2) rpgraphics.cs rpprintout.cs rpcollections.cs rpprintoutprint.cs rpprintoutpdf.cs rptranslator.cs rpmetafile.cs rppdffile.cs rpinfoprovgdi.cs rpinfoprovid.cs  rptypes.cs $(BASICLIBS2)
	$(COMPILE2) -target:library -out:output2\Reportman.Reporting.dll $(DEFINES2) rpvariant.cs rpreport.cs rpxmlstream.cs rpdatainfo.cs rpsubreport.cs rpsection.cs rpchartitem.cs rpbarcodeitem.cs rpdrawitem.cs rpdataset.cs rpalias.cs rptypeval.cs rpparser.cs rpeval.cs rpevalfunc.cs rpparams.cs rpbasereport.cs rplabelitem.cs rpprintitem.cs $(BASICLIBS2) -r:System.Data -r:output2\Reportman.Drawing.dll
	$(MONORES2) rppreviewwinform.resx Reportman.Drawing.Forms.PreviewWinForms.resources
	$(COMPILE2) -target:library -out:output2\Reportman.Drawing.Forms.dll $(DEFINES2) rppreviewmeta.cs rppreviewwinform.cs rpreportprogress.cs rpprintraw.cs rpprintwinforms.cs $(BASICLIBS) -r:output2\Reportman.Drawing.dll -r:System.Windows.Forms -resource:Reportman.Drawing.Forms.PreviewWinForms.resources
	$(COMPILE2) -target:library -out:output2\Reportman.Reporting.Forms.dll $(DEFINES2) rpdatashow.cs $(BASICLIBS) -r:System.Data -r:output2\Reportman.Drawing.dll -r:output2\Reportman.Reporting.dll -r:output2\Reportman.Drawing.Forms.dll -r:System.Windows.Forms -r:System.Data  


printreport:
	$(COMPILE) printreport\rpprintreport.cs $(DEFINES) -out:output\printreport.exe rpreportprogress.cs rppreviewwinform.cs rpreport.cs rppreviewmeta.cs rpprintoutprint.cs rpxmlstream.cs rpdatainfo.cs rpcollections.cs rpsubreport.cs rpsection.cs rpchartitem.cs rpbarcodeitem.cs rpdrawitem.cs rpdataset.cs rpalias.cs rptypeval.cs rpparser.cs rpeval.cs rpevalfunc.cs rpparams.cs rpbasereport.cs rplabelitem.cs rpprintraw.cs rpgraphics.cs rpprintitem.cs rpprintwinforms.cs rpprintout.cs rpprintoutpdf.cs rptranslator.cs rpmetafile.cs rptypes.cs rppdffile.cs rpinfoprovgdi.cs rpvariant.cs rpinfoprovid.cs rpdatashow.cs -resource:rppreviewwinform.resx -doc:documentation\printreport.xml $(BASICLIBS) $(DATALIBS) -r:System.Windows.Forms -r:System.Data

printreport2:
#	$(COMPILE2) printreport\rpprintreport.cs $(DEFINES2) -out:output2\printreport.exe rpreportprogress.cs rppreviewwinform.cs rpreport.cs rppreviewmeta.cs rpprintoutprint.cs rpxmlstream.cs rpdatainfo.cs rpcollections.cs rpsubreport.cs rpsection.cs rpchartitem.cs rpbarcodeitem.cs rpdrawitem.cs rpdataset.cs rpalias.cs rptypeval.cs rpparser.cs rpeval.cs rpevalfunc.cs rpparams.cs rpbasereport.cs rplabelitem.cs rpprintraw.cs rpgraphics.cs rpprintitem.cs rpprintwinforms.cs rpprintout.cs rpprintoutpdf.cs rptranslator.cs rpmetafile.cs rptypes.cs rppdffile.cs rpinfoprovgdi.cs rpvariant.cs rpinfoprovid.cs rpdatashow.cs -resource:Reportman.Drawing.Forms.PreviewWinForms.resources -doc:documentation\printreport2.xml $(BASICLIBS2) -r:System.Windows.Forms -r:System.Data
	$(COMPILE2) printreport\rpprintreport.cs $(DEFINES2) -out:output2\printreport.exe -doc:documentation\printreport2.xml $(BASICLIBS2) -r:System.Windows.Forms -r:System.Data -r:output2\Reportman.Drawing -r:output2\Reportman.Drawing.Forms -r:output2\Reportman.Reporting -r:output2\Reportman.Reporting.Forms

doc:
	cd documentation
	$(NDOCPATH) -project=printreport.ndoc -vervose
	cd ..
doc2:
	cd documentation
	$(NDOCPATH) -project=printreport2.ndoc -vervose
	cd ..