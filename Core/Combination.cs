using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Telesyk.GraphCalculator
{
	public class Combination
	{
		#region Private fields

		private ReadOnlyCollection<int> _elements;

		#endregion

		#region Constructors

		public Combination(Calculator calculator, decimal value, int[] valueIndexes)
		{
			Value = value;

			int[] elements = new int[valueIndexes.Length];

			for (int i = 0; i < valueIndexes.Length; i++)
				elements[i] = calculator.SetValues[valueIndexes[i]];
		
			_elements = new ReadOnlyCollection<int>(elements);

			for (int i = 0; i < elements.Length; i++)
				Key += (i > 0 ? "." : "") + elements[i].ToString();
		}

		#endregion

		#region Public Properties

		public string Key { get; }

		public IReadOnlyList<int> Elements => _elements;

		public decimal Value { get; }

		#endregion
	}
}
