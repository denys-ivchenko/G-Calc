using System;

namespace Telesyk.GraphCalculator
{
	public sealed class CalculatorStateEventArgs
	{
		#region Constructors

		public CalculatorStateEventArgs(Calculator calculator, CalculatorState stage)
		{
			Calculator = calculator;
			State = stage;
		}

		#endregion

		#region Public Properties

		public Calculator Calculator { get; }

		public CalculatorState State { get; }

		#endregion
	}
	
	public delegate void CalculatorStateEventHandler(object sender, CalculatorStateEventArgs args);

	public sealed class ErrorEventArgs
	{
		#region Constructors

		public ErrorEventArgs(string message)
		{
			Message = message;
		}

		public ErrorEventArgs(string message, string title)
			: this(message)
		{
			Title = title;
		}

		#endregion

		#region Public Properties

		public string Message { get; }

		public string Title { get; }

		#endregion
	}

	public delegate void ErrorEventHandler(object sender, ErrorEventArgs args);
}
