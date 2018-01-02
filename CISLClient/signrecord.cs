using System;

namespace CISLClient
{
	public class signrecord
	{
		public long id
		{
			get;
			set;
		}

		public int userId
		{
			get;
			set;
		}

		public string zh_name
		{
			get;
			set;
		}

		public bool in_out
		{
			get;
			set;
		}

		public DateTime signTimestamp
		{
			get;
			set;
		}

		public sbyte valid
		{
			get;
			set;
		}
	}
}
