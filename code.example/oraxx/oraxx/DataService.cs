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

		public System.Data.IDataReader Query(string sql)
		{
			if (this._reader != null)
				this._reader.Close();
			System.Data.IDbConnection connection = this.Connect();
			System.Data.IDbCommand command = connection.CreateCommand();
			command.CommandText = sql;
			this._reader = command.ExecuteReader();
			return this._reader;
		}

		public void Close()
		{
			if (this._reader != null)
				this._reader.Close();
			this._reader = null;
			if (this._connection != null)
				this._connection.Close();
			this._connection = null;
		}
	}
}
