using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess;

namespace oraxx
{
	public sealed class Program
	{
		public static void Main(string[] args)
		{
			DataService s = new DataService();

	
			
			try
			{
				System.Data.IDataReader reader = s.Query("SELECT CURRENT_TIMESTAMP FROM DUAL");
				while (reader.Read())
				{
					object value = reader[0];
					Console.WriteLine(Util.ToString(value));
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
			finally
			{
				if (s != null)
					s.Close();
			}
		}
	}
}
