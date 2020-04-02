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
using System.Text;
using System.IO;
#if REPMAN_DOTNET2
using System.Collections.Generic;
#endif

namespace Reportman.Drawing
{

#if REPMAN_DOTNET1
	/// <summary>
	/// Helper class for typed sorted lists in Dot Net 1.1
	/// </summary>
	public class SortedListStringIndexer
	{
		private SortedList list;
		public SortedListStringIndexer(SortedList list)
		{
			this.list = list;
		}
		public string this[int index]
		{
			get { return (string)list.GetKey(index); }
		}
		public int Count
		{
			get { return list.Count; }
		}
	}	
	public class Strings:ICloneable
	{
		string[] FItems;
		const int FIRST_ALLOCATION_OBJECTS = 1;
		int FCount;
		public Strings()
		{
			FCount = 0;
			FItems = new string[FIRST_ALLOCATION_OBJECTS];
		}
		public void Clear()
		{
			for (int i = 0; i < FCount; i++)
				FItems[i] = null;
			FCount = 0;
		}
		public int IndexOf(string avalue)
		{
			int aresult = -1;
			for (int i = 0; i < Count; i++)
			{
				if (FItems[i] == avalue)
				{
					aresult = i;
					break;
				}
			}
			return aresult;
		}
		private void CheckRange(int index)
		{
			if ((index < 0) || (index >= FCount))
				throw new Exception("Index out of range on Strings collection");
		}
		public string this[int index]
		{
			get { CheckRange(index); return FItems[index]; }
			set { CheckRange(index); FItems[index] = value; }
		}
		public int Count { get { return FCount; } }
		public void Add(string obj)
		{
			if (FCount > (FItems.Length - 2))
			{
				string[] nobjects = new string[FCount];
				System.Array.Copy(FItems, 0, nobjects, 0, FCount);
				FItems = new string[FItems.Length * 2];
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
			return new StringEnumerator(this);
		}
		// Inner class implements IEnumerator interface:
		public class StringEnumerator : IEnumerator
		{
			private int position = -1;
			private Strings t;

			public StringEnumerator(Strings t)
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
    /// <summary>
    /// String collection
    /// </summary>
	public class Strings:System.Collections.Generic.List<string>,ICloneable
	{
#endif
        /// <summary>
        /// Clone the object
        /// </summary>
        /// <returns>A object clone</returns>
		public object Clone()
		{
			Strings st=new Strings();
			foreach (string s in this)
			{
				st.Add(s);
			}
			return st;
		}
        /// <summary>
        /// Get a single string separating strings by a LINE FEED char, (char)10
        /// </summary>
		public string Text
		{
			get
			{
				StringBuilder astring=new StringBuilder();
				if (Count>0)
					astring.Append(this[0]);
				int i=1;
				while (i<Count)
				{
					astring.Append(""+(char)13+(char)10+this[i]);
					i++;
				}
				return astring.ToString();
			}
			set
			{
				Fill(value);
			}
		}
		private void Fill(string astring)
		{
			Clear();
			int i=0;
			StringBuilder partial=new StringBuilder();
			while (i<astring.Length)
			{
				if (astring[i]==(char)10)
				{
					Add(partial.ToString());
					partial=new StringBuilder();
					if (i<astring.Length-1)
						if (astring[i+1]==(char)13)
							i++;
				}
                else
				{
					partial.Append(astring[i]);
				}
				i++;
			}
			Add(partial.ToString());
		}
        /// <summary>
        /// Obtain a single string from a collection of strings stored in a single string
        /// </summary>
        /// <param name="astring">A list of strings represented by a single string separated by LF (char)10</param>
        /// <param name="index">Index to check</param>
        /// <returns>Returns a single string, obtained from index or an empty string if not found</returns>
		static public string GetStringByIndex(string astring,int index)
		{
			Strings alist;
			alist =new Strings();
			alist.Fill(astring);
			if (alist.Count<1)
				return "";
			if (index<0)
				index=0;
			if (alist.Count<index)
			{
				return alist[index];
			}
			else
			{
				return alist[0];
			}
		}
        /// <summary>
        /// Generates a string collection (inside a single string) from a collection of strings an index and a string.
        /// So really, it inserts (or update) a line inside a group of lines.
        /// </summary>
        /// <param name="astring">A list of strings represented by a single string separated by LF (char)10</param>
        /// <param name="avalue">String value to insert into the list</param>
        /// <param name="index"></param>
        /// <returns>Collection of strings separated by LF with the line inserted or updated</returns>
		static public string SetStringByIndex(string astring,string avalue, int index)
		{
			Strings alist;
			string defaultvalue="";
			alist = new Strings();
			if (index<0)
				index = 0;
			alist.Fill(astring);
			if (alist.Count >0)
				defaultvalue = alist[0];
			while ((alist.Count-1) < index)
			{
				alist.Add(defaultvalue);
			}
			alist[index]=avalue;
			return alist.Text;
		}
        /// <summary>
        /// Converts a string with semicolons to a Strings collection
        /// </summary>
        /// <param name="semicolonstring"></param>
        /// Semicolon separated strings
        /// <returns></returns>
        public static Strings FromSemiColon(string semicolonstring)
        {
            return Strings.FromSeparator(';', semicolonstring);
        }
        /// <summary>
        /// Converts a string with any separator to a Strings collection
        /// </summary>
        /// <param name="separator"></param>
        /// Separator as char
        /// <returns></returns>
        /// <param name="nstring"></param>
        /// Semicolon separated strings
        /// <returns></returns>
        public static Strings FromSeparator(char separator,string nstring)
        {
            Strings aresult = new Strings();
            string partial = nstring;
            int index = partial.IndexOf(separator);
            while (index >= 0)
            {
                aresult.Add(partial.Substring(0, index));
                partial = partial.Substring(index + 1);
                index = partial.IndexOf(separator);
            }
            aresult.Add(partial);
            return aresult;
        }
        /// <summary>
        /// Remove the empty strings
        /// </summary>
        public void RemoveBlanks()
        {
            int i=0;
            while (i<Count)
            {
                if (this[i].Length==0)
                    this.RemoveAt(i);
                else
                    i++;
            }
        }
        /// <summary>
        /// Converts the string collection into a semicolon separated string
        /// </summary>
        /// <returns></returns>
        public string ToSemiColon()
        {
            if (Count<1)
            {
                return "";
            }
            StringBuilder sbuilder = new StringBuilder(this[0]);
            int i;
            for (i = 1; i < Count; i++)
            {
                sbuilder.Append(';');
                sbuilder.Append(this[i]);
            }
            return sbuilder.ToString();
        }
        /// <summary>
        /// Converts the string collection into a semicolon separated string
        /// </summary>
        /// <returns></returns>
        public string ToCharSeparated(char separator)
        {
            if (Count < 1)
            {
                return "";
            }
            StringBuilder sbuilder = new StringBuilder(this[0]);
            int i;
            for (i = 1; i < Count; i++)
            {
                sbuilder.Append(separator);
                sbuilder.Append(this[i]);
            }
            return sbuilder.ToString();
        }
        public void LoadFromFile(string filename)
        {
            using (FileStream nstream = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                LoadFromStream(nstream);
            }
        }
        public void LoadFromStream(Stream nstream)
        {
#if REPMAN_COMPACT
            using (MemoryStream mems = StreamUtil.StreamToMemoryStream(nstream))
            {
                string nstring = StreamUtil.ByteArrayToString(mems.ToArray(), System.Convert.ToInt32(mems.Length), true);
                Clear();
                StringBuilder nline = new StringBuilder();
                foreach (char c in nstring)
                {
                    if (c != (char)13)
                    {
                        if (c == (char)10)
                        {
                            Add(nline.ToString());
                            nline = new StringBuilder();
                        }
                        else
                            nline.Append(c);
                    }
                }
                string aline = nline.ToString();
                if (aline.Length > 0)
                    Add(aline);
            }
#else
            using (MemoryStream mems = StreamUtil.StreamToMemoryStream(nstream))
            {
                mems.Seek(0, SeekOrigin.Begin);
                StreamReader nreader = new StreamReader(nstream, Encoding.UTF8);
                if (nreader.EndOfStream)
                {
                    string nstring = StreamUtil.ByteArrayToString(mems.ToArray(), System.Convert.ToInt32(mems.Length), true);
                    nstring = nstring.Replace((char)65279 + "", "");
                    Clear();
                    StringBuilder nline = new StringBuilder();
                    foreach (char c in nstring)
                    {
                        if (c != (char)13)
                        {
                            if (c == (char)10)
                            {
                                Add(nline.ToString());
                                nline = new StringBuilder();
                            }
                            else
                                nline.Append(c);
                        }
                    }
                    string aline = nline.ToString();
                    if (aline.Length > 0)
                        Add(aline);
                }
                else
                {
                    //string nstring = Encoding.UTF8.GetString(mems.ToArray());
                    // StreamUtil.ByteArrayToString(mems.ToArray(),System.Convert.ToInt32(mems.Length),true);
                    Clear();

                    while (!nreader.EndOfStream)
                    {
                        string nline = nreader.ReadLine().Trim();
                        if (nline.Length > 0)
                            Add(nline);
                    }
                }
            }
#endif
        }
    }
#if REPMAN_DOTNET1
	public class Doubles
	{
		double[] FItems;
		const int FIRST_ALLOCATION_OBJECTS = 100;
		int FCount;
		public Doubles()
		{
			FCount = 0;
			FItems = new double[FIRST_ALLOCATION_OBJECTS];
		}
		public void Clear()
		{
			FCount = 0;
		}
		private void CheckRange(int index)
		{
			if ((index < 0) || (index >= FCount))
				throw new Exception("Index out of range on Doubles collection");
		}
		public double this[int index]
		{
			get { CheckRange(index); return FItems[index]; }
			set { CheckRange(index); FItems[index] = value; }
		}
		public int Count { get { return FCount; } }
		public void Add(double obj)
		{
			if (FCount > (FItems.Length - 2))
			{
				double[] nobjects = new double[FCount];
				System.Array.Copy(FItems, 0, nobjects, 0, FCount);
				FItems = new double[FItems.Length * 2];
				System.Array.Copy(nobjects, 0, FItems, 0, FCount);
			}
			FItems[FCount] = obj;
			FCount++;
		}
	}
#else
    /// <summary>
    /// Collection of double values
    /// </summary>
    public class Doubles : System.Collections.Generic.List<double>
	{
	}
#endif
#if REPMAN_DOTNET1
	public class Integers
	{
		int[] FItems;
		const int FIRST_ALLOCATION_OBJECTS = 100;
		int FCount;
		public Integers()
		{
			FCount = 0;
			FItems = new int[FIRST_ALLOCATION_OBJECTS];
		}
		public void Clear()
		{
			FCount = 0;
		}
		private void CheckRange(int index)
		{
			if ((index < 0) || (index >= FCount))
				throw new Exception("Index out of range on Integers collection");
		}
		public int this[int index]
		{
			get { CheckRange(index); return FItems[index]; }
			set { CheckRange(index); FItems[index] = value; }
		}
		public int Count { get { return FCount; } }
		public void Add(int obj)
		{
			if (FCount > (FItems.Length - 2))
			{
				int[] nobjects = new int[FCount];
				System.Array.Copy(FItems, 0, nobjects, 0, FCount);
				FItems = new int[FItems.Length * 2];
				System.Array.Copy(nobjects, 0, FItems, 0, FCount);
			}
			FItems[FCount] = obj;
			FCount++;
		}
	}
#else
    /// <summary>
    /// Collection of integer values
    /// </summary>
    public class Integers : System.Collections.Generic.List<int>
	{

	}
#endif
#if REPMAN_DOTNET1
	public class Longs
	{
		long[] FItems;
		const int FIRST_ALLOCATION_OBJECTS = 100;
		int FCount;
		public Longs()
		{
			FCount = 0;
			FItems = new long[FIRST_ALLOCATION_OBJECTS];
		}
		public void Clear()
		{
			FCount = 0;
		}
		private void CheckRange(int index)
		{
			if ((index < 0) || (index >= FCount))
				throw new Exception("Index out of range on Integers collection");
		}
		public long this[int index]
		{
			get { CheckRange(index); return FItems[index]; }
			set { CheckRange(index); FItems[index] = value; }
		}
		public int Count { get { return FCount; } }
		public void Add(long obj)
		{
			if (FCount > (FItems.Length - 2))
			{
				long[] nobjects = new long[FCount];
				System.Array.Copy(FItems, 0, nobjects, 0, FCount);
				FItems = new long[FItems.Length * 2];
				System.Array.Copy(nobjects, 0, FItems, 0, FCount);
			}
			FItems[FCount] = obj;
			FCount++;
		}
	}
#else
    /// <summary>
    /// Collection of Long values
    /// </summary>
    public class Longs : System.Collections.Generic.List<long>
	{

	}
#endif

}
