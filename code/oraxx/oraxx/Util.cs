using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace oraxx
{
	internal sealed class Util
	{
		private Util()
		{

		}

		public static string ToString(DateTime value)
		{
			if (value == null)
				return "";
			return value.ToString("yyyy-MM-dd HH:mm:ss") + "." + String.Format("{0:000}", value.Millisecond);
		}

		public static string ToString(object unknown)
		{
			if (unknown == null)
				return "";
			if (unknown is DateTime)
				return ToString((DateTime)unknown);
			return "" + unknown;
		}
	}
}
