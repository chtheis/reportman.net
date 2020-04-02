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
using System.Data;
using Reportman.Drawing;
using System.Collections;

namespace Reportman.Reporting
{
	public class Param : ReportItem,ICloneable
	{
		private Variant FValue;
		public Variant Value
		{
			get
			{
                if (ParamType == ParamType.Multiple)
                    return GetMultiValue();
                else

                    return FValue;
			}
			set
			{
				FValue=value;

			}
		}
		public ParamType ParamType;
		public string Alias;
		public string Descriptions;
		public string Description
		{
				get
				{
					return Strings.GetStringByIndex(Descriptions,Report.Language);	
				}
				set
				{
					Descriptions=Strings.SetStringByIndex(Descriptions,value, Report.Language);
				}
		}
		public string Hints;
        protected override string GetClassName()
        {
            return "TRPPARAM";
        }
        public bool UserVisible
        {
            get { return (Visible && (!NeverVisible)); }
        }
		public string Hint
		{
			get
			{
				return Strings.GetStringByIndex(Hints, Report.Language);
			}
			set
			{
				Hints = Strings.SetStringByIndex(Hints,value, Report.Language);
			}
		}
		public string ErrorMessages;
		public string ErrorMessage
		{
			get
			{
                if (ErrorMessages.Length > Report.Language)
                    return Strings.GetStringByIndex(ErrorMessages, Report.Language);
                else
                {
                    if (ErrorMessages.Length > 0)
                        return Strings.GetStringByIndex(ErrorMessages, 0);
                    else
                        return "";
                }
			}
			set
			{
				ErrorMessages = Strings.SetStringByIndex(ErrorMessages,value, Report.Language);
			}
		}
		public string Validation;
		public string LookupDataset, SearchDataset, Search, SearchParam;
		public Strings Items;
		public Strings Values;
		public Strings Selected;
		public Strings Datasets;
        private Variant FLastValue;
		public Variant LastValue
        {
            get
            {
                return FLastValue;
            }
            set
            {

                FLastValue = value;
            }

        }
		public bool Visible, IsReadOnly, NeverVisible, AllowNulls;
		public Param(BaseReport rp)
			: base(rp)
		{
			Items = new Strings();
			Values = new Strings();
			Selected = new Strings();
			Datasets = new Strings();
			Descriptions = "";
			Hints = "";
			ErrorMessages = "";
            Validation = "";
			Alias = "";
			FValue = new Variant();
			LookupDataset = ""; SearchDataset = ""; Search = ""; SearchParam = "";
		}
		public DbType GetDbType()
		{
			DbType aresult = DbType.Object;
			switch (ParamType)
			{
				case ParamType.Bool:
					aresult = DbType.Boolean;
					break;
				case ParamType.Currency:
					aresult = DbType.Currency;
					break;
				case ParamType.Date:
					aresult = DbType.Date;
					break;
				case ParamType.Time:
					aresult = DbType.Time;
					break;
				case ParamType.DateTime:
					aresult = DbType.DateTime;
					break;
				case ParamType.Double:
					aresult = DbType.Double;
					break;
				case ParamType.String:
					aresult = DbType.String;
					break;
				case ParamType.ExpreA:
				case ParamType.ExpreB:
				case ParamType.List:
                case ParamType.SubsExpreList:
                case ParamType.Multiple:
					aresult = LastValue.GetDbType();
					break;
			}
			return aresult;
		}
        public string GetSubExpreValue()
        {
            if (FValue.IsInteger()) 
            {
                return Values[FValue];
            }
            else
                return FValue;
        }
		public string GetMultiValue()
		{
			int i;

			string aresult = "";
            if (ParamType != ParamType.Multiple)
                return aresult;
			for (i = 0; i < Selected.Count; i++)
			{
/*				astring = Selected[i];
				aindex = System.Convert.ToInt32(astring);
				if (Values.Count > aindex)
				{
					if (aresult.Length > 0)
						aresult = aresult + "," + Values[aindex];
					else
						aresult = Values[aindex];
				}
*/
                if (aresult.Length > 0)
                 aresult = aresult + "," + Selected[i];
                else
                 aresult = Selected[i];
			}
			return aresult;
		}

		public Variant ListValue
		{
			get
			{
				Variant aresult = new Variant();
				string aexpression;
				int aoption;
				if (!((ParamType == ParamType.List) ||
                    (ParamType == ParamType.Multiple) || (ParamType == ParamType.SubsExpreList)))
					aresult = Value;
				else
				{
					if (ParamType == ParamType.Multiple)
						aresult = GetMultiValue();
					else
					{
						aoption = 0;
						if (Value.IsInteger())
						{
							aoption = Value;
							if (aoption < 0)
								aoption = 0;
						}
						else
						{
							if (Value.IsString())
							{
								aoption = Values.IndexOf(Value);
								if (aoption < 0)
									aoption = 0;
							}
						}
						if (aoption >= Values.Count)
						{
							aresult = Value;
						}
						else
						{
							aexpression = Values[aoption];
							aresult = Report.Evaluator.EvaluateText(aexpression);
						}
					}
				}
				return aresult;
			}
		}
		public object Clone()
		{
			Param p=new Param(Report);
			p.AllowNulls=AllowNulls;
			p.Alias=Alias;
			p.Datasets=(Strings)Datasets.Clone();
			p.Descriptions=Descriptions;
			p.ErrorMessage=ErrorMessage;
			p.FValue=FValue;
			p.Hint=Hint;
			p.Hints=Hints;
			p.IsReadOnly=IsReadOnly;
			p.Items=(Strings)Items.Clone();
			p.LastValue=LastValue;
			p.LookupDataset=LookupDataset;
			p.Name=Name;
			p.NeverVisible=NeverVisible;
			p.ParamType=ParamType;
			p.Search=Search;
			p.SearchDataset=SearchDataset;
			p.SearchParam=SearchParam;
			p.Selected=(Strings)Selected.Clone();
			p.Validation=Validation;
			p.Values=(Strings)Values.Clone();
			p.Visible=Visible;
			return p;
		}
        public void SelectAllValues()
        {
            Selected.Clear();
            foreach (string s in Values)
                Selected.Add(s);
        }
        public void UpdateLookupValues()
        {
            if (LookupDataset.Length > 0)
            {
                Values.Clear();
                Items.Clear();
                DataInfo dinfo = Report.DataInfo[LookupDataset];
                dinfo.DisConnect();
                dinfo.Connect();
                try
                {
                    int indexvalue = 0;
                    if (dinfo.Data.Columns.Count > 1)
                        indexvalue = 1;
                    while (!dinfo.Data.Eof)
                    {
                        Items.Add(dinfo.Data.CurrentRow[0].ToString());
                        Values.Add(dinfo.Data.CurrentRow[indexvalue].ToString());
                        dinfo.Data.Next();
                    }
                }
                finally
                {
                    dinfo.DisConnect();
                }
            }
        }
	}
	public class Params:IEnumerable,ICloneable
	{
		Param[] FItems;
		const int FIRST_ALLOCATION_OBJECTS = 10;
		int FCount;
		public Params()
		{
			FCount = 0;
			FItems = new Param[FIRST_ALLOCATION_OBJECTS];
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
				throw new Exception("Index out of range on Params collection");
		}
		public int IndexOf(string avalue)
		{
			int aresult = -1;
			for (int i = 0; i < Count; i++)
			{
				if (FItems[i].Alias == avalue)
				{
					aresult = i;
					break;
				}
			}
			return aresult;
		}
        public int IndexOf(Param avalue)
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
        public Param this[int index]
		{
			get { CheckRange(index); return FItems[index]; }
			set { CheckRange(index); FItems[index] = value; }
		}
        public void Remove(Param nparam)
        {
            int index = IndexOf(nparam);
            if (index < 0)
                throw new Exception("Parameter does not exists:" + nparam.Alias);
            for (int i = index; i < FCount - 1; i++)
            {
                FItems[i] = FItems[i + 1];
            }
            FCount--;
        }
        public void RemoveAt(int index)
        {
            if ((index >= FCount) || (index<0))
                throw new Exception("Parameter index out of range: " + index.ToString());
            for (int i = index; i < FCount - 1; i++)
            {
                FItems[i] = FItems[i + 1];
            }
            FCount--;
        }
        public Param this[string paramname]
        {
            get
            {
                int index = IndexOf(paramname);
                if (index >= 0)
                    return FItems[index];
                else
                    return null;
            }
        }
        public int Count { get { return FCount; } }
		public void Add(Param obj)
		{
			if (FCount > (FItems.Length - 2))
			{
				Param[] nobjects = new Param[FCount];
				System.Array.Copy(FItems, 0, nobjects, 0, FCount);
				FItems = new Param[FItems.Length * 2];
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
            return new ParamEnumerator(this);
        }
        // Inner class implements IEnumerator interface:
        public class ParamEnumerator : IEnumerator
        {
            private int position = -1;
            private Params t;

            public ParamEnumerator(Params t)
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
		public Params Clone(Report rp)
		{
			Params aparams=(Params)Clone();
			foreach (Param p in aparams)
			{
				p.Report=rp;
			}
			return aparams;
		}
		public object Clone()
		{
			Params aparams=new Params();
			foreach (Param p in this)
			{
				aparams.Add((Param)p.Clone());
			}
			return aparams;
		}
        public void Switch(int index1, int index2)
        {
            if ((index1 < 0) || (index2 < 0))
                throw new Exception("Index out of bounds in Params.Switch");
            if ((index1 >= FCount) || (index2 >=FCount))
                throw new Exception("Index out of bounds in Params.Switch");
            Param buf = FItems[index1];
            FItems[index1] = FItems[index2];
            FItems[index2] = buf;
        }
    }
    public class IdenVariableParam : IdenVariable
    {
        Param FParam;
        /// <summary>
        /// Constructor for the variable parameter
        /// </summary>
        /// <param name="eval"></param>
        public IdenVariableParam(Evaluator eval,Param nparam)
            : base(eval)
        {
            FParam=nparam;
        }
        /// <summary>
        /// The value is assigned to the internal value and to the parameter
        /// </summary>
        /// <param name="avalue"></param>
        protected override void SetValue(Variant avalue)
        {
            FParam.LastValue = avalue;
        }
        /// <summary>
        /// Gets the internal value
        /// </summary>
        /// <returns></returns>
        protected override Variant GetValue()
        {
            if (FParam.ParamType == ParamType.Multiple)
                return FParam.GetMultiValue();
            else
                return FParam.LastValue;
        }

    }

}
