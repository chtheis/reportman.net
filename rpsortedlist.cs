using System;

namespace Reportman.Drawing
{
	public class SortedList
	{
		object[] FItems;
		string[] FKeys;
		const int FIRST_ALLOCATION_OBJECTS=50;
		int FCount;
		public SortedList()
		{
			FCount=0;
			FItems=new object[FIRST_ALLOCATION_OBJECTS];
			FKeys=new string[FIRST_ALLOCATION_OBJECTS];
		}
		public void Clear()
		{
			for (int i=0;i<FCount;i++)
			{
				FItems[i]=null;
				FKeys[i]=null;
			}
			FCount=0;
		}
		public int IndexOfKey(char key)
		{
			return IndexOfKey(key.ToString());
		}
		public int IndexOfKey(string key)
		{
			int aresult=-1;
			for (int i=0;i<FCount;i++)
			{
				if (FKeys[i]==key)
				{
					aresult=i;
					break;
				}
			}
			return aresult;
		}
		public object this[string key]
		{
			get
			{
				int index=IndexOfKey(key);
				object aresult=null;
				if (index>=0)
					return FItems[index];
				return (aresult);	
			}
		}
		public object this[char key]
		{
			get
			{
				return this[key.ToString()];
			}
		}
		public object GetByIndex(int index)
		{
			if (index<FCount)
				return FItems[index];
			else
				return null;
		}
		public void Remove(string key)
		{
			int index=IndexOfKey(key);
			if (index<0)
				return;
			for (int i=index;i<Count-1;i++)
			{
				FKeys[i]=FKeys[i+1];
				FItems[i]=FItems[i+1];
			}
		}
		public string GetKey(int index)
		{
			if (index<FCount)
				return FKeys[index];
			else
				return null;
		}
		public int Count {get {return FCount;}}
		public void Add(char key,object obj)
		{
			Add(key.ToString(),obj);
		}			
		public void Add(string key,object obj)
		{
			if (FCount>(FItems.Length-2))
			{
				object[] nobjects=new object[FCount];
				System.Array.Copy(FItems,0,nobjects,0,FCount);
				FItems=new object[FItems.Length*2];
				System.Array.Copy(nobjects,0,FItems,0,FCount);
				//
				string[] nkeys=new string[FCount];
				System.Array.Copy(FKeys,0,nkeys,0,FCount);
				FKeys=new string[FKeys.Length*2];
				System.Array.Copy(nkeys,0,FKeys,0,FCount);
			}
			FItems[FCount]=obj;
			FKeys[FCount]=key;
			FCount++;
		}
	}
}
