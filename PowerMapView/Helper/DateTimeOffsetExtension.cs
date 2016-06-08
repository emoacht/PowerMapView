using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerMapView.Helper
{
	public static class DateTimeOffsetExtension
	{
		public static DateTimeOffset ToJst(this DateTimeOffset source)
		{
			return source.ToOffset(TimeSpan.FromHours(9));
		}
	}
}