using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telesyk.GraphCalculator
{
	public class Processor
	{
		#region Private fields

		private List<Combination> _results = new List<Combination>();
		private ReadOnlyCollection<Combination> _resultsReadOnly;

		private ReadOnlyDictionary<int, DublicateIndex> _dublicateIndexesReadOnly;
		private DateTime _start;
		private Task<Processor> _task;
		private List<Task> _tasks = new List<Task>();

		#endregion

		#region Constructors

		public Processor(Calculator calculator)
		{
			Calculator = calculator;
			_resultsReadOnly = new ReadOnlyCollection<Combination>(_results);
		}

		#endregion

		#region Public Properties

		public Task<Processor> Calculate()
		{
			_task = calculate();

			return _task;
		}

		public Calculator Calculator { get; private set; }

		public IReadOnlyDictionary<int, DublicateIndex> DublicateIndexes => _dublicateIndexesReadOnly;

		public IReadOnlyList<Combination> Results => _resultsReadOnly;

		public decimal ResultValue { get; private set; }

		public TimeSpan Duration { get; private set; }

		public int Combinations { get; private set; }

		#endregion

		#region Public methods

		#endregion

		#region Private methods

		#region Calculating

		private async Task<Processor> calculate()
		{
			var dublicates = Calculator.GetDublicateIndexesOfSetVelues();
			_dublicateIndexesReadOnly = new ReadOnlyDictionary<int, DublicateIndex>(dublicates);
			_start = DateTime.Now;

			await Task.Run(() => _eachCombinations());

			//foreach (Task task in _tasks)
			//	task.Wait();

			Duration = DateTime.Now - _start;

			return this;
		}

		private void _eachCombinations() => _eachCombinations(new int[0]);

		private void _eachCombinations(int[] usedIndexes) => _eachCombinations(usedIndexes, false);

		private void _eachCombinations(int[] usedIndexes, bool ignore)
		{
			if (usedIndexes.Length > 0 || ignore)
			{
				for (int i = 0; i < Calculator.SetValues.Count; i++)
				{
					bool skip = false;

					if (DublicateIndexes.ContainsKey(i))
						skip = !usedIndexes.Contains(DublicateIndexes[i].TargetIndex);

					if (skip)
						skip = true;

					if (usedIndexes.Contains(i) || skip)
						continue;

					int[] indexes = new int[usedIndexes.Length + 1];

					for (int j = 0; j < usedIndexes.Length; j++)
						indexes[j] = usedIndexes[j];

					indexes[indexes.Length - 1] = i;

					if (indexes.Length == Calculator.Placement)
					{
						Combinations++;
					
						if (isInLimitations(indexes))
						{
							decimal value = getFunctionValue(Calculator.CombinedFunction, indexes);

							if (Math.Abs(value) <= ResultValue || Results.Count == 0)
							{
								Combination result = new Combination(Calculator, value, indexes);

								if (Math.Abs(value) < ResultValue || Results.Count == 0)
								{
									ResultValue = Math.Abs(value);
									_results.Clear();
								}

								_results.Add(result);
							}
						}
					}
					else
						_eachCombinations(indexes);
				}
			}
			else
				_eachCombinations(usedIndexes, true);
		}

		private bool isInLimitations(int[] valueIndexes)
		{
			foreach (var function in Calculator.LimitationFunctions)
			{
				decimal result = getFunctionValue(function, valueIndexes);

				if (function.Condition == LimitationFunctionCondition.LessThan && !(result < function.ConditionValue))
					return false;

				if (function.Condition == LimitationFunctionCondition.LessThanOrEqual && !(result <= function.ConditionValue))
					return false;

				if (function.Condition == LimitationFunctionCondition.GreaterThanOrEqual && !(result >= function.ConditionValue))
					return false;

				if (function.Condition == LimitationFunctionCondition.GreaterThan && !(result > function.ConditionValue))
					return false;
			}

			return true;
		}

		private decimal getFunctionValue(Function function, int[] valueIndexes)
		{
			decimal result = 0;

			for (int i = 0; i < function.Elements.Count; i++)
			{
				var value = function.Elements[i].Value * Calculator.SetValues[valueIndexes[i]];
				result = function.Elements[i].Operator == FunctionOperator.Addition ? result + value : result - value;
			}

			return result;
		}

		#endregion

		#endregion
	}
}
