using System;

namespace Telesyk.GraphCalculator
{
	public sealed class CalculatorWrongStateActionException : ApplicationException
	{
		#region Constructors

		public CalculatorWrongStateActionException(CalculatorState currentState, CalculatorState targetState, Calculator.Action action)
		{
			CurrentState = currentState;
			TargetState = targetState;
			Action = action;
		}

		//public CalculatorWrongStateActionException(CalcutatorState currentState, CalcutatorState targetState)
		//	=> new CalculatorWrongStateActionException(currentState, targetState, Calculator.Action.Action);

		#endregion

		#region Public Properties

		public CalculatorState CurrentState { get; private set; }

		public CalculatorState TargetState { get; private set; }

		public Calculator.Action Action { get; private set; }

		#endregion

		#region Overridies

		public override string Message { get => $"Спроба здійснення змін невідповідних стану.\r\nПоточний стан: {CurrentState}, Цільовий стан: {TargetState}, Дія: {Action}"; }

		#endregion
	}
}
