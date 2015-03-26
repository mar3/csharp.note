using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace oraxx
{
	internal sealed class DataService
	{
		private System.Data.IDbConnection _connection = null;
		private System.Data.IDataReader _reader = null; 

		public DataService()
		{

		}

		private System.Data.IDbConnection Connect()
		{
			if (this._connection != null)
				return this._connection;
			this._connection = ConnectionFactory.Open();
			return this._connection;
		}

		private static void Close(System.Data.IDataReader reader)
		{
			if (reader == null)
				return;
			reader.Close();
		}

		private static void Close(System.Data.IDbConnection connection)
		{
			if (connection == null)
				return;
			connection.Close();
		}

		private void Free()
		{
			Close(this._reader);
			this._reader = null;
		}

		private System.Data.IDbCommand CreateCommand(string sql)
		{
			System.Data.IDbConnection connection = this.Connect();
			System.Data.IDbCommand command = connection.CreateCommand();
			command.CommandText = sql;
			return command;
		}

		public System.Data.IDataReader Query(string sql)
		{
			this.Free();
			System.Data.IDbCommand command = this.CreateCommand(sql);
			this._reader = command.ExecuteReader();
			return this._reader;
		}

		public int Execute(string sql)
		{
			try
			{
				System.Data.IDbCommand command = this.CreateCommand(sql);
				return command.ExecuteNonQuery();
			}
			catch (Oracle.ManagedDataAccess.Client.OracleException e)
			{
				if (e.Number == 1)
					return 0; // UNIQUE CONSTRAINT VIOLATION
				throw e;
			}
		}

		public void Close()
		{
			Close(this._reader);
			this._reader = null;
	
			Close(this._connection);
			this._connection = null;
		}
	}
}
