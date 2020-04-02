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
using System.Data;
using System.Collections;
#if REPMAN_DOTNET2
using System.Collections.Generic;
#endif
using Reportman.Drawing;


namespace Reportman.Reporting
{
	/// <summary>
	/// EvalIdentifier is the base class for all objects capable of integration
	/// with Evaluator,this includes variables, functions, constants...
	/// </summary>
	public abstract class EvalIdentifier
	{
        /// <summary>
        /// Main constructor for identifiers
        /// </summary>
        /// <param name="eval"></param>
		protected EvalIdentifier(Evaluator eval)
		{
			EvalObject = eval;
		}
        /// <summary>
        /// Override the function to allow the returning of values by the identifier (functions)
        /// </summary>
        /// <returns></returns>
		protected abstract Variant GetValue();
        /// <summary>
        /// Override the function to allow the assignment of values to the identifier (variables)
        /// </summary>
        /// <param name="avalue"></param>
		protected abstract void SetValue(Variant avalue);
        /// <summary>
        /// Name of the identifier
        /// </summary>
		public string Name;
        /// <summary>
        /// Model, used usually for functions, string representing function model
        /// </summary>
		protected string FModel;
        /// <summary>
        /// Help string used by the expression builder wizard
        /// </summary>
		protected string FHelp;
        /// <summary>
        /// Evaluator that owns the ident
        /// </summary>
		public Evaluator EvalObject;
        /// <summary>
        /// Gets the model of the identifier, usually used in functions to define
        /// the name, input parameters and output
        /// </summary>
		public string Model
		{
			get
			{
				return FModel;
			}
		}
        /// <summary>
        /// Defines the purpose of the identifier, usually used by functions and constants to
        /// explain how to use the function
        /// </summary>
		public string Help
		{
			get
			{
				return FHelp;
			}
		}
        /// <summary>
        /// Gets or sets the value of the identifier, internally calls the protected methods, 
        /// GetValue and SetValue
        /// </summary>
		public Variant Value
		{
			get
			{
				return GetValue();
			}
			set
			{
				SetValue(value);
			}
		}
	}
	// Classes and constats for parser and evaluator
    /// <summary>
    ///  This enumeration is used in string parsing functions, a string is divided in tokens,
    /// each token can represent operators, symbols...
    /// </summary>
	public enum TokenType { 
        /// <summary>There is no more tokens to parse so current token is End of File</summary>
        Eof,
        /// <summary>The token is an identifier, usually a variable or function name</summary>
        Symbol,
        /// <summary>The token is a string</summary>
        String,
        /// <summary>The token is an integer</summary>
        Integer,
        /// <summary>The token is a dobule value</summary>
        Double,
        /// <summary>The token is a decimal value</summary>
        Decimal,
        /// <summary>The token is an operator (sum,multiply...)</summary>
        Operator
    };
    /// <summary>
    /// Definition of a specialiced exception used by expression evaluator
    /// </summary>
    public class EvalException : System.Exception
	{
		private int FSourceLine, FSourcePos;
        /// <summary>
        /// The element name that caused the exception
        /// </summary>
		public string ElementName;
        /// <summary>
        /// Source line number in the original expression (full expression)
        /// </summary>
		public int SourceLine
		{
			get
			{
				return FSourceLine;
			}
		}
        /// <summary>
        /// Position where the exception have been thrown, in the original expression
        /// </summary>
		public int SourcePos
		{
			get
			{
				return FSourcePos;
			}
		}
        /// <summary>
        /// Constructor, this will provide additional debug information for the designer
        /// to find the source of the problem inside a expression
        /// </summary>
        /// <param name="amessage"></param>
        /// <param name="asourceline"></param>
        /// <param name="asourcepos"></param>
        /// <param name="aelementname"></param>
		public EvalException(string amessage, int asourceline, int asourcepos, string aelementname)
			: base(amessage)
		{
			FSourceLine = asourceline;
			ElementName = aelementname;
			FSourcePos = asourcepos;
		}
	}
    /// <summary>
    /// Class avaible to define constants in expression evaluator
    /// </summary>
	public class IdenConstant : EvalIdentifier
	{
		protected Variant FValue;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="eval"></param>
		public IdenConstant(Evaluator eval)
			: base(eval)
		{
			FValue = new Variant();
		}
        /// <summary>
        /// Returns the value of the constant
        /// </summary>
        /// <returns></returns>
		protected override Variant GetValue()
		{
			return FValue;
		}
        /// <summary>
        /// Constants can not be assigned so a exception is lauched
        /// </summary>
        /// <param name="avalue"></param>
		protected override void SetValue(Variant avalue)
		{
			throw new UnNamedException(Translator.TranslateStr(365));
		}
	}
    /// <summary>
    /// Base class to implement evaluator functions
    /// </summary>
	public abstract class IdenFunction : EvalIdentifier
	{
        /// <summary>
        /// Function constructor
        /// </summary>
        /// <param name="eval"></param>
		protected IdenFunction(Evaluator eval)
			: base(eval)
		{

		}
		private int FParamCount;
        /// <summary>
        /// Function parameters
        /// </summary>
		public Variant[] Params;
        /// <summary>
        /// Sets the param count
        /// </summary>
        /// <param name="newparamcount"></param>
		protected void SetParamCount(int newparamcount)
		{
			if (newparamcount <= 0)
				Params = null;
			else
				Params = new Variant[newparamcount];
			FParamCount = newparamcount;
		}
        /// <summary>
        /// Gets the param count for the function, read only
        /// </summary>
		public int ParamCount
		{
			get
			{
				return FParamCount;
			}
		}
        /// <summary>
        /// Setting a value to a function is not possible, a exception is thrown
        /// </summary>
        /// <param name="avalue"></param>
		protected override void SetValue(Variant avalue)
		{
			throw new UnNamedException(Translator.TranslateStr(364));
		}
	}
    /// <summary>
    /// Identifier with the functionality of a variable
    /// </summary>
	public class IdenVariable : EvalIdentifier
	{
        /// <summary>
        /// Constructor for the variable
        /// </summary>
        /// <param name="eval"></param>
		public IdenVariable(Evaluator eval)
			: base(eval)
		{
			FValue = new Variant();
		}
        /// <summary>
        /// Internal value
        /// </summary>
		protected Variant FValue;
        /// <summary>
        /// The value is assigned to the internal value
        /// </summary>
        /// <param name="avalue"></param>
		protected override void SetValue(Variant avalue)
		{
			FValue = avalue;
		}
        /// <summary>
        /// Gets the internal value
        /// </summary>
        /// <returns></returns>
		protected override Variant GetValue()
		{
			return FValue;
		}

	}
    /// <summary>
    /// True constant for expression evaluator
    /// </summary>
	class IdenTrue : IdenConstant
	{
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="eval"></param>
		public IdenTrue(Evaluator eval)
			: base(eval)
		{
			FValue = true;
			Name = "TRUE";
		}
	}
    /// <summary>
    /// Null constant for expression evaluator
    /// </summary>
    class IdenNull : IdenConstant
	{
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="eval"></param>
		public IdenNull(Evaluator eval)
			: base(eval)
		{
			Name = "NULL";
		}
	}
    /// <summary>
    /// False constant for expression evaluator
    /// </summary>
    class IdenFalse : IdenConstant
	{
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="eval"></param>
		public IdenFalse(Evaluator eval)
			: base(eval)
		{
			FValue = false;
			Name = "FALSE";
		}
	}
    /// <summary>
    /// This identifier have a datarow column linked so, it will return the
    /// value of a field
    /// </summary>
	class IdenField : EvalIdentifier
	{
        /// <summary>
        /// Constructor
        /// </summary>
		public IdenField() : base(null) { }
		private string FField;
		private DataTable FData;
        /// <summary>
        /// Link to a row index in the datatable
        /// </summary>
		public int CurrentRow;
        /// <summary>
        /// Column name in the data row
        /// </summary>
		public string Field
		{
			get
			{
				return (FField);
			}
			set
			{
				FField = value;
			}
		}
        /// <summary>
        /// Link to the DataTable
        /// </summary>
		public DataTable Data
		{
			get
			{
				return (FData);
			}
			set
			{
				FData = value;
			}
		}
        /// <summary>
        /// Setting a value to a field is not possible, a exception is thrown
        /// </summary>
        /// <param name="avalue"></param>
		protected override void SetValue(Variant avalue)
		{
			throw new UnNamedException(Translator.TranslateStr(365));
		}
        /// <summary>
        /// Gets the value of the field
        /// </summary>
        /// <returns></returns>
		protected override Variant GetValue()
		{
            try
            {
                Variant FValue = new Variant();
                if (FData != null)
                    if (FField != null)
                    {
                        if (FData is ReportDataset)
                        {
                            if (((ReportDataset)FData).CurrentRowCount > 0)
                            {
                                FValue.AssignFromObject(((ReportDataset)FData).CurrentRow[FField]);
                            }
                        }
                        else
                        {
                            FValue.AssignFromObject(FData.Rows[CurrentRow][FField]);
                        }
                    }
                return (FValue);
            }
            catch
            {
                throw;
            }
		}

	}
	/// <summary>
	/// Collections of Evaluator identifiers
	/// </summary>
#if REPMAN_DOTNET1
	public class EvalIdentifiers
	{
		SortedList FItems;
		SortedListStringIndexer FIndexer;
		
		public int Count
		{
			get
			{
				return FItems.Count;
			}
		}
		public EvalIdentifiers()
		{
			FItems = new SortedList();
			FIndexer=new SortedListStringIndexer(FItems);
		}
		public void Clear()
		{
			FItems.Clear();
		}
		public void Add(string key,EvalIdentifier obj)
		{
			FItems.Add(key,obj);
		}
		/// <summary>
		/// Returns the eval identifier by index
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public EvalIdentifier this[int index]
		{
			get
			{ 
				CheckRange(index); 
				string key=(string)FItems.GetKey(index);
				return (EvalIdentifier)FItems[key]; 
			}
			set 
			{ 
				CheckRange(index); 
				string key=(string)FItems.GetKey(index);
				FItems[key] = value; 
			}
		}
		/// <summary>
		/// Returns the index of a key in the list
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public int IndexOfKey(string key)
		{
			return FItems.IndexOfKey(key);
		}
		/// <summary>
		/// Removes a key
		/// </summary>
		/// <param name="key"></param>
		public void Remove(string key)
		{
			FItems.Remove(key);
		}
		/// <summary>
		/// Returns the report component by name
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public EvalIdentifier this[string key]
		{
			get 
			{ 
				return (EvalIdentifier)FItems[key]; 
			}
			set 
			{ 
				FItems[key] = value; 
			}
		}
		/// <summary>
		/// Returns the key of an index
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public SortedListStringIndexer Keys
		{
			get 
			{ 
				return FIndexer; 
			}
		}
#else
    public class EvalIdentifiers : System.Collections.Generic.SortedList<string,EvalIdentifier>
    {
        /// <summary>
        /// Returns the eval identifier by index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public EvalIdentifier this[int index]
        {
            get 
            { 
                CheckRange(index); 
                return this[Keys[index]]; 
            }
            set 
            { 
                CheckRange(index); 
                this[Keys[index]] = value; 
            }
        }

#endif
		private void CheckRange(int index)
		{
			if ((index < 0) || (index >= Count))
				throw new UnNamedException("Index out of range on EvalIdentifiers collection");
		}
		/// <summary>
		/// IEnumerable Interface Implementation:
		///   Declaration of the GetEnumerator() method 
		///   required by IEnumerable
		/// </summary>
		/// <returns></returns>
		public new IEnumerator GetEnumerator()
		{
			return new EvalIdentifierEnumerator(this);
		}
		/// <summary>
		/// Inner class implements IEnumerator interface:
		/// </summary>
		public class EvalIdentifierEnumerator : IEnumerator
		{
			private int position = -1;
			private EvalIdentifiers t;
			/// <summary>
			/// Constructor for a enumerator of report items
			/// </summary>
			/// <param name="t"></param>
			public EvalIdentifierEnumerator(EvalIdentifiers t)
			{
				this.t = t;
			}

			/// <summary>
			/// Go to next element in the lis
			/// </summary>
			/// <returns></returns>
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

			/// <summary>
			/// Declare the Reset method required by IEnumerator
			/// </summary>
			public void Reset()
			{
				position = -1;
			}
			/// <summary>
			/// Declare the Current property required by IEnumerator
			/// </summary>
			public object Current
			{
				get
				{
					return t[position];
				}
			}
		}
	}
}
