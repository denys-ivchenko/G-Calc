using System;

namespace Telesyk.GraphCalculator
{
	public class LimitationFunction : Function
	{
		#region Constructors

		public LimitationFunction()
		{
			
		}

		#endregion

		#region Public Properties

		public LimitationFunctionCondition Condition { get; set; }

		public int ConditionValue { get; set; }

		#endregion
	}
}
