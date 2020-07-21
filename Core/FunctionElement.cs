using System;

namespace Telesyk.GraphCalculator
{
	public class FunctionElement
	{
		#region Constructors

		internal FunctionElement(FunctionOperator oper, decimal value)
		{
			Operator = oper;
			Value = value;
		}

		#endregion

		#region Public Properties
	
		public FunctionOperator Operator { get; private set; }

		public decimal Value { get; private set; }

		#endregion
	}
}
