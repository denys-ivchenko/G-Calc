using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Telesyk.GraphCalculator
{
	public class Function
	{
		#region Private fields

		private List<FunctionElement> _elements = new List<FunctionElement>();
		private ReadOnlyCollection<FunctionElement> _elementsReadOnly;

		#endregion

		#region Constructors

		public Function()
		{
			_elementsReadOnly = new ReadOnlyCollection<FunctionElement>(_elements);
		}

		#endregion

		#region Public Properties

		public IReadOnlyList<FunctionElement> Elements { get { return _elementsReadOnly; } }

		#endregion

		#region Public methods

		public void AddElement(FunctionOperator oper, decimal value)
		{
			var element = new FunctionElement(oper, value);
			_elements.Add(element);
		}

		#endregion
	}
}
