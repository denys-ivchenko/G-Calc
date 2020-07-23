using System;
using System.Collections.Generic;
using System.Text;

namespace Telesyk.GraphCalculator.WPF.UserControls
{
	interface ITabContent
	{
		public const string STRING_ERROR_READ_FILE = "Помилка читання файлу";
		CalculatorState TargetState { get; }
		event ErrorEventHandler OnError;
	}
}
