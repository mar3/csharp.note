using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace oraxx
{
	internal sealed class ConnectionFactory
	{
		private ConnectionFactory()
		{

		}

		public static System.Data.IDbConnection Open()
		{
			string connection_string = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.141.129)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=xe)));User Id=mass10;Password=mass10;";
			System.Data.IDbConnection connection = new Oracle.ManagedDataAccess.Client.OracleConnection(connection_string);
			connection.Open();
			return connection;
		}

	}
}
