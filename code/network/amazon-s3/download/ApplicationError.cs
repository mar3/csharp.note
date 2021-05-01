using System;
using System.Collections.Generic;
using System.Text;

namespace download
{
    internal sealed class ApplicationError : Exception
    {
        private string _description = "";

        public ApplicationError(string description)
		{
            _description = description;
        }

        public override string ToString()
		{
            return this._description;
		}

		public override string Message
		{
            get
			{
				return this._description;
			}
		}
	}
}
