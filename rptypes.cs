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
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing;
using System.Threading;
#if REPMAN_ZLIB
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
#endif
using System.Text;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Security.Cryptography;


namespace Reportman.Drawing
{
    public class ProgressArgs
    {
        private long fcount;
        private long ftotal;
        public bool Cancel;
        public long Count
        {
            get
            {
                return fcount;
            }
        }
        public long Total
        {
            get
            {
                return ftotal;
            }
        }
        public ProgressArgs(long ncount,long ntotal)
        {
            fcount = ncount;
            ftotal = ntotal;
            Cancel = true;
        }
    }
    public delegate void ProgressEvent(object sender,ProgressArgs args);
    /// <summary>
    /// Provide utitilies about handling double values
    /// </summary>
    public class DoubleUtil
    {
        /// <summary>
        /// Check if a string can be converted to a number
        /// </summary>
        /// <param name="val"></param>
        /// <param name="NumberStyle"></param>
        /// <returns></returns>
        public static bool IsNumeric(string val, System.Globalization.NumberStyles NumberStyle)
        {
#if REPMAN_COMPACT
            bool aresult = true;
            Double result = 0;
            try
            {
                result = Double.Parse(val, NumberStyle,
                    System.Globalization.CultureInfo.CurrentCulture);
            }
            catch
            {
                aresult = false;
            }
            if (aresult)
            {
                if (NumberStyle == System.Globalization.NumberStyles.Integer)
                {
                    if ((result > System.Int32.MaxValue) || (result < System.Int32.MinValue))
                        aresult = false;
                }
            }
            return aresult;
#else
            Double result;
            bool boolresult = (Double.TryParse(val, NumberStyle,
                System.Globalization.CultureInfo.CurrentCulture, out result));
            if (boolresult)
            {
                if (NumberStyle == System.Globalization.NumberStyles.Integer)
                {
                    if ((result > System.Int32.MaxValue) || (result < System.Int32.MinValue))
                        boolresult = false;
                }
            }
            return boolresult;
#endif
        }
        public static bool IsNumericType(object o)
        {
            switch (Type.GetTypeCode(o.GetType()))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }
        /// <summary>
        /// Truncate a decimal number
        /// </summary>
        /// <param name="nvalue"></param>
        /// <returns></returns>
        public static decimal Truncate(decimal nvalue)
        {
            if (nvalue >= 0)
                return System.Convert.ToDecimal(Math.Floor(System.Convert.ToDouble(nvalue)));
            else
                return System.Convert.ToDecimal(Math.Ceiling(System.Convert.ToDouble(nvalue)));
        }
        /// <summary>
        /// Calculate standard deviation
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        ///    
        public static double StandardDeviation(IEnumerable<double> values)
        {
            double avg = values.Average();
            return Math.Sqrt(values.Average(v => Math.Pow(v - avg, 2)));
        }

        /// <summary>
        /// Format a number with a mask
        /// </summary>
        public static string FormatCurrAdv(string mask, double number)
        {
            return (number.ToString(mask));
        }
        /// <summary>
        /// Default format string for a number of decimals
        /// </summary>
        /// <param name="decimals"></param>
        /// <returns></returns>
        public static string FormatStringDecimals(int decimals)
        {
            string aformat = "N0";
            if (decimals > 0)
            {
                //                StringBuilder sbuild = new StringBuilder("0.");
                //                for (int i = 0; i < decimals; i++)
                //                    sbuild.Append('0');
                aformat = "N" + decimals.ToString();
            }
            return aformat;
        }
        /// <summary>
        /// Calculates the fractionary value of a number
        /// </summary>
        /// <param name="number">Decimal number</param>
        /// <returns>Fractionary part of the decimal number</returns>
        public static decimal Frac(decimal number)
        {
#if REPMAN_DOTNET1
			return (number - System.Convert.ToDecimal(System.Convert.ToInt64(number)));
#else
#if REPMAN_COMPACT
            return (number - System.Convert.ToInt64(number));
#else
			return (number - Math.Truncate(number));
#endif
#endif
        }
        /// <summary>
        /// Calculates the integer part of a number
        /// </summary>
        /// <param name="number">Decimal number</param>
        /// <returns>Truncated numberr</returns>
        public static long Trunc(decimal number)
        {
#if REPMAN_DOTNET1
			return (System.Convert.ToInt64(number));
#else
#if REPMAN_COMPACT
            return (System.Convert.ToInt64(number));
#else
			return (System.Convert.ToInt64(Math.Truncate(number)));
#endif
#endif
        }
        /// <summary>
        /// Round a decimal to the nearest multiple value, the midpoint will be rounded up
        /// </summary>
        public static decimal RoundDecimalUp(decimal number, decimal multiple)
        {
            if (multiple == 0)
                return 0;
            bool negative = false;
            if (number < 0)
            {
                number = -number;
                negative = true;
            }
            int scale = 1;
            while (DoubleUtil.Frac(number * scale) != 0)
            {
                scale = scale * 10;
                if (scale > 10000000)
                    break;
            }
            while (DoubleUtil.Frac(multiple * scale) != 0)
            {
                scale = scale * 10;
                if (scale > 10000000)
                    break;
            }
            decimal numberscaled = Math.Round(number * scale);
            
            decimal multiplescaled = multiple * scale;
            decimal division = Math.Round(numberscaled / multiplescaled);
            decimal moddiv = (numberscaled - (multiplescaled * division));
            if (moddiv < (multiplescaled / 2))
                numberscaled = numberscaled - moddiv;
            else
                numberscaled = numberscaled + moddiv;
            decimal aresult = numberscaled / scale;
            if (negative)
                aresult = -aresult;

            return aresult;
        }
        /// <summary>
        /// Compare values, but an difference lower than epsilon will return as equal
        /// </summary>
        public static int CompareValue(decimal p1, decimal p2, decimal epsilon)
        {
            int aresult = 0;
            decimal dif = Math.Abs(p1 - p2);
            epsilon = Math.Abs(epsilon);
            if (dif >= epsilon)
            {
                if (p1 < p2)
                    aresult = -1;
                else
                    aresult = 1;
            }
            return aresult;
        }
        // Function cortesy of Hamza Al-Aradi (AradBox@hotmail.com)
        // Altered to support negative and not show fractionary
        // when there is not fractionary part
        // Converted to dot net by Toni Martir
        private static string NumToStr(long Num)
        {
            string Num2Str = "";
            const int hundred = 100;
            const int thousand = 1000;
            const int million = 1000000;
            const int billion = 1000000000;
            if (Num >= billion)
                if ((Num % billion) == 0)
                    Num2Str = NumToStr(Num / billion) + " Billion";
                else
                    Num2Str = NumToStr(Num / billion) + " Billion " + NumToStr(Num % billion);
            else
                if (Num >= million)
                    if ((Num % million) == 0)
                        Num2Str = NumToStr(Num / million) + " Million";
                    else
                        Num2Str = NumToStr(Num / million) + " Million " + NumToStr(Num % million);
                else
                    if (Num >= thousand)
                        if ((Num % thousand) == 0)
                            Num2Str = NumToStr(Num / thousand) + " Thousand";
                        else
                            Num2Str = NumToStr(Num / thousand) + " Thousand " + NumToStr(Num % thousand);
                    else
                        if (Num >= hundred)
                            if ((Num % hundred) == 0)
                                Num2Str = NumToStr(Num / hundred) + " Hundred";
                            else
                                Num2Str = NumToStr(Num / hundred) + " Hundred " + NumToStr(Num % hundred);
                        else
                            switch (Num / 10)
                            {
                                case 6:
                                case 7:
                                case 9:
                                    if ((Num % 10) == 0)
                                        Num2Str = NumToStr(Num / 10) + "ty";
                                    else
                                        Num2Str = NumToStr(Num / 10) + "ty-" + NumToStr(Num % 10);
                                    break;
                                case 8:
                                    if (Num == 80)
                                        Num2Str = "Eighty";
                                    else
                                        Num2Str = "Eighty-" + NumToStr(Num % 10);
                                    break;
                                case 5:
                                    if (Num == 50)
                                        Num2Str = "Fifty";
                                    else
                                        Num2Str = "Fifty-" + NumToStr(Num % 10);
                                    break;
                                case 4:
                                    if (Num == 40)
                                        Num2Str = "Forty";
                                    else
                                        Num2Str = "Forty-" + NumToStr(Num % 10);
                                    break;
                                case 3:
                                    if (Num == 30)
                                        Num2Str = "Thirty";
                                    else
                                        Num2Str = "Thirty-" + NumToStr(Num % 10);
                                    break;
                                case 2:
                                    if (Num == 20)
                                        Num2Str = "Twenty";
                                    else
                                        Num2Str = "Twenty-" + NumToStr(Num % 10);
                                    break;
                                case 1:
                                case 0:
                                    switch (Num)
                                    {
                                        case 0: Num2Str = "Zero";
                                            break;
                                        case 1: Num2Str = "One";
                                            break;
                                        case 2: Num2Str = "Two";
                                            break;
                                        case 3: Num2Str = "Three";
                                            break;
                                        case 4: Num2Str = "Four";
                                            break;
                                        case 5: Num2Str = "Five";
                                            break;
                                        case 6: Num2Str = "Six";
                                            break;
                                        case 7: Num2Str = "Seven";
                                            break;
                                        case 8: Num2Str = "Eight";
                                            break;
                                        case 9: Num2Str = "Nine";
                                            break;
                                        case 10: Num2Str = "Ten";
                                            break;
                                        case 11: Num2Str = "Eleven";
                                            break;
                                        case 12: Num2Str = "Twelve";
                                            break;
                                        case 13: Num2Str = "Thirteen";
                                            break;
                                        case 14: Num2Str = "Fourteen";
                                            break;
                                        case 15: Num2Str = "Fifteen";
                                            break;
                                        case 16: Num2Str = "Sixteen";
                                            break;
                                        case 17: Num2Str = "Seventeen";
                                            break;
                                        case 18: Num2Str = "Eightteen";
                                            break;
                                        case 19: Num2Str = "Nineteen";
                                            break;
                                    }
                                    break;
                            }
            return Num2Str;
        }
        private static string NumberToTextEnglish(decimal amount)
        {
            amount = Math.Abs(amount);
            long Num = System.Convert.ToInt64(DoubleUtil.Trunc(amount));
            long Fracture = Convert.ToInt64(Math.Round(1000 * (amount - Num)));
            string aresult = NumToStr(Num);
            if (Fracture > 0)
                aresult = aresult + " and " + Fracture.ToString() + "/1000";
            return aresult;
        }
        // Esta funci”n nos da la longitud del n˙mero que vamos a
        // deletrear
        private static long Longitud(long numero)
        {
            if ((numero / 10) == 0)
                return 1;
            else
                return 1 + Longitud(numero / 10);
        }

        private static string UnidadesToStrS(long numero, bool female)
        {
            string Unidades = "";
            switch (numero)
            {
                case 1:
                    if (!female)
                        Unidades = "un";
                    else
                        Unidades = "una";
                    break;
                case 2: Unidades = "dos";
                    break;
                case 3: Unidades = "tres";
                    break;
                case 4: Unidades = "cuatro";
                    break;
                case 5: Unidades = "cinco";
                    break;
                case 6: Unidades = "seis";
                    break;
                case 7: Unidades = "siete";
                    break;
                case 8: Unidades = "ocho";
                    break;
                case 9: Unidades = "nueve";
                    break;
            }
            return Unidades;
        }

        private static string DecenasToStrS(long numero, bool female)
        {
            string Decenas = "";
            switch (numero)
            {
                case 0:
                    break;
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                    Decenas = UnidadesToStrS(numero, female);
                    break;
                case 10: Decenas = "diez";
                    break;
                case 11: Decenas = "once";
                    break;
                case 12: Decenas = "doce";
                    break;
                case 13: Decenas = "trece";
                    break;
                case 14: Decenas = "catorce";
                    break;
                case 15: Decenas = "quince";
                    break;
                case 16: Decenas = "diecisÈis";
                    break;
                case 17: Decenas = "diecisiete";
                    break;
                case 18: Decenas = "dieciocho";
                    break;
                case 19: Decenas = "diecinueve";
                    break;
                case 20: Decenas = "veinte";
                    break;
                case 21:
                    if (!female)
                        Decenas = "veintiuno";
                    else
                        Decenas = "veintiuna";
                    break;
                case 22: Decenas = "veintidÛs";
                    break;
                case 23: Decenas = "veintitrÈs";
                    break;
                case 24:
                case 25:
                case 26:
                case 27:
                case 28:
                case 29:
                    Decenas = "veinti" + UnidadesToStrS(numero % 10, female);
                    break;
                case 30: Decenas = "treinta";
                    break;
                case 40: Decenas = "cuarenta";
                    break;
                case 50: Decenas = "cincuenta";
                    break;
                case 60: Decenas = "sesenta";
                    break;
                case 70: Decenas = "setenta";
                    break;
                case 80: Decenas = "ochenta";
                    break;
                case 90: Decenas = "noventa";
                    break;
                default:
                    Decenas = DecenasToStrS(numero - (numero % 10), female) + " y " + UnidadesToStrS(numero % 10, female);
                    break;
            }
            return Decenas;
        }
        private static string CentenasToStrS(long numero, bool female)
        {
            string centenas = "";
            if ((numero >= 1) && (numero <= 99))
                centenas = DecenasToStrS(numero, female);
            else
                if (numero == 100)
                    centenas = "cien";
                else
                    if ((numero >= 101) && (numero <= 199))
                        centenas = "ciento " + DecenasToStrS(numero % 100, female);
                    else
                        if (numero == 100)
                            if (female)
                                centenas = "doscientas";
                            else
                                centenas = "doscientos";
                        else
                            if (numero == 500)
                                if (female)
                                    centenas = "quinientas";
                                else
                                    centenas = "quinientos";
                            else
                                if ((numero >= 501) && (numero <= 599))
                                    if (female)
                                        centenas = "quinientas " + DecenasToStrS(numero % 100, female);
                                    else
                                        centenas = "quinientos " + DecenasToStrS(numero % 100, female);
                                else
                                    if (numero == 700)
                                        if (female)
                                            centenas = "setecientas";
                                        else
                                            centenas = "setecientos";
                                    else
                                        if ((numero >= 701) && (numero <= 799))
                                            if (female)
                                                centenas = "setecientas " + DecenasToStrS(numero % 100, female);
                                            else
                                                centenas = "setecientos " + DecenasToStrS(numero % 100, female);
                                        else
                                            if (numero == 900)
                                                if (female)
                                                    centenas = "novecientas";
                                                else
                                                    centenas = "novecientos";
                                            else
                                                if ((numero >= 901) && (numero <= 999))
                                                    if (female)
                                                        centenas = "novecientas " + DecenasToStrS(numero % 100, female);
                                                    else
                                                        centenas = "novecientos " + DecenasToStrS(numero % 100, female);
                                                else
                                                    {
                                                        string cientosstr = UnidadesToStrS(numero / 100, female);
                                                        if (cientosstr == "")
                                                        {
                                                            centenas = "";
                                                        }
                                                        else
                                                        {
                                                            if (female)
                                                                centenas = cientosstr + "cientas ";
                                                            else
                                                                centenas = cientosstr + "cientos ";
                                                        }
                                                        centenas = centenas + DecenasToStrS(numero % 100, female);
                                                    }
            return centenas;
        }

        private static string UnidadesDeMillarToStrS(long numero, bool female)
        {
            string UnidadesDeMillar = "";
            if (numero > 999)
                if (numero > 1999)
                    UnidadesDeMillar = UnidadesToStrS(numero / 1000, female) + " mil " + CentenasToStrS(numero % 1000, female);
                else
                    UnidadesDeMillar = "mil " + CentenasToStrS(numero % 1000, female);
            else
                UnidadesDeMillar = CentenasToStrS(numero, female);
            return UnidadesDeMillar;
        }
        private static string DecenasDeMillarToStrS(long numero, bool female)
        {
            string DecenasDeMillar = "";
            if (numero > 9999)
                DecenasDeMillar = DecenasToStrS(numero / 1000, female) + " mil " + CentenasToStrS(numero % 1000, female);
            else
                DecenasDeMillar = UnidadesDeMillarToStrS(numero, female);
            return DecenasDeMillar;
        }
        private static string CentenasDeMillarToStrS(long numero, bool female)
        {
            string CentenasDeMillar = "";
            if (numero > 99999)
                CentenasDeMillar = CentenasToStrS(numero / 1000, female) + " mil " + CentenasToStrS(numero % 1000, female);
            else
                CentenasDeMillar = DecenasDeMillarToStrS(numero, female);
            return CentenasDeMillar;
        }
        private static string UnidadesDeMillonToStrS(long numero, bool female)
        {
            string UnidadesDeMillon = "";
            if (numero > 1999999)
                UnidadesDeMillon = UnidadesToStrS(numero / 1000000, false) + " millones " + CentenasDeMillarToStrS(numero % 1000000, female);
            else
                UnidadesDeMillon = "un millÛn " + CentenasDeMillarToStrS(numero % 1000000, female);
            return UnidadesDeMillon;
        }
        private static string DecenasDeMillonToStrS(long numero, bool female)
        {
            return DecenasToStrS(numero / 1000000, false) + " millones " + CentenasDeMillarToStrS(numero % 1000000, female);
        }
        private static string CentenasDeMillonToStrS(long numero, bool female)
        {
            return CentenasToStrS(numero / 1000000, false) + " millones " + CentenasDeMillarToStrS(numero % 1000000, female);
        }
        private static string MilesDeMillonToStrS(long numero, bool female)
        {
            return UnidadesDeMillarToStrS(numero / 100000, false) + " millones " + CentenasDeMillarToStrS(numero % 1000000, female);
        }
        private static string DecenasDeMilesDeMillonToStrS(long numero, bool female)
        {
            return DecenasDeMillarToStrS(numero / 1000000, false) + " millones " + CentenasDeMillarToStrS(numero % 1000000, female);
        }
        private static string CentenasDeMilesDeMillonToStrS(long numero, bool female)
        {
            return CentenasDeMillarToStrS(numero / 1000000, false) + " millones " + CentenasDeMillarToStrS(numero % 1000000, female);
        }
        private static string NumberToTextSpanish(decimal number, bool female)
        {
            string s = "";
            long centavos;
            long numero;
            string aresult = "";
            number = Math.Abs(number);
            numero = System.Convert.ToInt64(DoubleUtil.Trunc(number));
            centavos = System.Convert.ToInt64(Math.Round((number - numero) * 100));
            switch (Longitud(numero))
            {
                case 1: s = UnidadesToStrS(numero, female);
                    break;
                case 2: s = DecenasToStrS(numero, female);
                    break;
                case 3: s = CentenasToStrS(numero, female);
                    break;
                case 4: s = UnidadesDeMillarToStrS(numero, female);
                    break;
                case 5: s = DecenasDeMillarToStrS(numero, female);
                    break;
                case 6: s = CentenasDeMillarToStrS(numero, female);
                    break;
                case 7: s = UnidadesDeMillonToStrS(numero, female);
                    break;
                case 8: s = DecenasDeMillonToStrS(numero, female);
                    break;
                case 9: s = CentenasDeMillonToStrS(numero, female);
                    break;
                case 10: s = MilesDeMillonToStrS(numero, female);
                    break;
                case 11: s = DecenasDeMilesDeMillonToStrS(numero, female);
                    break;
                case 12: s = CentenasDeMilesDeMillonToStrS(numero, female);
                    break;
                default:
                    s = "Demasiado grande";
                    break;
            }
            if (centavos > 0)
            {
                switch (Longitud(centavos))
                {
                    case 1: aresult = UnidadesToStrS(centavos, female);
                        break;
                    case 2: aresult = DecenasToStrS(centavos, female);
                        break;
                }
                aresult = s + " con " + aresult;
            }
            else
            {
                aresult = s;

            }
            if (aresult.Length > 0)
                aresult = char.ToUpper(aresult[0]).ToString() + aresult.Substring(1, aresult.Length - 1);
            return aresult;
        }
        private static string UnidadesToStrC(long numero, bool female)
        {
            string Unidades = "";
            switch (numero)
            {
                case 1:
                    if (!female)
                        Unidades = "un";
                    else
                        Unidades = "una";
                    break;
                case 2: Unidades = "dos";
                    break;
                case 3: Unidades = "tres";
                    break;
                case 4: Unidades = "quatre";
                    break;
                case 5: Unidades = "cinc";
                    break;
                case 6: Unidades = "sis";
                    break;
                case 7: Unidades = "set";
                    break;
                case 8: Unidades = "vuit";
                    break;
                case 9: Unidades = "nou";
                    break;
            }
            return Unidades;
        }

        private static string DecenasToStrC(long numero, bool female)
        {
            string Decenas = "";
            switch (numero)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                    Decenas = UnidadesToStrC(numero, female);
                    break;
                case 10: Decenas = "deu";
                    break;
                case 11: Decenas = "onze";
                    break;
                case 12: Decenas = "dotze";
                    break;
                case 13: Decenas = "tretze";
                    break;
                case 14: Decenas = "catorze";
                    break;
                case 15: Decenas = "quinze";
                    break;
                case 16: Decenas = "setze";
                    break;
                case 17: Decenas = "disset";
                    break;
                case 18: Decenas = "divuit";
                    break;
                case 19: Decenas = "dinou";
                    break;
                case 20: Decenas = "vint";
                    break;
                case 21: if (!female)
                        Decenas = "vint-i-un";
                    else
                        Decenas = "vint-i-una";
                    break;
                case 22: Decenas = "vint-i-dos";
                    break;
                case 23: Decenas = "vint-i-tres";
                    break;
                case 24:
                case 25:
                case 26:
                case 27:
                case 28:
                case 29:
                    Decenas = "vint-i-" + UnidadesToStrC(numero % 10, female);
                    break;
                case 30: Decenas = "trenta";
                    break;
                case 31:
                    if (!female)
                        Decenas = "trenta-un";
                    else
                        Decenas = "trenta-una";
                    break;
                case 40: Decenas = "quaranta";
                    break;
                case 41:
                    if (!female)
                        Decenas = "quaranta-un";
                    else
                        Decenas = "quaranta-una";
                    break;
                case 50: Decenas = "cinquanta";
                    break;
                case 51:
                    if (!female)
                        Decenas = "cincuanta-un";
                    else
                        Decenas = "cincuanta-una";
                    break;
                case 60: Decenas = "seixanta";
                    break;
                case 36:
                    if (!female)
                        Decenas = "seixanta-un";
                    else
                        Decenas = "seixanta-una";
                    break;
                case 70: Decenas = "setanta";
                    break;
                case 71:
                    if (!female)
                        Decenas = "setanta-un";
                    else
                        Decenas = "setanta-una";
                    break;
                case 80: Decenas = "vuitanta";
                    break;
                case 81:
                    if (!female)
                        Decenas = "vuitanta-un";
                    else
                        Decenas = "vuitanta-una";
                    break;
                case 90: Decenas = "noranta";
                    break;
                case 91:
                    if (!female)
                        Decenas = "noranta-un";
                    else
                        Decenas = "noranta-una";
                    break;
                case 0:
                    break;
                default:
                    Decenas = DecenasToStrC(numero - (numero % 10), female) + "-" + UnidadesToStrC(numero % 10, female);
                    break;
            }
            return Decenas;
        }
        private static string CentenasToStrC(long numero, bool female)
        {
            string centenas = "";
            if ((numero >= 1) && (numero <= 99))
                centenas = DecenasToStrC(numero, female);
            else
                if (numero == 100)
                    centenas = "cent";
                else
                    if ((numero >= 101) && (numero <= 199))
                        centenas = "cent " + DecenasToStrC(numero % 100, female);
                    else
                        if (numero == 100)
                            if (female)
                                centenas = "dos-centes";
                            else
                                centenas = "dos-cents";
                        else
                            if (numero == 500)
                                if (female)
                                    centenas = "cinc-cents";
                                else
                                    centenas = "cinc-centes";
                            else
                                if ((numero >= 501) && (numero <= 599))
                                    if (female)
                                        centenas = "cinc-centes " + DecenasToStrC(numero % 100, female);
                                    else
                                        centenas = "cinc-cents " + DecenasToStrC(numero % 100, female);
                                else
                                    if (numero == 700)
                                        if (female)
                                            centenas = "set-centes";
                                        else
                                            centenas = "set-cents";
                                    else
                                        if ((numero >= 701) && (numero <= 799))
                                            if (female)
                                                centenas = "set-centes " + DecenasToStrC(numero % 100, female);
                                            else
                                                centenas = "set-cents " + DecenasToStrC(numero % 100, female);
                                        else
                                            if (numero == 900)
                                                if (female)
                                                    centenas = "nou-centes";
                                                else
                                                    centenas = "nou-cents";
                                            else
                                                if ((numero >= 901) && (numero <= 999))
                                                    if (female)
                                                        centenas = "nou-centes " + DecenasToStrC(numero % 100, female);
                                                    else
                                                        centenas = "nou-cents " + DecenasToStrC(numero % 100, female);
                                                else
                                                    if (female)
                                                        centenas = UnidadesToStrC(numero / 100, female) + "-centes " +
                                                            DecenasToStrC(numero % 100, female);
                                                    else
                                                        centenas = UnidadesToStrC(numero / 100, female) + "-cents " +
                                                            DecenasToStrC(numero % 100, female);
            return centenas;
        }

        private static string UnidadesDeMillarToStrC(long numero, bool female)
        {
            string UnidadesDeMillar = "";
            if (numero > 999)
                if (numero > 1999)
                    UnidadesDeMillar = UnidadesToStrC(numero / 1000, female) + " mil " + CentenasToStrC(numero % 1000, female);
                else
                    UnidadesDeMillar = "mil " + CentenasToStrC(numero % 1000, female);
            else
                UnidadesDeMillar = CentenasToStrC(numero, female);
            return UnidadesDeMillar;
        }
        private static string DecenasDeMillarToStrC(long numero, bool female)
        {
            string DecenasDeMillar = "";
            if (numero > 9999)
                DecenasDeMillar = DecenasToStrC(numero / 1000, female) + " mil " + CentenasToStrC(numero % 1000, female);
            else
                DecenasDeMillar = UnidadesDeMillarToStrC(numero, female);
            return DecenasDeMillar;
        }
        private static string CentenasDeMillarToStrC(long numero, bool female)
        {
            string CentenasDeMillar = "";
            if (numero > 99999)
                CentenasDeMillar = CentenasToStrC(numero / 1000, female) + " mil " + CentenasToStrC(numero % 1000, female);
            else
                CentenasDeMillar = DecenasDeMillarToStrC(numero, female);
            return CentenasDeMillar;
        }
        private static string UnidadesDeMillonToStrC(long numero, bool female)
        {
            string UnidadesDeMillon = "";
            if (numero > 1999999)
                UnidadesDeMillon = UnidadesToStrC(numero / 1000000, false) + " milions " + CentenasDeMillarToStrC(numero % 1000000, female);
            else
                UnidadesDeMillon = "un miliÛ " + CentenasDeMillarToStrC(numero % 1000000, female);
            return UnidadesDeMillon;
        }
        private static string DecenasDeMillonToStrC(long numero, bool female)
        {
            return DecenasToStrC(numero / 1000000, false) + " milions " + CentenasDeMillarToStrC(numero % 1000000, female);
        }
        private static string CentenasDeMillonToStrC(long numero, bool female)
        {
            return CentenasToStrC(numero / 1000000, false) + " milions " + CentenasDeMillarToStrC(numero % 1000000, female);
        }
        private static string MilesDeMillonToStrC(long numero, bool female)
        {
            return UnidadesDeMillarToStrC(numero / 100000, false) + " milions " + CentenasDeMillarToStrC(numero % 1000000, female);
        }
        private static string DecenasDeMilesDeMillonToStrC(long numero, bool female)
        {
            return DecenasDeMillarToStrC(numero / 1000000, false) + " milions " + CentenasDeMillarToStrC(numero % 1000000, female);
        }
        private static string CentenasDeMilesDeMillonToStrC(long numero, bool female)
        {
            return CentenasDeMillarToStrC(numero / 1000000, false) + " milions " + CentenasDeMillarToStrC(numero % 1000000, female);
        }
        private static string NumberToTextCatalan(decimal number, bool female)
        {
            string s = "";
            long centavos;
            long numero;
            string aresult = "";
            number = Math.Abs(number);
            numero = System.Convert.ToInt64(DoubleUtil.Trunc(number));
            centavos = System.Convert.ToInt64(Math.Round((number - numero) * 100));
            switch (Longitud(numero))
            {
                case 1: s = UnidadesToStrC(numero, female);
                    break;
                case 2: s = DecenasToStrC(numero, female);
                    break;
                case 3: s = CentenasToStrC(numero, female);
                    break;
                case 4: s = UnidadesDeMillarToStrC(numero, female);
                    break;
                case 5: s = DecenasDeMillarToStrC(numero, female);
                    break;
                case 6: s = CentenasDeMillarToStrC(numero, female);
                    break;
                case 7: s = UnidadesDeMillonToStrC(numero, female);
                    break;
                case 8: s = DecenasDeMillonToStrC(numero, female);
                    break;
                case 9: s = CentenasDeMillonToStrC(numero, female);
                    break;
                case 10: s = MilesDeMillonToStrC(numero, female);
                    break;
                case 11: s = DecenasDeMilesDeMillonToStrC(numero, female);
                    break;
                case 12: s = CentenasDeMilesDeMillonToStrC(numero, female);
                    break;
                default:
                    s = "Massa gran";
                    break;
            }
            if (centavos > 0)
            {
                switch (Longitud(centavos))
                {
                    case 1: aresult = UnidadesToStrC(centavos, female);
                        break;
                    case 2: aresult = DecenasToStrC(centavos, female);
                        break;
                }
                aresult = s + " amb " + aresult;
            }
            else
            {
                aresult = s;

            }
            if (aresult.Length > 0)
                aresult = char.ToUpper(aresult[0]).ToString() + aresult.Substring(1, aresult.Length - 1);
            return aresult;
        }
        private static string NumberToTextSpanishMex(decimal number, bool female)
        {
            return "";
        }
        private static string NumberToTextPortuguese(decimal number)
        {
            return "";
        }
        private static string NumberToTextTurkish(decimal number)
        {
            return "";
        }
        private static string NumberToTextLithuanian(decimal number, bool female)
        {
            return "";
        }
        /// <summary>
        /// Converts a number to his representation in natural words
        /// </summary>
        public static string NumberToText(decimal number, bool female, int lang)
        {
            string aresult = "";
            switch (lang)
            {
                case -1:
                case 0:
                    aresult = NumberToTextEnglish(number);
                    break;
                case 1:
                    aresult = NumberToTextSpanish(number, female);
                    break;
                case 2:
                    aresult = NumberToTextCatalan(number, female);
                    break;
                case 4:
                    aresult = NumberToTextPortuguese(number);
                    break;
                case 7:
                    aresult = NumberToTextTurkish(number);
                    break;
                case 8:
                    aresult = NumberToTextLithuanian(number, female);
                    break;
                case 13:
                    aresult = NumberToTextSpanishMex(number, female);
                    break;
            }
            return aresult;
        }
    }
    /// <summary>
    /// Provide utitilies about handling DateTime values
    /// </summary>
    public class DateUtil
    {
        public static DateTime FIRST_DELPHI_DAY = new DateTime(1899, 12, 30);
        /// <summary>
        /// Converts a double representing the number of days from 30 Dec 1899 to DateTime
        /// </summary>
        public static DateTime DelphiDateToDateTime(double avalue)
        {
            return FIRST_DELPHI_DAY.Add(DateUtil.DelphiDateTimeToTimeSpan(avalue));
        }

        /// <summary>
        /// Converts a DateTime to a double value representing the number of days from 30 Dec 1899
        /// </summary>
        public static double DateTimeToDelphiDate(DateTime avalue)
        {
            TimeSpan difdate = avalue - FIRST_DELPHI_DAY;
            return difdate.Days + difdate.Hours / 24 + difdate.Minutes / (24 * 60) + difdate.Seconds / (24 * 60 * 60);
        }
        /// <summary>
        /// Converts a Delphi DateTime to a TimeSpan, time since 30 DEC 1899
        /// </summary>
        public static TimeSpan DelphiDateTimeToTimeSpan(double avalue)
        {
            int days = (int)avalue;
            double atime = avalue - (int)avalue;
            int seconds = (int)(atime * 86400);
            int hours = (int)(seconds / 3600);
            seconds = seconds - hours * 3600;
            int minutes = (int)(seconds / 60);
            seconds = seconds - minutes * 60;
            return new TimeSpan(days, hours, minutes, seconds);
        }
#if REPMAN_DOTNET1
		public static bool IsDateTime(string val,out DateTime result)
		{   
			bool aresult=true;
			try
			{
				result=System.Convert.ToDateTime(val);
			}
			catch
			{
				aresult=false;
				result=DateTime.Now;
			}
			return aresult;
		}
#else
        public static bool IsDateTime(string val, out DateTime result)
        {
            result = DateTime.MinValue;
            // 
            if ((val.Length<8) || (val.Length>10))
                return false;
            bool aresult = false;
#if REPMAN_COMPACT
            result = DateTime.Now;
            try
            {
                result = DateTime.Parse(val);
            }
            catch
            {
                aresult = false;
            }
#else
            aresult = DateTime.TryParse(val, out result);
#endif
            if ((result.Year < 1800) || (result.Year >= 9000))
                aresult = false;
            return aresult;
        }
#endif
        public static DateTime NextSaturday(DateTime value)
        {
            DateTime result = value;
            while (result.DayOfWeek != DayOfWeek.Saturday)
                result = result.AddDays(1);
            return result;
        }
        public static DateTime NextFriDay(DateTime value)
        {
            DateTime result = value;
            while (result.DayOfWeek != DayOfWeek.Friday)
                result = result.AddDays(1);
            return result;
        }
        public static DateTime NextDayOfMonth(DateTime value, int nday)
        {
            int dayofmon = value.Day;
            if (dayofmon <= nday)
            {
                return value.AddDays(nday - dayofmon);
            }
            else
            {
                value = value.AddMonths(1);
                dayofmon = value.Day;
                if (dayofmon <= nday)
                {
                    return value.AddDays(nday - dayofmon);
                }
                else
                    return value.AddDays(-(dayofmon - nday));
            }
        }
        public static DateTime LastDayOfMonth(DateTime value)
        {
            DateTime dtTo = value;


            dtTo = dtTo.AddMonths(1);
            dtTo = dtTo.AddDays(-(dtTo.Day));

            return dtTo; 
        }
        public static DateTime AddWorkableDays(DateTime value, int days)
        {
            while (days > 0)
            {
                if ((value.DayOfWeek != DayOfWeek.Saturday) && (value.DayOfWeek != DayOfWeek.Sunday))
                {
                    days--;
                }
                value = value.AddDays(1);
            }
            return value;
        }
    }
    /// <summary>
    /// Provide utitilies about handling string values
    /// </summary>
    public class StringUtil
    {
        private static char GetControlDigit(string CadenaNumerica)
        {
            int[] pesos = { 1, 2, 4, 8, 5, 10, 9, 7, 3, 6 };
            UInt32 suma = 0;
            UInt32 resto;

            for (int i = 0; i < pesos.Length; i++)
            {
                suma += (UInt32)pesos[i] * UInt32.Parse(CadenaNumerica.Substring(i, 1));
            }
            resto = 11 - (suma % 11);

            if (resto == 10) resto = 1;
            if (resto == 11) resto = 0;

            return resto.ToString("0")[0];
        }
        public static string CheckBankAccount20(string cadena)
        {
            string resultado = "";
            if (cadena.Length != 20)
                return "La cuenta debe tener 20 dÌgitos";
            foreach (char nchar in cadena)
            {
                if (!char.IsDigit(nchar))
                    return "La cuenta debe ser numÈrica";
            }
            string entidad = cadena.Substring(0, 4);
            string oficina = cadena.Substring(4, 4);
            char dc1 = cadena[8];
            char dc2 = cadena[9];

            string cuenta = cadena.Substring(10, 10);

            if (GetControlDigit("00" + entidad + oficina) != dc1)
                return "Cuenta incorrecta";
            if (GetControlDigit(cuenta) != dc2)
                return "Cuenta incorrecta";

            return resultado;
        }
        public static string ConvertLineBreaks(string nstring)
        {
            StringBuilder nresult = new StringBuilder();
            for (int i = 0; i < nstring.Length; i++)
            {
                if (nstring[i] == '\r')
                {
                    nresult.Append(nstring[i]);
                    if (i >= nstring.Length - 1)
                    {
                        nresult.Append((char)10);
                    }
                    else
                    {
                        nresult.Append((char)10);
                        if (nstring[i + 1] == (char)10)
                            i++;
                    }
                }
                else
                {
                    if (nstring[i] == '\n')
                    {
                        if (i == 0)
                        {
                            nresult.Append('\r');
                            nresult.Append(nstring[i]);
                        }
                        else
                        {
                            if (nstring[i - 1] != '\r')
                            {
                                nresult.Append('\r');
                                nresult.Append(nstring[i]);
                            }
                        }
                    }
                    else
                        nresult.Append(nstring[i]);

                }
            }
            return nresult.ToString();
        }
        public static string ConvertToHtml(string plaintext)
        {
            string nresult = plaintext.Replace("" + (char)13 + (char)10, "<br/>");
            nresult = plaintext.Replace("" + (char)10, "<br/>");
            return nresult;
        }
        private const string consignos = "¡¿ƒ¬…»À ÕÃœŒ”“÷‘⁄Ÿ‹€";
        private const string sinsignos = "AAAAEEEEIIIIOOOOUUUU                    ";
        private const string signosvalidos = "«— &',-./:;_0123456789";
        public static string UpperCaseSpecial(string source)
        {
            string nresult = source.ToUpper();
            char[] narray = nresult.ToCharArray();
            for (int i = 0; i < narray.Length; i++)
            {
                int idx = consignos.IndexOf(narray[i]);
                if (idx >= 0)
                    narray[i] = sinsignos[idx];
                if ((narray[i] < 'A') || (narray[i] > 'Z'))
                {
                    idx = signosvalidos.IndexOf(narray[i]);
                    if (idx < 0)
                    {
                        narray[i] = ' ';
                    }
                }
            }
            string xresult = new String(narray);
            xresult = xresult.Replace("  ", " ");
            xresult = xresult.Replace("  ", " ");
            return xresult;
        }
        public static string PadStringRightN(string source, int total)
        {
            string nresult = UpperCaseSpecial(source.Trim());
            if (nresult.Length >= total)
            {
                nresult = nresult.Substring(0, total);
            }
            else
            {
                nresult = nresult.PadRight(total);
            }
            return nresult;
        }
        public static bool VerifyHash(string plainText,
    string hashAlgorithm,
    string hashString)
        {
            // Convert base64-encoded hash value into a byte array.
            byte[] hashWithSaltBytes = Convert.FromBase64String(hashString);

            // We must know size of hash (without salt).
            int hashSizeInBits, hashSizeInBytes;

            // Make sure that hashing algorithm name is specified.
            if (hashAlgorithm == null)
                hashAlgorithm = "";

            // Size of hash is based on the specified algorithm.
            switch (hashAlgorithm.ToUpper())
            {
                case "SHA1":
                    hashSizeInBits = 160;
                    break;

                case "SHA256":
                    hashSizeInBits = 256;
                    break;

                case "SHA384":
                    hashSizeInBits = 384;
                    break;

                case "SHA512":
                    hashSizeInBits = 512;
                    break;

                default: // Must be MD5
                    hashSizeInBits = 128;
                    break;
            }

            // Convert size of hash from bits to bytes.
            hashSizeInBytes = hashSizeInBits / 8;

            // Make sure that the specified hash value is long enough.
            if (hashWithSaltBytes.Length < hashSizeInBytes)
                return false;

            // Allocate array to hold original salt bytes retrieved from hash.
            byte[] saltBytes = new byte[hashWithSaltBytes.Length -
                hashSizeInBytes];

            // Copy salt from the end of the hash to the new array.
            for (int i = 0; i < saltBytes.Length; i++)
                saltBytes[i] = hashWithSaltBytes[hashSizeInBytes + i];

            // Compute a new hash string.
            string expectedHashString =
                ComputeHash(plainText, hashAlgorithm, saltBytes);

            // If the computed hash matches the specified hash,
            // the plain text value must be correct.
            return (hashString == expectedHashString);
        }
        public static string ComputeHash(string plainText,
        string hashAlgorithm,
        byte[] saltBytes)
        {
            // If salt is not specified, generate it on the fly.
            if (saltBytes == null)
            {
                // Define min and max salt sizes.
                int minSaltSize = 4;
                int maxSaltSize = 8;

                // Generate a random number for the size of the salt.
                Random random = new Random();
                int saltSize = random.Next(minSaltSize, maxSaltSize);

                // Allocate a byte array, which will hold the salt.
                saltBytes = new byte[saltSize];

                // Initialize a random number generator.
                RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

                // Fill the salt with cryptographically strong byte values.
                rng.GetNonZeroBytes(saltBytes);
            }

            // Convert plain text into a byte array.
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            // Allocate array, which will hold plain text and salt.
            byte[] plainTextWithSaltBytes =
                new byte[plainTextBytes.Length + saltBytes.Length];

            // Copy plain text bytes into resulting array.
            for (int i = 0; i < plainTextBytes.Length; i++)
                plainTextWithSaltBytes[i] = plainTextBytes[i];

            // Append salt bytes to the resulting array.
            for (int i = 0; i < saltBytes.Length; i++)
                plainTextWithSaltBytes[plainTextBytes.Length + i] = saltBytes[i];

            // Because we support multiple hashing algorithms, we must define
            // hash object as a common (abstract) base class. We will specify the
            // actual hashing algorithm class later during object creation.
            HashAlgorithm hash;

            // Make sure hashing algorithm name is specified.
            if (hashAlgorithm == null)
                hashAlgorithm = "";

            // Initialize appropriate hashing algorithm class.
            switch (hashAlgorithm.ToUpper())
            {
                case "SHA1":
                    hash = new SHA1Managed();
                    break;
#if PocketPC
#else
                case "SHA256":
                    hash = new SHA256Managed();
                    break;

                case "SHA384":
                    hash = new SHA384Managed();
                    break;

                case "SHA512":
                    hash = new SHA512Managed();
                    break;

#endif
                case "MD5":
                    hash = new MD5CryptoServiceProvider();
                    break;
                default:
                    throw new Exception("Hash algorithm not supported: " + hashAlgorithm.ToUpper());
            }

            // Compute hash value of our plain text with appended salt.
            byte[] hashBytes = hash.ComputeHash(plainTextWithSaltBytes);

            // Create array which will hold hash and original salt bytes.
            byte[] hashWithSaltBytes = new byte[hashBytes.Length +
                saltBytes.Length];

            // Copy hash bytes into resulting array.
            for (int i = 0; i < hashBytes.Length; i++)
                hashWithSaltBytes[i] = hashBytes[i];

            // Append salt bytes to the result.
            for (int i = 0; i < saltBytes.Length; i++)
                hashWithSaltBytes[hashBytes.Length + i] = saltBytes[i];

            // Convert result into a base64-encoded string.
            string hashString = Convert.ToBase64String(hashWithSaltBytes);

            // Return the result.
            return hashString;
        }

#if PocketPC
#else
        public static string PadDecimalLeftN(decimal source, int totallength,int decimals)
        {
            string nresult = "";
            if (source >= 0)
                nresult = nresult + " ";
            else
            {
                source = -source;
                nresult = nresult + "N";
            }
            int intlen = totallength-1-decimals;
            long npart = System.Convert.ToInt64(Math.Truncate(source));
            nresult = nresult + npart.ToString().PadLeft(intlen,'0');
            decimal frac = (source - npart)*System.Convert.ToDecimal(Math.Pow(10,decimals));
            npart = System.Convert.ToInt64(Math.Truncate(frac));
            nresult = nresult + npart.ToString().PadLeft(decimals, '0');


            return nresult;
        }
        public static string PadDecimalLeftS(decimal source, int totallength, int decimals)
        {
            string nresult = "";
            if (source < 0)
                throw new Exception("Only positive numbers in PadDecimalLeftS");
            int intlen = totallength - decimals;
            long npart = System.Convert.ToInt64(Math.Truncate(source));
            nresult = nresult + npart.ToString().PadLeft(intlen, '0');
            decimal frac = (source - npart) * System.Convert.ToDecimal(Math.Pow(10, decimals));
            npart = System.Convert.ToInt64(Math.Truncate(frac));
            nresult = nresult + npart.ToString().PadLeft(decimals, '0');


            return nresult;
        }
#endif

        public static string StripHTML(string source)
        {
            try
            {
                string result;

                // Remove HTML Development formatting
                // Replace line breaks with space
                // because browsers inserts space
                result = source.Replace("\r", " ");
                // Replace line breaks with space
                // because browsers inserts space
                result = result.Replace("\n", " ");
                // Remove step-formatting
                result = result.Replace("\t", string.Empty);
                // Remove repeating spaces because browsers ignore them
                result = System.Text.RegularExpressions.Regex.Replace(result,
                                                                      @"( )+", " ");

                // Remove the header (prepare first by clearing attributes)
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*head([^>])*>", "<head>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"(<( )*(/)( )*head( )*>)", "</head>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(<head>).*(</head>)", string.Empty,
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // remove all scripts (prepare first by clearing attributes)
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*script([^>])*>", "<script>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"(<( )*(/)( )*script( )*>)", "</script>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                //result = System.Text.RegularExpressions.Regex.Replace(result,
                //         @"(<script>)([^(<script>\.</script>)])*(</script>)",
                //         string.Empty,
                //         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"(<script>).*(</script>)", string.Empty,
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // remove all styles (prepare first by clearing attributes)
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*style([^>])*>", "<style>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"(<( )*(/)( )*style( )*>)", "</style>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(<style>).*(</style>)", string.Empty,
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // insert tabs in spaces of <td> tags
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*td([^>])*>", "\t",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // insert line breaks in places of <BR> and <LI> tags
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*br( )*>", "\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*li( )*>", "\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // insert line paragraphs (double line breaks) in place
                // if <P>, <DIV> and <TR> tags
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*div([^>])*>", "\r\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*tr([^>])*>", "\r\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*p([^>])*>", "\r\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // Remove remaining tags like <a>, links, images,
                // comments etc - anything that's enclosed inside < >
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<[^>]*>", string.Empty,
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // replace special characters:
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @" ", " ",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&bull;", " * ",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&lsaquo;", "<",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&rsaquo;", ">",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&trade;", "(tm)",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&frasl;", "/",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&lt;", "<",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&gt;", ">",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&copy;", "(c)",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&reg;", "(r)",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                // Remove all others. More can be added, see
                // http://hotwired.lycos.com/webmonkey/reference/special_characters/
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&(.{2,6});", string.Empty,
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // for testing
                //System.Text.RegularExpressions.Regex.Replace(result,
                //       this.txtRegex.Text,string.Empty,
                //       System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // make line breaking consistent
                result = result.Replace("\n", "\r");

                // Remove extra line breaks and tabs:
                // replace over 2 breaks with 2 and over 4 tabs with 4.
                // Prepare first to remove any whitespaces in between
                // the escaped characters and remove redundant tabs in between line breaks
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(\r)( )+(\r)", "\r\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(\t)( )+(\t)", "\t\t",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(\t)( )+(\r)", "\t\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(\r)( )+(\t)", "\r\t",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                // Remove redundant tabs
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(\r)(\t)+(\r)", "\r\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                // Remove multiple tabs following a line break with just one tab
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(\r)(\t)+", "\r\t",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                // Initial replacement target string for line breaks
                string breaks = "\r\r\r";
                // Initial replacement target string for tabs
                string tabs = "\t\t\t\t\t";
                for (int index = 0; index < result.Length; index++)
                {
                    result = result.Replace(breaks, "\r\r");
                    result = result.Replace(tabs, "\t\t\t\t");
                    breaks = breaks + "\r";
                    tabs = tabs + "\t";
                }

                // That's it.
                return result;
            }
            catch (Exception E)
            {
                return E.Message + (char)10 + source;
            }
        }
        /// <summary>
        /// Return a string representing the size of a stream in bytes, kbytes, megabytes
        /// </summary>
        /// <param name="sizeinbytes"></param>
        /// <returns></returns>
        public static string GetSizeAsString(long sizeinbytes)
        {
            if (sizeinbytes < 1024)
                return sizeinbytes.ToString("##0") + " bytes";
            if (sizeinbytes < 1024 * 1024)
                return (((double)sizeinbytes) / 1024).ToString("##0.00") + " Kilobytes";
            return (((double)sizeinbytes) / (1024 * 1024)).ToString("##0.00") + " Megabytes";
        }
        /// <summary>
        /// Return a string representing the size of a stream in bytes, kbytes, megabytes
        /// </summary>
        /// <param name="sizeinbytes"></param>
        /// <returns></returns>
        public static string GetSizeAsSmallString(long sizeinbytes)
        {
            if (sizeinbytes < 1024)
                return sizeinbytes.ToString("##0") + "b";
            if (sizeinbytes < 1024 * 1024)
                return (((double)sizeinbytes) / 1024).ToString("##0") + "K";
            return (((double)sizeinbytes) / (1024 * 1024)).ToString("##0") + "M";
        }
        /// <summary>
        /// Number of occurrences of a char inside string
        /// </summary>
        /// <param name="achar"></param>
        /// <param name="nstring"></param>
        /// <returns></returns>
        public static int CountOfChar(char achar, string nstring)
        {
            int ncount = 0;
            foreach (char xchar in nstring)
            {
                if (xchar == achar)
                    ncount++;
            }
            return ncount;
        }
        /// <summary>
        /// Returns a string quoted with single quotes, if
        /// a single quote is contained, doubles de quote
        /// </summary>
        /// <param name="ident"></param>
        /// <returns></returns>
        public static string QuoteStr(string ident)
        {
            StringBuilder sbuilder = new StringBuilder();
            sbuilder.Append('\'');
            foreach (char c in ident)
            {
                if (c == '\'')
                {
                    sbuilder.Append(c);
                }
                sbuilder.Append(c);

            }
            sbuilder.Append('\'');
            return sbuilder.ToString();
        }
        /// <summary>
        /// Returns a string quoted with double quotes, if
        /// a double quote is contained, doubles de double quote
        /// </summary>
        /// <param name="ident"></param>
        /// <returns></returns>
        public static string DoubleQuoteStr(string ident)
        {
            StringBuilder sbuilder = new StringBuilder();
            sbuilder.Append('"');
            foreach (char c in ident)
            {
                if (c == '"')
                {
                    sbuilder.Append(c);
                }
                sbuilder.Append(c);

            }
            sbuilder.Append('"');
            return sbuilder.ToString();
        }
        /// <summary>
        /// Returns a string quoted with custom 'quote' separator, if
        /// quote separator is contained, doubles de double quote
        /// </summary>
        /// <param name="ident"></param>
        /// <returns></returns>
        public static string CustomQuoteStr(string ident,char quote)
        {
           StringBuilder sbuilder = new StringBuilder();
           sbuilder.Append(quote);
           foreach (char c in ident)
           {
              if (c == quote)
              {
                 sbuilder.Append(c);
              }
              sbuilder.Append(c);

           }
           sbuilder.Append(quote);
           return sbuilder.ToString();
        }
        /// <summary>
        /// Returns a string repeating a character n times
        /// </summary>
        public static string RepeatChar(char c, int count)
        {
            if (count <= 0)
                count = 10;
            StringBuilder s = new StringBuilder(count);
            for (int i = 0; i < count; i++)
            {
                s.Append(c);
            }
            return s.ToString();
        }
        /// <summary>
        /// Transform a byte to his hexadecimal representation
        /// </summary>
        public static string ByteToHex(byte avalue)
        {
            char char1, char2;
            byte hvalue = (byte)(avalue >> 4);
            byte lvalue = (byte)(avalue & 0x0F);

            if (hvalue > 9)
                char1 = (char)(((byte)'A') + (hvalue - 10));
            else
                char1 = (char)(((byte)'0') + (hvalue));
            if (lvalue > 9)
                char2 = (char)(((byte)'A') + (lvalue - 10));
            else
                char2 = (char)(((byte)'0') + (lvalue));
            return char1.ToString() + char2.ToString();
        }
        /// <summary>
        /// Returns true if the char is inside the set: ['0'..'9','a'..'z','A'..'Z','(',')','.',' ',';',':','_','=']
        /// </summary>
        public static bool IsAlpha(char achar)
        {
            bool aresult = false;
            if ((achar >= '0') && (achar <= '9'))
                aresult = true;
            else
                if ((achar >= 'A') && (achar <= 'Z'))
                    aresult = true;
                else
                    if ((achar >= 'a') && (achar <= 'z'))
                        aresult = true;
                    else
                        if ((achar == '_') || (achar == ' ') || (achar == '.') || (achar == '(') ||
                         (achar == ')') || (achar == '=') || (achar == ';') || (achar == ':'))
                            aresult = true;
            return aresult;
        }
        /// <summary>
        /// Returns true if the char is inside the set: ['0'..'9']
        /// </summary>
        public static bool IsDigit(char achar)
        {
            bool aresult = false;
            if ((achar >= '0') && (achar <= '9'))
                aresult = true;
            return aresult;
        }
        /// <summary>
        /// Returns true if the string contains only digits: ['0'..'9']
        /// </summary>
        public static bool IsAllDigits(string nstring)
        {
            if (nstring == null)
                return false;
            if (nstring.Length == 0)
                return false;
            bool aresult = true;
            foreach (char achar in nstring)
            {
                if (!((achar >= '0') && (achar <= '9')))
                {
                    aresult = false;
                    break;
                }
            }
            return aresult;
        }
        /// <summary>
        /// Transform a hexadecimal representation to an array of byte, return number of bytes writed into the buffer
        /// </summary>
        public static int HexToBytes(string hex, byte[] buf)
        {
            int binindex = 0;
            int hexindex = 0;
            int highnibble = -1;
            char nchar;
            int aval;
            while (hexindex < hex.Length)
            {
                nchar = hex[hexindex];
                aval = -1;
                switch (nchar)
                {
                    case '0':
                        aval = 0;
                        break;
                    case '1':
                        aval = 1;
                        break;
                    case '2':
                        aval = 2;
                        break;
                    case '3':
                        aval = 3;
                        break;
                    case '4':
                        aval = 4;
                        break;
                    case '5':
                        aval = 5;
                        break;
                    case '6':
                        aval = 6;
                        break;
                    case '7':
                        aval = 7;
                        break;
                    case '8':
                        aval = 8;
                        break;
                    case '9':
                        aval = 9;
                        break;
                    case 'A':
                        aval = 10;
                        break;
                    case 'B':
                        aval = 11;
                        break;
                    case 'C':
                        aval = 12;
                        break;
                    case 'D':
                        aval = 13;
                        break;
                    case 'E':
                        aval = 14;
                        break;
                    case 'F':
                        aval = 15;
                        break;
                }
                if (aval >= 0)
                {
                    if (highnibble >= 0)
                    {
                        aval = aval + (highnibble << 4);
                        highnibble = -1;
                        buf[binindex] = (byte)aval;
                        binindex++;
                    }
                    else
                        highnibble = aval;
                }
                hexindex++;
            }
            return (binindex);
        }
        public static string Decode(string value, int codepage)
        {
            if (codepage == 0)
                return value;
            Encoding enc = Encoding.GetEncoding(codepage);
            byte[] bytes = new byte[value.Length];

            int i = 0;
            foreach (char c in value)
            {
                bytes[i] = (byte)c;
                i++;
            }

            string newresult = enc.GetString(bytes, 0, bytes.Length);

            return newresult;
        }
        public static string Encode(string value, int codepage)
        {
            if (codepage == 0)
                return value;
            Encoding enc = Encoding.GetEncoding(codepage);
            byte[] bytes = enc.GetBytes(value);
            StringBuilder nbuild = new StringBuilder();
            foreach (byte c in bytes)
            {
                nbuild.Append((char)c);
            }


            return nbuild.ToString();
        }
#if PocketPC
#else
        public static bool comprovarNif(string nif)
        {
            bool correcte = false;
            if (nif.Length == 0)
                return false;
            if (nif.Length == 1)
                return false;
            if (((nif[0] >= 'a' && nif[0] <= 'z') || (nif[0] >= 'A' && nif[0] <= 'Z')) && ((nif[1] >= 'a' && nif[1] <= 'z') || (nif[1] >= 'A' && nif[1] <= 'Z')))
            {
                //es un nif intracomunitario
                if (nif.Length > 5)
                    correcte = true;
                else
                    correcte = false;
            }
            //if (nif.Length == 0 || nif.Equals("XXXXXXXX")) correcte = true;//no s'ha escrit un nif
            else
            {
                //s'ha escrit un nif o cif
                char a = nif[0];
                if ((nif[0] >= 'a' && nif[0] <= 'z') || (nif[0] >= 'A' && nif[0] <= 'Z'))
                {
                    //es un CIF o NIE
                    if ((nif[nif.Length - 1] >= 'a' && nif[nif.Length - 1] <= 'z') || nif[nif.Length - 1] >= 'A' && nif[nif.Length - 1] <= 'Z')
                    {
                        switch (nif[0])
                        {
                            case 'X':
                            case 'Y':
                            case 'Z':
                            case 'x':
                            case 'y':
                            case 'z':
                                nif = nif.ToUpper();
                                char primer_digito='0';
                                switch (nif[0])
                                {
                                    case 'Y':
                                        primer_digito = '1';
                                        break;
                                    case 'Z':
                                        primer_digito = '2';
                                        break;
                                }
                                nif = primer_digito + nif.Substring(1,nif.Length-1);
                                correcte = nifCorrecto(nif);//es NIE
                                break;
                            default:
                                if ((nif[0] >= 'a' && nif[0] <= 'z') || nif[0] >= 'A' && nif[0] <= 'Z')
                                    correcte = cifCorrecto(nif);//es CIF
                                else
                                    correcte = nifCorrecto(nif.Substring(1, nif.Length - 1));//es NIE
                                break;
                        }
                        
                    }
                    else correcte = cifCorrecto(nif);//es CIF
                }
                else
                {
                    //es un nif
                    correcte = nifCorrecto(nif);
                }
            }
            return correcte;
        }
        private enum TiposCodigosEnum { NIF, NIE, CIF };

         private static TiposCodigosEnum GetTipoDocumento(char letra)
        {
            Regex regexNumeros = new Regex("[0-9]");
            if ( regexNumeros.IsMatch(letra.ToString()) )
                return TiposCodigosEnum.NIF;

            Regex regexLetrasNIE = new Regex("[LKXYM]");
            if ( regexLetrasNIE.IsMatch(letra.ToString()) )
                return TiposCodigosEnum.NIE ;

            Regex regexLetrasCIF = new Regex("[ABCDEFGHJPQRSUVNW]");
            if ( regexLetrasCIF.IsMatch(letra.ToString()) )
                return TiposCodigosEnum.CIF;

            throw new ApplicationException("El cÛdigo no es reconocible");
        }

        private static bool cifCorrecto(string nif)
        {
             bool correcte = false;
            string[] letrasCodigo = { "J", "A", "B", "C", "D", "E", "F", "G", "H", "I" };

            string LetraInicial = nif[0].ToString();
            string DigitoControl = nif[nif.Length - 1].ToString();
            string n = nif.ToString().Substring(1, nif.Length - 2);
            int sumaPares = 0;
            int sumaImpares = 0;
            int sumaTotal = 0;
            int i = 0;
            bool retVal = false;
            // Recorrido por todos los dÌgitos del n˙mero
            for (i = 0; i < n.Length; i++)
            {
                int aux;
                Int32.TryParse(n[i].ToString(), out aux);
                if ((i + 1) % 2 == 0)
                {
                    // Si es una posiciÛn par, se suman los dÌgitos
                    sumaPares += aux;
                }
                else
                {
                    // Si es una posiciÛn impar, se multiplican los dÌgitos por 2
                    aux = aux * 2;

                    // se suman los dÌgitos de la suma
                    sumaImpares += SumaDigitos(aux);
                }
            }
            // Se suman los resultados de los n˙meros pares e impares
            sumaTotal += sumaPares + sumaImpares;
            // Se obtiene el dÌgito de las unidades
            Int32 unidades = sumaTotal % 10;
            // Si las unidades son distintas de 0, se restan de 10
            if (unidades != 0) unidades = 10 - unidades;
            switch (LetraInicial)
            {
                // SÛlo n˙meros
                case "A":
                case "B":
                case "E":
                case "H":
                    retVal = DigitoControl == unidades.ToString();
                    break;

                // SÛlo letras
                case "K":
                case "P":
                case "Q":
                case "S":
                    retVal = DigitoControl == letrasCodigo[unidades];
                    break;
                default:
                    retVal = (DigitoControl == unidades.ToString()) || (DigitoControl == letrasCodigo[unidades]);
                    break;
            }
            correcte = retVal;
            return correcte;
        }
        private static bool nifCorrecto(string nif)
        {
            bool correcte = false;
            String aux = null;
            nif = nif.ToUpper();

            // ponemos la letra en may˙scula
            aux = nif.Substring(0, nif.Length - 1);
            // quitamos la letra del NIF
            bool numero = DoubleUtil.IsNumeric(aux, NumberStyles.Integer);
            if (aux.Length >= 7 && numero)
                aux = CalculaNIF(aux); // calculamos la letra del NIF para comparar con la que tenemos
            else
                return false;
            // comparamos las letras
            if (nif == aux) correcte = true;
            else correcte = false;
            return correcte;
        }
        private static int SumaDigitos(Int32 digitos)
        {
            string sNumero = digitos.ToString();
            Int32 suma = 0;
            for (Int32 i = 0; i < sNumero.Length; i++)
            {
                Int32 aux;
                Int32.TryParse(sNumero[i].ToString(), out aux);
                suma += aux;
            }
            return suma;
        }

        private static String CalculaNIF(String strA)
        {
            const String cCADENA = "TRWAGMYFPDXBNJZSQVHLCKE";
            const String cNUMEROS = "0123456789";
            int a = 0;
            int b = 0;
            int c = 0;
            int NIF = 0;
            StringBuilder sb = new StringBuilder();
            strA = strA.Trim();
            if (strA.Length == 0) return "";
            // Dejar sÛlo los n˙meros
            for (int i = 0; i <= strA.Length - 1; i++)
                if (cNUMEROS.IndexOf(strA[i]) > -1) sb.Append(strA[i]);
            strA = sb.ToString();
            a = 0;
            NIF = Convert.ToInt32(strA);
            do
            {
                b = Convert.ToInt32((NIF / 24));
                c = NIF - (24 * b);
                a = a + c;
                NIF = b;
            } while (b != 0);

            b = Convert.ToInt32((a / 23));
            c = a - (23 * b);
            return strA.ToString() + cCADENA.Substring(c, 1);
        }
        public static bool ValidaCuentaBancaria(string cuentaCompleta)
        {
            // Comprobaciones de la cadena
            if (cuentaCompleta.Length != 20)
                throw new ArgumentException("El n˙mero de cuenta no el formato adecuado");

            string banco = cuentaCompleta.Substring(0, 4);
            string oficina = cuentaCompleta.Substring(4, 4);
            string dc = cuentaCompleta.Substring(8, 2);
            string cuenta = cuentaCompleta.Substring(10, 10);

            return CompruebaCuenta(banco, oficina, dc, cuenta);

        }

        private static bool CompruebaCuenta(string banco, string oficina, string dc, string cuenta)
        {
            return GetDigitoControl("00" + banco + oficina) + GetDigitoControl(cuenta) == dc;
        }

        private static string GetDigitoControl(string CadenaNumerica)
        {
            int[] pesos = { 1, 2, 4, 8, 5, 10, 9, 7, 3, 6 };
            UInt32 suma = 0;
            UInt32 resto;

            for (int i = 0; i < pesos.Length; i++)
            {
                suma += (UInt32)pesos[i] * UInt32.Parse(CadenaNumerica.Substring(i, 1));
            }
            resto = 11 - (suma % 11);

            if (resto == 10) resto = 1;
            if (resto == 11) resto = 0;

            return resto.ToString("0");
        }
        /*02/04/2013 aÒadido sergi
         comprueba que el correo electronico tenga un formato valido
         */
        public static Boolean comprobarMail(String email)
        {
            String expresion;
            expresion = "\\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*";
            if (Regex.IsMatch(email, expresion))
            {
                if (Regex.Replace(email, expresion, String.Empty).Length == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        public static string RemoveDiacritics(string stIn)
        {
            string stFormD = stIn.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();

            for (int ich = 0; ich < stFormD.Length; ich++)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(stFormD[ich]);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(stFormD[ich]);
                }
            }

            return (sb.ToString().Normalize(NormalizationForm.FormC));
        }
        public static string NormalizeLineBreaks(string input)
        {
            // Allow 10% as a rough guess of how much the string may grow.
            // If we're wrong we'll either waste space or have extra copies -
            // it will still work
            StringBuilder builder = new StringBuilder((int)(input.Length * 1.1));

            bool lastWasCR = false;

            foreach (char c in input)
            {
                if (lastWasCR)
                {
                    lastWasCR = false;
                    if (c == '\n')
                    {
                        continue; // Already written \r\n
                    }
                }
                switch (c)
                {
                    case '\r':
                        builder.Append("\r\n");
                        lastWasCR = true;
                        break;
                    case '\n':
                        builder.Append("\r\n");
                        break;
                    default:
                        builder.Append(c);
                        break;
                }
            }
            return builder.ToString();
        }

#endif
    }
    /// <summary>
    /// Provide utitilies about exception handling
    /// </summary>
    public class ExceptionUtil
    {
        /// <summary>
        /// Check if the provided exception is critical
        /// </summary>
        public static bool IsCritical(Exception ex)
        {
            if (ex is OutOfMemoryException)
                return true;
#if REPMAN_COMPACT
#else
			if (ex is AppDomainUnloadedException)
				return true;
			if (ex is BadImageFormatException)
				return true;
			if (ex is CannotUnloadAppDomainException)
				return true;
#endif
            if (ex is InvalidProgramException)
                return true;
            return false;
        }

    }
    /// <summary>
    /// Measurement units
    /// </summary>
    public enum Units
    {
        /// <summary>Inch measurement</summary>
        Inch,
        /// <summary>Centimeters measurement</summary>
        Cms
    };
    /// <summary>
    /// Provide utitilies and constants related to Twips measurement unit.
    /// One inch contains 1440 twips
    /// </summary>
    public class Twips
    {
        /// <summary>
        /// One inch measures 1440 twips
        /// </summary>
        public const int TWIPS_PER_INCH = 1440;
        /// <summary>
        /// One inch measures 2.54 centimeters
        /// </summary>
        public const decimal CMS_PER_INCH = 2.54M;
        /// <summary>
        /// One centimeter measures 1440/2.54 twips
        /// </summary>
        public const decimal TWIPS_PER_CM = TWIPS_PER_INCH / CMS_PER_INCH;
        /// <summary>
        /// Align to grid a point
        /// </summary>
        /// <param name="npoint"></param>
        /// <param name="gridx"></param>
        /// <param name="gridy"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static Point AlignToGridPixels(Point npoint, int gridx,int gridy,double scale)
        {
            npoint = new Point(PixelsToTwips(npoint.X, scale),
                 PixelsToTwips(npoint.Y, scale));
            npoint = new Point(((npoint.X+gridx/2) / gridx) * gridx, ((npoint.Y+gridy/2) / gridy) * gridy);
            return new Point(TwipsToPixels(npoint.X, scale), TwipsToPixels(npoint.Y, scale));
        }
        public static Point AlignToGridTwips(Point npoint, int gridx, int gridy)
        {
            npoint = new Point(((npoint.X+gridx/2) / gridx) * gridx, ((npoint.Y+gridy/2) / gridy) * gridy);
            return npoint;
        }
        public static int AlignToGridTwips(int nx, int gridx, int gridy)
        {
          nx = ((nx + gridx / 2) / gridx) * gridx;
          return nx;
        }
        public static int AlignToGridPixels(int x, int gridx, int gridy, double scale)
        {
          x = PixelsToTwips(x, scale);
          x = ((x + gridx / 2) / gridx) * gridx;
          return TwipsToPixels(x,scale);
        }
        /// <summary>
        /// Helper to convert Twips to centimeters
        /// </summary>
        public static decimal TwipsToCms(int atwips)
        {
            decimal at = atwips;
            return (at / TWIPS_PER_CM);
        }
        /// <summary>
        /// Helper to convert Twips to inch
        /// </summary>
        public static decimal TwipsToInch(int atwips)
        {
            decimal at = atwips;
            return (at / TWIPS_PER_INCH);
        }
        /// <summary>
        /// Helper to convert centimeters to Twips
        /// </summary>
        public static int CmsToTwips(decimal acms)
        {
            return ((int)Math.Round(TWIPS_PER_CM * acms));
        }
        /// <summary>
        /// Helper to convert inch to Twips 
        /// </summary>
        public static int InchToTwips(decimal ainch)
        {
            return ((int)Math.Round(TWIPS_PER_INCH * ainch));
        }
        /// <summary>
        /// Returns the abreviated description for a measurement unit
        /// </summary>
        /// <param name="unit">The unit type</param>
        /// <returns>A string</returns>
        public static string TranslateUnit(Units unit)
        {
            if (unit == Units.Cms)
                return Translator.TranslateStr(1437);
            else
                return Translator.TranslateStr(1436);
        }
        /// <summary>
        /// Returns the default measurement unit form current region as string
        /// </summary>
        public static string DefaultUnitString()
        {
            if (System.Globalization.RegionInfo.CurrentRegion.IsMetric)
                return TranslateUnit(Units.Cms);
            else
                return TranslateUnit(Units.Inch);
        }
        /// <summary>
        /// Returns the default measurement unit form current region
        /// </summary>
        public static Units DefaultUnit()
        {
            if (System.Globalization.RegionInfo.CurrentRegion.IsMetric)
                return Units.Cms;
            else
                return Units.Inch;
        }
        /// <summary>
        /// Returns a string in default measurement, converting twips to that unit
        /// </summary>
        public static string TextFromTwips(int twips)
        {
            return UnitsFromTwips(twips).ToString("##,##0.000");
        }
        /// <summary>
        /// Returns a twips value from a text, it uses the default measurement unit
        /// </summary>
        public static int TwipsFromText(string text)
        {
            decimal floatvalue = System.Convert.ToDecimal(text);
            return TwipsFromUnits(floatvalue);
        }
        /// <summary>
        /// Returns a double value, converting twips to default unit
        /// </summary>
        public static decimal UnitsFromTwips(int twips)
        {
            if (DefaultUnit() == Units.Cms)
                return Twips.TwipsToCms(twips);
            else
                return Twips.TwipsToInch(twips);
        }
        /// <summary>
        /// Returns a double value, converting the default unit to twips
        /// </summary>
        public static int TwipsFromUnits(decimal measure)
        {
            if (DefaultUnit() == Units.Cms)
                return Twips.CmsToTwips(measure);
            else
                return Twips.InchToTwips(measure);
        }
        private static object flag = 1;
        private static float ScreenDPI = 0.0F;
        /// <summary>
        /// Returns number of pixels (uses screen pixels per inch) from a twips measure
        /// scaled
        /// </summary>
        /// <param name="twips"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static int TwipsToPixels(int twips, double scale)
        {
            Monitor.Enter(flag);
            try
            {
                if (ScreenDPI == 0.0F)
                {
                    using (System.Drawing.Bitmap bm = new System.Drawing.Bitmap(5, 5))
                    {
                        using (System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(bm))
                        {
                            ScreenDPI = gr.DpiX;
                            if (ScreenDPI == 0)
                                ScreenDPI = 96.0F;
                        }
                    }
                }
            }
            finally
            {
                Monitor.Exit(flag);
            }

            return (int)Math.Round(((double)twips / TWIPS_PER_INCH) * ScreenDPI * scale);
        }
        /// <summary>
        /// Returns number of twips from a scaled pixels measure (uses screen pixels per inch)
        /// </summary>
        /// <param name="twips"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static int PixelsToTwips(int pixels, double scale)
        {
            Monitor.Enter(flag);
            try
            {
                if (ScreenDPI == 0.0F)
                {
                    using (System.Drawing.Bitmap bm = new System.Drawing.Bitmap(5, 5))
                    {
                        using (System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(bm))
                        {
                            ScreenDPI = gr.DpiX;
                            if (ScreenDPI == 0)
                                ScreenDPI = 96.0F;
                        }
                    }
                }
            }
            finally
            {
                Monitor.Exit(flag);
            }

            return (int)Math.Round(((double)pixels * TWIPS_PER_INCH) / (ScreenDPI * scale));
        }
    }
    /// <summary>
    /// Provide utitilies for handling streams, write strings, read/write
    /// datatypes etc.
    /// </summary>
    public class StreamUtil
    {
#if PocketPC
#else
        public static string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
        {
            string[] searchPatterns = searchPattern.Split('|');
            Strings files = new Strings();
            foreach (string sp in searchPatterns)
            {
               string[] nresult = System.IO.Directory.GetFiles(path, sp, searchOption);
                foreach (string nfile in nresult)
                {
                    files.Add(nfile);
                }
            }
            return files.ToArray();
        }
#endif


        public static byte[] LFarray = { 13, 10 };
        /// <summary>
        /// Check for a file if in use
        /// return true
        /// </summary>
        public static bool FileInUse(string path)
        {
            if (!File.Exists(path))
                return false;
            try
            {
                //Just opening the file as open/create
#if PocketPC
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Write))
                {
                    //If required we can check for read/write by using fs.CanRead or fs.CanWrite
                }
#else
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Write, FileShare.None))
                {
                    //If required we can check for read/write by using fs.CanRead or fs.CanWrite
                }
#endif
                return false;
            }
            catch
            {
                return true;
            }
        }

        /// <summary>
        /// Write a string to a stream, the string should contain only
        /// single byte characters
        /// </summary>
        public static void SWriteLine(Stream astream, string astring)
        {
            //astring = astring + (char)13 + (char)10;
            byte[] buf = new byte[astring.Length];
            for (int i = 0; i < astring.Length; i++)
                buf[i] = (byte)astring[i];
            astream.Write(buf, 0, astring.Length);

            astream.Write(LFarray, 0, 2);
            /*
            byte[] buf = ASCIIEncoding.ASCII.GetBytes(astring+(char)13+(char)10);
            astream.Write(buf, 0, buf.Length);*/
        }
        /// <summary>
        /// Default buffer size for buffered applications
        /// </summary>
        public const int DEFAULT_BUFFER_SIZE = 65535;


        /// <summary>
        /// Generates a MemoryStream from any Stream input
        /// </summary>
        /// <param name="astream">Source stream</param>
        /// <param name="bufsize">
        /// Buffer size can be specified size for performance enhacement
        /// when handling long streams.
        /// You can specify 0 value, so the DEFAULT_BUFFER_SIZE (65535) will
        /// be used.
        /// Note that standard input does not allow reading long segments
        /// so keep the default value if you don't know the source stream
        /// procedence
        /// </param>
        /// <returns>The resulting memory stream</returns>
        static public MemoryStream StreamToMemoryStream(Stream astream, int bufsize)
        {
            // Don't increase BUF_SIZE, standerd input
            if (bufsize == 0)
                bufsize = 65535;
            byte[] buf = new byte[bufsize];

            MemoryStream aresult = new MemoryStream();
            int readed;
            readed = astream.Read(buf, 0, bufsize);
            while (readed > 0)
            {
                aresult.Write(buf, 0, readed);
                readed = astream.Read(buf, 0, bufsize);
            }
            return aresult;
        }
        /// <summary>
        /// Generates a MemoryStream from any Stream input
        /// </summary>
        static public MemoryStream StreamToMemoryStream(Stream astream)
        {
            // Don't increase BUF_SIZE, standerd input
            return StreamToMemoryStream(astream, 0);
        }
        /// <summary>
        /// Generates a MemoryStream from a file
        /// </summary>
        static public MemoryStream FileToMemoryStream(string filename)
        {
            MemoryStream rstream = null;
            // Don't increase BUF_SIZE, standerd input
            using (FileStream fstream = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                rstream = StreamToMemoryStream(fstream, 0);
            }
            return rstream;
        }
        /// <summary>
        /// Generate a file from a memory stream
        /// </summary>
        static public void MemoryStreamToFile(MemoryStream memstream, string filename)
        {
            memstream.Seek(0, SeekOrigin.Begin);
            // Don't increase BUF_SIZE, standerd input
            using (FileStream fstream = new FileStream(filename, FileMode.Create, FileAccess.Write))
            {
                memstream.WriteTo(fstream);
            }
        }

        /// <summary>
        /// Writes a string to a stream formatted as multibyte UFT8 encondig
        /// </summary>
        static public int WriteStringToUTF8Stream(string astring, Stream astream)
        {
            int len = astring.Length;
            if (len == 0)
                return 0;
            UTF8Encoding utf8 = new UTF8Encoding();
            byte[] abuf = utf8.GetBytes(astring);
            astream.Write(abuf, 0, abuf.Length);
            return abuf.Length;
        }
        /// <summary>
        /// Writes a string to a stream formatted as multibyte UFT8 encondig
        /// </summary>
        static public int WriteStringToStream(string astring, Stream astream,System.Text.Encoding nencoding)
        {
            int len = astring.Length;
            if (len == 0)
                return 0;
            byte[] abuf = nencoding.GetBytes(astring);
            astream.Write(abuf, 0, abuf.Length);
            return abuf.Length;
        }

        /// <summary>
        /// Writes a string to a stream formatted as multibyte UFT8 encondig
        /// </summary>
        static public int WriteCharArrayToStream(char[] chars, int len, Stream astream)
        {
            byte[] abuf = new byte[2];
            if (len == 0)
                return 0;
            for (int i = 0; i < len; i++)
            {
                abuf[0] = (byte)chars[i];
                abuf[1] = (byte)(((int)chars[i]) >> 8);
                astream.Write(abuf, 0, 2);
            }
            return len;
        }
        /// <summary>
        /// Transforms stream content to string representation as hexadecimal bytes
        /// </summary>
        public static string StreamToHex(Stream astream)
        {
            StringBuilder astring = new StringBuilder();

            const int BUFSIZE = 120000;

            byte[] buf = new byte[BUFSIZE];
            byte[] dest = new byte[BUFSIZE * 2];
            long readed;
            readed = astream.Read(buf, 0, BUFSIZE);
            while (readed > 0)
            {
                for (int i = 0; i < readed; i++)
                    astring.Append(StringUtil.ByteToHex(buf[i]));
                readed = astream.Read(buf, 0, BUFSIZE);
            }
            return astring.ToString();
        }
        /// <summary>
        /// Converts an int to a byte array
        /// </summary>
        public static byte[] IntToByteArray(int avalue)
        {
            byte[] aresult = new byte[4];
            aresult[0] = (byte)avalue;
            avalue = avalue >> 8;
            aresult[1] = (byte)avalue;
            avalue = avalue >> 8;
            aresult[2] = (byte)avalue;
            avalue = avalue >> 8;
            aresult[3] = (byte)avalue;
            return aresult;
        }
        /// <summary>
        /// Converts an int to a byte array
        /// </summary>
        public static byte[] Int64ToByteArray(long avalue)
        {
          byte[] aresult = new byte[8];
          aresult[0] = (byte)avalue;
          avalue = avalue >> 8;
          aresult[1] = (byte)avalue;
          avalue = avalue >> 8;
          aresult[2] = (byte)avalue;
          avalue = avalue >> 8;
          aresult[3] = (byte)avalue;
          avalue = avalue >> 8;
          aresult[4] = (byte)avalue;
          avalue = avalue >> 8;
          aresult[5] = (byte)avalue;
          avalue = avalue >> 8;
          aresult[6] = (byte)avalue;
          avalue = avalue >> 8;
          aresult[7] = (byte)avalue;
          return aresult;
        }
        /// <summary>
        /// Converts a short to a byte array
        /// </summary>
        public static byte[] ShortToByteArray(int avalue)
        {
            byte[] aresult = new byte[2];
            aresult[0] = (byte)avalue;
            avalue = avalue >> 8;
            aresult[1] = (byte)avalue;
            avalue = avalue >> 8;
            return aresult;
        }
        /// <summary>
        /// Converts a bool to a byte array
        /// </summary>
        public static byte[] BoolToByteArray(bool avalue)
        {
            byte[] aresult = new byte[1];
            if (avalue)
                aresult[0] = 1;
            else
                aresult[0] = 0;
            return aresult;
        }
        /// <summary>
        /// Converts a string (not unicode) to a byte array
        /// the array is created with the first byte of the char value
        /// only ANSI alowed
        /// </summary>
        public static byte[] StringToByteArray(string avalue, int length)
        {
           byte[] aresult = new byte[length];
            for (int i = 0; i < length; i++)
                aresult[i] = 0;
            if (avalue == null)
                return aresult;
            int alen = avalue.Length;
            if (alen > length)
                alen = length;
            for (int i = 0; i < alen; i++)
                aresult[i] = (byte)avalue[i];
            return aresult;
        }
        /// <summary>
        /// Converts a string (not unicode) to a byte array
        /// </summary>
        public static byte[] StringToByteArray(string avalue, int length,int codepage)
        {
            /*            byte[] aresult = new byte[length];
                        for (int i = 0; i < length; i++)
                            aresult[i] = 0;
                        if (avalue == null)
                            return aresult;
                        int alen = avalue.Length;
                        if (alen > length)
                            alen = length;
                        for (int i = 0; i < alen; i++)
                            aresult[i] = (byte)avalue[i];
                        return aresult;*/
            Encoding encoder = Encoding.GetEncoding(codepage);
            return encoder.GetBytes(avalue);
        }
        /// <summary>
        /// Converts a string to a byte array, choosing unicode or not
        /// </summary>
        public static byte[] StringToByteArray(string avalue, int length, bool unicode)
        {
            if (!unicode)
                return StringToByteArray(avalue, length);
            UTF8Encoding encoder = new UTF8Encoding();
            return encoder.GetBytes(avalue.ToCharArray(), 0, length);
        }
        /// <summary>
        /// Converts a byte to a byte array
        /// </summary>
        public static byte[] ByteToByteArray(byte avalue)
        {
            byte[] aresult = new byte[1];
            aresult[0] = avalue;
            return aresult;
        }
        /// <summary>
        /// Converts a byte array to an int value
        /// </summary>
        public static int ByteArrayToInt(byte[] b1, int index, int alen)
        {
            int aresult = 0;
            switch (alen)
            {
                case 0:
                    aresult = 0;
                    break;
                case 1:
                    aresult = (int)b1[index + 0];
                    break;
                case 2:
                    aresult = (int)b1[index + 0] + (((int)b1[index + 1]) << 8);
                    break;
                case 3:
                    aresult = (int)b1[index + 0] + ((int)b1[index + 1]) * 0xFF + ((int)b1[index + 2]) * 0xFFFF;
                    break;
                default:
                    aresult = (int)b1[index + 0] + (((int)b1[index + 1]) << 8) +
                        (((int)b1[index + 2]) << 16) + (((int)b1[index + 3]) << 24);
                    break;
            }
            return (aresult);

        }
        /// <summary>
        /// Converts a byte array to an int64  value
        /// </summary>
        public static Int64 ByteArrayToInt64(byte[] b1, int index, int alen)
        {
          Int64 aresult = 0;
          switch (alen)
          {
            case 0:
              aresult = 0;
              break;
            case 1:
              aresult = (int)b1[index + 0];
              break;
            case 2:
              aresult = (int)b1[index + 0] + (((int)b1[index + 1]) << 8);
              break;
            case 3:
              aresult = (int)b1[index + 0] + ((int)b1[index + 1]) * 0xFF + ((int)b1[index + 2]) * 0xFFFF;
              break;
            case 4:
              aresult = (int)b1[index + 0] + (((int)b1[index + 1]) << 8) +
                  (((int)b1[index + 2]) << 16) + (((int)b1[index + 3]) << 24);
              break;
            case 5:
              aresult = (Int64)b1[index + 0] + (((Int64)b1[index + 1]) << 8) +
                  (((Int64)b1[index + 2]) << 16) + (((Int64)b1[index + 3]) << 24) +
                  (((Int64)b1[index + 4]) << 32);
              break;
            case 6:
              aresult = (Int64)b1[index + 0] + (((Int64)b1[index + 1]) << 8) +
                  (((Int64)b1[index + 2]) << 16) + (((Int64)b1[index + 3]) << 24) +
                  (((Int64)b1[index + 4]) << 32) + (((Int64)b1[index + 5]) << 40);
              break;
            case 7:
              aresult = (Int64)b1[index + 0] + (((Int64)b1[index + 1]) << 8) +
                  (((Int64)b1[index + 2]) << 16) + (((Int64)b1[index + 3]) << 24) +
                  (((Int64)b1[index + 4]) << 32) + (((Int64)b1[index + 5]) << 40) +
                  (((Int64)b1[index + 6]) << 48);
              break;
            case 8:
              aresult = (Int64)b1[index + 0] + (((Int64)b1[index + 1]) << 8) +
                  (((Int64)b1[index + 2]) << 16) + (((Int64)b1[index + 3]) << 24) +
                  (((Int64)b1[index + 4]) << 32) + (((Int64)b1[index + 5]) << 40) +
                  (((Int64)b1[index + 6]) << 48) + (((Int64)b1[index + 7]) << 56);
              break;
            default:
              throw new Exception("Not supported Int64, alen=" + alen.ToString());
          }
          return (aresult);

        }
        /// <summary>
        /// Converts a byte array to an uint value
        /// </summary>
        public static uint ByteArrayToUInt(byte[] b1, int index, int alen)
        {
            uint aresult = 0;
            switch (alen)
            {

                case 0:
                    aresult = 0;
                    break;
                case 1:
                    aresult = (uint)b1[index + 0];
                    break;
                case 2:
                    aresult = (uint)b1[index + 0] + (((uint)b1[index + 1]) << 8);
                    break;
                case 3:
                    aresult = (uint)b1[index + 0] + ((uint)b1[index + 1]) * 0xFF + ((uint)b1[index + 2]) * 0xFFFF;
                    break;
                default:
                    aresult = (uint)b1[index + 0] + (((uint)b1[index + 1]) << 8) +
                        (((uint)b1[index + 2]) << 16) + (((uint)b1[index + 3]) << 24);
                    break;
            }
            return (aresult);

        }
        /// <summary>
        /// Converts a byte array to an ushort value
        /// </summary>
        public static ushort ByteArrayToUShort(byte[] b1, int index, int alen)
        {
            ushort aresult = 0;
            switch (alen)
            {

                case 0:
                    aresult = 0;
                    break;
                case 1:
                    aresult = (ushort)b1[index + 0];
                    break;
                default:
                    aresult = (ushort)((ushort)b1[index + 0] + (ushort)(((ushort)b1[index + 1]) << 8));
                    break;
            }
            return (aresult);

        }
        /// <summary>
        /// Converts a byte array to an int value
        /// </summary>
        public static int ByteArrayToInt(byte[] b1, int alen)
        {
            return ByteArrayToInt(b1, 0, alen);
        }
        /// <summary>
        /// Converts a byte array to a short value
        /// </summary>
        public static short ByteArrayToShort(byte[] b1, int alen)
        {
            return (short)ByteArrayToInt(b1, 0, alen);
        }
        /// <summary>
        /// Converts a byte array to a short value
        /// </summary>
        public static short ByteArrayToShort(byte[] b1, int index, int alen)
        {
            return (short)ByteArrayToInt(b1, index, alen);
        }
        /// <summary>
        /// Converts a byte array to a long value
        /// </summary>
        public static long ByteArrayToLong(byte[] b1, int alen)
        {
            return (ByteArrayToLong(b1, 0, alen));

        }
        /// <summary>
        /// Compare the content of two byte arrays, returns true if the content is the same
        /// </summary>
        public static bool CompareArrayContent(byte[] b1, byte[] b2)
        {
            if (b1.Length != b2.Length)
                return false;
            bool aresult = true;
            for (int i = 0; i < b1.Length; i++)
            {
                if (b1[i] != b2[i])
                {
                    aresult = false;
                    break;
                }
            }
            return aresult;
        }
        /// <summary>
        /// Converts a long value to a byte array to a long value
        /// </summary>
        public static byte[] LongToByteArray(long avalue)
        {
            byte[] aresult = new byte[8];
            for (int i = 0; i < 8; i++)
            {
                aresult[i] = (byte)avalue;
                avalue = avalue >> 8;
            }
            return aresult;
        }
        /// <summary>
        /// Converts a byte array to a long value
        /// </summary>
        public static long ByteArrayToLong(byte[] b1, int index, int alen)
        {
            long aresult = 0;
            long int1 = (long)ByteArrayToInt(b1, index, 4);
            long int2 = (long)ByteArrayToInt(b1, index + 4, 4);
            aresult = int1 + (int2 << 32);
            return (aresult);

        }
        /// <summary>
        /// Converts a byte array to a string (not unicode)
        /// </summary>
        public static string ByteArrayToString(byte[] b1, int alen)
        {
            StringBuilder aresult = new StringBuilder(alen);
            for (int i = 0; i < b1.Length; i++)
                aresult.Append((char)b1[i]);
            return aresult.ToString();
        }
        /// <summary>
        /// Converts a byte array to a string (not unicode)
        /// </summary>
        public static string ByteArrayToString(byte[] b1, int nindex,int alen)
        {
            StringBuilder aresult = new StringBuilder();
            for (int i = 0; i < alen; i++)
                aresult.Append((char)b1[i+nindex]);
            return aresult.ToString();
        }
        /// <summary>
        /// Converts a byte array to a string, choosing unicode
        /// </summary>
        public static string ByteArrayToString(byte[] b1, int alen, bool unicode)
        {
            if (!unicode)
                return ByteArrayToString(b1, alen);
            UTF8Encoding nencode = new UTF8Encoding();

            return nencode.GetString(b1, 0, alen);

        }
        /// <summary>
        /// Converts a byte array to a MemoryStream
        /// </summary>
        public static MemoryStream ByteArrayToStream(byte[] b1)
        {
            MemoryStream nstream = new MemoryStream();
            int len = b1.Length;
            nstream.Write(b1, 0, len);
            nstream.Seek(0, SeekOrigin.Begin);
            return nstream;
        }
        /// <summary>
        /// Check if the stream is a compressed stream
        /// </summary>
        static public bool IsCompressed(MemoryStream mems)
        {
            bool aresult = false;
            mems.Seek(0, System.IO.SeekOrigin.Begin);
            byte[] buf = new byte[1];
            if (mems.Read(buf, 0, 1) > 0)
            {
                if (buf[0] == 'x')
                    aresult = true;
            }
            return aresult;
        }
        /// <summary>
        /// Check if the stream is a compressed stream
        /// </summary>
        static public bool IsCompressed(byte[] buf)
        {
            if (buf.Length == 0)
                return false;
            return (buf[0] == 'x');
        }
#if REPMAN_ZLIB
        /// <summary>
        /// Dercompress the first memory stream into the second memory stream
        /// </summary>
        static public void DeCompressStream(MemoryStream memstream,
            MemoryStream dest)
        {
            //#if PocketPC
            //            int bufsize = 10000;
            //#else
            int bufsize = 100000;
            //#endif

            //dest.Capacity = 0;
            byte[] bufuncomp = new byte[bufsize];
            ICSharpCode.SharpZipLib.Zip.Compression.Inflater inf = new ICSharpCode.SharpZipLib.Zip.Compression.Inflater();
            ICSharpCode.SharpZipLib.Zip.Compression.Streams.InflaterInputStream zstream = new ICSharpCode.SharpZipLib.Zip.Compression.Streams.InflaterInputStream(memstream, inf);
            int readed = zstream.Read(bufuncomp, 0, bufsize);
            while (readed > 0)
            {
                dest.Write(bufuncomp, 0, readed);
                readed = zstream.Read(bufuncomp, 0, bufsize);
            }
            dest.Seek(0, System.IO.SeekOrigin.Begin);
        }
        /// <summary>
        /// Dercompress the first memory stream into the second memory stream
        /// </summary>
        static public void DeCompressBuffer(byte[] buffer, int count,
            Stream dest,ProgressEvent progevent)
        {
            //#if PocketPC
            //            int bufsize = 10000;
            //#else
            int bufsize = 100000;
            //#endif

            //dest.Capacity = 0;
            byte[] bufuncomp = new byte[bufsize];
            ICSharpCode.SharpZipLib.Zip.Compression.Inflater inf = new ICSharpCode.SharpZipLib.Zip.Compression.Inflater();
            inf.SetInput(buffer, 0, count);
            int readed = inf.Inflate(bufuncomp, 0, bufsize);
            while (readed > 0)
            {
                dest.Write(bufuncomp, 0, readed);
                readed = inf.Inflate(bufuncomp, 0, bufsize);
                if (progevent!=null)
                {
                    progevent(dest,new ProgressArgs(dest.Length,dest.Length));
                }
            }
            dest.Seek(0, System.IO.SeekOrigin.Begin);
        }
        /// <summary>
        /// Dercompress the first memory stream into the second memory stream
        /// </summary>
        static public void DeCompressBuffer(byte[] buffer,int count,
            Stream dest)
        {
            // Check if it's a GZip stream

            
            //#if PocketPC
            //            int bufsize = 10000;
            //#else
            int bufsize = 100000;
            int readed = 0;
            //#endif

            //dest.Capacity = 0;
            byte[] bufuncomp = new byte[bufsize];
            if ((buffer[0] == 31) && (buffer[1] == 139))
            {
                using (MemoryStream mstream = new MemoryStream(buffer,0,count))
                {
                    ICSharpCode.SharpZipLib.GZip.GZipInputStream nstream = new ICSharpCode.SharpZipLib.GZip.GZipInputStream(mstream);
                    readed = nstream.Read(bufuncomp, 0, bufsize);
                    while (readed > 0)
                    {
                        dest.Write(bufuncomp, 0, readed);
                        readed = nstream.Read(bufuncomp, 0, bufsize);
                    }
                    dest.Seek(0, System.IO.SeekOrigin.Begin);
                    
                }
            }
            else
            {


                ICSharpCode.SharpZipLib.Zip.Compression.Inflater inf = new ICSharpCode.SharpZipLib.Zip.Compression.Inflater();
                inf.SetInput(buffer, 0, count);
                readed = inf.Inflate(bufuncomp, 0, bufsize);
                while (readed > 0)
                {
                    dest.Write(bufuncomp, 0, readed);
                    readed = inf.Inflate(bufuncomp, 0, bufsize);
                }
                dest.Seek(0, System.IO.SeekOrigin.Begin);
            }
        }
#endif
#if REPMAN_ZLIB
        /// <summary>
        /// Compress the first memory stream into the second memory stream
        /// </summary>
        static public void CompressStream(MemoryStream memstream,
            MemoryStream dest)
        {
            byte[] bufuncomp = new byte[100000];
            ICSharpCode.SharpZipLib.Zip.Compression.Deflater inf = new ICSharpCode.SharpZipLib.Zip.Compression.Deflater(ICSharpCode.SharpZipLib.Zip.Compression.Deflater.BEST_SPEED);
            ICSharpCode.SharpZipLib.Zip.Compression.Streams.DeflaterOutputStream zstream = new ICSharpCode.SharpZipLib.Zip.Compression.Streams.DeflaterOutputStream(dest, inf, 131072);
            int readed = memstream.Read(bufuncomp, 0, 100000);
            while (readed > 0)
            {
                zstream.Write(bufuncomp, 0, readed);
                readed = memstream.Read(bufuncomp, 0, 100000);
            }
            zstream.Finish();
        }
        /// <summary>
        /// Compress the first memory stream into the second memory stream using Gzip
        /// </summary>
        static public void CompressStreamGZip(MemoryStream memstream,
            MemoryStream dest)
        {
            byte[] bufuncomp = new byte[100000];
            ICSharpCode.SharpZipLib.GZip.GZipOutputStream zstream = new ICSharpCode.SharpZipLib.GZip.GZipOutputStream(dest);
            int readed = memstream.Read(bufuncomp, 0, 100000);
            while (readed > 0)
            {
                zstream.Write(bufuncomp, 0, readed);
                readed = memstream.Read(bufuncomp, 0, 100000);
            }
            zstream.Finish();
        }
        /// <summary>
        /// Compress the first memory stream into the second memory stream, option for optimize size
        /// </summary>
        static public void CompressStream(MemoryStream memstream,
            MemoryStream dest,bool optimizesize)
        {
            int level = ICSharpCode.SharpZipLib.Zip.Compression.Deflater.BEST_SPEED;
            if (optimizesize)
            level = ICSharpCode.SharpZipLib.Zip.Compression.Deflater.BEST_COMPRESSION;

            byte[] bufuncomp = new byte[100000];
            ICSharpCode.SharpZipLib.Zip.Compression.Deflater inf = new ICSharpCode.SharpZipLib.Zip.Compression.Deflater(level);
            ICSharpCode.SharpZipLib.Zip.Compression.Streams.DeflaterOutputStream zstream = new ICSharpCode.SharpZipLib.Zip.Compression.Streams.DeflaterOutputStream(dest, inf, 131072);
            int readed = memstream.Read(bufuncomp, 0, 100000);
            while (readed > 0)
            {
                zstream.Write(bufuncomp, 0, readed);
                readed = memstream.Read(bufuncomp, 0, 100000);
            }
            zstream.Finish();
        }
        static public void CompressFile(string source, string destination)
        {
            using (FileStream fstream = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                using (FileStream dest = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    byte[] bufuncomp = new byte[100000];
                    ICSharpCode.SharpZipLib.Zip.Compression.Deflater inf = new ICSharpCode.SharpZipLib.Zip.Compression.Deflater();
                    ICSharpCode.SharpZipLib.Zip.Compression.Streams.DeflaterOutputStream zstream = new ICSharpCode.SharpZipLib.Zip.Compression.Streams.DeflaterOutputStream(dest, inf, 131072);
                    int readed = fstream.Read(bufuncomp, 0, 100000);
                    while (readed > 0)
                    {
                        zstream.Write(bufuncomp, 0, readed);
                        readed = fstream.Read(bufuncomp, 0, 100000);
                    }
                    zstream.Finish();
                }
            }
        }
        static public void DeCompressFile(string source, string destination)
        {
            using (FileStream fstream = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                using (FileStream dest = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    byte[] bufuncomp = new byte[100000];
                    ICSharpCode.SharpZipLib.Zip.Compression.Inflater inf = new ICSharpCode.SharpZipLib.Zip.Compression.Inflater();
                    ICSharpCode.SharpZipLib.Zip.Compression.Streams.InflaterInputStream zstream = new ICSharpCode.SharpZipLib.Zip.Compression.Streams.InflaterInputStream(fstream, inf);
                    int readed = zstream.Read(bufuncomp, 0, 100000);
                    while (readed > 0)
                    {
                        dest.Write(bufuncomp, 0, readed);
                        readed = zstream.Read(bufuncomp, 0, 100000);
                    }
                }
            }
        }
#endif
    }
    /// <summary>
    /// Common exception, providing a message for information
    /// </summary>
    public class UnNamedException : System.Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public UnNamedException(string amessage)
            : base(amessage)
        {
        }
    }
    /// <summary>
    /// Common exception, providing a name as additional exception information
    /// </summary>
    public class NamedException : System.Exception
    {
        private string FName;
        /// <summary>
        /// Provide aditional information, usually the name of the component throwing the exception
        /// </summary>
        public string Name
        {
            get { return FName; }
        }
        /// <summary>
        /// Constructor
        /// </summary>
        public NamedException(string amessage, string name)
            : base(amessage)
        {
            FName = name;
        }
    }
    /// <summary>
    /// Custom exception to be controlled,when the report is empty
    /// </summary>
    public class NoDataToPrintException : System.Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public NoDataToPrintException(string amessage)
            : base(amessage)
        {
        }
    }
    /// <summary>
    /// DataTable and DataSet utilities
    /// </summary>
    public class DataUtilities
    {
        /// <summary>
        /// Copy a DataTable, the standard Copy() command does not work for byte[] columns
        /// </summary>
        /// <param name="ntable"></param>
        /// <returns></returns>
        public static DataTable Copy(DataTable ntable)
        {
            DataTable newtable = ntable.Clone();

            int colcount = newtable.Columns.Count;
            object[] values = new object[colcount];
            foreach (DataRow xrow in ntable.Rows)
            {
                for (int i = 0; i < colcount; i++)
                    values[i] = xrow[i];
                newtable.Rows.Add(values);
            }
            return newtable;
        }
    }
    /// <summary>
    /// Process utilities
    /// </summary>
    public class ProcessUtil
    {
        /// <summary>
        /// Returns true if the process is running in 32bits mode
        /// </summary>
        public static bool Is64BitProcess
        {
            get { return IntPtr.Size == 8; }
        }

    }
    public class GS1_128
    {
        public DateTime ExpirationDate = DateTime.MinValue;
        public DateTime SellByDate = DateTime.MinValue;
        public string SSCC = "";
        public string SSCC_Company = "";
        public string SSCC_Number = "";
        public string ProductCode = "";
        public DateTime ProductionDate = DateTime.MinValue;
        public DateTime PackagingDate = DateTime.MinValue;
        public string ProductVariant = "";
        public string SerialNumber = "";
        public string HIBCC = "";
        public string LotNumber = "";
        public string SecondSerialNumber = "";
        public decimal Quantity = 0.0m;
        public decimal ProductVolume = 0.0m;
        public decimal ContainerGrossWeight = 0.0m;
        public string CustomerPurchaseNumber = "";
        public string BatchNumber = "";
        public string Mutual = "";
        public string InternalCode = "";
        // 31
        public decimal ProductNetWeightKg = 0m;
        public decimal ProductLengthMeters = 0m;
        public decimal ProductWidthMeters = 0m;
        public decimal ProductDepthMeters = 0m;
        public decimal ProductAreaMeters2 = 0m;
        public decimal ProductNetVolumeLiters = 0m;
        public decimal ProductNetVolumeMeters3 = 0m;
        // 32
        public decimal ProductNetWeightPounds = 0m;
        public decimal ProductLengthInch = 0m;
        public decimal ProductLengthFeet = 0m;
        public decimal ProductLengthYard = 0m;
        public decimal ProductWidthInch = 0m;
        public decimal ProductWidthFeet = 0m;
        public decimal ProductWidthYard = 0m;
        public decimal ProductDepthInch = 0m;
        public decimal ProductDepthFeet = 0m;
        public decimal ProductDepthYard = 0m;
        // 33
        public decimal ContainerGrossWeightKg = 0m;
        public decimal ContainerLengthMeters = 0m;
        public decimal ContainerWidthMeters = 0m;
        public decimal ContainerDepthMeters = 0m;
        public decimal ContainerAreaMeters2 = 0m;
        public decimal ContainerGrossVolumeLiters = 0m;
        public decimal ContainerGrossVolumeMeters3 = 0m;
        // 34
        public decimal ContainerGrossWeightPounds = 0m;
        public decimal ContainerLengthInch = 0m;
        public decimal ContainerLengthFeet = 0m;
        public decimal ContainerLengthYard = 0m;
        public decimal ContainerWidthInch = 0m;
        public decimal ContainerWidthFeet = 0m;
        public decimal ContainerWidthYard = 0m;
        public decimal ContainerDepthInch = 0m;
        public decimal ContainerDepthFeet = 0m;
        public decimal ContainerDepthYard = 0m;
        // 35
        public decimal ProductAreaInch2 = 0m;
        public decimal ProductAreaFeet2 = 0m;
        public decimal ProductAreaYard2 = 0m;
        public decimal ContainerAreaInch2 = 0m;
        public decimal ContainerAreaFeet2 = 0m;
        public decimal ContainerAreaYard2 = 0m;
        public decimal NetWeightTroyOunces = 0m;
        public decimal NetWeightOunces = 0m;
        // 36
        public decimal ProductVolumeQuarts = 0m;
        public decimal ProductVolumeGallons = 0m;
        public decimal ContainerVolumeQuarts = 0m;
        public decimal ContainerVolumeGallons = 0m;
        public decimal ProductVolumeInch3 = 0m;
        public decimal ProductVolumeFeet3 = 0m;
        public decimal ProductVolumeYard3 = 0m;
        public decimal ContainerVolumeInch3 = 0m;
        public decimal ContainerVolumeFeet3 = 0m;
        public decimal ContainerVolumeYard3 = 0m;




        public decimal DecodeNumber(string nbarcode, ref int idx)
        {
            if (!(char.IsDigit(nbarcode[idx])))
                throw new Exception("Invalid digit number DecodeNumber 128");
            int decimalplaces = System.Convert.ToInt32(nbarcode[idx] + "");
            if (decimalplaces > 6)
                throw new Exception("Invalid digit number decimal places DecodeNumber 128");
            idx++;
            decimal intpart = 0;
            if (decimalplaces == 0)
            {
                intpart = System.Convert.ToDecimal(nbarcode.Substring(idx, 6));
            }
            else
            {
                if (decimalplaces > 4)
                    decimalplaces = 4;
                intpart = System.Convert.ToDecimal(nbarcode.Substring(idx, 6 - decimalplaces));
                decimal decpart = System.Convert.ToDecimal(nbarcode.Substring(idx + 6 - decimalplaces, decimalplaces));
                while (decimalplaces > 0)
                {
                    decpart = decpart / 10.0m;
                    decimalplaces--;
                }
                intpart = intpart + decpart;
            }
            idx = idx + 6;
            return intpart;
        }
        public string Advance(string nbarcode, ref int idx)
        {
            StringBuilder nresult = new StringBuilder();
            for (; idx < nbarcode.Length; idx++)
            {
                if (nbarcode[idx] == (char)29)
                {
                    idx++;
                    break;
                }
                else
                    nresult.Append(nbarcode[idx]);
            }
            return nresult.ToString();
        }
        public void Decode(string nbarcode)
        {
            int idx = 0;
            string previousprefix = "";
            while (idx < nbarcode.Length-1)
            {
                string prefix2 = nbarcode.Substring(idx,2);
                string prefix3 = nbarcode.Substring(idx,3);
                switch (prefix2)
                {
                    case "00":
                        SSCC = nbarcode.Substring(idx + 2, 18);
                        SSCC_Company = SSCC.Substring(1,8);
                        SSCC_Number = SSCC.Substring(9,9);
                        idx = idx + 20;
                        break;
                    case "01":
                    case "02":
                        ProductCode = nbarcode.Substring(idx + 2, 14);
                        idx = idx + 16;
                        break;
                    case "10":
                        idx = idx + 2;
                        BatchNumber = Advance(nbarcode, ref idx);;
                        
                        break;
                    case "11":
                        ProductionDate = DecodeDate(nbarcode, idx + 2);
                        idx = idx + 8;
                        break;
                    case "13":
                        PackagingDate = DecodeDate(nbarcode, idx + 2);
                        idx = idx + 8;
                        break;
                    case "15":
                        SellByDate = DecodeDate(nbarcode, idx + 2);
                        idx = idx + 8;
                        break;
                    case "17":
                        ExpirationDate = DecodeDate(nbarcode, idx + 2);
                        idx = idx + 8;
                        break;
                    case "20":
                        ProductVariant = nbarcode.Substring(idx + 2,2);
                        idx = idx + 4;
                        break;
                    case "21":
                        idx = idx + 2;
                        SerialNumber = Advance(nbarcode, ref idx);;
                        

                        break;
                    case "37":
                        idx = idx + 2;
                        string numberunits = Advance(nbarcode, ref idx);
                        if (DoubleUtil.IsNumeric(numberunits,System.Globalization.NumberStyles.Number))
                            Quantity = System.Convert.ToDecimal(numberunits);
                        break;
                    case "30":
                        idx = idx + 2;
                        string numberunits2 = Advance(nbarcode, ref idx);
                        if (DoubleUtil.IsNumeric(numberunits2, System.Globalization.NumberStyles.Number))
                            Quantity = System.Convert.ToDecimal(numberunits2);
                        break;
                    case "40":
                        if (prefix3 == "400")
                        {
                            idx = idx + 3;
                            CustomerPurchaseNumber = Advance(nbarcode, ref idx);
                        }
                        break;
                    case "24":
                        if (prefix3 == "240")
                        {
                            idx = idx + 3;
                            Advance(nbarcode, ref idx);
                        }
                        else
                            throw new Exception("Unimplemented prefix: " + prefix3);
                        break;
                    case "90":
                        idx = idx + 2;
                        Mutual = Advance(nbarcode, ref idx);
                        break;
                    case "91":
                    case "92":
                    case "93":
                    case "94":
                    case "95":
                    case "96":
                    case "97":
                    case "98":
                    case "99":
                        idx = idx + 2;
                        InternalCode = Advance(nbarcode, ref idx);
                        break;
                    case "31":
                        switch (prefix3)
                        {
                            case "310":
                                idx = idx + 3;
                                ProductNetWeightKg = DecodeNumber(nbarcode, ref idx);
                                break;
                            case "311":
                                idx = idx + 3;
                                ProductLengthMeters = DecodeNumber(nbarcode, ref idx);
                                break;
                            case "312":
                                idx = idx + 3;
                                ProductWidthMeters = DecodeNumber(nbarcode, ref idx);
                                break;
                            case "313":
                                idx = idx + 3;
                                ProductDepthMeters = DecodeNumber(nbarcode, ref idx);
                                break;
                            case "314":
                                idx = idx + 3;
                                ProductAreaMeters2 = DecodeNumber(nbarcode, ref idx);
                                break;
                            case "315":
                                idx = idx + 3;
                                ProductNetVolumeLiters = DecodeNumber(nbarcode, ref idx);
                                break;
                            case "316":
                                idx = idx + 3;
                                ProductNetVolumeMeters3 = DecodeNumber(nbarcode, ref idx);
                                break;
                            default:
                                throw new Exception("Unimplemented prefix: " + prefix3);
                        }
                        break;
                    case "32":
                        switch (prefix3)
                        {
                            case "320":
                                idx = idx + 3;
                                ProductNetWeightPounds = DecodeNumber(nbarcode, ref idx);
                                break;
                            case "321":
                                idx = idx + 3;
                                ProductLengthInch = DecodeNumber(nbarcode, ref idx);
                                break;
                            case "322":
                                idx = idx + 3;
                                ProductLengthFeet = DecodeNumber(nbarcode, ref idx);
                                break;
                            case "323":
                                idx = idx + 3;
                                ProductLengthYard = DecodeNumber(nbarcode, ref idx);
                                break;
                            case "324":
                                idx = idx + 3;
                                ProductWidthInch = DecodeNumber(nbarcode, ref idx);
                                break;
                            case "325":
                                idx = idx + 3;
                                ProductWidthFeet = DecodeNumber(nbarcode, ref idx);
                                break;
                            case "326":
                                idx = idx + 3;
                                ProductWidthYard = DecodeNumber(nbarcode, ref idx);
                                break;
                            case "327":
                                idx = idx + 3;
                                ProductDepthInch = DecodeNumber(nbarcode, ref idx);
                                break;
                            case "328":
                                idx = idx + 3;
                                ProductDepthFeet = DecodeNumber(nbarcode, ref idx);
                                break;
                            case "329":
                                idx = idx + 3;
                                ProductDepthYard = DecodeNumber(nbarcode, ref idx);
                                break;
                            default:
                                throw new Exception("Unimplemented prefix: " + prefix3);
                        }
                        break;
                    case "33":
                        switch (prefix3)
                        {
                            case "330":
                                idx = idx + 3;
                                ContainerGrossWeightKg = DecodeNumber(nbarcode, ref idx);
                                break;
                            case "331":
                                idx = idx + 3;
                                ContainerLengthMeters = DecodeNumber(nbarcode, ref idx);
                                break;
                            case "332":
                                idx = idx + 3;
                                ContainerWidthMeters = DecodeNumber(nbarcode, ref idx);
                                break;
                            case "333":
                                idx = idx + 3;
                                ContainerDepthMeters = DecodeNumber(nbarcode, ref idx);
                                break;
                            case "334":
                                idx = idx + 3;
                                ContainerAreaMeters2 = DecodeNumber(nbarcode, ref idx);
                                break;
                            case "335":
                                idx = idx + 3;
                                ContainerGrossVolumeLiters = DecodeNumber(nbarcode, ref idx);
                                break;
                            case "336":
                                idx = idx + 3;
                                ContainerGrossVolumeMeters3 = DecodeNumber(nbarcode, ref idx);
                                break;
                            default:
                                throw new Exception("Unimplemented prefix: " + prefix3);
                        }
                        break;
                    case "34":
                        switch (prefix3)
                        {
                            case "340":
                                idx = idx + 3;
                                ContainerGrossWeightPounds = DecodeNumber(nbarcode, ref idx);
                                break;
                            case "341":
                                idx = idx + 3;
                                ContainerLengthInch = DecodeNumber(nbarcode, ref idx);
                                break;
                            case "342":
                                idx = idx + 3;
                                ContainerLengthFeet = DecodeNumber(nbarcode, ref idx);
                                break;
                            case "343":
                                idx = idx + 3;
                                ContainerLengthYard = DecodeNumber(nbarcode, ref idx);
                                break;
                            case "344":
                                idx = idx + 3;
                                ContainerWidthInch = DecodeNumber(nbarcode, ref idx);
                                break;
                            case "345":
                                idx = idx + 3;
                                ContainerWidthFeet = DecodeNumber(nbarcode, ref idx);
                                break;
                            case "346":
                                idx = idx + 3;
                                ContainerWidthYard = DecodeNumber(nbarcode, ref idx);
                                break;
                            case "347":
                                idx = idx + 3;
                                ContainerDepthInch = DecodeNumber(nbarcode, ref idx);
                                break;
                            case "348":
                                idx = idx + 3;
                                ContainerDepthFeet = DecodeNumber(nbarcode, ref idx);
                                break;
                            case "349":
                                idx = idx + 3;
                                ContainerDepthYard = DecodeNumber(nbarcode, ref idx);
                                break;
                            default:
                                throw new Exception("Unimplemented prefix: " + prefix3);
                        }
                        break;
                    case "35":
                        switch (prefix3)
                        {
                            case "350":
                                idx = idx + 3;
                                ProductAreaInch2 = DecodeNumber(nbarcode, ref idx);
                                break;
                            case "351":
                                idx = idx + 3;
                                ProductAreaFeet2 = DecodeNumber(nbarcode, ref idx);
                                break;
                            case "352":
                                idx = idx + 3;
                                ProductAreaYard2 = DecodeNumber(nbarcode, ref idx);
                                break;
                            case "353":
                                idx = idx + 3;
                                ContainerAreaInch2 = DecodeNumber(nbarcode, ref idx);
                                break;
                            case "354":
                                idx = idx + 3;
                                ContainerAreaFeet2 = DecodeNumber(nbarcode, ref idx);
                                break;
                            case "355":
                                idx = idx + 3;
                                ContainerAreaYard2 = DecodeNumber(nbarcode, ref idx);
                                break;
                            case "356":
                                idx = idx + 3;
                                NetWeightTroyOunces = DecodeNumber(nbarcode, ref idx);
                                break;
                            case "357":
                                idx = idx + 3;
                                NetWeightOunces = DecodeNumber(nbarcode, ref idx);
                                break;
                            default:
                                throw new Exception("Unimplemented prefix: " + prefix3);
                        }
                        break;
                    case "36":
                        switch (prefix3)
                        {
                            case "360":
                                idx = idx + 3;
                                ProductVolumeQuarts = DecodeNumber(nbarcode, ref idx);
                                break;
                            case "361":
                                idx = idx + 3;
                                ProductVolumeGallons = DecodeNumber(nbarcode, ref idx);
                                break;
                            case "362":
                                idx = idx + 3;
                                ContainerVolumeQuarts = DecodeNumber(nbarcode, ref idx);
                                break;
                            case "363":
                                idx = idx + 3;
                                ContainerVolumeGallons = DecodeNumber(nbarcode, ref idx);
                                break;
                            case "364":
                                idx = idx + 3;
                                ProductVolumeInch3 = DecodeNumber(nbarcode, ref idx);
                                break;
                            case "365":
                                idx = idx + 3;
                                ProductVolumeFeet3 = DecodeNumber(nbarcode, ref idx);
                                break;
                            case "366":
                                idx = idx + 3;
                                ProductVolumeYard3 = DecodeNumber(nbarcode, ref idx);
                                break;
                            case "367":
                                idx = idx + 3;
                                ContainerVolumeInch3 = DecodeNumber(nbarcode, ref idx);
                                break;
                            case "368":
                                idx = idx + 3;
                                ContainerVolumeFeet3 = DecodeNumber(nbarcode, ref idx);
                                break;
                            case "369":
                                idx = idx + 3;
                                ContainerVolumeYard3 = DecodeNumber(nbarcode, ref idx);
                                break;
                            default:
                                throw new Exception("Unimplemented prefix: " + prefix3);
                        }
                        break;

                    default:
                        {
                            if (previousprefix == "01")
                                idx--;
                            else
                                throw new Exception("Unimplemented prefix: " + prefix2);
                        }
                        break;
                }
                previousprefix = prefix2;
            }
        }
        private DateTime DecodeDate(string barcode, int position)
        {
            string nyear = barcode.Substring(position, 2);
            int prefixyear = System.Convert.ToInt32(nyear);
            if (prefixyear > 80)
            {
                prefixyear = 1900 + prefixyear;
            }
            else
            {
                prefixyear = 2000 + prefixyear;
            }
            int nday = System.Convert.ToInt32(barcode.Substring(position + 4, 2));
            int nmonth = System.Convert.ToInt32(barcode.Substring(position + 2, 2));
            if (nday == 0)
            {
                nday = DateTime.DaysInMonth(prefixyear, nmonth);
            }
            return new DateTime(prefixyear, nmonth,
                nday);
        }
    }
    public static class AssemblyResolver
    {
        public static void HandleUnresolvedAssemblies()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += currentDomain_AssemblyResolve;
        }
        private static System.Reflection.Assembly currentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string sqlitename = "System.Data.SQLite";
            if (args.Name.Length >= sqlitename.Length)
            {
                if (args.Name.Substring(0, sqlitename.Length) == "System.Data.SQLite")
                {
                    string pathToWhereYourNativeFolderLives = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    pathToWhereYourNativeFolderLives = Path.GetDirectoryName(pathToWhereYourNativeFolderLives);
                    //var path = Path.Combine(pathToWhereYourNativeFolderLives, "Native");
                    string path = pathToWhereYourNativeFolderLives;
                    if (IntPtr.Size == 8) // or for .NET4 use Environment.Is64BitProcess
                    {
                        path = Path.Combine(path, "Win64");
                    }
                    else
                    {
                        path = Path.Combine(path, "Win32");
                    }
                    string dirpath = path;
                    string dllpath = Path.Combine(path, "System.Data.SQLite.dll");
                    if (!File.Exists(dllpath))
                    {
                        throw new Exception("System.Data.SQLite.dll could not be found in " + dllpath);
                    }

                    System.Reflection.Assembly assembly = System.Reflection.Assembly.LoadFrom(dllpath);
                    return assembly;
                }
            }

            return null;
        }
    }

}

