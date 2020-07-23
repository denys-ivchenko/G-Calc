using System;
using System.Collections.Generic;
using System.Text;

namespace Telesyk.GraphCalculator.WPF.UserControls
{
	public class FunctionEventArgs
	{
		public FunctionEventArgs(bool isValid)
		{
			IsValid = isValid;
		}

		public bool IsValid { get; }
	}

	public delegate void FunctionEventHandler(object sender, FunctionEventArgs args);

	public class OperatorEventArgs
	{
		public OperatorEventArgs(int position, FunctionOperator oper)
		{
			Position = position;
			Operator = oper;
		}
	
		public int Position { get; }

		public FunctionOperator Operator { get; }
	}
	
	public delegate void OperatorEventHandler(object sender, OperatorEventArgs args);
}
