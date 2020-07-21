using System;

namespace Telesyk.GraphCalculator
{
	public enum CalculatorState
	{
		Undefined = 0,
		SetValues = 1,
		Placement = 2,
		Functions = 3,
		LimitationFunctions = 4,
		Calculating = 5,
		Calculated = 6
	}
}
