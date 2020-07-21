using System;
using System.Collections.Generic;
using System.Text;

namespace Telesyk.GraphCalculator.WPF.Calculation
{
	public class Processor
	{
		#region Private fields

		private static Lazy<Processor> _current = new Lazy<Processor>(() => new Processor());

		#endregion

		#region Constructors

		private Processor()
		{
			//Calculator = new Calculator();
		}

		#endregion

		#region Public Properties

		public static Processor Current => _current.Value;

		public Calculator Calculator { get; }

		#endregion

		#region Public methods

		#endregion

		#region Private methods

		#endregion

		#region Interface implementations

		#endregion

		#region

		#endregion
	}
}
