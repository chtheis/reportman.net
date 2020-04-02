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
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;

namespace Reportman.Drawing
{
    public class  LogFontFt
    {
        public bool fixedpitch;
        public string postcriptname;
        public string familyname;
        public string stylename;
        public bool italic;
        public bool bold;
        public string filename;
        public int ascent;
        public int descent;
        public int weight;
        public int MaxWidth;
        public int avCharWidth;
        public int Capheight;
        public double ItalicAngle;
        public int leading;
        public Rectangle BBox;
        public bool fullinfo;
        public double StemV;
        public FT_FaceRec ftface;
        public bool faceinit;
        public bool havekerning;
        public bool type1;
        public double convfactor,widthmult;
        public string keyname;
        public IntPtr ftlibrary;
        public string kerningfile;
        public static SortedList<string,IntPtr> FontFaces = new SortedList<string,IntPtr>();
        public IntPtr iface;
        public LogFontFt()
        {
            kerningfile = "";
            iface = (IntPtr)0;
        }
        public void Dispose()
        {
        }
        public void OpenFont()
        {
            if (faceinit)
                return;
            Monitor.Enter(FontInfoFt.flag);
            try
            {
                if (FontFaces.IndexOfKey(keyname) >= 0)
                {
                    iface = FontFaces[keyname];
                    faceinit = true;
                }
                else
                {

                    FontInfoFt.CheckFreeType(FT.FT_New_Face(ftlibrary, filename, 0, out iface));
                    FT_FaceRec aface = new FT_FaceRec();
                    FontInfoFt.CheckFreeType(FT.FT_New_Face(ftlibrary, filename, 0, out iface));
                    aface = (FT_FaceRec)Marshal.PtrToStructure(iface, typeof(FT_FaceRec));
                    faceinit = true;
                    if (type1)
                    {
                        kerningfile = System.IO.Path.ChangeExtension(filename, ".afm");
                        if (File.Exists(kerningfile))
                        {
                            FontInfoFt.CheckFreeType(FT.FT_Attach_File(iface, kerningfile));
                        }
                    }
                    // Don't need scale, but this is a scale that returns
                    // exact widht for pdf if you divide the result
                    // of Get_Char_Width by 64
                    FontInfoFt.CheckFreeType(FT.FT_Set_Char_Size(iface, 0, 64 * 100, 720, 720));
                    FontFaces.Add(keyname, iface);
                }
            }
            finally
            {
                Monitor.Exit(FontInfoFt.flag);
            }
        }
    }
 
    public class FontInfoFt:FontInfoProvider,IDisposable
    {
        LogFontFt  currentfont;
        public static object flag = 12345;
        static IntPtr FreeTypeLib;
        static bool libraryinitialized;
        static SortedList<string,LogFontFt> fontlist = new SortedList<string,LogFontFt>();
        static SortedList<string, MemoryStream> FontStreams = new SortedList<string, MemoryStream>();
        static Strings fontpaths = new Strings();
        static SortedList<string,string> fontfiles = new SortedList<string,string>();
        static LogFontFt defaultfont;
        static LogFontFt defaultfontb;
        static LogFontFt defaultfontit;
        static LogFontFt defaultfontbit;

        public static void CheckFreeType(int nerror)
        {
            if (nerror == 0)
                return;
            if (FT.ErrorStrings.ContainsKey(nerror))
            {
                throw new Exception("Freetype function call error: "+FT.ErrorStrings[nerror]);
            }
            else
                throw new Exception("Freetype function call error: "+nerror.ToString());
        }

        private static void InitLibrary()
        {
            Monitor.Enter(flag);
            try
            {
                if (libraryinitialized)
                    return;
                CheckFreeType(FT.FT_Init_FreeType(out FreeTypeLib));
                libraryinitialized = true;
                
                Strings npaths = GetFontDirectories();
                foreach (string ndir in npaths)
                {
					if (Directory.Exists(ndir))
					{
                    string[] nfiles = StreamUtil.GetFiles(ndir,"*.TTF|*.ttf|*.pf*",SearchOption.TopDirectoryOnly);
                    foreach (string nfile in nfiles)
                    {
                        IntPtr iface = (IntPtr)0;
                        FT_FaceRec aface = new FT_FaceRec();
                        CheckFreeType(FT.FT_New_Face(FreeTypeLib, nfile, 0, out iface));
                        try
                        {
                            aface = (FT_FaceRec)Marshal.PtrToStructure(iface, typeof(FT_FaceRec));
                            if ((aface.face_flags & (int)FT_Face_Flags.FT_FACE_FLAG_SCALABLE)!=0)
                            {
                                LogFontFt aobj = new LogFontFt();
                                aobj.ftlibrary = FreeTypeLib;
                                aobj.fullinfo = false;
                                // Fill font properties
                                aobj.type1 = ((int)FT_Face_Flags.FT_FACE_FLAG_SFNT & aface.face_flags)==0;
                                if (aobj.type1)
                                {
                                   aobj.convfactor=1;
                                    //       aobj.convfactor:=1000/aface.units_per_EM;
                                   aobj.widthmult=aobj.convfactor;
                                }
                                else
                                {
                                   aobj.convfactor=1;
                                    //       aobj.convfactor:=1000/aface.units_per_EM;
                                   aobj.widthmult=aobj.convfactor;
                                }
                                aobj.filename=nfile;
                                string family_name = Marshal.PtrToStringAnsi(aface.family_name);
                                aobj.postcriptname=family_name.Replace(" ","");
                                aobj.familyname=family_name;
                                aobj.keyname = family_name + "____";
                                aobj.fixedpitch=(aface.face_flags & (int)FT_Face_Flags.FT_FACE_FLAG_FIXED_WIDTH)!=0;
                                aobj.havekerning=(aface.face_flags & (int)FT_Face_Flags.FT_FACE_FLAG_KERNING)!=0;
                                int nleft = System.Convert.ToInt32(Math.Round(aobj.convfactor*aface.bbox.xMin));
                                int nright = System.Convert.ToInt32(Math.Round(aobj.convfactor*aface.bbox.xMax));
                                int ntop = System.Convert.ToInt32(Math.Round(aobj.convfactor*aface.bbox.yMax));
                                int nbottom = System.Convert.ToInt32(Math.Round(aobj.convfactor*aface.bbox.yMin));
                                aobj.BBox = new Rectangle(nleft,ntop,nright-nleft,nbottom-ntop);
                                aobj.ascent=System.Convert.ToInt32(Math.Round(aobj.convfactor*aface.ascender));
                                aobj.descent=System.Convert.ToInt32(Math.Round(aobj.convfactor*aface.descender));
                                aobj.leading=System.Convert.ToInt32(Math.Round(aobj.convfactor*aface.height)-(aobj.ascent-aobj.descent));
                                aobj.MaxWidth=System.Convert.ToInt32(Math.Round(aobj.convfactor*aface.max_advance_width));
                                aobj.Capheight=System.Convert.ToInt32(Math.Round(aobj.convfactor*aface.ascender));
                                string style_name = Marshal.PtrToStringAnsi(aface.style_name);
                                aobj.stylename=style_name;
                                aobj.bold=(aface.style_flags & (int)FT_Style_Flags.FT_STYLE_FLAG_BOLD)!=0;
                                aobj.italic=(aface.style_flags & (int)FT_Style_Flags.FT_STYLE_FLAG_ITALIC)!=0;
                                if (aobj.bold)
                                    aobj.keyname = aobj.keyname + "B1";
                                else
                                    aobj.keyname = aobj.keyname + "B0";

                                if (aobj.italic)
                                    aobj.keyname = aobj.keyname + "I1";
                                else
                                    aobj.keyname = aobj.keyname + "I0";
                                // Default font configuration, LUXI SANS is default
                                if ((!aobj.italic) && (!aobj.bold))
                                {
                                   if (defaultfont==null)
                                    defaultfont=aobj;
                                   else
                                   {
                                        if (aobj.familyname.ToUpper()=="LUXI SANS")
                                        {
                                             defaultfont=aobj;
                                         }
                                   }
                                }
                                else
                                    if ((!aobj.italic) && (aobj.bold))
                                    {
                                       if  (defaultfontb==null)
                                        defaultfontb=aobj;
                                       else
                                       {
                                            if (aobj.familyname.ToUpper()=="LUXI SANS")
                                            {
                                                defaultfontb=aobj;
                                            }
                                       }
                                    }
                                    else
                                        if ((aobj.italic) && (!aobj.bold))
                                        {
                                               if (defaultfontit==null)
                                                    defaultfontit=aobj;
                                               else
                                               {
                                                if (aobj.familyname.ToUpper()=="LUXI SANS")
                                                {
                                                    defaultfontit=aobj;
                                                }
                                               }
                                        }
                                        else
                                            if ((aobj.italic) && (aobj.bold))
                                            {
                                               if (defaultfontbit==null)
                                                defaultfontbit=aobj;
                                               else
                                               {
                                                if (aobj.familyname.ToUpper()=="LUXI SANS")
                                                {
                                                 defaultfontbit=aobj;
                                                }
                                               }
                                            }

                                aobj.keyname = aobj.keyname.ToUpper();
                                if (fontlist.IndexOfKey(aobj.keyname)<0)
                                    fontlist.Add(aobj.keyname.ToUpper(),aobj);

                            }
                            int nindex = fontfiles.IndexOfKey(nfile);
                            if (nindex < 0)
                                fontfiles.Add(nfile, nfile);
                        }
                        finally
                        {
                            CheckFreeType(FT.FT_Done_Face(iface));
                        }
						}
                    }
                }
            }
            finally
            {
                Monitor.Exit(flag);
            }
        }
        private void SelectFont(PDFFont pdfFont)
        {
            string fontname = "";
            if ((System.Environment.OSVersion.Platform == PlatformID.Unix) || (System.Environment.OSVersion.Platform == PlatformID.MacOSX))
            {
                fontname = pdfFont.LFontName.ToUpper();
            }
            else
            {
                fontname = pdfFont.WFontName.ToUpper();
            }
            string familyname = fontname;
            string suffix = "";
            bool isbold = (pdfFont.Style & 1) > 0;
            bool isitalic = (pdfFont.Style & 2) > 0;
            if (isbold)
                suffix = "____B1";
            else
                suffix = "____B0";
            if (isitalic)
                suffix = suffix + "I1";
            else
                suffix = suffix + "I0";
            fontname = fontname+suffix;
            if (fontlist.IndexOfKey(fontname) >= 0)
            {
                currentfont = fontlist[fontname];
                return;
            }
            // Search similar font
            string familyonly = "";
            
            foreach (string fname in fontlist.Keys)
            {
                int idx = fname.IndexOf(familyname);
                if (idx >= 0)
                {
                    familyonly = fname;
                    idx = fname.IndexOf(suffix);
                    if (idx >= 0)
                    {
                        currentfont = fontlist[fname];
                        return;
                    }
                }
            }
            if (familyonly.Length>0)
            {
                currentfont = fontlist[familyonly];
                return;
            }
            if (isbold && isitalic)
            {
                currentfont = defaultfontbit;
            }
            else
                if (isbold && (!isitalic))
                {
                    currentfont = defaultfontb;
                }
                else
                    if ((!isbold) && (isitalic))
                        currentfont = defaultfontit;
                    else
                    {
                        currentfont = defaultfont;
                    }
            fontlist.Add(fontname, currentfont);
        }
		public override void FillFontData(PDFFont pdfFont, TTFontData data)
        {
            InitLibrary();

            SelectFont(pdfFont);

            data.IsUnicode = true;
            if (!currentfont.type1)
            {
                Monitor.Enter(flag);
                try
                {
                    if (data.FontData == null)
                    {
                        if (FontStreams.IndexOfKey(currentfont.keyname) >= 0)
                        {
                            data.FontData = new AdvFontData();
                            data.FontData.Data = FontStreams[currentfont.keyname].ToArray();
                        }
                        MemoryStream nstream = StreamUtil.FileToMemoryStream(currentfont.filename);
                        data.FontData = new AdvFontData();
                        data.FontData.Data = nstream.ToArray();
                        FontStreams.Add(currentfont.keyname, nstream);
                    }
                }
                finally
                {
                    Monitor.Exit(flag);
                }
            }
            data.PostcriptName = currentfont.postcriptname;
            data.FontFamily = currentfont.familyname;
            data.FaceName = currentfont.familyname;
            data.Ascent = currentfont.ascent;
            data.Descent = currentfont.descent;
            data.Leading = currentfont.leading;
            data.CapHeight = currentfont.Capheight;
            data.Encoding = "WinAnsiEncoding";
            data.FontWeight = 0;
            data.MaxWidth = currentfont.MaxWidth;
            data.AvgWidth = currentfont.avCharWidth;
            data.HaveKerning = currentfont.havekerning;
            data.StemV = 0;
            data.FontStretch = "/Normal";
            data.FontBBox = currentfont.BBox;
            data.LogFont = currentfont;
            if (currentfont.italic)
                data.ItalicAngle = -15;
            else
                data.ItalicAngle = 0;
            data.StyleName = currentfont.stylename;
            data.Flags = 32;
            if (currentfont.fixedpitch)
                data.Flags = data.Flags + 1;
            if (pdfFont.Bold)
                data.PostcriptName = data.PostcriptName + ",Bold";
            if (pdfFont.Italic)
            {
                if (pdfFont.Bold)
                    data.PostcriptName = data.PostcriptName + "Italic";
                else
                    data.PostcriptName = data.PostcriptName + ",Italic";
            }
            data.Type1 = currentfont.type1;
        }
        public override int GetCharWidth(PDFFont pdfFont, TTFontData data,
				 char charCode)
        {
            InitLibrary();

            int aint = (int)charCode;
            if (data.Widths.IndexOfKey(charCode) >= 0)
            {
                return data.Widths[charCode];
            }
            LogFontFt cfont = data.LogFont;
            cfont.OpenFont();

            int awidth = 0;
            Monitor.Enter(flag);
            try
            {
                if (data.Widths.IndexOfKey(charCode) >= 0)
                {
                    awidth = data.Widths[charCode];
                }
                else
                {
                    if (0 == FT.FT_Load_Char(cfont.iface, (uint)charCode, (int)FT.FT_LOAD_NO_SCALE))
                    {
                        FT_FaceRec aface = (FT_FaceRec)Marshal.PtrToStructure(cfont.iface, typeof(FT_FaceRec));
                        FT_GlyphSlotRec aglyph = (FT_GlyphSlotRec)Marshal.PtrToStructure(aface.glyph, typeof(FT_GlyphSlotRec));
                        ushort width1 = (ushort)(aglyph.linearHoriAdvance >> 16);
                        ushort width2 = (ushort)(aglyph.linearHoriAdvance & 0x0000FFFF);
                        double dwidth = width1 + width2 / (double)65535;
                        awidth = System.Convert.ToInt32(Math.Round(cfont.widthmult * dwidth));

                    }
                    data.Widths[charCode] = awidth;
                    data.Glyphs[charCode] = System.Convert.ToInt32(FT.FT_Get_Char_Index(cfont.iface, charCode));
                    if (data.FirstLoaded > aint)
                        data.FirstLoaded = aint;
                    if (data.LastLoaded < aint)
                        data.LastLoaded = aint;
                }
            }
            finally
            {
                Monitor.Exit(flag);
            }
            return awidth;
        }
        public override int GetKerning(PDFFont pdfFont, TTFontData data,
				 char leftChar, char rightChar)
        {
            LogFontFt cfont = data.LogFont;
            if (!cfont.havekerning)
                return 0;
            int nresult = 0;
            //string nkerning = ""+leftChar+rightChar;
            ulong nkerning = (ulong)((int)leftChar << 32) + (ulong)rightChar;

            if (data.Kernings.IndexOfKey(nkerning) >= 0)
            {
                return data.Kernings[nkerning];
            }
            cfont.OpenFont();
            Monitor.Enter(flag);
            try
            {
                if (data.Kernings.IndexOfKey(nkerning) >= 0)
                {
                    nresult = data.Kernings[nkerning];
                }
                uint w1 = FT.FT_Get_Char_Index(cfont.iface,(uint)leftChar);
                if (w1 > 0)
                {
                    uint w2 = FT.FT_Get_Char_Index(cfont.iface, (uint)rightChar);
                    if (w2 > 0)
                    {
                        FT_Vector akerning;
                        CheckFreeType(FT.FT_Get_Kerning(cfont.iface,w1,w2,(uint)FT_Kerning_Flags.FT_KERNING_UNSCALED, out akerning));
                        nresult = System.Convert.ToInt32(Math.Round(cfont.widthmult*-akerning.x));
                    }
                    else
                        data.Kernings.Add(nkerning, 0);
                }
                else
                    data.Kernings.Add(nkerning, 0);
            }
            finally
            {
                Monitor.Exit(flag);
            }
            return nresult;
        }
        public override MemoryStream GetFontStream(TTFontData data)
        {
#if REPMAN_COMPACT              
            return null;
#else
            Dictionary<int, int[]> glyps = new Dictionary<int, int[]>();
            foreach (char xchar in data.Glyphs.Keys)
            {
                int gl = data.Glyphs[xchar];
                int width = data.Widths[xchar];
                if (!glyps.ContainsKey(gl))
                    glyps[gl] = new int[] { gl, width, (int)xchar };
            }
            TrueTypeFontSubSet subset = new TrueTypeFontSubSet(data.PostcriptName, data.FontData.Data,
                glyps, 0);
            byte[] nresult = subset.Execute();
            return new MemoryStream(nresult);
#endif
        }

        public FontInfoFt()
        {
            InitLibrary();
        }
        public void Dispose()
        {

        }
        static public string GetFontPath()
        {
            string systemPath = Environment.GetFolderPath(Environment.SpecialFolder.System);
            string result = Path.GetDirectoryName(systemPath)
                + Path.DirectorySeparatorChar 
                + "FONTS"
                + Path.DirectorySeparatorChar;
                return result;
        }


        public static Strings GetFontDirectories()
        {
            Strings dirs = new Strings();
            Strings afile = null;
            switch (System.Environment.OSVersion.Platform)
            {
                case PlatformID.MacOSX:
                    dirs.Add("/Library/Fonts");
                    dirs.Add("~/Library/Fonts");
                    dirs.Add("/System/Library/Fonts");
                    break;
                case PlatformID.Unix:
                    if (File.Exists("/etc/fonts/fonts.conf"))
                    {
                        afile = new Strings();
                        afile.LoadFromFile("/etc/fonts/fonts.conf");
                    }
                    else
                        throw new Exception("File not found: /etc/fonts/fonts.conf");
                    string nstring = afile.ToSemiColon();
         int index = nstring.IndexOf("<dir");
         if (index >= 0)
            nstring = nstring.Substring(index + 4, nstring.Length  - (index + 4));
         index = nstring.IndexOf(">");
         if (index >= 0)
            nstring = nstring.Substring(index + 1, nstring.Length - (index + 1));
         index = nstring.IndexOf("</dir");
         while (index >= 0)
         {
            string ndir = nstring.Substring(0,index);
            dirs.Add(ndir);
            nstring = nstring.Substring(index+4,nstring.Length-(index+4));
            
            index = nstring.IndexOf("<dir");
            if (index >= 0)
               nstring = nstring.Substring(index + 4, nstring.Length - (index + 4));
            index = nstring.IndexOf(">");
            if (index >= 0)
               nstring = nstring.Substring(index + 1, nstring.Length - (index + 1));
            index = nstring.IndexOf("</dir");
         }
                    break;
                default:
                    dirs.Add(GetFontPath());
                    break;
            }
            return dirs;
        }
    }
}
