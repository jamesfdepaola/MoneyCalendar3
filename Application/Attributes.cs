using System;

namespace MoneyCalendar.Attributes
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	internal class ObservesPropertyAttribute : Attribute
	{
		public string PropertyName { get; private set; }

		public ObservesPropertyAttribute(string propertyname)
		{
			this.PropertyName = propertyname;
		}
	}

	[AttributeUsage(AttributeTargets.Property)]
	internal class ResetOnToggleAttribute : Attribute 
	{
		public bool NeedsValueReset(object oldvalue, object newvalue, out object resetvalue)
		{
			resetvalue = null;

			if (oldvalue is bool oldboolvalue && newvalue is bool newboolvalue && oldboolvalue == newboolvalue)
			{
				resetvalue = !newboolvalue;
				return true;
			}
			else
				return false;
		}
	}
}
