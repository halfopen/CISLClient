using System;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace CISLClient
{
	public class infosysEntities : DbContext
	{
		public DbSet<finance> finance
		{
			get;
			set;
		}

		public DbSet<review> review
		{
			get;
			set;
		}

		public DbSet<user> user
		{
			get;
			set;
		}

		public DbSet<history> history
		{
			get;
			set;
		}

		public DbSet<signrecord> signrecord
		{
			get;
			set;
		}

		public infosysEntities() : base("name=infosysEntities")
		{
			DbConnection expr_18 = base.Database.Connection;
			expr_18.ConnectionString += ";password=fudancscisl;Charset=utf8;";
		}

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			throw new UnintentionalCodeFirstException();
		}
	}
}
