using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace PowerMapView.Common
{
	public abstract class BindableBase : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
		{
			if (Equals(storage, value))
				return false;

			storage = value;
			this.RaisePropertyChanged(propertyName);
			return true;
		}

		protected void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpression)
		{
			if (propertyExpression == null)
				throw new ArgumentNullException("propertyExpression");

			var memberExpression = propertyExpression.Body as MemberExpression;
			if (memberExpression == null)
				throw new ArgumentException("The Expression is not MemberExpression.");

			var propertyInfo = memberExpression.Member as PropertyInfo;
			if (propertyInfo == null)
				throw new ArgumentException("The MemberExpression is not PropertyInfo.");
			
			var propertyName = memberExpression.Member.Name;
			this.RaisePropertyChanged(propertyName);
		}

		protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = this.PropertyChanged;
			if (handler != null)
				handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}