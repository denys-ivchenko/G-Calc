using System;
using System.Collections.Generic;
using System.Text;

namespace Telesyk.GraphCalculator
{
	public struct DublicateIndex
	{
		public DublicateIndex(int index, int targetIndex)
		{
			Index = index;
			TargetIndex = targetIndex;
		}
	
		public int Index { get; set; }

		public int TargetIndex { get; set; }
	}
}
