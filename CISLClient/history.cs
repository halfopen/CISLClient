using System;

namespace CISLClient
{
	public class history
	{
		public int id
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

		public DateTime in_timestamp
		{
			get;
			set;
		}

		public DateTime out_timestamp
		{
			get;
			set;
		}

		public int valid_time
		{
			get;
			set;
		}

		public sbyte allowance
		{
			get;
			set;
		}

		public int duration
		{
			get;
			set;
		}
	}
}
