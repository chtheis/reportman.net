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
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
#if REPMAN_DOTNET1
#else
using System.Collections.Generic;
#endif

namespace Reportman.Drawing
{
    /// <summary>
    /// Class containig graphic processing utilities
    /// </summary>
	public class GraphicUtils
	{
        // Flag used by Monitor object to provide multithread capability
        private static object flag = 2;
        private static Bitmap gridbitmap = null;
        private static Bitmap smallbit = null;
#if PocketPC
#else
        private static System.Drawing.Imaging.Metafile gridmetafile = null;
#endif
        private static double gridscale;
        private static int gridx;
        private static int gridy;
        private static Color gridcolor;
        private static Color gridbackcolor;
        private static bool gridlines;
        public static int DefaultDPI = 96;
        private static float fdpiscale = 0.0f;
        private static float fdpiscalex = 0.0f;
        private static float fdpiscaley = 0.0f;
        public static float DPIScale
        {
            get
            {
                if (fdpiscale != 0.0f)
                    return fdpiscale;
                fdpiscale = (float)ScreenDPI() / (float)DefaultDPI;
                return fdpiscale;
            }

        }
        public static float DPIScaleX
        {
            get
            {
                if (fdpiscalex != 0.0f)
                    return fdpiscalex;
                fdpiscalex = (float)ScreenDPIX() / (float)DefaultDPI;
                return fdpiscalex;
            }

        }
        public static float DPIScaleY
        {
            get
            {
                if (fdpiscaley != 0.0f)
                    return fdpiscaley;
                fdpiscaley = (float)ScreenDPIY() / (float)DefaultDPI;
                return fdpiscaley;
            }

        }
        static int ndpiy = 0;
        public static int ScreenDPIY()
        {
            if (ndpiy == 0)
            {
                using (Bitmap nbit = new Bitmap(10, 10))
                {
                    using (Graphics gr = Graphics.FromImage(nbit))
                    {
                        ndpiy = System.Convert.ToInt32(gr.DpiY);
                    }
                }

            }
            return ndpiy;
        }
        public static int ScreenDPIX()
        {
            return ScreenDPI();
        }
        static int ndpi = 0;
        public static int ScreenDPI()
        {
            if (ndpi == 0)
            {
                using (Bitmap nbit = new Bitmap(10, 10))
                {
                    using (Graphics gr = Graphics.FromImage(nbit))
                    {
                        ndpi = System.Convert.ToInt32(gr.DpiX);
                    }
                }

            }
            return ndpi;
        }
        /// <summary>
        /// Obtain a Image (Metafile or Bitmap) to perform fast drawing of a grid, 
        /// so consecutive calls with similar parameters will execute faster.
        /// The implementation uses a shared bitmap or metafile.
        /// </summary>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <param name="GWidth"></param>
        /// <param name="GHeight"></param>
        /// <param name="GColor"></param>
        /// <param name="BackColor"></param>
        /// <param name="Lines"></param>
        /// <param name="Scale"></param>
        /// <returns></returns>
        public static Image GetImageGrid(int Width, int Height, int GWidth, int GHeight, Color GColor,
            Color BackColor, bool Lines, double Scale)
        {
            const int MAX_GRID_BITMAP = 2500;
#if PocketPC
            return GetBitmapGrid(Width, Height, GWidth, GHeight, GColor, BackColor, Lines, Scale);
#else
            if ((Width > MAX_GRID_BITMAP) || (Height > MAX_GRID_BITMAP))
            {
                return GetMetafileGrid(Width, Height, GWidth, GHeight, GColor, BackColor, Lines, Scale);
            }
            else
            {
                return GetBitmapGrid(Width, Height, GWidth, GHeight, GColor, BackColor, Lines, Scale);
            }
#endif
        }
        /// <summary>
        /// Obtain a bitmap to perform fast drawing of a grid, 
        /// so consecutive calls with similar parameters will execute faster.
        /// The implementation uses a shared bitmap.
        /// </summary>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <param name="GWidth"></param>
        /// <param name="GHeight"></param>
        /// <param name="GColor"></param>
        /// <param name="BackColor"></param>
        /// <param name="Lines"></param>
        /// <param name="Scale"></param>
        /// <returns></returns>
        public static Bitmap GetBitmapGrid(int Width, int Height, int GWidth, int GHeight, Color GColor,
            Color BackColor,bool Lines, double Scale)
        {
          if (Height == 0)
            Height = 1;
          if (Width == 0) 
            Width = 1;
            Monitor.Enter(flag);
            try
            {

                bool forceredraw = false;
                if (gridbitmap == null)
                    forceredraw = true;
                else
                {
                    if ((Height > gridbitmap.Height) || (Width > gridbitmap.Width))
                        forceredraw = true;
                    else
                        if ((GWidth != gridx) || (GHeight != gridy))
                            forceredraw = true;
                        else
                            if ((gridlines != Lines) || (gridscale != Scale) || 
                                (GColor != gridcolor) || (BackColor != gridbackcolor))
                                forceredraw = true;
                }
                if (!forceredraw)
                    return gridbitmap;
                if (gridbitmap != null)
                    gridbitmap.Dispose();
                gridbitmap = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);
                using (Graphics gr=Graphics.FromImage(gridbitmap))
                {
                    using (SolidBrush sbrush = new SolidBrush(BackColor))
                    {
                        gr.FillRectangle(sbrush, 0, 0, Width, Height);
                        DrawGrid(gr, GWidth, GHeight, Width, Height, GColor, Lines, 0, 0, Scale);
                    }
                }
                gridx = GWidth;
                gridy = GHeight;
                gridcolor = GColor;
                gridbackcolor = BackColor;
                gridlines = Lines;
                gridscale = Scale;                
            }
            finally
            {
                Monitor.Exit(flag);
            }
            return gridbitmap;
        }
#if PocketPC
#else
        public static System.Drawing.Imaging.Metafile CreateWindowsMetafile(int Width,int Height)
        {
            System.Drawing.Imaging.Metafile nmeta = null;
            Monitor.Enter(flag);
            try
            {
                if (smallbit == null)
                {
                    smallbit = new Bitmap(10, 10);
                }
                using (Graphics metagr = Graphics.FromImage(smallbit))
                {
                    nmeta = new System.Drawing.Imaging.Metafile(metagr.GetHdc(), new Rectangle(0, 0, Width, Height), MetafileFrameUnit.Pixel);
                }
            }
            finally
            {
                Monitor.Exit(flag);
            }
            return nmeta;
        }
        /// <summary>
        /// Obtain a metafile to perform fast drawing of a grid, 
        /// so consecutive calls with similar parameters will execute faster.
        /// The implementation uses a shared metafile.
        /// </summary>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <param name="GWidth"></param>
        /// <param name="GHeight"></param>
        /// <param name="GColor"></param>
        /// <param name="BackColor"></param>
        /// <param name="Lines"></param>
        /// <param name="Scale"></param>
        /// <returns></returns>
        public static System.Drawing.Imaging.Metafile GetMetafileGrid(int Width, int Height, int GWidth, int GHeight, Color GColor,
            Color BackColor, bool Lines, double Scale)
        {
            if (Height == 0)
                Height = 1;
            if (Width == 0)
                Width = 1;
            Monitor.Enter(flag);
            try
            {
                if (smallbit == null)
                {
                    smallbit = new Bitmap(10, 10);
                }

                bool forceredraw = false;
                if (gridmetafile == null)
                    forceredraw = true;
                else
                {
                    if ((Height > gridmetafile.Height) || (Width > gridmetafile.Width))
                        forceredraw = true;
                    else
                        if ((GWidth != gridx) || (GHeight != gridy))
                            forceredraw = true;
                        else
                            if ((gridlines != Lines) || (gridscale != Scale) ||
                                (GColor != gridcolor) || (BackColor != gridbackcolor))
                                forceredraw = true;
                }
                if (!forceredraw)
                    return gridmetafile;
                if (gridmetafile != null)
                    gridmetafile.Dispose();
                using (Graphics metagr = Graphics.FromImage(smallbit))
                {
                    gridmetafile = new System.Drawing.Imaging.Metafile(metagr.GetHdc(), new Rectangle(0, 0, Width, Height), MetafileFrameUnit.Pixel);
                }
                using (Graphics gr = Graphics.FromImage(gridmetafile))
                {
                    using (SolidBrush sbrush = new SolidBrush(BackColor))
                    {
                        gr.FillRectangle(sbrush, 0, 0, Width, Height);
                        DrawGrid(gr, GWidth, GHeight, Width, Height, GColor, Lines, 0, 0, Scale);
                    }
                }
                gridx = GWidth;
                gridy = GHeight;
                gridcolor = GColor;
                gridbackcolor = BackColor;
                gridlines = Lines;
                gridscale = Scale;
            }
            finally
            {
                Monitor.Exit(flag);
            }
            return gridmetafile;
        }
#endif
        private static int LogicalPointToDevicePoint(int origin, int destination, int avalue)
        {
            int aresult = (int)Math.Round(avalue*(destination/(double)origin));
            return aresult;
        }
        /// <summary>
        /// Draw a grid given a distance in twips
        /// </summary>
        /// <param name="gr">Destination</param>
        /// <param name="XWidth">Width in twips (1440 twips=1 inch)</param>
        /// <param name="XHeight">Height in twips</param>
        /// <param name="PixelsWidth">Width in pixels</param>
        /// <param name="PixelsHeight">Height in pixels</param>
        /// <param name="GridColor">Color of the points or lines drawn</param>
        /// <param name="Lines">Set to true to draw lines instead of points</param>
        /// <param name="XOffset">Horizontal offset </param>
        /// <param name="YOffset">Vertical offset</param>
        /// <param name="Scale">Scale the grid, set to 1.0 to draw in real size</param>
        public static void DrawGrid(Graphics gr, int XWidth, int XHeight,
            int PixelsWidth, int PixelsHeight, Color GridColor, bool Lines,
            int XOffset, int YOffset, double Scale)
        {
            double DpiX = gr.DpiX;
            double DpiY = gr.DpiY;
            if (XHeight <= 0)
                return;
            if (XWidth <= 0)
                return;
            Rectangle rect = new Rectangle(0, 0, PixelsWidth + XOffset, PixelsHeight + YOffset);
            double pixelsperinchx = DpiX * Scale;
            double pixelsperinchy = DpiY * Scale;
            int xof = (int)Math.Round(XOffset / pixelsperinchx * Twips.TWIPS_PER_INCH);
            int yof = (int)Math.Round(YOffset / pixelsperinchy * Twips.TWIPS_PER_INCH);
            int windowwidth = (int)Math.Round(Twips.TWIPS_PER_INCH * (rect.Width + XOffset) / pixelsperinchx);
            int windowheight = (int)Math.Round(Twips.TWIPS_PER_INCH * (rect.Height + XOffset) / pixelsperinchy);

            int originX = windowwidth;
            int originY = windowheight;
            int destinationX = rect.Width;
            int destinationY = rect.Height;
            int x, y;
            int avaluex, avaluey;
            int avalue2x, avalue2y;
            // Draw the grid
            if (Lines)
            {
                using (Pen gpen = new Pen(GridColor))
                {
                    x = xof + XWidth;
                    y = xof + XHeight;
                    while ((x < windowwidth) || (y < windowheight))
                    {
                        if (x < windowwidth)
                        {
                            avaluex = x;
                            avaluey = yof;
                            avaluex = LogicalPointToDevicePoint(originX, destinationX, avaluex);
                            avaluey = LogicalPointToDevicePoint(originY, destinationY, avaluey);
                            avalue2x = avaluex;
                            avalue2y = windowheight;
                            avalue2y = LogicalPointToDevicePoint(originY, destinationY, avalue2y);
                            gr.DrawLine(gpen, avaluex, avaluey, avalue2x, avalue2y);
                            x = x + XWidth;
                        }
                        if (y < windowheight)
                        {
                            avaluex = xof;
                            avaluey = y;
                            avaluex = LogicalPointToDevicePoint(originX, destinationX, avaluex);
                            avaluey = LogicalPointToDevicePoint(originY, destinationY, avaluey);
                            avalue2y = avaluey;
                            avalue2x = windowwidth;
                            avalue2x = LogicalPointToDevicePoint(originX, destinationX, avalue2x);
                            gr.DrawLine(gpen, avaluex, avaluey, avalue2x, avalue2y);
                            y = y + XHeight;
                        }
                    }
                }
            }
            else
            {
                using (Brush gbrush = new SolidBrush(GridColor))
                {
                    x = xof + XWidth;
                    while (x < windowwidth)
                    {
                        y = yof + XHeight;
                        while (y < windowheight)
                        {
                            avaluex = LogicalPointToDevicePoint(originX, destinationX, x);
                            avaluey = LogicalPointToDevicePoint(originY, destinationY, y);

                            gr.FillRectangle(gbrush, avaluex, avaluey, 1, 1);

                            y = y + XHeight;
                        }
                        x = x + XWidth;
                    }
                }
            }
        }
        /// <summary>
        /// Calculates the with in current graphics units of a text
        /// </summary>
        /// <param name="graphics">Graphics object where the measurement will be done</param>
        /// <param name="text">Text to be measured</param>
        /// <param name="font">Font used for the measurement</param>
        /// <returns>Width in graphic units of the measured text</returns>
        static public double MeasureDisplayStringWidth(Graphics graphics,
            string text, Font font)
        {
#if REPMAN_COMPACT
			return graphics.MeasureString(text,font).Width;
#else
            System.Drawing.StringFormat format = new System.Drawing.StringFormat();
            System.Drawing.RectangleF rect = new System.Drawing.RectangleF(0, 0,
                                                                  1000, 1000);
            System.Drawing.CharacterRange[] ranges =
                                       { new System.Drawing.CharacterRange(0,
                                                               text.Length) };
            System.Drawing.Region[] regions = new System.Drawing.Region[1];

            format.SetMeasurableCharacterRanges(ranges);
            regions = graphics.MeasureCharacterRanges(text, font, rect, format);
            if (regions.Length > 0)
            {
                rect = regions[0].GetBounds(graphics);
                return rect.Width;
            }
            else
                return 0;
#endif
        }
#if REPMAN_DOTNET1
		/// <summary>
		/// Create an integer value based on a Color        /// </summary>
		/// <param name="acolor">Color value</param>
		/// <returns>Returs an integer value  in the form of $00BBGGRR</returns>
		public static int IntegerFromColor(Color acolor)
		{
			int aresult;
			aresult = (int)acolor.R + (int)(acolor.G << 8) + ((int)acolor.B << 16);
			return aresult;
		}
#else
#if REPMAN_COMPACT
#else
        private static void UpdateColorNames()
        {
            Monitor.Enter(flag);
            try
            {
                if (ColorNames == null)
                {
                    ColorNames=new SortedList<string,KnownColor>();
                    KnownColor ncolor=KnownColor.ActiveBorder;
                    string[] names = Enum.GetNames(ncolor.GetType());
                    KnownColor[] values= (KnownColor[])Enum.GetValues(ncolor.GetType());
                    int i=0;
                    foreach (string s in names)
                    {
                        ColorNames.Add(s, values[i]);
                        i++;
                    }
                }
            }
            finally
            {
                Monitor.Exit(flag);
            }
        }
#endif
#if REPMAN_COMPACT
#else
		public static SortedList<string, KnownColor> ColorNames;
#endif
        /// <summary>
        /// Create an integer value based on a Color        /// </summary>
        /// <param name="acolor">Color value</param>
        /// <returns>Returs an integer value  in the form of $00BBGGRR</returns>
        public static int IntegerFromColor(Color acolor)
        {
            int aresult;
#if REPMAN_COMPACT
#else
            UpdateColorNames();
            if (acolor.IsKnownColor)
            {
                aresult=-ColorNames.IndexOfValue(acolor.ToKnownColor());
            }
            else
#endif
            aresult = (int)acolor.R + (int)(acolor.G << 8) + ((int)acolor.B << 16);
            return aresult;
        }
#endif
        public static int IntegerFromColorA(Color acolor)
        {
            int aresult;
            aresult =(int)acolor.R + (int)(acolor.G << 8) + ((int)acolor.B << 16);
            return aresult;
        }

        public static Color ColorFromString(string ncolor)
        {
            if (ncolor.Length==0)
                throw new Exception("Invalid color, ColorFromString, empty string");
#if REPMAN_COMPACT
#else
            if (ncolor[0] == '#')
                return ColorTranslator.FromHtml(ncolor);
#endif
            if (ncolor[0] == '(')
            {
                ncolor = ncolor.Substring(1, ncolor.Length - 1);
                int index = ncolor.IndexOf(')');
                if (index == (ncolor.Length - 1))
                    ncolor = ncolor.Substring(0, ncolor.Length - 1);
                char separator = ';';
                index = ncolor.IndexOf(",");
                if (index >= 0)
                    separator = ',';
                string[] colorarray = ncolor.Split(separator);
                byte r = 0;
                byte g = 0;
                byte b = 0;
                int i = 0;
                foreach (string acolor in colorarray)
                {
                    switch (i)
                    {
                        case 0:
                            r = System.Convert.ToByte(acolor);
                            break;
                        case 1:
                            g = System.Convert.ToByte(acolor);
                            break;
                        default:
                            b = System.Convert.ToByte(acolor);
                            break;
                    }
                    i++;
                }
                return Color.FromArgb(r, g, b);
            }
            else
#if REPMAN_COMPACT
                throw new Exception("Unknown color "+ncolor);
#else
                return Color.FromName(ncolor);            
#endif

        }
		/// <summary>
		/// Create a Color based on a 32 bit integer
		/// </summary>
		/// <param name="aint">Integer color value in the form of $00BBGGRR</param>
		/// <returns>Returs a Color usable in any System.Drawing function</returns>
        public static Color ColorFromInteger(int aint)
        {
#if REPMAN_DOTNET1
#else
#if REPMAN_COMPACT
#else
			UpdateColorNames();
#endif
#endif
            if ((aint >= 0) || (aint < -100000))
            {
                byte r = (byte)(aint);
                byte g = (byte)(aint >> 8);
                byte b = (byte)(aint >> 16);
                Color ncolor = Color.FromArgb(r, g, b);
                return ncolor;
            }
#if REPMAN_DOTNET1
			aint=-aint;
			return ColorFromInteger(aint);
#else
#if REPMAN_COMPACT
            aint = -aint;
            return ColorFromInteger(aint);
        }
#else
            else
			{
				string keycolor = ColorNames.Keys[-aint];
				return Color.FromKnownColor(ColorNames[keycolor]);
			}
        }
        /// <summary>
        /// Create a Color based on a 32 bit integer
        /// </summary>
        /// <param name="aint">Integer color value in the form of $00BBGGRR</param>
        /// <returns>Returs a Color usable in any System.Drawing function</returns>
        public static Color ColorFromIntegerA(int aint)
        {
            byte r = (byte)(aint);
            byte g = (byte)(aint >> 8);
            byte b = (byte)(aint >> 16);
            Color ncolor = Color.FromArgb(255,r, g, b);
            return ncolor;
        }
#endif
#endif
		private struct BitmapFileHeader
		{
			public uint bfSize;
			public uint bfOffBits;
		}
		private struct BitmapInfoHeader
		{
			public uint biSize;
			public int biWidth;
			public int biHeight;
			public ushort biPlanes;
			public ushort biBitCount;
			public uint biClrUsed;
		}
		private struct BitmapCoreHeader
		{
			public uint bcSize;
			public uint bcWidth;
			public uint bcHeight;
			public ushort bcPlanes;
			public ushort bcBitCount;
		}


		private const byte M_SOF0 = 0xC0;        // Start Of Frame N 
		private const byte M_SOF1 = 0xC1;        // N indicates which compression process 
		private const byte M_SOF2 = 0xC2;        // Only SOF0-SOF2 are now in common use 
		private const byte M_SOF3 = 0xC3;
		private const byte M_SOF5 = 0xC5;        // NB: codes C4 and CC are NOT SOF markers }
		private const byte M_SOF6 = 0xC6;
		private const byte M_SOF7 = 0xC7;
		private const byte M_SOF9 = 0xC9;
		private const byte M_SOF10 = 0xCA;
		private const byte M_SOF11 = 0xCB;
		private const byte M_SOF13 = 0xCD;
		private const byte M_SOF14 = 0xCE;
		private const byte M_SOF15 = 0xCF;
		private const byte M_SOI = 0xD8;        // (beginning of datastream) }
		private const byte M_EOI = 0xD9;       //{ (end of datastream) }
		private const byte M_SOS = 0xDA;        // (begins compressed data) }
		private const byte M_COM = 0xFE;        // Comment }

		private static byte NextMarker(Stream astream)
		{
			byte[] c1 = new byte[2];
			int readed;

			// Find 0xFF byte; count and skip any non-FFs. }
			readed = astream.Read(c1, 0, 1);
			if (readed < 1)
				throw new UnNamedException("Invalid JPEG");
			while (c1[0] != 0xFF)
			{
				readed = astream.Read(c1, 0, 1);
				if (readed < 1)
					throw new UnNamedException("Invalid JPEG");
			}
			// Get marker code byte, swallowing any duplicate FF bytes.  Extra FFs
			// are legal as pad bytes, so don't count them in discarded_bytes. }
			do
			{
				readed = astream.Read(c1, 0, 1);
				if (readed < 1)
					throw new UnNamedException("Invalid JPEG");
			} while (c1[0] == 0xFF);
			return c1[0];
		}
		// Skip over an unknown or uninteresting variable-length marker
		private static void skip_variable(Stream astream)
		{
			int alength, readed;
			byte[] c1 = new byte[2];

			//{ Get the marker parameter length count }
			readed = astream.Read(c1, 0, 2);
			if (readed < 2)
				throw new UnNamedException("Invalid JPEG");
			alength = ((int)c1[1]) + (((int)c1[0]) << 8);
			// Length includes itself, so must be at least 2 }
			if (alength < 2)
				throw new UnNamedException("Invalid JPEG");
			alength = alength - 2;
			// Skip over the remaining bytes }
			byte[] abuf = new byte[alength];
			readed = astream.Read(abuf, 0, alength);
			if (readed < alength)
				throw new UnNamedException("Invalid JPEG");
		}
		private static void process_SOFn(Stream astream, out int height, 
			out int width)
		{
			int alength, readed;
			byte[] c1 = new byte[2];
			//{ Get the marker parameter length count }
			readed = astream.Read(c1, 0, 2);
			if (readed < 2)
				throw new UnNamedException("Invalid JPEG");
			// data_precission skiped
			readed = astream.Read(c1, 0, 1);
			if (readed < 1)
				throw new UnNamedException("Invalid JPEG");
			// Height
			readed = astream.Read(c1, 0, 2);
			if (readed < 2)
				throw new UnNamedException("Invalid JPEG");
			alength = ((int)c1[1]) + (((int)c1[0]) << 8);
			height = alength;
			// Width
			readed = astream.Read(c1, 0, 2);
			if (readed < 2)
				throw new UnNamedException("Invalid JPEG");
			alength = ((int)c1[1]) + (((int)c1[0]) << 8);
			width = alength;
		}
        /// <summary>
        /// Obtain information about a jpeg stream
        /// </summary>
        /// <param name="astream">Input stream</param>
        /// <param name="width">Ouput parameter, width in pixels</param>
        /// <param name="height">Ouput parameter, height in pixels</param>
        /// <returns>Returns false if it's not a jpeg</returns>
		public static bool GetJPegInfo(Stream astream, out int width, 
			out int height)
		{
			width = 0;
			height = 0;
			byte[] c1 = new byte[2];
			byte[] c2 = new byte[2];
			int readed;
			byte marker;
			bool aresult = false;
			// Checks it's a jpeg image
			readed = astream.Read(c1, 0, 1);
			if (readed < 1)
			{
				astream.Seek(0, System.IO.SeekOrigin.Begin);
				return aresult;
			}
			if (c1[0] != 0xFF)
			{
				astream.Seek(0, System.IO.SeekOrigin.Begin);
				return aresult;
			}
			readed = astream.Read(c2, 0, 1);
			if (readed < 1)
			{
				astream.Seek(0, System.IO.SeekOrigin.Begin);
				return aresult;
			}
			if (c2[0] != M_SOI)
			{
				astream.Seek(0, System.IO.SeekOrigin.Begin);
				return aresult;
			}
			aresult = true;
			// Read segments until M_SOS
			do
			{
				marker = NextMarker(astream);
				switch (marker)
				{
					case M_SOF0:		// Baseline }
					case M_SOF1:		// Extended sequential, Huffman }
					case M_SOF2:		// Progressive, Huffman }
					case M_SOF3:		// Lossless, Huffman }
					case M_SOF5:		// Differential sequential, Huffman }
					case M_SOF6:		// Differential progressive, Huffman }
					case M_SOF7:		// Differential lossless, Huffman }
					case M_SOF9:		// Extended sequential, arithmetic }
					case M_SOF10:		// Progressive, arithmetic }
					case M_SOF11:		// Lossless, arithmetic }
					case M_SOF13:		// Differential sequential, arithmetic }
					case M_SOF14:		// Differential progressive, arithmetic }
					case M_SOF15:		// Differential lossless, arithmetic }
						process_SOFn(astream, out height, out width);
						// Exit, no more info need
						marker = M_SOS;
						break;
					default:
						skip_variable(astream);
						break;
				}
			}
			while ((marker != M_SOS) && (marker != M_EOI));
			astream.Seek(0, System.IO.SeekOrigin.Begin);
			return aresult;
		}
        /// <summary>
        /// Obtain information about a bitmap stream
        /// </summary>
        /// <param name="astream">Input stream</param>
        /// <param name="width">Output parameter, with of the bitmap in pixels</param>
        /// <param name="height">Output parameter, height of the bitmap in pixels</param>
        /// <param name="imagesize">Size in bytes of the image information part</param>
        /// <param name="MemBits">Bits containing information converted to Adobe PDF compatible form</param>
        /// <param name="indexed">Output parameter, returns true if the image is paletized</param>
        /// <param name="bitsperpixel">Output parameter, number of bits of information for each pixel</param>
        /// <param name="usedcolors">Output parameter, valid for indexed bitmaps, number of colors used from the palette</param>
        /// <param name="palette">Output parameter, palette in Adobe PDF compatible form, valid only in indexed bitmaps</param>
        /// <returns>Returns false if the stream is not abitmap</returns>
		public static bool GetBitmapInfo(Stream sourcestream, out int width, 
			out int height, out int imagesize, MemoryStream MemBits,
			out bool indexed, out int bitsperpixel, out int usedcolors,
			out string palette,out bool isgif)
		{
            bool aresult = false;
            MemoryStream newstream = null;
            try
            {
                const int MAX_BITMAPHEADERSIZE = 32000;
                Stream astream = sourcestream;

                isgif = false;
                byte[] buf = new byte[16];
                width = 0;
                height = 0;
                imagesize = 0;
                indexed = false;
                bool iscoreheader = false;
                bitsperpixel = 8;
                usedcolors = 0;
                palette = "";
                int readed, index;
                BitmapFileHeader fileheader = new BitmapFileHeader();
                BitmapInfoHeader infoheader = new BitmapInfoHeader();
                BitmapCoreHeader coreheader = new BitmapCoreHeader();
                // Read the file header
                readed = astream.Read(buf, 0, 14);
                if (readed != 14)
                {
                    astream.Seek(0, System.IO.SeekOrigin.Begin);
                    return aresult;
                }
                isgif = (((char)buf[0] == 'G') && ((char)buf[1] == 'I') && ((char)buf[2] == 'F'));
                if (isgif)
                {
#if PocketPC
                    return aresult;
#else
                    astream.Seek(0, System.IO.SeekOrigin.Begin);
                        using (Image nimage = Image.FromStream(astream))
                        {
                            astream.Seek(0, System.IO.SeekOrigin.Begin);
                            newstream = new MemoryStream();
                            nimage.Save(newstream, System.Drawing.Imaging.ImageFormat.Bmp);
                            newstream.Seek(0, System.IO.SeekOrigin.Begin);
                            astream = newstream;
                        }
                        readed = astream.Read(buf, 0, 14);
                        if (readed != 14)
                        {
                            astream.Seek(0, System.IO.SeekOrigin.Begin);
                            return aresult;
                        }
#endif
                }

                if (((char)buf[0] != 'B') || ((char)buf[1] != 'M'))
                {
                    return aresult;
                }
                fileheader.bfSize = StreamUtil.ByteArrayToUInt(buf, 2, 4);
                fileheader.bfOffBits = StreamUtil.ByteArrayToUInt(buf, 10, 4);
                // Read the size of bitmap info
                readed = astream.Read(buf, 0, 4);
                if (readed != 4)
                {
                    astream.Seek(0, System.IO.SeekOrigin.Begin);
                    return aresult;
                }
                uint bsize = StreamUtil.ByteArrayToUInt(buf, 0, 4);
                if ((bsize < 2) || (bsize > MAX_BITMAPHEADERSIZE))
                {
                    astream.Seek(0, System.IO.SeekOrigin.Begin);
                    return aresult;
                }
                if (bsize < 15)
                    iscoreheader = true;
                else
                    buf = new byte[bsize + 1];
                readed = astream.Read(buf, 0, (int)(bsize - 4));
                if (readed != (int)(bsize - 4))
                {
                    astream.Seek(0, System.IO.SeekOrigin.Begin);
                    return aresult;
                }
                if (iscoreheader)
                {
                    coreheader.bcSize = bsize;
                    coreheader.bcWidth = StreamUtil.ByteArrayToUInt(buf, 0, 4);
                    coreheader.bcHeight = StreamUtil.ByteArrayToUInt(buf, 4, 4);
                    coreheader.bcPlanes = StreamUtil.ByteArrayToUShort(buf, 8, 2);
                    coreheader.bcBitCount = StreamUtil.ByteArrayToUShort(buf, 10, 2);
                    bitsperpixel = coreheader.bcBitCount;
                    width = (int)coreheader.bcWidth;
                    height = (int)coreheader.bcHeight;
                    imagesize = width * height * 3;
                    if (MemBits == null)
                    {
                        aresult = true;
                        return aresult;
                    }
                }
                else
                {
                    infoheader.biSize = bsize;
                    infoheader.biWidth = StreamUtil.ByteArrayToInt(buf, 0, 4);
                    infoheader.biHeight = StreamUtil.ByteArrayToInt(buf, 4, 4);
                    infoheader.biPlanes = StreamUtil.ByteArrayToUShort(buf, 8, 2);
                    infoheader.biBitCount = StreamUtil.ByteArrayToUShort(buf, 10, 2);
                    infoheader.biClrUsed = (uint)StreamUtil.ByteArrayToInt(buf, 28, 4);
                    width = infoheader.biWidth;
                    height = infoheader.biHeight;
                    imagesize = width * height * 3;
                    bitsperpixel = infoheader.biBitCount;
                    if (bitsperpixel < 16)
                        usedcolors = (int)infoheader.biClrUsed;
                    if (MemBits == null)
                    {
                        aresult = true;
                        return aresult;
                    }
                }
                // Obtain the DIBBits
                int y, scanwidth;
                int toread;
                byte divider;
                int origwidth;
                int acolor, numcolors;
                byte[] bufcolors;
                // Read color entries
                switch (bitsperpixel)
                {
                    case 1:
                        numcolors = 2;
                        break;
                    case 4:
                        numcolors = 16;
                        break;
                    case 8:
                        numcolors = 256;
                        break;
                    case 16:
                    case 15:
                    case 24:
                    case 32:
                        numcolors = 0;
                        break;
                    default:
                        throw new UnNamedException("Bad bitcount in GetBitmapInfo");
                }
                if (numcolors > 0)
                {
                    if (iscoreheader)
                    {
                        usedcolors = numcolors;
                        bufcolors = new byte[usedcolors * 3];
                        readed = astream.Read(bufcolors, 0, usedcolors * 3);
                        if (readed != (usedcolors * 3))
                        {
                            throw new UnNamedException("Invalid bitmap palette");
                        }
                        palette = "";
                        for (y = 0; y < usedcolors; y++)
                        {
                            index = y * 3;
                            acolor = (((int)(bufcolors[index + 2])) << 16) + (((int)(bufcolors[index + 1])) << 8) +
                                (int)bufcolors[index];
                            if (palette == "")
                                palette = "<" + acolor.ToString("X").PadLeft(6, '0');
                            else
                                palette = palette + " " + acolor.ToString("X").PadLeft(6, '0');
                        }
                        if (usedcolors > 0)
                            palette = palette + ">";
                    }
                    else
                    {
                        if (usedcolors == 0)
                            usedcolors = numcolors;
                        bufcolors = new byte[usedcolors * 4];
                        readed = astream.Read(bufcolors, 0, usedcolors * 4);
                        if (readed != (usedcolors * 4))
                        {
                            throw new UnNamedException("Invalid bitmap palette");
                        }
                        palette = "";
                        for (y = 0; y < usedcolors; y++)
                        {
                            index = y * 4;
                            acolor = (((int)(bufcolors[index + 2])) << 16) + (((int)(bufcolors[index + 1])) << 8) +
                                (int)bufcolors[index];
                            if (palette == "")
                                palette = "<" + acolor.ToString("X").PadLeft(6, '0');
                            else
                                palette = palette + " " + acolor.ToString("X").PadLeft(6, '0');
                        }
                        if (usedcolors > 0)
                            palette = palette + ">";
                    }
                }
                // Go to position bits
                astream.Seek(fileheader.bfOffBits, System.IO.SeekOrigin.Begin);
                if (numcolors > 0)
                {
                    switch (numcolors)
                    {
                        case 2:
                            divider = 8;
                            break;
                        case 16:
                            divider = 2;
                            break;
                        case 256:
                            divider = 1;
                            break;
                        default:
                            divider = 1;
                            break;
                    }
                    scanwidth = (int)width / divider;
                    indexed = true;
                    if ((width % divider) > 0)
                        scanwidth = scanwidth + 1;
                    // bitmap file format is aligned on double word
                    // the alignment must be removed from datafile
                    origwidth = scanwidth;
                    while ((scanwidth % 4) > 0)
                        scanwidth = scanwidth + 1;
                    buf = new byte[scanwidth];
                    MemBits.SetLength(height * origwidth);
                    MemBits.Seek(0, System.IO.SeekOrigin.Begin);

                    for (y = height - 1; y >= 0; y--)
                    {
                        astream.Read(buf, 0, scanwidth);
                        MemBits.Seek(y * origwidth, System.IO.SeekOrigin.Begin);
                        MemBits.Write(buf, 0, origwidth);
                    }
                    MemBits.Seek(0, System.IO.SeekOrigin.Begin);
                    imagesize = (int)MemBits.Length;
                    aresult = true;
                }
                else
                {

                    MemBits.SetLength(imagesize);
                    int module;
                    if (bitsperpixel == 32)
                    {
                        scanwidth = width * 4;
                        toread = 0;
                        module = 4;
                    }
                    else
                        if ((bitsperpixel == 16) || (bitsperpixel == 15))
                        {
                            scanwidth = width * 2;
                            toread = 0;
                            // Align to 32 bit
                            toread = 4 - (scanwidth % 4);
                            if (toread == 4)
                                toread = 0;
                            module = 2;
                        }
                        else
                        {
                            scanwidth = width * 3;
                            // Align to 32 bit
                            toread = 4 - (scanwidth % 4);
                            if (toread == 4)
                                toread = 0;
                            module = 3;
                        }
                    MemBits.Seek(0, System.IO.SeekOrigin.Begin);
                    scanwidth = scanwidth + toread;
                    buf = new byte[scanwidth];
                    int linewidth = width * 3;
                    byte[] bufdest = new byte[linewidth];
                    for (y = height - 1; y >= 0; y--)
                    {
                        readed = astream.Read(buf, 0, scanwidth);
                        if (readed != scanwidth)
                            throw new UnNamedException("Bad bitmap stream");
                        MemBits.Seek((width * 3) * y, System.IO.SeekOrigin.Begin);
                        if (bitsperpixel > 16)
                        {
                            for (int h = 0; h < width; h++)
                            {
                                bufdest[h * 3] = buf[module * h + 2];
                                bufdest[h * 3 + 1] = buf[module * h + 1];
                                bufdest[h * 3 + 2] = buf[module * h];
                            }
                        }
                        else
                        {
                            if (bitsperpixel == 15)
                            {
                                // 5-5-5
                                for (int h = 0; h < width; h++)
                                {
                                    ushort num = (ushort)buf[module * h];
                                    ushort num2 = (ushort)((ushort)buf[module * h + 1] << 8);
                                    num = (ushort)(num | num2);
                                    byte rcolor = (byte)(num & 0x1F);
                                    byte gcolor = (byte)((num & 0x3FF) >> 5);
                                    byte bcolor = (byte)((num & 0x7FFF) >> 10);
                                    rcolor = (byte)Math.Round((double)rcolor / 31 * 255);
                                    gcolor = (byte)Math.Round((double)gcolor / 31 * 255);
                                    bcolor = (byte)Math.Round((double)bcolor / 31 * 255);
                                    bufdest[h * 3] = bcolor;
                                    bufdest[h * 3 + 1] = gcolor;
                                    bufdest[h * 3 + 2] = rcolor;
                                }
                            }
                            else
                            {
                                // 5-6-5
                                for (int h = 0; h < width; h++)
                                {
                                    ushort num = (ushort)buf[module * h];
                                    ushort num2 = (ushort)((ushort)buf[module * h + 1] << 8);
                                    num = (ushort)(num | num2);
                                    byte rcolor = (byte)(num & 0x1F);
                                    byte gcolor = (byte)((num & 0x7FF) >> 5);
                                    byte bcolor = (byte)(num >> 11);
                                    rcolor = (byte)Math.Round((double)rcolor / 31 * 255);
                                    gcolor = (byte)Math.Round((double)gcolor / 63 * 255);
                                    bcolor = (byte)Math.Round((double)bcolor / 31 * 255);
                                    bufdest[h * 3] = bcolor;
                                    bufdest[h * 3 + 1] = gcolor;
                                    bufdest[h * 3 + 2] = rcolor;
                                }
                            }
                        }
                        MemBits.Write(bufdest, 0, linewidth);
                    }
                    MemBits.Seek(0, System.IO.SeekOrigin.Begin);
                    aresult = true;
                }
                // Adobe PDF counts from 0 to usedcolors
                if (usedcolors > 0)
                    usedcolors--;
            }
            finally
            {
                if (newstream != null)
                    newstream.Dispose();
            }
			return aresult;
		}
#if REPMAN_COMPACT
#else
        /// <summary>
        /// Write a Windows Metafile into a stream
        /// </summary>
        /// <param name="metaf">Metafile to write</param>
        /// <param name="destination">Stream destination</param>
        /// <param name="Scale">Scale of the Windows Metafile</param>
		public static void WriteWindowsMetaFile(
			System.Drawing.Imaging.Metafile	metaf,Stream destination,float Scale)
		{
			Bitmap bm = new Bitmap(1, 1, PixelFormat.Format32bppArgb);
			bm.SetResolution(1440,1440);
			Graphics g = Graphics.FromImage(bm);
			int awidth = (int)Math.Round(metaf.Width * Scale);
			int aheight = (int)Math.Round(metaf.Height * Scale);
			Metafile nwm;
			IntPtr dc = g.GetHdc();
			try
			{
				nwm = new Metafile(destination, dc, 
					new Rectangle(0, 0, awidth, aheight),
					MetafileFrameUnit.Pixel, EmfType.EmfOnly);
			}
			finally
			{
				g.ReleaseHdc(dc);
			}
			try
			{
				g = Graphics.FromImage(nwm);
				try
				{
					g.PageUnit = System.Drawing.GraphicsUnit.Pixel;
					g.DrawImage(metaf, new Rectangle(0, 0, awidth, aheight));
				}
				finally
				{
					g.Dispose();
				}
			}
			finally
			{
				nwm.Dispose();
			}
		}
        /// <summary>
        /// Retruns a codec with the mime type or null if not found
        /// </summary>
        /// <param name="mimeType">Codec string, example "image/jpeg"</param>
        /// <returns></returns>
        public static ImageCodecInfo GetImageCodec(string mimeType)
        {
            ImageCodecInfo[] codecs
              = ImageCodecInfo.GetImageEncoders();


            foreach (ImageCodecInfo codec in codecs)
            {
                if (String.Compare(codec.MimeType,
                                   mimeType, true) == 0)
                {
                    return codec;
                }
            }


            return null;
        }

        /// <summary>
        /// Obtain a list of available image codecs
        /// </summary>
        /// <returns>A list, string collection of image codecs</returns>
		public static Strings GetImageCodecs()
		{
			ImageCodecInfo[] codecs=
				System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders();
			Strings alist = new Strings();
			string aext;
			alist.Add("WMF");
			foreach (ImageCodecInfo codec in codecs)
			{
				aext = codec.FormatDescription;
				alist.Add(aext);
			}
			return alist;
		}
    public static Size GetAvgFontSizeTwips(string FontFamily, float FontSize, FontStyle FStyle)
    {
      Size nresult = new Size(0, 0);
      Graphics gr;
      Bitmap nbitmap = new Bitmap(1, 1, PixelFormat.Format24bppRgb);
      using (nbitmap)
      {
        Font afont = new Font(FontFamily, FontSize,FStyle);
        gr = Graphics.FromImage(nbitmap);
        using (gr)
        {
          SizeF nsize;
          string text = "0000000000";
          gr.PageUnit = GraphicsUnit.Pixel;
          nsize = gr.MeasureString(text, afont, new PointF(0,0),
            StringFormat.GenericDefault);
          nresult.Width = System.Convert.ToInt32(nsize.Width / 10 * 1440 / nbitmap.HorizontalResolution);
          text = "Mg";
          nsize = gr.MeasureString(text, afont, new PointF(0,0),
            StringFormat.GenericDefault);
          nresult.Height = System.Convert.ToInt32(nsize.Height * 1440 / nbitmap.HorizontalResolution);
        }
        return nresult;
      }

    }
        /// <summary>
        /// Converts a text to his representation in bitmap form
        /// </summary>
        /// <param name="width">Witdh of the resulting bitmap</param>
        /// <param name="text">Text to be printed into the bitmap</param>
        /// <param name="fontfamily">Font family</param>
        /// <param name="fontsize">Font size</param>
        /// <returns>Returns a bitmap with the text drawn on it</returns>
		public static Bitmap TextToBitmap(int width,string text,string fontfamily,
			float fontsize)
		{
			SizeF asize;
			Font afont;
			Graphics gr;
			Bitmap nbitmap=new Bitmap(width,1,PixelFormat.Format24bppRgb);
			using (nbitmap)
			{
				afont=new Font(fontfamily,fontsize);
				gr = Graphics.FromImage(nbitmap);
				using (gr)
				{
					gr.PageUnit = GraphicsUnit.Pixel;
					asize=gr.MeasureString(text,afont,width,
						StringFormat.GenericDefault);				
				}
			}
			nbitmap = new Bitmap(width, (int)Math.Round(asize.Height),
				PixelFormat.Format24bppRgb);
			try
			{
				gr = Graphics.FromImage(nbitmap);
				using (gr)
				{
					gr.PageUnit = GraphicsUnit.Pixel;
					Brush abrush = new SolidBrush(Color.White);
					using (abrush)
						gr.FillRectangle(abrush,0,0,nbitmap.Width,nbitmap.Height);
					abrush = new SolidBrush(Color.Black);
					using (abrush)
						gr.DrawString(text, afont, abrush,0,0,StringFormat.GenericDefault);
				}
			}
			catch
			{
				nbitmap.Dispose();
				throw;
			}
			return nbitmap;
		}
        /// <summary>
        /// Convert a windows metafile to a image format saved into a stream
        /// </summary>
        /// <param name="metaf">Windows Metafile to be converted</param>
        /// <param name="destination">Destination stream</param>
        /// <param name="Scale">Scale of the metafile</param>
        /// <param name="codecstring">Codec to be used you can obtain a list by using GetImageCodecs</param>
        /// <param name="mimetype">Mimetype of the selected codec</param>
		public static void WriteWindowsMetaFileCodec(Metafile metaf,
			Stream destination,
			float Scale,string codecstring,out string mimetype)
		{
			mimetype = "";
			ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
			ImageCodecInfo codec = null;
			bool isbitmap = false;
			if (codecstring.Length >= 3)
			{
				if (codecstring.Substring(0, 3) == "BMP")
					isbitmap = true;
			}
			if (isbitmap)
				mimetype = "image/bmp";
			else
			{
				string aext;
				foreach (ImageCodecInfo lcodec in codecs)
				{
					aext = lcodec.FormatDescription;
					if (aext == codecstring)
					{
						codec = lcodec;
						mimetype = lcodec.MimeType;
						break;
					}
				}
				if (codec == null)
					throw new Exception("Codec not found:" + codecstring);
			}
			Bitmap bm = new Bitmap(1, 1, PixelFormat.Format24bppRgb);
			bm.SetResolution(1440, 1440);
			Graphics g = Graphics.FromImage(bm);
			int awidth = (int)Math.Round(metaf.Width * Scale);
			int aheight = (int)Math.Round(metaf.Height * Scale);
			Bitmap output = new Bitmap(awidth, aheight, PixelFormat.Format24bppRgb);
			try
			{
				g = Graphics.FromImage(output);
				g.PageUnit = System.Drawing.GraphicsUnit.Pixel;
				g.DrawImage(metaf, new Rectangle(0, 0, awidth, aheight));
				g.Dispose();
				if (!isbitmap)
				{
					output.Save(destination, codec, null);
				}
				else
					output.Save(destination,ImageFormat.Bmp);
			}
			finally
			{
				output.Dispose();
			}
		}
        public static Color GetInvertedBlackWhite(Color c)
        {
            if (((int)c.R + (int)c.G + (int)c.B) > ((int)255 * 3 / 2))
                return Color.Black;
            else
                return Color.White;
        }
#endif
        public static FontStyle FontStyleFromInteger(int intfontstyle)
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
            return astyle;
        }
        public static int IntegerFromFontStyle(FontStyle astyle)
        {
            int intfontstyle = 0;
            if ((astyle & FontStyle.Bold)>0)
                intfontstyle = intfontstyle + 1;
            if ((astyle & FontStyle.Italic)>0)
                intfontstyle = intfontstyle + 2;
            if ((astyle & FontStyle.Underline)>0)
                intfontstyle = intfontstyle + 4;
            if ((astyle & FontStyle.Strikeout)>0)
                intfontstyle = intfontstyle + 8;
            return intfontstyle;
        }
        public static string StringFontStyleFromInteger(int intstyle)
        {
            return StringFromFontStyle(FontStyleFromInteger(intstyle));
        }
        public static int IntegerFromStringFontStyle(string sfontstyle)
        {
            int astyle = 0;
            if (sfontstyle.IndexOf(Translator.TranslateStr(547)) >= 0)
                astyle=astyle+1;
            if (sfontstyle.IndexOf(Translator.TranslateStr(549)) >= 0)
                astyle = astyle + 2;
            if (sfontstyle.IndexOf(Translator.TranslateStr(548)) >= 0)
                astyle = astyle + 4;
            if (sfontstyle.IndexOf(Translator.TranslateStr(550)) >= 0)
                astyle = astyle + 8;
            return astyle;
        }
        public static string StringFromFontStyle(FontStyle astyle)
        {
            string sfontstyle = "[";
            if ((astyle & FontStyle.Bold) > 0)
            {
                if (sfontstyle != "[")
                    sfontstyle = sfontstyle + ",";
                sfontstyle = sfontstyle + Translator.TranslateStr(547);
            }
            if ((astyle & FontStyle.Italic) > 0)
            {
                if (sfontstyle != "[")
                    sfontstyle = sfontstyle + ",";
                sfontstyle = sfontstyle + Translator.TranslateStr(549);
            }
            if ((astyle & FontStyle.Underline) > 0)
            {
                if (sfontstyle != "[")
                    sfontstyle = sfontstyle + ",";
                sfontstyle = sfontstyle + Translator.TranslateStr(548);
            }
            if ((astyle & FontStyle.Strikeout) > 0)
            {
                if (sfontstyle != "[")
                    sfontstyle = sfontstyle + ",";
                sfontstyle = sfontstyle + Translator.TranslateStr(550);
            }
            sfontstyle = sfontstyle+"]";
            return sfontstyle;
        }
#if REPMAN_COMPACT
#else
        public static Image RemapImageTransparentColor(Image img,Color OldColor)
        {

            Bitmap nbitmap = new Bitmap(img);
            nbitmap.MakeTransparent(OldColor);
            return nbitmap;
        }
#endif
        public static MemoryStream ImageToStream(Image nimage)
        {
            MemoryStream nstream = new MemoryStream();
            nimage.Save(nstream,ImageFormat.Bmp);
            nstream.Seek(0, SeekOrigin.Begin);
            return nstream;
        }
        public static byte[] ImageToByteArray(Image nimage)
        {
            byte[] narray;
            using (MemoryStream nstream = ImageToStream(nimage))
            {
                narray = nstream.ToArray();
            }
            return narray;
        }

        /*public static bool ThumbnailCallback()
        {
            return false;
        }*/
#if PocketPC
#else

        /// <summary>
        /// Scale bitmap to a maximum of width and height
        /// </summary>
        /// <param name="image"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="highquality"></param>
        /// <returns></returns>
        public static System.Drawing.Image ScaledBitmapRatio(Image image, int width, int height, bool highquality)
        {
            if ((image.Width <= width) && (image.Height <= height))
                return image;


            //float scale = Math.Min((float)width / image.Width, (float)height / image.Height);

            float scaledWidth = ((float)width) / image.Width;
            float scaledHeight = ((float)height) / image.Height;
            float newscale = scaledWidth;
            if (scaledHeight < scaledWidth)
                newscale = scaledHeight;


            //var brush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);

            int scaleWidth = (int)(image.Width * newscale);
            int scaleHeight = (int)(image.Height * newscale);

            /*System.Drawing.Image.GetThumbnailImageAbort myCallback = new System.Drawing.Image.GetThumbnailImageAbort(ThumbnailCallback);
            return image.GetThumbnailImage(scaleWidth, scaleHeight, myCallback, IntPtr.Zero);
            */
            var bmp = new System.Drawing.Bitmap((int)scaleWidth, (int)scaleHeight);

            using (System.Drawing.Graphics graph = System.Drawing.Graphics.FromImage(bmp))
            {

                if (highquality)
                {

                    graph.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    graph.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    graph.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    graph.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                }




                graph.DrawImage(image, new System.Drawing.Rectangle(0, 0, bmp.Width - 1, bmp.Height - 1));

                //graph.FillRectangle(brush, new System.Drawing.RectangleF(0, 0, width, height));
                //graph.DrawImage(image, new System.Drawing.Rectangle(((int)width - System.Convert.ToInt32(scaleWidth)) / 2,
                //    ((int)height - System.Convert.ToInt32(scaleHeight)) / 2,
                //    System.Convert.ToInt32(scaleWidth),
                //    System.Convert.ToInt32(scaleHeight)));
            }

            return bmp;
        }

#endif
    }
}

