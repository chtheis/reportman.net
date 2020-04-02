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
using System.Drawing;
using System.IO;
#if REPMAN_COMPACT
#else
using System.Drawing.Text;
#endif
using System.Threading;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text;
using System.Collections.Generic;


namespace Reportman.Drawing
{
#if REPMAN_COMPACT
#else
    /// <summary>
    /// This enumeration indicates diferent optimization implementations while drawing pages 
    /// (preview, web presentation or print). Microsoft .Net have an optimized graphic object, 
    /// called Windows Metafile, so when the same graphic operations must be done multiple times,
    /// they can be stored in it and "played" multiple times. This is the case for example, 
    /// when previewing a Report, you can go forward and backwards drawing again and again the
    /// same pages. If the engine have the capability of drawing multiple pages in the screen,
    /// like Report Manager Windows Forms preview does, the use of playing optimized windows
    /// metafiles can enhace performance, specially if the pages are dense (lot of text items).
    /// The drawbacks of playing metafiles is that they must be stored into memory, so when you
    /// optimize performance you hit memory consumption.
    /// <see cref="Variant">PrintOutNet</see>
    /// </summary>   
    public enum WMFOptimization : int { 
        /// <summary>No Windows Metafile caching will be done</summary>
        None,
        /// <summary>Windows Metafile caching will be done, EmfType.EmfOnly Windows Metafile will be used</summary>
        Gdi,
        /// <summary>Windows Metafile caching will be done, EmfType.EmfPlusDual Windows Metafile will be used</summary>
        Gdiplus,
        /// <summary>Windows Metafile caching will be done, EmfType.EmfPlusOnly Windows Metafile will be used</summary>
        Gdiplusonly
    };
#endif
    /// <summary>
    /// Event launched when there is no data available to print. Should return
    /// true if the action have been handled and no futher processing must happen
    /// </summary>
    /// <param name="pageindex">Page requested</param>
    /// <returns></returns>
    public delegate bool NoDataToPrintEvent();
    /// <summary>
    /// This structure is used to store information about a block of text properties, alignment and content, 
    /// it is used by some functions to measure texts or pass text and format information in a single variable
    /// </summary>
	public struct TextObjectStruct
	{
        /// <summary>Text content</summary>
		public string Text;
        /// <summary>Family name of the font when the text is drawn in Linux</summary>
        public string LFontName;
        /// <summary>Family name of the font when the text is drawn in Microsoft Windows</summary>
        public string WFontName;
        /// <summary>Font size</summary>
        public short FontSize;
        /// <summary>Font rotation</summary>
        public short FontRotation;
        /// <summary>Font style as a short</summary>
        public short FontStyle;
        /// <summary>Font color</summary>
        public int FontColor;
        /// <summary>Font type when exporting to Adobe PDF</summary>
        public PDFFontType Type1Font;
        /// <summary>Flag to indicate if the text must be clip to the box</summary>
        public bool CutText;
        /// <summary>Text alignment as an integer</summary>
        public int Alignment;
        /// <summary>Flag to indicate if the text must wrap multiple lines</summary>
        public bool WordWrap;
        /// <summary>Flag to indicate if the text must be dran right to left</summary>
        public bool RightToLeft;
        /// <summary>Print step, for dot matrix printers</summary>
        public PrintStepType PrintStep;
        /// <summary>
        /// Returns a TextObjectStruct from a MetaObject
        /// </summary>
        /// <param name="apage">Parent page of the MetaObject</param>
        /// <param name="aobj">MetaObject with data</param>
        public static TextObjectStruct FromMetaObjectText(MetaPage apage,MetaObjectText aobj)
        {
            TextObjectStruct aresult = new TextObjectStruct();
            aresult.Text = apage.GetText(aobj);
            aresult.LFontName = apage.GetLFontNameText(aobj);
            aresult.WFontName = apage.GetWFontNameText(aobj);
            aresult.WordWrap = aobj.WordWrap;
            aresult.Type1Font = aobj.Type1Font;
            aresult.RightToLeft = aobj.RightToLeft;
            aresult.FontColor = aobj.FontColor;
            aresult.FontRotation = aobj.FontRotation;
            aresult.FontStyle = aobj.FontStyle;
            aresult.PrintStep = aobj.PrintStep;
            aresult.CutText = aobj.CutText;
            aresult.FontSize = aobj.FontSize;
            aresult.Alignment = aobj.Alignment;
            return aresult;
        }
	}
    /// <summary>
    /// Event triggered when a driver starts printing
    /// </summary>
    /// <param name="driver"></param>
	public delegate void BeginPrintOut(PrintOut driver);
    /// <summary>
    /// Base class for print drivers define the funtions to be implemented by
    /// any print driver (Report processing driver)
    /// </summary>
	public abstract class PrintOut
	{
        public static string DefaultPrinterName = "";
        public string ForcePrinterName = "";
        public bool Previewing;
        /// <summary>
        /// Array of predefined page sizes
        /// </summary>
  		public static int[,] PageSizeArray=new int[149,2]
    		{
      { 8268, 11693},  // psA4
      { 7165, 10118},  // psB5
      { 8500, 11000},  // psLetter
      { 8500, 14000},  // psLegal
      { 7500, 10000},  // psExecutive
      { 33110, 46811}, // psA0
      { 23386, 33110}, // psA1
      { 16535, 23386}, // psA2
      { 11693, 16535}, // psA3
      { 5827, 8268},   // psA5
      { 4134, 5827},   // psA6
      { 2913, 4134},   // psA7
      { 2047, 2913},   // psA8
      { 1457, 2047},   // psA9
      { 40551, 57323}, // psB0
      { 28661, 40551}, // psB1
      { 1260, 1772},   // psB10
      { 20276, 28661}, // psB2
      { 14331, 20276}, // psB3
      { 10118, 14331}, // psB4
      { 5039, 7165},   // psB6
      { 3583, 5039},   // psB7
      { 2520, 3583},   // psB8
      { 1772, 2520},   // psB9
      { 6417, 9016},   // psC5E
      { 4125, 9500},   // psComm10E
      { 4331, 8661},   // psDLE
      { 8250, 13000},  // psFolio
      { 17000, 11000}, // psLedger
      { 11000, 17000}, // psTabloid
      { -1, -1},        // psNPageSize
                                    // Windows equivalents begins at 31
      { 8500, 11000}, // Letter 8 12 x 11 in
      { 8500, 11000}, // Letter Small 8 12 x 11 in
      { 11000, 17000},  // Tabloid 11 x 17 in
      { 17000, 11000},  // Ledger 17 x 11 in
      { 8500, 14000},  // Legal 8 12 x 14 in
      { 55000, 8500},  // Statement 5 12 x 8 12 in
      { 7500, 10500}, // Executive 7 14 x 10 12 in
      { 11693, 16535}, // A3 297 x 420 mm                     
      { 8268, 11693},      // A4 210 x 297 mm                     
      { 8268, 11693},// A4 Small 210 x 297 mm               
      { 5827, 8268}, // A5 148 x 210 mm                     
      { 10118, 14331},    // B4 (JIS) 250 x 354                  
      { 7165, 10118}, // B5 (JIS) 182 x 257 mm               
      { 8250, 13000}, // Folio 8 12 x 13 in                  
      { 8465, 10827}, // Quarto 215 x 275 mm                 
      { 10000, 14000}, // 10x14 in                            
    { 11000, 17000},// 11x17 in                            
    { 8500, 11000}, // Note 8 12 x 11 in                  
    { 3875, 8875},// Envelope #9 3 78 x 8 78             
    { 4125, 9500},// Envelope #10 4 18 x 9 12            
    { 4500, 10375},// Envelope #11 4 12 x 10 38           
    { 4276, 11000},// Envelope #12 4 \276 x 11            
    { 5000, 11500},// Envelope #14 5 x 11 12              
    { 16969, 21969},// C size sheet 431 x 558 mm                       
    { 21969, 33976},// D size sheet 558 x 863 mm                      
    { 33976, 43976},// E size sheet 863 x 1117 mm                       
    { 4331, 8661},// Envelope DL 110 x 220mm             
    { 6378, 9016},// Envelope C5 162 x 229 mm            
    { 12756, 18031},// Envelope C3  324 x 458 mm           
    { 9016, 12756},// Envelope C4  229 x 324 mm           
    { 4488, 6378},// Envelope C6  114 x 162 mm           
    { 4488, 9016},// Envelope C65 114 x 229 mm           
    { 9843, 13898},// Envelope B4  250 x 353 mm           
    { 6929, 9843},// Envelope B5  176 x 250 mm           
    { 6929, 4921},// Envelope B6  176 x 125 mm           
    { 4331, 9056},// Envelope 110 x 230 mm               
    { 3875, 7500}, // Envelope Monarch 3.875 x 7.5 in     
    { 3625, 6500},// 6 34 Envelope 3 58 x 6 12 in        
    { 14875, 11000},// US Std Fanfold 14 78 x 11 in        
    { 8500, 12000},// German Std Fanfold 8 12 x 12 in    
    { 8500, 13000},// German Legal Fanfold 8 12 x 13 in  
    { 9843, 13898},// B4 (ISO) 250 x 353 mm               
    { 3937, 5827},// Japanese Postcard 100 x 148 mm      
    { 9000, 11000}, // 9 x 11 in                           
    { 10000, 11000}, // 10 x 11 in                          
    { 15000, 11000}, // 15 x 11 in                          
    { 8661, 8661}, // Envelope Invite 220 x 220 mm        
    { 100, 100}, // RESERVED--DO NOT USE                
    { 100, 100}, // RESERVED--DO NOT USE                
    { 9275, 12000}, // Letter Extra 9 \275 x 12 in         
    { 9275, 15000}, // Legal Extra 9 \275 x 15 in          
    { 11690, 18000}, // Tabloid Extra 11.69 x 18 in         
    { 9270, 12690}, // A4 Extra 9.27 x 12.69 in            
    { 8275, 11000},  // Letter Transverse 8 \275 x 11 in    
    { 8268, 11693},  // A4 Transverse 210 x 297 mm          
    { 9275, 12000}, // Letter Extra Transverse 9\275 x 12 in  
    { 8937, 14016},     // SuperASuperAA4 227 x 356 mm      
    { 12008, 19172},    // SuperBSuperBA3 305 x 487 mm       
    { 8500, 12690},    // Letter Plus 8.5 x 12.69 in          
    { 8268, 12992},    // A4 Plus 210 x 330 mm                
    { 5828, 8268},    // A5 Transverse 148 x 210 mm          
    { 7166, 10118},    // B5 (JIS) Transverse 182 x 257 mm    
    { 13071, 17520},    // A3 Extra 322 x 445 mm               
    { 6850, 9252},    // A5 Extra 174 x 235 mm               
    { 7913, 10867},    // B5 (ISO) Extra 201 x 276 mm         
    { 16536, 23386},    // A2 420 x 594 mm                     
    { 11693, 16535},    // A3 Transverse 297 x 420 mm          
    { 13071, 17520},     // A3 Extra Transverse 322 x 445 mm    
    { 7874, 5827}, // Japanese Double Postcard 200 x 148 mm 
    { 4173,5827},  // A6 105 x 148 mm                 }
    { 9449, 13071},  // Japanese Envelope Kaku #2 240 x 332 mm       
    { 8504, 10906},  // Japanese Envelope Kaku #3 216 x 277 mm     
    { 4724, 9252},  // Japanese Envelope Chou #3 120 x 235 mm      
    { 3543, 8071},  // Japanese Envelope Chou #4  90 x 205 mm    
    { 11000, 8500},  // Letter Rotated 11 x 8 1/2 11 in 
    { 16535, 11693},  // A3 Rotated 420 x 297 mm         
    { 11693, 8268},  // A4 Rotated 297 x 210 mm         
    { 8268, 5828},  // A5 Rotated 210 x 148 mm         
    { 14331, 10118},  // B4 (JIS) Rotated 364 x 257 mm   
    { 10118, 7165},  // B5 (JIS) Rotated 257 x 182 mm   
    { 5827, 3937}, // Japanese Postcard Rotated 148 x 100 mm 
    { 5827, 7874}, // Double Japanese Postcard Rotated 148 x 200 mm 
    { 5827, 4173}, // A6 Rotated 148 x 105 mm         
    { 13071, 9449},  // Japanese Envelope Kaku #2 Rotated
    { 10906, 8504},  // Japanese Envelope Kaku #3 Rotated
    { 9252, 4724},  // Japanese Envelope Chou #3 Rotated
    { 8071, 3543},  // Japanese Envelope Chou #4 Rotated
    { 5039, 7165},  // B6 (JIS) 128 x 182 mm           
    { 7165, 5039},  // B6 (JIS) Rotated 182 x 128 mm   
    { 12000, 11000},  // 12 x 11 in                      
    { 4134, 9252},  // Japanese Envelope You #4 105 x 235 mm       
    { 9252, 4134},  // Japanese Envelope You #4 Rotated
    { 5748, 8465},  // PRC 16K 146 x 215 mm            
    { 3819, 5945},  // PRC 32K 97 x 151 mm             
    { 3819, 5945},  // PRC 32K(Big) 97 x 151 mm        
    { 4134, 6496},  // PRC Envelope #1 102 x 165 mm    
    { 4134, 6929},  // PRC Envelope #2 102 x 176 mm    
    { 4921, 5929},  // PRC Envelope #3 125 x 176 mm    
    { 4331, 8189},  // PRC Envelope #4 110 x 208 mm    
    { 4331, 8661}, // PRC Envelope #5 110 x 220 mm    
    { 4724, 9055}, // PRC Envelope #6 120 x 230 mm    
    { 6299, 9055}, // PRC Envelope #7 160 x 230 mm    
    { 4724, 12165}, // PRC Envelope #8 120 x 309 mm    
    { 9016, 12756}, // PRC Envelope #9 229 x 324 mm    
    { 12756, 18031}, // PRC Envelope #10 324 x 458 mm   
    { 8465, 5748}, // PRC 16K Rotated                 
    { 5945, 3819}, // PRC 32K Rotated                 
    { 5945, 3819}, // PRC 32K(Big) Rotated            
    { 6496, 4134}, // PRC Envelope #1 Rotated 165 x 102 mm
    { 6929, 4134}, // PRC Envelope #2 Rotated 176 x 102 mm
    { 6929, 4921}, // PRC Envelope #3 Rotated 176 x 125 mm
    { 8189, 4331}, // PRC Envelope #4 Rotated 208 x 110 mm
    { 8661, 4331}, // PRC Envelope #5 Rotated 220 x 110 mm
    { 9055, 4724}, // PRC Envelope #6 Rotated 230 x 120 mm
    { 9055, 6299}, // PRC Envelope #7 Rotated 230 x 160 mm
    { 12165, 4724}, // PRC Envelope #8 Rotated 309 x 120 mm
    { 12756, 9016}, // PRC Envelope #9 Rotated 324 x 229 mm
    { 18031, 12756} // PRC Envelope #10 Rotated 458 x 324 mm

		};
        /// <summary>
        /// Returns page size name for provided index
        /// </summary>
        /// <param name="index">Index inside page size array</param>
        /// <returns>String result, containing page size name</returns>
        public static string PageSizeName(int index)
        {
            string aresult = "";
            switch (index)
            {
                case 0:
                    aresult = "A4";
                    break;
                case 1:
                    aresult = "B5";
                    break;
                case 2:
                    aresult = "Letter";
                    break;
                case 3:
                    aresult = "Legal";
                    break;
                case 4:
                    aresult = "Executive";
                    break;
                case 5:
                    aresult = "A0";
                    break;
                case 6:
                    aresult = "A1";
                    break;
                case 7:
                    aresult = "A2";
                    break;
                case 8:
                    aresult = "A3";
                    break;
                case 9:
                    aresult = "A5";
                    break;
                case 10:
                    aresult = "A6";
                    break;
                case 11:
                    aresult = "A7";
                    break;
                case 12:
                    aresult = "A8";
                    break;
                case 13:
                    aresult = "A9";
                    break;
                case 14:
                    aresult = "B0";
                    break;
                case 15:
                    aresult = "B1";
                    break;
                case 16:
                    aresult = "B10";
                    break;
                case 17:
                    aresult = "B2";
                    break;
                case 18:
                    aresult = "B3";
                    break;
                case 19:
                    aresult = "B4";
                    break;
                case 20:
                    aresult = "B6";
                    break;
                case 21:
                    aresult = "B7";
                    break;
                case 22:
                    aresult = "B8";
                    break;
                case 23:
                    aresult = "B9";
                    break;
                case 24:
                    aresult = "C5E";
                    break;
                case 25:
                    aresult = "C10E";
                    break;
                case 26:
                    aresult = "DLE";
                    break;
                case 27:
                    aresult = "Folio";
                    break;
                case 28:
                    aresult = "Ledger";
                    break;
                case 29:
                    aresult = "Tabloid";
                    break;
                case 30:
                    aresult = "Custom";
                    break;
                // Windows
                case 31:
                    aresult = "Letter";
                    break;
                // Windows
                case 32:
                    aresult = "Letter Small";
                    break;
                case 33:
                    aresult = "Tabloid";
                    break;
                case 34:
                    aresult = "Ledger";
                    break;
                case 35:
                    aresult = "Legal";
                    break;
                case 36:
                    aresult = "Statement";
                    break;
                case 37:
                    aresult = "Executive";
                    break;
                case 38:
                    aresult = "A3";
                    break;
                case 39:
                    aresult = "A4";
                    break;
                case 40:
                    aresult = "A4 Small";
                    break;
                case 41:
                    aresult = "A5";
                    break;
                case 42:
                    aresult = "B4";
                    break;
                case 43:
                    aresult = "B5";
                    break;
                case 44:
                    aresult = "Folio";
                    break;
                case 45:
                    aresult = "Quarto";
                    break;
                case 46:
                    aresult = "10x14 in";
                    break;
                case 47:
                    aresult = "11x17 in";
                    break;
                case 48:
                    aresult = "Note";
                    break;
                case 49:
                    aresult = "Envelope #9";
                    break;
                case 50:
                    aresult = "Envelope #10";
                    break;
                case 51:
                    aresult = "Envelope #11";
                    break;
                case 52:
                    aresult = "Envelope #12";
                    break;
                case 53:
                    aresult = "Envelope #14";
                    break;
                case 54:
                    aresult = "C";
                    break;
                case 55:
                    aresult = "D";
                    break;
                case 56:
                    aresult = "E";
                    break;
                case 57:
                    aresult = "Envelope DL";
                    break;
                case 58:
                    aresult = "Envelope C5";
                    break;
                case 59:
                    aresult = "Envelope C3";
                    break;
                case 60:
                    aresult = "Envelope C4";
                    break;
                case 61:
                    aresult = "Envelope C6";
                    break;
                case 62:
                    aresult = "Envelope C65";
                    break;
                case 63:
                    aresult = "Envelope B4";
                    break;
                case 64:
                    aresult = "Envelope B5";
                    break;
                case 65:
                    aresult = "Envelope B6";
                    break;
                case 66:
                    aresult = "Envelope";
                    break;
                case 67:
                    aresult = "Envelope Monarch";
                    break;
                case 68:
                    aresult = "Envelope 6 34";
                    break;
                case 69:
                    aresult = "US Std Fanfold";
                    break;
                case 70:
                    aresult = "German Std Fanfold";
                    break;
                case 71:
                    aresult = "German Legal Fanfold";
                    break;
                case 72:
                    aresult = "B4 ISO";
                    break;
                case 73:
                    aresult = "Japanese Postcard";
                    break;
                case 74:
                    aresult = "9x11 in";
                    break;
                case 75:
                    aresult = "10x11 in";
                    break;
                case 76:
                    aresult = "15x11 in";
                    break;
                case 77:
                    aresult = "Envelope invite";
                    break;
                case 78:
                    aresult = "---";
                    break;
                case 79:
                    aresult = "---";
                    break;
                case 80:
                    aresult = "Letter Extra 1";
                    break;
                case 81:
                    aresult = "Letter Extra 2";
                    break;
                case 82:
                    aresult = "Tabloid Extra";
                    break;
                case 83:
                    aresult = "A4 Extra";
                    break;
                case 84:
                    aresult = "Letter trans.";
                    break;
                case 85:
                    aresult = "A4 trans.";
                    break;
                case 86:
                    aresult = "Letter Extra trans.";
                    break;
                case 87:
                    aresult = "SuperASuperAA4";
                    break;
                case 88:
                    aresult = "SuperVSuperBA3";
                    break;
                case 89:
                    aresult = "Letter Plus";
                    break;
                case 90:
                    aresult = "A4 Plus";
                    break;
                case 91:
                    aresult = "A5 trans.";
                    break;
                case 92:
                    aresult = "B5 trans.";
                    break;
                case 93:
                    aresult = "A3 Extra";
                    break;
                case 94:
                    aresult = "A5 Extra";
                    break;
                case 95:
                    aresult = "B5 Extra";
                    break;
                case 96:
                    aresult = "A2";
                    break;
                case 97:
                    aresult = "A3 trans.";
                    break;
                case 98:
                    aresult = "A3 Extra trans.";
                    break;
                case 99:
                    aresult = "Japanese Double Postcard";
                    break;
                case 100:
                    aresult = "A6";
                    break;
                case 101:
                    aresult = "Japanese Envelope Kaku #2";
                    break;
                case 102:
                    aresult = "Japanese Envelope Kaku #3";
                    break;
                case 103:
                    aresult = "Japanese Envelope Chou #3";
                    break;
                case 104:
                    aresult = "Japanese Envelope Chou #4";
                    break;
                case 105:
                    aresult = "Letter Rotated";
                    break;
                case 106:
                    aresult = "A3 Rotated";
                    break;
                case 107:
                    aresult = "A4 Rotated";
                    break;
                case 108:
                    aresult = "A5 Rotated";
                    break;
                case 109:
                    aresult = "B4 Rotated";
                    break;
                case 110:
                    aresult = "B5 Rotated";
                    break;
                case 111:
                    aresult = "Japanese Postcard Rotated";
                    break;
                case 112:
                    aresult = "Double Japanese Postcard Rotated";
                    break;
                case 113:
                    aresult = "A6 Rotated";
                    break;
                case 114:
                    aresult = "Japanese Envelope Kaku #2 Rotated";
                    break;
                case 115:
                    aresult = "Japanese Envelope Kaku #3 Rotated";
                    break;
                case 116:
                    aresult = "Japanese Envelope Chou #3 Rotated";
                    break;
                case 117:
                    aresult = "Japanese Envelope Chou #4 Rotated";
                    break;
                case 119:
                    aresult = "B6";
                    break;
                case 120:
                    aresult = "B6 Rotated";
                    break;
                case 121:
                    aresult = "12x11 in";
                    break;
                case 122:
                    aresult = "Japanese Envelope You #4";
                    break;
                case 123:
                    aresult = "Japanese Envelope You #4 Rotated";
                    break;
                case 124:
                    aresult = "PRC 16K";
                    break;
                case 125:
                    aresult = "PRC 32K";
                    break;
                case 126:
                    aresult = "PRC 32K (Big)";
                    break;
                case 127:
                    aresult = "PRC Envelope #1";
                    break;
                case 128:
                    aresult = "PRC Envelope #2";
                    break;
                case 129:
                    aresult = "PRC Envelope #3";
                    break;
                case 130:
                    aresult = "PRC Envelope #4";
                    break;
                case 131:
                    aresult = "PRC Envelope #5";
                    break;
                case 132:
                    aresult = "PRC Envelope #6";
                    break;
                case 133:
                    aresult = "PRC Envelope #7";
                    break;
                case 134:
                    aresult = "PRC Envelope #8";
                    break;
                case 135:
                    aresult = "PRC Envelope #9";
                    break;
                case 136:
                    aresult = "PRC Envelope #10";
                    break;
                case 137:
                    aresult = "PRC 16K Rotated";
                    break;
                case 138:
                    aresult = "PRC 32K Rotated";
                    break;
                case 139:
                    aresult = "PRC 32K (Big) Rotated";
                    break;
                case 140:
                    aresult = "PRC Envelope #1 Rotated";
                    break;
                case 141:
                    aresult = "PRC Envelope #2 Rotated";
                    break;
                case 142:
                    aresult = "PRC Envelope #3 Rotated";
                    break;
                case 143:
                    aresult = "PRC Envelope #4 Rotated";
                    break;
                case 144:
                    aresult = "PRC Envelope #5 Rotated";
                    break;
                case 145:
                    aresult = "PRC Envelope #6 Rotated";
                    break;
                case 146:
                    aresult = "PRC Envelope #7 Rotated";
                    break;
                case 147:
                    aresult = "PRC Envelope #8 Rotated";
                    break;
                case 148:
                    aresult = "PRC Envelope #9 Rotated";
                    break;
                case 149:
                    aresult = "PRC Envelope #10 Rotated";
                    break;
                default:
                    aresult = "Unknown";
                    break;
            }
            return aresult;
        }
        /// <summary>
        /// Maximum number of pages allowed (this is to avoid stack overflow recursions)
        /// </summary>
		protected int PRINTOUT_MAX_PAGES = 100000;
        /// <summary>
        /// Starting page, by default 1
        /// </summary>
        public int FromPage;
        /// <summary>
        /// End page, by default, the maximum
        /// </summary>
        public int ToPage;
        /// <summary>
        /// Number of copies, by default 1
        /// </summary>
        public int Copies;
        /// <summary>
        /// Default orientation, by default Portrait
        /// </summary>
        protected OrientationType FOrientation;
        /// <summary>
        /// Throw exception when there is no data available to print
        /// </summary>
        public bool EmptyReportThrowException;
        /// <summary>
        /// Event to control 
        /// </summary>
        public NoDataToPrintEvent NoData;
        /// <summary>
        /// Internally determines if the searched texts must be drawn as a selection.
        /// Used in preview.
        /// </summary>
        protected bool DrawFound;
        /// <summary>
        /// Constructor, set default values only
        /// </summary>
        protected PrintOut()
		{
            DrawFound = true;
			FromPage = 1;
			ToPage = PRINTOUT_MAX_PAGES;
			Copies = 0;
			FOrientation=OrientationType.Portrait;
		}
        /// <summary>
        /// Free memory consumed by graphics resources
        /// </summary>
        public virtual void Dispose()
        {

        }
        /// <summary>
        /// The driver should do initialization here, a print driver should start a print document,
        /// while a preview driver should initialize a bitmap
        /// </summary>
        public virtual void NewDocument(MetaFile meta)
        {
        }
        /// <summary>
        /// The driver should do cleanup here, a print driver should finish print document.
        /// </summary>
        public virtual void EndDocument(MetaFile meta)
        {
        }
        /// <summary>
        /// The driver should start a new page
        /// </summary>
        public virtual void NewPage(MetaFile meta, MetaPage page)
        {
        }
        /// <summary>
        /// The driver should end current page
        /// </summary>
        public virtual void EndPage(MetaFile meta)
        {
        }
        /// <summary>
        /// The driver should draw the page
        /// </summary>
        public abstract void DrawPage(MetaFile meta, MetaPage page);
        /// <summary>
        /// The driver must return the text extent in twips
        /// </summary>
        public abstract Point TextExtent(TextObjectStruct aobj, Point extent);
        /// <summary>
        /// The driver must return the image extent in twips
        /// </summary>
        public abstract Point GraphicExtent(MemoryStream astream, Point extent,
			int dpi);
        /// <summary>
        /// The driver must select a printer
        /// </summary>
        public virtual void SelectPrinter(PrinterSelectType PrinterSelect)
        {
        }
        /// <summary>
        /// Sets page orientation
        /// </summary>
        virtual public void SetOrientation(OrientationType PageOrientation)
        {
            FOrientation = PageOrientation;
        }
        virtual public void DrawChart(Series nseries, MetaFile ametafile, int posx, int posy, object achart)
        {

        }
        /// <summary>
        /// The driver must set a new page size
        /// </summary>
        public abstract Point SetPageSize(PageSizeDetail psize);
        /// <summary>
        /// The driver must return the default page size
        /// </summary>
        public abstract Point GetPageSize(out int indexqt);
        /// <summary>
        /// Print initialization, it calls Metafile.BeginPrint, and NewDocument for this driver
        /// </summary>
        virtual public bool PreparePrint(MetaFile meta)
		{
            bool prepareresult = false;
            bool shownodata = false;
            try
            {
                meta.BeginPrint(this);
                prepareresult = true;
            }
            catch (Exception E)
            {
                if (!EmptyReportThrowException)
                {
                     if (E is NoDataToPrintException)
                         shownodata = true;
                     else
                         throw;

                }
                     else
                         throw;
            }
            if (shownodata)
            {
                if (NoData != null)
                {
                    if (!NoData())
                    {
                        ProcessNoDataToPrint();
                    }
                }
                else
                    ProcessNoDataToPrint();
            }
			if (prepareresult)
                NewDocument(meta);
            return prepareresult;
		}
        /// <summary>
        /// This procedure is called when the user does not handle the
        /// no data to print event. Depending on the ouput driver the output
        /// will be a message to the user.
        /// </summary>
        virtual protected void ProcessNoDataToPrint()
        {

        }
        /// <summary>
        /// Print method, it the PreparePrint, the driver should override this to perform additional
        /// actions
        /// </summary>
        virtual public bool Print(MetaFile meta)
		{
            bool aresult = PreparePrint(meta);
            return aresult;
		}

	}
    /// <summary>
    /// This is the base class for all Report processing drivers working in Reportman.Drawing space,
    /// that is print to printer, preview on screen or exporting to bitmap, provides basic functionality
    /// to measure texts, bitmaps and shapes, but still not work with printers (System.Drawing.Printing)
    /// or printer dialogs (Windows Forms).
    /// So it's a step forward to the implementation of some Report processing drivers, useful for
    /// preview, becuase it can use bitmap as output
    /// <see cref="Variant">PrintOutPrint</see>
    /// </summary>
	public class PrintOutNet : PrintOut,IDisposable
	{
		private Graphics gbitmap;
        /// <summary>
        /// This property allows scaling, usefull for drawing a preview inside a sized bitmap
        /// </summary>
		public float Scale;
		private Font stock_font;
		private int stock_style;
#if REPMAN_COMPACT
#else
		StringFormat fl;
		private bool stock_WordWrap;
		private bool stock_CutText;
		private int stock_Alignment;
		private bool stock_RightToLeft;
        /// <summary>
        /// Optimization selection.
        /// <see cref="Variant">WMFOptimization</see>
        /// </summary>
		public WMFOptimization OptimizeWMF;
#endif
        private PrintOutPDF npdfdriver;

		private Color stock_brushcolor;
        private Color stock_backbrushcolor;
        private float stock_fontsize;
		private SolidBrush stock_brush;
        private SolidBrush stock_backbrush;
        private string stock_fontname;
        /// <summary>
        /// This variables can be used as an offset, this is used by print preview to represent the non-printable
        /// area of the page
        /// </summary>
		protected int HardMarginX,HardMarginY;
        /// <summary>
        /// Because HardMargins are non-printable area, when printing to the device with this limitation, the offsets
        /// must be disabled
        /// </summary>
        protected bool UseHardMargin;
		private System.Drawing.Bitmap bitmap;
        /// <summary>
        /// The output in this driver is a bitmap, the output can be used to preview or convert it to serve in a
        /// web application
        /// </summary>
		public Bitmap Output;
        /// <summary>
        /// Free memory consumed by graphics resources
        /// </summary>
		public override void Dispose()
		{
#if REPMAN_COMPACT
#else
			if (stock_brush!=null)
			  stock_brush.Dispose();
          if (stock_backbrush != null)
              stock_backbrush.Dispose();
          if (stock_font != null)
			  stock_font.Dispose();
			if (fl!=null)
				fl.Dispose();
#endif
			if (bitmap!=null)
			  bitmap.Dispose();
          if (npdfdriver != null)
              npdfdriver.Dispose();
          base.Dispose();
		}
        /// <summary>
        /// Constructor and initialization of graphics objects required for the driver
        /// </summary>
		public PrintOutNet():base ()
		{
#if REPMAN_COMPACT
			bitmap=new System.Drawing.Bitmap(100,100);
#else
			OptimizeWMF = WMFOptimization.None;
			bitmap = new System.Drawing.Bitmap(100, 100, PixelFormat.Format32bppArgb);
			bitmap.SetResolution(1440, 1440);
#endif
            gbitmap = Graphics.FromImage(bitmap);
			Scale = 1.0F;
		}
        /// <summary>
        /// Calculate graphic extent using System.Drawing, the stream must be a valid bitmap
        /// </summary>
        /// <param name="astream">Memory stream containing a valid bitmap</param>
        /// <param name="extent">Initial extent in twips of the drawing box)</param>
        /// <param name="dpi">Dots per inch, resolution of the bitmap</param>
        /// <returns>Final size of the bitmap in twips</returns>
		override public Point GraphicExtent(MemoryStream astream, Point extent,
			int dpi)
		{
			Point aresult = extent;
			astream.Seek(0, System.IO.SeekOrigin.Begin);
			System.Drawing.Bitmap abit = new Bitmap(astream);
			try
			{
				double bitwidth = abit.Width;
				double bitheight = abit.Height;
				aresult.X = (int)Math.Round(bitwidth * Twips.TWIPS_PER_INCH/ dpi );
				aresult.Y = (int)Math.Round(bitheight * Twips.TWIPS_PER_INCH/ dpi );
			}
			finally
			{
				abit.Dispose();
			}
			return aresult;

		}
        /// <summary>
        /// Calculate text size, font rotation is ignored
        /// </summary>
        /// <param name="aobj">Structure containing text properties like font, size and style</param>
        /// <param name="extent">Text box in twips where the text will be drawn</param>
        /// <returns>Returns the extent in twips of the text, it can be larger than the input extent</returns>
		override public Point TextExtent(TextObjectStruct aobj, Point extent)
		{
            // Text extent for justify is implemented separately
            if ((aobj.Alignment & MetaFile.AlignmentFlags_AlignHJustify)>0)
            {
                if (npdfdriver==null)
                    npdfdriver=new PrintOutPDF();
                aobj.Type1Font = PDFFontType.Linked;
                extent=npdfdriver.TextExtent(aobj, extent);

                return extent;
            }
#if REPMAN_COMPACT
			float dpix=DEFAULT_RESOLUTION;
			float dpiy=DEFAULT_RESOLUTION;
#else
			float dpix = gbitmap.DpiX;
			float dpiy = gbitmap.DpiY;
#endif
			Point maxextent = new Point(extent.X, extent.Y);
            // This procedure allways returns the extension without font rotation
//			if (aobj.FontRotation != 0)
//				return extent;
			Font font = FontFromStruct(aobj);
			string atext = aobj.Text;
			// Implement text rotation here
			// by using Transform matrix for Graphics
			SizeF asize = new SizeF(extent.X, extent.Y);
			SizeF layout = new SizeF(asize.Width * dpix / 1440, asize.Height * dpiy / 1440);
#if REPMAN_COMPACT
			asize=gbitmap.MeasureString(atext,font);
#else
			int charsfit, linesfit;
			asize = MeasureString(gbitmap,atext, font, layout, MetStructToStringFormat(aobj), out charsfit, out linesfit);
#endif
			extent.X = (int)System.Math.Round(asize.Width * 1440 / dpix);
			extent.Y = (int)System.Math.Round(asize.Height * 1440 / dpiy);
			if (aobj.CutText)
			{
				if (extent.Y > maxextent.Y)
					extent.Y = maxextent.Y;
			}
			return extent;
		}
        /// <summary>
        /// Set page size
        /// </summary>
        /// <param name="psize">Customized page size description</param>
        /// <returns>Returns the size in twips of the page</returns>
		override public Point SetPageSize(PageSizeDetail psize)
		{
			int nwidth,nheight;
 			int index=psize.Index;
 			if (psize.Custom)
			{
				nwidth=psize.CustomWidth;
				nheight=psize.CustomHeight;
			}
			else
			{
  				nwidth=(int)Math.Round((double)PageSizeArray[index,0]/1000*Twips.TWIPS_PER_INCH);
  				nheight=(int)Math.Round((double)PageSizeArray[index,1]/1000*Twips.TWIPS_PER_INCH);
			}
 			if (FOrientation==OrientationType.Landscape)
			{
				int nwidth2=nwidth;
				nwidth=nheight;
				nheight=nwidth2;
			}
			return (new Point(nwidth, nheight));
		}
        /// <summary>
        /// Obtain current page size and also an index to the PageSizeArray
        /// </summary>
        /// <param name="indexqt">Output parameter, it will be filled with the index inside the PageSizeArray</param>
        /// <returns>Returns the size of the page in twips</returns>
		override public Point GetPageSize(out int indexqt)
		{
			indexqt = 0;
			Point apoint = new Point(11904, 16836);
			return apoint;
		}
        /// <summary>
        /// Create a Font object from a MetaObjectText structure
        /// </summary>
        /// <param name="page">MetaFilePage</param>
        /// <param name="obj">MetaObjectText, containing information to create the font</param>
        /// <returns>A Font object, created with parameter information</returns>
		public Font FontFromObject(MetaPage page, MetaObjectText obj)
		{
			const float MIN_FONT_SIZE = 2.3F;
			int intfontstyle = obj.FontStyle;
			float fontsize = obj.FontSize;
			fontsize = fontsize * Scale;
			if (fontsize < MIN_FONT_SIZE)
				fontsize = MIN_FONT_SIZE;
			string fontname = page.GetWFontNameText(obj);
			if (stock_font != null && fontname == stock_fontname && fontsize == stock_fontsize && intfontstyle == stock_style)
			{
				return stock_font;
			}
			else
			{
				FontStyle astyle = new FontStyle();
				if ((intfontstyle & 1) > 0)
					astyle = astyle | FontStyle.Bold;
				if ((intfontstyle & 2) > 0)
					astyle = astyle | FontStyle.Italic;
				if ((intfontstyle & 4) > 0)
					astyle = astyle | FontStyle.Underline;
				if ((intfontstyle & 8) > 0)
					astyle = astyle | FontStyle.Strikeout;
                float nfontsize = fontsize;
//                if (fontsize == 11)
//                    nfontsize = 12f;
				stock_font = new Font(page.GetWFontNameText(obj), nfontsize, astyle);
				stock_fontname = fontname;
				stock_fontsize = fontsize;
				stock_style = intfontstyle;
				
				return stock_font;
			}
		}
        /// <summary>
        /// Create a Font object from a TextObjectStruct structure
        /// </summary>
        /// <param name="objt">TextObjectStruct structure with text parameters like font name, size and style</param>
        /// <returns>Returns a Font object created from parameter information</returns>
		public Font FontFromStruct(TextObjectStruct objt)
		{
			FontStyle astyle = new FontStyle();
			int intfontstyle = objt.FontStyle;
			if ((intfontstyle & 1) > 0)
				astyle = astyle | FontStyle.Bold;
			if ((intfontstyle & 2) > 0)
				astyle = astyle | FontStyle.Italic;
			if ((intfontstyle & 4) > 0)
				astyle = astyle | FontStyle.Underline;
			if ((intfontstyle & 8) > 0)
				astyle = astyle | FontStyle.Strikeout;
			Font newfont = new Font(objt.WFontName, objt.FontSize, astyle);
			return newfont;
		}
#if REPMAN_COMPACT
		public const int DEFAULT_RESOLUTION=96;
#else
    /// <summary>
    /// Generate Stringformat from align properties
    /// </summary>
    /// <param name="align"></param>
    /// <returns></returns>
    public static StringFormat IntAlignToStringFormat(int Alignment,bool CutText,bool WordWrap,bool RightToLeft)
    {
      StringFormat fl = new StringFormat();
      fl.HotkeyPrefix = HotkeyPrefix.None;

		  fl.FormatFlags = (StringFormatFlags)0;
		  fl.Alignment = StringAlignment.Near;
      fl.LineAlignment = StringAlignment.Near;
			if (!CutText)
		  {
					fl.FormatFlags = fl.FormatFlags | StringFormatFlags.NoClip;
					fl.Trimming = StringTrimming.None;
			}
			if (!WordWrap)
				fl.FormatFlags = fl.FormatFlags | StringFormatFlags.NoWrap;
			if (RightToLeft)
				fl.FormatFlags = fl.FormatFlags | StringFormatFlags.DirectionRightToLeft;
			if ((Alignment & MetaFile.AlignmentFlags_AlignRight) > 0)
				fl.Alignment = StringAlignment.Far;
			if ((Alignment & MetaFile.AlignmentFlags_AlignHCenter) > 0)
				fl.Alignment = StringAlignment.Center;
			if ((Alignment & MetaFile.AlignmentFlags_AlignBottom) > 0)
				fl.LineAlignment = StringAlignment.Far;
			if ((Alignment & MetaFile.AlignmentFlags_AlignVCenter) > 0)
				fl.LineAlignment = StringAlignment.Center;
			return fl;
		}
        /// <summary>
        /// Generates a StringFormat from formatting information at MetaObjectText structure
        /// </summary>
        /// <param name="obj">MetaObjectText structure containint CutText,WordWrap and Alignment</param>
        /// <returns>Returns a StringFormat usable in any System.Drawing function</returns>
        public StringFormat MetaObjectToStringFormat(MetaObjectText obj)
		{
			if (fl == null || stock_WordWrap != obj.WordWrap || stock_RightToLeft != obj.RightToLeft
				|| stock_Alignment != obj.Alignment || obj.CutText != stock_CutText)
			{
				if (fl==null)
					fl = new StringFormat();
                fl.HotkeyPrefix = HotkeyPrefix.None;

				fl.FormatFlags = (StringFormatFlags)0;
				fl.Alignment = StringAlignment.Near;
                fl.LineAlignment = StringAlignment.Near;


				if (!obj.CutText)
				{
					fl.FormatFlags = fl.FormatFlags | StringFormatFlags.NoClip;
					fl.Trimming = StringTrimming.None;
				}
				if (!obj.WordWrap)
					fl.FormatFlags = fl.FormatFlags | StringFormatFlags.NoWrap;
				if (obj.RightToLeft)
					fl.FormatFlags = fl.FormatFlags | StringFormatFlags.DirectionRightToLeft;
                if ((obj.Alignment & MetaFile.AlignmentFlags_AlignRight) > 0)
                    fl.Alignment = StringAlignment.Far;
				if ((obj.Alignment & MetaFile.AlignmentFlags_AlignHCenter) > 0)
					fl.Alignment = StringAlignment.Center;
				if ((obj.Alignment & MetaFile.AlignmentFlags_AlignBottom) > 0)
					fl.LineAlignment = StringAlignment.Far;
				if ((obj.Alignment & MetaFile.AlignmentFlags_AlignVCenter) > 0)
					fl.LineAlignment = StringAlignment.Center;
				stock_CutText = obj.CutText;
				stock_Alignment = obj.Alignment;
				stock_RightToLeft = obj.RightToLeft;
				stock_WordWrap = obj.WordWrap;
			}
			return fl;
		}

        /// <summary>
        /// Generates a StringFormat from formatting information at TextObjectStruct structure
        /// </summary>
        /// <param name="obj">TextObjectStruct structure containint CutText,WordWrap and Alignment</param>
        /// <returns>Returns a StringFormat usable in any System.Drawing function</returns>
		public static StringFormat MetStructToStringFormat(TextObjectStruct obj)
		{
			StringFormat fl = new StringFormat();
            fl.HotkeyPrefix = HotkeyPrefix.None;
            fl.FormatFlags = (StringFormatFlags)0;

			if (!obj.CutText)
			{
				fl.FormatFlags = fl.FormatFlags | StringFormatFlags.NoClip;
				fl.Trimming = StringTrimming.None;
			}
			if (!obj.WordWrap)
				fl.FormatFlags = fl.FormatFlags | StringFormatFlags.NoWrap;
			if (obj.RightToLeft)
				fl.FormatFlags = fl.FormatFlags | StringFormatFlags.DirectionRightToLeft;
			if ((obj.Alignment & MetaFile.AlignmentFlags_AlignRight) > 0)
				fl.Alignment = StringAlignment.Far;
			if ((obj.Alignment & MetaFile.AlignmentFlags_AlignHCenter) > 0)
				fl.Alignment = StringAlignment.Center;
			if ((obj.Alignment & MetaFile.AlignmentFlags_AlignBottom) > 0)
				fl.LineAlignment = StringAlignment.Far;
			if ((obj.Alignment & MetaFile.AlignmentFlags_AlignVCenter) > 0)
				fl.LineAlignment = StringAlignment.Center;
			return fl;
		}
#endif
		private Rectangle SquareRect(Rectangle arec)
		{
			int Left = arec.Left;
			int Top = arec.Top;
			int Width = arec.Width;
			int Height = arec.Height;
			if (Width > Height)
			{
				Left = Left + (Width - Height) / 2;
				Width = Height;
			}
			else
			{
				Top = Top + (Height - Width) / 2;
				Height = Width;
			}

			return new Rectangle(Left, Top, Width, Height);
		}
        public virtual void DrawString(Graphics gr, string atext, Font font, Brush brush, Rectangle arec, StringFormat sformat)
        {
            //graph.TextRenderingHint = TextRenderingHint.SingleBitPerPixel;
            //gr.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
            gr.DrawString(atext, font, brush, arec, sformat);

        }
        public virtual void DrawString(Graphics gr, string atext, Font font, Brush brush, float posx,float posy, StringFormat sformat)
        {
            //graph.TextRenderingHint = TextRenderingHint.SingleBitPerPixel;
            gr.DrawString(atext, font, brush, posx,posy, sformat);
        }
        public virtual SizeF MeasureString(Graphics gr, string atext, Font font, SizeF layoutarea, StringFormat sformat, out int charsfit, out int linesfit)
        {
#if REPMAN_COMPACT
            charsfit = atext.Length;
            linesfit = 1;
            return gr.MeasureString(atext, font);
#else
            return gr.MeasureString(atext, font, layoutarea, sformat, out charsfit, out linesfit);
#endif
        }

        /// <summary>
        /// Draws an object, into a Graphics surface
        /// </summary>
        /// <param name="graph">The grapchis surface</param>
        /// <param name="page">The MetaFilePage that contains the object</param>
        /// <param name="obj">The object to be drawn</param>
		public void DrawObject(Graphics graph, MetaPage page, MetaObject obj)
		{
#if REPMAN_COMPACT
			float dpix=DEFAULT_RESOLUTION;
			float dpiy=DEFAULT_RESOLUTION;
#else
			float dpix = graph.DpiX;
			float dpiy = graph.DpiY;
			graph.InterpolationMode = InterpolationMode.HighQualityBicubic;
#endif
			int atop,aleft;
			if (UseHardMargin)
			{
				atop = obj.Top - HardMarginY;
				aleft = obj.Left - HardMarginX;
			}
			else
			{
				atop = obj.Top;
				aleft = obj.Left;
			}
			Rectangle arec = new Rectangle((int)Math.Round((float)aleft * dpix / 1440 * Scale),
				 (int)Math.Round((float)atop * dpiy / 1440 * Scale),
				 (int)Math.Round((float)obj.Width * dpix / 1440 * Scale),
				 (int)Math.Round((float)obj.Height * dpiy / 1440 * Scale));
			switch (obj.MetaType)
			{
				case MetaObjectType.Text:
					MetaObjectText objt = (MetaObjectText)obj;
                    bool selected = false;
					Font font = FontFromObject(page, objt);
                    Color BackColor = GraphicUtils.ColorFromInteger(objt.BackColor);
                    Color FontColor = GraphicUtils.ColorFromInteger(objt.FontColor);
                    // Change colors if drawselecte
                    if (DrawFound)
                    {
                        if (page.MetaFile.ObjectFound(obj))
                        {
                            BackColor = SystemColors.Highlight;
                            FontColor = SystemColors.HighlightText;
                            selected = true;
                        }
                    }
                    bool drawbackground = (!objt.Transparent) || selected;
                    if (stock_brush == null || stock_brushcolor != FontColor)
					{
                        if (stock_brush != null)
                            stock_brush.Dispose();
						stock_brush = new SolidBrush(FontColor);
						stock_brushcolor = FontColor;
					}
					string atext = page.GetText(objt);
                    // Implement text rotation here
					// by using Transform matrix for Graphics
#if REPMAN_COMPACT
					graph.DrawString(atext,font,stock_brush,arec);
#else
					//					graph.TextRenderingHint=TextRenderingHint.ClearTypeGridFit;
					if (objt.FontRotation != 0)
					{
						graph.TranslateTransform(arec.Left, arec.Top);
						graph.RotateTransform(-objt.FontRotation / 10);
						arec = new Rectangle(0, 0, arec.Width, arec.Height);
                        aleft = 0;
                        atop = 0;
					}
                    if (drawbackground)
                    {
                        if (stock_backbrush == null || stock_backbrushcolor != BackColor)
                        {
                            if (stock_backbrush != null)
                                stock_backbrush.Dispose();
                            stock_backbrush = new SolidBrush(BackColor);
                            stock_backbrushcolor = BackColor;
                        }
                        Point oldextent = new Point(objt.Width, objt.Height);
                        Point extent;
                        if ((objt.Alignment & MetaFile.AlignmentFlags_AlignHJustify) > 0)
                        {
                            if (npdfdriver == null)
                                npdfdriver = new PrintOutPDF();
                            objt.Type1Font = PDFFontType.Linked;
                            extent = npdfdriver.TextExtent(TextObjectStruct.FromMetaObjectText(page, objt), oldextent);
                        }
                        else
                            extent = TextExtent(TextObjectStruct.FromMetaObjectText(page, objt), oldextent);
                        int bleft,btop,bwidth,bheight;
                        if ((objt.Alignment & MetaFile.AlignmentFlags_AlignHCenter) > 0)
                            bleft = aleft + obj.Width/2 - extent.X/2;
                        else
                            if ((objt.Alignment & MetaFile.AlignmentFlags_AlignRight) > 0)
                                bleft = aleft+obj.Width-extent.X;
                            else
                                bleft = aleft;
                        if ((objt.Alignment & MetaFile.AlignmentFlags_AlignVCenter) > 0)
                            btop = atop + obj.Height / 2 - extent.Y / 2;
                        else
                            if ((objt.Alignment & MetaFile.AlignmentFlags_AlignBottom) > 0)
                                btop = atop + obj.Height - extent.Y;
                            else
                                btop = atop;
                        
                        bwidth = extent.X;
                        bheight = extent.Y;
                        if ((objt.Alignment & MetaFile.AlignmentFlags_AlignHJustify) > 0)
                        {
                            bwidth = obj.Width;
                        }

                        Rectangle nrec = new Rectangle((int)Math.Round((float)bleft * dpix / 1440 * Scale),
                            (int)Math.Round((float)btop * dpiy / 1440 * Scale),
                            (int)Math.Round((float)bwidth * dpix / 1440 * Scale),
                            (int)Math.Round((float)bheight * dpiy / 1440 * Scale));
                        graph.FillRectangle(stock_backbrush, nrec);
                    }
                    // Text justify is implemented separaterly
                    if ((objt.Alignment & MetaFile.AlignmentFlags_AlignHJustify) > 0)
                    {
                        TextRectJustify(graph, new Rectangle(aleft, atop, obj.Width, obj.Height), TextObjectStruct.FromMetaObjectText(page, objt), font, stock_brush);
                    }
                    else
                    {
                         DrawString(graph,atext, font, stock_brush, arec, MetaObjectToStringFormat(objt));
                    }
					if (objt.FontRotation != 0)
					{
						graph.ResetTransform();
					}
#endif
					break;
				case MetaObjectType.Draw:
					MetaObjectDraw objd = (MetaObjectDraw)obj;
					bool drawoutside = true;
					bool drawinside = true;
#if REPMAN_COMPACT
					Pen apen=new Pen(GraphicUtils.ColorFromInteger(objd.PenColor));
                    Brush abrush = new SolidBrush(GraphicUtils.ColorFromInteger(objd.BrushColor));
#else
                    Pen apen = new Pen(GraphicUtils.ColorFromInteger(objd.PenColor), (float)objd.PenWidth / 1440 * dpix * Scale);
					switch (objd.PenStyle)
					{
						case 1:
							apen.DashStyle = DashStyle.Dash;
							break;
						case 2:
							apen.DashStyle = DashStyle.Dot;
							break;
						case 3:
							apen.DashStyle = DashStyle.DashDot;
							break;
						case 4:
							apen.DashStyle = DashStyle.DashDotDot;
							break;
						case 5:
							drawoutside = false;
							break;
					}
					HatchStyle hstyle = HatchStyle.SolidDiamond;
					switch ((BrushType)objd.BrushStyle)
					{
						case BrushType.Clear:
							drawinside = false;
							break;
						case BrushType.Horizontal:
							hstyle = HatchStyle.Horizontal;
							break;
						case BrushType.Vertical:
							hstyle = HatchStyle.Vertical;
							break;
						case BrushType.ADiagonal:
							hstyle = HatchStyle.LightUpwardDiagonal;
							break;
						case BrushType.BDiagonal:
							hstyle = HatchStyle.LightDownwardDiagonal;
							break;
						case BrushType.ACross:
							hstyle = HatchStyle.Cross;
							break;
						case BrushType.BCross:
							hstyle = HatchStyle.DiagonalCross;
							break;
						case BrushType.Dense1:
							hstyle = HatchStyle.Percent10;
							break;
						case BrushType.Dense2:
							hstyle = HatchStyle.Percent20;
							break;
						case BrushType.Dense3:
							hstyle = HatchStyle.Percent20;
							break;
						case BrushType.Dense4:
							hstyle = HatchStyle.Percent40;
							break;
						case BrushType.Dense5:
							hstyle = HatchStyle.Percent50;
							break;
						case BrushType.Dense6:
							hstyle = HatchStyle.Percent60;
							break;
						case BrushType.Dense7:
							hstyle = HatchStyle.Percent70;
							break;
					}
					Brush abrush;
					if (hstyle == HatchStyle.SolidDiamond)
                        abrush = new SolidBrush(GraphicUtils.ColorFromInteger(objd.BrushColor));
					else
                        abrush = new HatchBrush(hstyle, GraphicUtils.ColorFromInteger(objd.BrushColor), Color.Empty);
#endif

					ShapeType shape = (ShapeType)objd.DrawStyle;
					if ((shape == ShapeType.Square) || (shape == ShapeType.RoundSquare)
						|| (shape == ShapeType.Circle))
						arec = SquareRect(arec);
					switch (shape)
					{
						case ShapeType.Rectangle:
						case ShapeType.Square:
							if (drawinside)
								graph.FillRectangle(abrush, arec);
							if (drawoutside)
								graph.DrawRectangle(apen, arec.Left, arec.Top, arec.Width, arec.Height);
							break;
						case ShapeType.RoundRect:
						case ShapeType.RoundSquare:
#if REPMAN_COMPACT
                            if (drawinside)
                                graph.FillRectangle(abrush, arec);
                            if (drawoutside)
                                graph.DrawRectangle(apen, arec.Left, arec.Top, arec.Width, arec.Height);
#else
							// Rounded rectangles implemented using a GraphicsPath instead of a rectangle (Alessio Pollero)
							const int CornerRadius = 45;
							int strokeOffset = Convert.ToInt32(Math.Ceiling(apen.Width));
    						arec = Rectangle.Inflate(arec, -strokeOffset, -strokeOffset);
						    apen.EndCap = apen.StartCap = LineCap.Round;
						    GraphicsPath gfxPath = new GraphicsPath();
						    gfxPath.AddArc(arec.X, arec.Y, CornerRadius, CornerRadius, 180, 90);
						    gfxPath.AddArc(arec.X + arec.Width - CornerRadius, arec.Y, CornerRadius, CornerRadius, 270, 90);
						    gfxPath.AddArc(arec.X + arec.Width - CornerRadius, arec.Y + arec.Height - CornerRadius, CornerRadius, CornerRadius, 0, 90);
						    gfxPath.AddArc(arec.X, arec.Y + arec.Height - CornerRadius, CornerRadius, CornerRadius, 90, 90);
						    gfxPath.CloseAllFigures();
							
							
							if (drawinside)
								graph.FillPath(abrush, gfxPath);
							if (drawoutside)
								graph.DrawPath(apen, gfxPath);
#endif
                            break;
						case ShapeType.Ellipse:
						case ShapeType.Circle:
							if (drawinside)
								graph.FillEllipse(abrush, arec);
							if (drawoutside)
								graph.DrawEllipse(apen, arec.Left, arec.Top, arec.Width, arec.Height);
							break;
						case ShapeType.HorzLine:
							if (drawoutside)
								graph.DrawLine(apen, arec.Left, arec.Top, arec.Left + arec.Width, arec.Top);
							break;
						case ShapeType.VertLine:
							if (drawoutside)
								graph.DrawLine(apen, arec.Left, arec.Top, arec.Left, arec.Top + arec.Height);
							break;
						case ShapeType.Oblique1:
							if (drawoutside)
								graph.DrawLine(apen, arec.Left, arec.Top, arec.Left + arec.Width, arec.Top + arec.Height);
							break;
						case ShapeType.Oblique2:
							if (drawoutside)
								graph.DrawLine(apen, arec.Left, arec.Top + arec.Height, arec.Left + arec.Width, arec.Top);
							break;

					}
					break;
				case MetaObjectType.Image:
					MetaObjectImage obji = (MetaObjectImage)obj;
                    if ((obji.PreviewOnly) && (!Previewing))
                        return;
					MemoryStream astream = page.GetStream(obji);
					System.Drawing.Image abit = Image.FromStream(astream);
					//Make trasparent the image background if necessary
#if REPMAN_COMPACT
#else
                    // This crashes some bitmaps
					//abit.MakeTransparent();
#endif

					ImageDrawStyleType dstyle = (ImageDrawStyleType)obji.DrawImageStyle;
					float dpires = obji.DPIRes;
					float bitwidth = abit.Width;
					float bitheight = abit.Height;
					//RectangleF srcrec=new RectangleF(0,0,bitwidth-1,bitheight-1);
					Rectangle srcrec = new Rectangle(0, 0, (int)Math.Round(bitwidth), (int)Math.Round(bitheight));
					Rectangle destrec = arec;
					switch (dstyle)
					{
						case ImageDrawStyleType.Crop:
                            double propx = (double)destrec.Width/bitwidth;
                            double propy = (double)destrec.Height/bitheight;
                            int H = 0;
                            int W = 0;
                            if (propy > propx)
                            {
                                H = Convert.ToInt32(Math.Round(destrec.Height * propx / propy));
                                destrec = new Rectangle(destrec.Left, Convert.ToInt32(destrec.Top + (destrec.Height - H) / 2), destrec.Width, H);
                            }
                            else
                            {
                                W = Convert.ToInt32(destrec.Width * propy / propx);
                                destrec = new Rectangle(destrec.Left + (destrec.Width - W) / 2, destrec.Top, W, destrec.Height);
                            }
                            graph.DrawImage(abit, destrec, srcrec, GraphicsUnit.Pixel);

                            /*Rectangle olddest = destrec;
                            destrec = new Rectangle(arec.Left, arec.Top, (int)Math.Round((float)abit.Width / dpires * dpix * Scale), (int)Math.Round((float)abit.Height / dpires * dpiy * Scale));
                            if (srcrec.Width > 0)
                            {
                                float imaratio = (float)destrec.Width / (float)srcrec.Width;
                                // Center image
                                if (srcrec.Width * imaratio < olddest.Width)
                                    destrec = new Rectangle(System.Convert.ToInt32(destrec.Left + (olddest.Width - srcrec.Width * imaratio) / 2),
                                        destrec.Top, destrec.Width, destrec.Height);
                                else
                                {
                                    if (srcrec.Width * imaratio > olddest.Width)
                                    {
                                        srcrec = new Rectangle(0,0,System.Convert.ToInt32((srcrec.Width * imaratio-olddest.Width)/imaratio),
                                            srcrec.Height);
                                        imaratio = (float)destrec.Width / (float)srcrec.Width;
                                        srcrec = new Rectangle(0, 0, srcrec.Width,
                                            System.Convert.ToInt32(srcrec.Height / imaratio));
                                    }
                                }
                                if (srcrec.Height * imaratio < olddest.Height)
                                    destrec = new Rectangle(System.Convert.ToInt32(destrec.Left + (olddest.Height - srcrec.Height * imaratio) / 2),
                                        destrec.Top, destrec.Width, destrec.Height);
                                graph.DrawImage(abit, destrec, srcrec, GraphicsUnit.Pixel);
                            }*/
                            break;
					case ImageDrawStyleType.Full:
							destrec = new Rectangle(arec.Left, arec.Top, (int)Math.Round((float)abit.Width / dpires * dpix * Scale), (int)Math.Round((float)abit.Height / dpires * dpiy * Scale));
                            if (destrec.Width < arec.Width)
                                destrec = new Rectangle(destrec.Left + (arec.Width - destrec.Width) / 2,destrec.Top,
                                    destrec.Width,destrec.Height);
                            if (destrec.Height < arec.Height)
                                destrec = new Rectangle(destrec.Left, destrec.Top + (arec.Height - destrec.Height ) / 2,
                                    destrec.Width, destrec.Height);
                            graph.DrawImage(abit, destrec, srcrec, GraphicsUnit.Pixel);
							break;
                    case ImageDrawStyleType.Stretch:
							graph.DrawImage(abit, destrec, srcrec, GraphicsUnit.Pixel);
							break;
#if REPMAN_COMPACT
						case ImageDrawStyleType.Tile:
							break;
						case ImageDrawStyleType.Tiledpi:
							// Pending, scale image to adjust dpi brush
							break;
#else
						case ImageDrawStyleType.Tile:
							TextureBrush br2 = new TextureBrush(abit);
							graph.FillRectangle(br2, destrec);
							break;
						case ImageDrawStyleType.Tiledpi:
							// Pending, scale image to adjust dpi brush
							TextureBrush br = new TextureBrush(abit, srcrec);
							graph.FillRectangle(br, destrec);
							break;
#endif
					}
					break;
			}

		}
		private void IntDrawPage(MetaFile meta, MetaPage page,Graphics gr)
		{
			for (int i = 0; i < page.Objects.Count; i++)
			{
				DrawObject(gr, page, page.Objects[i]);
			}
		}
        /// <summary>
        /// Draws all the objects inside a page to the Bitmap Output
        /// </summary>
        /// <param name="meta">MetaFile containing the page</param>
        /// <param name="page">MetaFilePage to be drawn into Output</param>
		override public void DrawPage(MetaFile meta, MetaPage page)
		{
			if (Output == null)
				throw new Exception("Ouptut not specified in printoutnet");
			Graphics gr = Graphics.FromImage(Output);
#if REPMAN_COMPACT
			gr.FillRectangle(new SolidBrush(GraphicUtils.ColorFromInteger(meta.BackColor)), 0, 0, Output.Width, Output.Height);
			IntDrawPage(meta,page,gr);
#else
			if (OptimizeWMF != WMFOptimization.None)
			{
				if (page.WindowsMetafile == null)
				{
					EmfType wmftype = EmfType.EmfPlusOnly;
					if (OptimizeWMF == WMFOptimization.Gdi)
						wmftype = EmfType.EmfOnly;
				  else
					if (OptimizeWMF == WMFOptimization.Gdiplus)
						wmftype = EmfType.EmfPlusDual;
					
					int awidth = (int)Math.Round(bitmap.HorizontalResolution * meta.CustomX / 1440);
					int aheight = (int)Math.Round(bitmap.VerticalResolution*meta.CustomY/1440);
					float oldscale = Scale;
					try
					{
						Scale = bitmap.HorizontalResolution/Output.HorizontalResolution;
						Rectangle arec = new Rectangle(0,0,awidth,aheight);						
						System.Drawing.Imaging.Metafile wmf;
						IntPtr dc = gbitmap.GetHdc();
						try
						{
							wmf = new System.Drawing.Imaging.Metafile(dc, arec, MetafileFrameUnit.Pixel, wmftype);
						}
						finally
						{
							gbitmap.ReleaseHdc(dc);
						}
						Graphics gr2 = Graphics.FromImage(wmf);
						try
						{
                            gr2.FillRectangle(new SolidBrush(GraphicUtils.ColorFromInteger(meta.BackColor)), 0, 0, awidth, aheight);
							IntDrawPage(meta, page, gr2);
						}
						finally
						{
							gr2.Dispose();
						}
						page.WindowsMetafile = wmf;
						page.WindowsMetafileScale = Scale;
					}
					finally
					{
						Scale = oldscale;
					}
				}
				gr.DrawImage(page.WindowsMetafile, new Rectangle(0, 0, Output.Width, Output.Height));
			}
			else
			{
				page.WindowsMetafileScale = bitmap.HorizontalResolution / Output.HorizontalResolution;
                gr.FillRectangle(new SolidBrush(GraphicUtils.ColorFromInteger(meta.BackColor)), 0, 0, Output.Width, Output.Height);
				IntDrawPage(meta, page, gr);
			}
#endif
		}
        /// <summary>
        /// Draws the text full justified
        /// </summary>
        /// <param name="gr"></param>
        /// <param name="arect"></param>
        /// <param name="atext"></param>
        /// <param name="nfont"></param>
        /// <param name="sbrush"></param>
        protected void TextRectJustify(Graphics gr,Rectangle arect,TextObjectStruct atext,Font nfont,SolidBrush sbrush)
        {
            int i,index;
            int posx,posy,currpos,alinedif;
            bool singleline;
            string astring;
            int alinesize;
            Strings lwords;
            Integers lwidths;
            StringBuilder aword;
            int nposx,nposy;
            string Text=atext.Text;
            int Alignment = atext.Alignment;
            bool wordbreak = atext.WordWrap;
            bool RightToLeft = atext.RightToLeft;
            StringFormat sformat = new StringFormat(StringFormatFlags.NoWrap | StringFormatFlags.NoClip);
            StringFormat rformat = new StringFormat(StringFormatFlags.NoWrap | StringFormatFlags.NoClip);
            rformat.Alignment = StringAlignment.Far;
            atext.Type1Font = PDFFontType.Linked;
            sformat.HotkeyPrefix = HotkeyPrefix.None;
            rformat.HotkeyPrefix = HotkeyPrefix.None;

            singleline=((Alignment & MetaFile.AlignmentFlags_SingleLine)>0);
            if (singleline)
                wordbreak=false;
            float intdpix=gr.DpiX;
            float intdpiy=gr.DpiY;
            // Calculates text extent and apply alignment
            if (npdfdriver==null)
                npdfdriver=new PrintOutPDF();
            Point full_extent=new Point(arect.Width,arect.Height);
            full_extent=npdfdriver.TextExtentLineInfo(atext,full_extent);
            Rectangle recsize=new Rectangle(0,0,full_extent.X,full_extent.Y);
            // Align bottom or center
            posy=arect.Top;
            if ((Alignment & MetaFile.AlignmentFlags_AlignBottom)>0)
                posy=arect.Bottom-recsize.Height;
            if ((Alignment & MetaFile.AlignmentFlags_AlignVCenter)>0)
                posy=arect.Top+(((arect.Bottom-arect.Top)-recsize.Bottom) / 2);
            LineInfos linfos=npdfdriver.LineInfo;
            bool dojustify;
            for (i = 0; i < linfos.Count;i++)
            {
                LineInfo linfo = linfos[i];
                astring = Text.Substring(linfo.Position, linfo.Size);
                dojustify = (((Alignment & MetaFile.AlignmentFlags_AlignHJustify) > 0) &&
                     (!linfo.LastLine));
                lwords = new Strings();
                posx = arect.Left;
                if (dojustify)
                {
                    // Calculate the sizes of the words, then
                    // share space between words
                    aword = new StringBuilder();
                    index = 0;
                    while (index < astring.Length)
                    {
                        if (astring[index] != ' ')
                            aword.Append(astring[index]);
                        else
                        {
                            if (aword.Length > 0)
                                lwords.Add(aword.ToString());
                            aword = new StringBuilder();
                        }
                        index++;
                    }
                    if (aword.Length > 0)
                        lwords.Add(aword.ToString());
                    // Calculate all words size
                    alinesize = 0;
                    lwidths = new Integers();
                    foreach (string nword in lwords)
                    {
                        Point extent = new Point(arect.Width, arect.Height);
                        extent = npdfdriver.WordExtent(nword, extent);
                        if (RightToLeft)
                            lwidths.Add(-extent.X);
                        else
                            lwidths.Add(extent.X);
                        alinesize = alinesize + extent.X;
                    }
                    alinedif = arect.Width - alinesize;
                    if ((alinedif > 0) || ((alinedif==0) && (lwords.Count==1)))
                    {
                        if (lwords.Count > 1)
                            alinedif = alinedif / (lwords.Count-1);
                        if (RightToLeft)
                        {
                            currpos = arect.Right;
                            alinedif = -alinedif;
                        }
                        else
                            currpos = posx;
                        index = 0;
                        for (int lindex=0;lindex<lwords.Count;lindex++)
                        {
                            string nword=lwords[lindex];
                            nposy = posy + linfo.TopPos;
                            nposy = (int)Math.Round((double)(nposy * intdpiy) / 1440 * Scale);
                            // Last word aligned to the right
                            if ((lwords.Count>1) && (lindex==lwords.Count-1))
                            {
                                nposx =arect.Left;
                                nposx = (int)Math.Round(((double)(nposx) * intdpix) / 1440 * Scale);
                                DrawString(gr,nword, nfont, sbrush, new Rectangle(nposx, nposy,
                                    (int)Math.Round(((float)(arect.Width) * intdpix) / 1440 * Scale),
                                    (int)Math.Round(((float)linfo.Height * intdpix) / 1440 * Scale)), rformat);
                            }
                            else
                            {
                                nposx = currpos;
                                nposx = (int)Math.Round(((double)nposx * intdpix) / 1440 * Scale);
                                DrawString(gr,nword, nfont, sbrush, nposx, nposy, sformat);
                            }
                            currpos = currpos + lwidths[index] + alinedif;
                            index++;
                        }
                    }
                    else
                        dojustify = false;
                }
                // Not justified alignment
                if (!dojustify)
                {
                    posx = arect.Left;
                    // Aligns horz.
                    // recsize.right contains the width of the full text
                    sformat.Alignment = StringAlignment.Near;
                    if ((Alignment & MetaFile.AlignmentFlags_AlignRight) > 0)
                        sformat.Alignment = StringAlignment.Far;
                    // Aligns Center
                    if ((Alignment & MetaFile.AlignmentFlags_AlignHCenter) > 0)
                        sformat.Alignment = StringAlignment.Center;
                    nposx = posx;
                    nposy=posy+linfo.TopPos;
                    nposx=(int)Math.Round((double)(nposx*intdpix)/1440*Scale);
                    nposy=(int)Math.Round((double)(nposy*intdpiy)/1440*Scale);
                    DrawString(gr,astring, nfont, sbrush, new Rectangle(nposx, nposy, 
                        (int)Math.Round(((float)(arect.Width+1000)* intdpix) / 1440 * Scale),
                        (int)Math.Round(((float)linfo.Height* intdpix) / 1440 * Scale)), sformat);
                }
            }
        }
	}
    public enum PrinterRawOperation  { CutPaper,OpenDrawer,LineFeed,CR,FF,TearOff,InitPrinter,Pulse,EndPrint,RedFont,BlackFont,
        Normal,Bold,Underline,Italic, StrikeOut, LineSpace6,LineSpace8,LineSpace7_72,LineSpacen_216,LineSpacen_180,Linespacen_60,
        cpi20,cpi17,cpi15,cpi12,cpi10,cpi6,cpi5};

    /// <summary>
    /// Printer configuration class to obtain default printer settigns
    /// </summary>
    public class PrinterConfig
    {
        public static bool PersistentConfiguration = true;
        private static IniFile config=null;
        public static object flag = 0;
        public static bool ForceSystemConfig = false;
        private static string filename;
        private static void CheckLoaded()
        {
            Monitor.Enter(flag);
            try
            {
                if ((config == null) || (!PersistentConfiguration))                    
                {
#if REPMAN_COMPACT
                    string filename = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
#else
                    filename = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                    filename = filename + Path.DirectorySeparatorChar + "reportman.ini";
                    if (!ForceSystemConfig)
                    {
                        FileInfo ninfo = new FileInfo(filename);
                        if (!ninfo.Exists)
                        {
                            filename = System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                            filename = filename + Path.DirectorySeparatorChar + "reportman.ini";
                        }
                    }
#endif
                    config = new IniFile(filename);
                }
            }
            finally
            {
                Monitor.Exit(flag);
            }

        }
        public static string ConfigFile()
        {
            CheckLoaded();
            return filename;
        }
        public static void ReloadParameters()
        {
            Monitor.Enter(flag);
            try
            {
                config = null;
            }
            finally
            {
                Monitor.Exit(flag);
            }
            CheckLoaded();
        }
        public static string GetDriverName(PrinterSelectType printselect)
        {
            string defvalue = "";
            switch (printselect)
            {
                case PrinterSelectType.Characterprinter:
                    defvalue = "EPSON";
                    break;
                case PrinterSelectType.PlainPrinter:
                    defvalue = "PLAIN";
                    break;
                case PrinterSelectType.PlainFullPrinter:
                    defvalue = "PLAINFULL";
                    break;
            }
            string valuename = "Printer"+((int)printselect).ToString();
            CheckLoaded();
            return config.ReadString("PrinterDriver", valuename, defvalue);
        }
        public static string GetPrinterName(PrinterSelectType printselect)
        {
            string defvalue = "";
            string valuename = "Printer" + ((int)printselect).ToString();
            CheckLoaded();
            return config.ReadString("PrinterNames", valuename, defvalue);
        }
        public static string DecodeEscapeString(string source)
        {
            string nresult = source;
            string newstring = "";
            int idx = 0;
            while (idx < nresult.Length)
            {
                char newchar = nresult[idx];
                if (newchar == '#')
                {
                    idx++;
                    string number = "";
                    while (idx < nresult.Length)
                    {
                        if (char.IsDigit(nresult[idx]))
                        {
                            number = number + nresult[idx];
                            idx++;
                        }
                        else
                            break;
                    }
                    if (number.Length > 0)
                    {
                        int idxchar = Convert.ToInt32(number);
                        char xchar = (char)idxchar;
                        newstring = newstring + xchar;
                    }
                }
                else
                {
                    newstring = newstring + nresult[idx];
                    idx++;
                }
            }
            return newstring;
        }
        public static string GetCutPaperOperation(PrinterSelectType printselect)
        {
            string defvalue = "";
            string valuename = "Printer" + ((int)printselect).ToString();
            CheckLoaded();
            string nresult = config.ReadString("CutPaper", valuename, defvalue);
            nresult = DecodeEscapeString(nresult);
            return nresult;
        }
        public static bool GetOpenDrawerOption(PrinterSelectType printselect)
        {
            string valuename = "Printer" + ((int)printselect).ToString();
            CheckLoaded();
            return config.ReadBool("OpenDrawerOn", valuename, false);
        }
        public static bool GetCutPaperOption(PrinterSelectType printselect)
        {
            string valuename = "Printer" + ((int)printselect).ToString();
            CheckLoaded();
            return config.ReadBool("CutPaperOn", valuename, false);
        }
        public static string GetOpenDrawerOperation(PrinterSelectType printselect)
        {
            string defvalue = "";
            string valuename = "Printer" + ((int)printselect).ToString();
            CheckLoaded();
            string nresult = config.ReadString("OpenDrawer", valuename, defvalue);
            nresult = DecodeEscapeString(nresult);
            return nresult;
        }
        public static bool GetOEMConvert(PrinterSelectType printselect)
        {
            string valuename = "Printer" + ((int)printselect).ToString();
            CheckLoaded();
            return config.ReadBool("PrinterEscapeOem", valuename, true);
        }

        public static Strings GetTextOnlyPrintDrivers()
        {
            Strings drivernames = new Strings();
            drivernames.Add(" ");
            drivernames.Add("PLAIN");
            drivernames.Add("EPSON");
            drivernames.Add("EPSON-MASTER");
            drivernames.Add("EPSON-ESCP");
            drivernames.Add("EPSON-ESCPQ");
            drivernames.Add("IBMPROPRINTER");
            drivernames.Add("EPSONTMU210");
            drivernames.Add("EPSONTMU210CUT");
            drivernames.Add("EPSONTM88IICUT");
            drivernames.Add("EPSONTM88II");
            drivernames.Add("HP-PCL");
            drivernames.Add("VT100");
            drivernames.Add("PLAINFULL");
            return drivernames;
        }
        public static Strings GetConfigurablePrinters()
        {
            Strings configs = new Strings();

            configs.Add(Translator.TranslateStr(467));
            configs.Add(Translator.TranslateStr(468));
            configs.Add(Translator.TranslateStr(469));
            configs.Add(Translator.TranslateStr(470));
            configs.Add(Translator.TranslateStr(471));
            configs.Add(Translator.TranslateStr(472));
            configs.Add(Translator.TranslateStr(473));
            configs.Add(Translator.TranslateStr(474));
            configs.Add(Translator.TranslateStr(475));
            configs.Add(Translator.TranslateStr(476));
            configs.Add(Translator.TranslateStr(477));
            configs.Add(Translator.TranslateStr(478));
            configs.Add(Translator.TranslateStr(479));
            configs.Add(Translator.TranslateStr(480));
            configs.Add(Translator.TranslateStr(481));
            configs.Add(Translator.TranslateStr(482));
            configs.Add(Translator.TranslateStr(1343));
            configs.Add(Translator.TranslateStr(1344));

            // More configurable printers
            for (int i = 1; i <= 50; i++)
                configs.Add("Printer" + i.ToString());

            return configs;
        }
    }
   	public enum ChartType
	{
		Line, Bar, Point, Horzbar, Area, Pie, Arrow,
		Bubble, Gantt
	};
	public enum ChartDriver { Default, Engine, Teechart };
	public enum BarType { None, Side, Stacked, Stacked100 };


	public class SeriesItem
	{
		public Doubles Values;
		public Integers Colors;
		public Strings ValueCaptions;
		public int Color;
		public string Caption;
		public double MaxValue;
		public double MinValue;
		public ChartType ChartStyle;
		public SeriesItem()
		{
			Color = -1;
			MaxValue = -10e300;
			MinValue = +10e300;
            Values = new Doubles();
            Colors = new Integers();
            ValueCaptions = new Strings();
            Caption = "";
            ChartStyle = ChartType.Bar;
            
		}
        public void SetLastValueColor(int newcolor)
        {
            if (Colors.Count == 0)
                return;
            Colors[Colors.Count-1] = newcolor;
        }
	}
	public class Series
	{
		public bool AutoRangeL;
		public bool AutoRangeH;
		public double LowValue;
		public double HighValue;
		public bool Logaritmic;
		public double LogBase;
		public bool Inverted;
        public float FontSize;
        public float Resolution = 100;
        public bool ShowLegend;
        public int HorzFontRotation;
        public int VertFontRotation;
        public bool ShowHint;
        public int PrintWidth;
        public int PrintHeight;
        public bool Effect3D;
        public int MarkStyle;
        public BarType MultiBar;
        public List<SeriesItem> SeriesItems = new List<SeriesItem>();
        public Series()
        {
            LowValue = 0.0;
            HighValue = 0.0;
            LogBase = 0.0;
            Inverted = false;
            AutoRangeH = true;
            AutoRangeL = true;
            Effect3D = false;
        }
        public void Clear()
        {
            SeriesItems.Clear();
        }
	}
}
