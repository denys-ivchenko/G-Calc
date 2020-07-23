using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;

namespace Telesyk.GraphCalculator
{
	public class Calculator
	{
		#region Private fields

		private static Lazy<Calculator> _current = new Lazy<Calculator>(new Calculator());

		private List<int> _setValues = new List<int>();
		private ReadOnlyCollection<int> _setValuesReadOnly;

		private List<Function> _functions = new List<Function>();
		private ReadOnlyCollection<Function> _functionsReadOnly;

		private List<LimitationFunction> _limitationFunctions = new List<LimitationFunction>();
		private ReadOnlyCollection<LimitationFunction> _limitationFunctionsReadOnly;

		#endregion

		#region Constructors

		private Calculator()
		{
			_setValuesReadOnly = new ReadOnlyCollection<int>(_setValues);
			_functionsReadOnly = new ReadOnlyCollection<Function>(_functions);
			_limitationFunctionsReadOnly = new ReadOnlyCollection<LimitationFunction>(_limitationFunctions);
		}

		#endregion

		#region Public Properties

		public static Calculator Current
		{
			get { return _current.Value; }
		}

		public IReadOnlyList<int> SetValues { get => _setValuesReadOnly; }

		public IReadOnlyList<Function> Functions { get => _functionsReadOnly; }

		public IReadOnlyList<LimitationFunction> LimitationFunctions { get => _limitationFunctionsReadOnly; }

		public int Placement { get; private set; }

		public CalculatorState State { get; private set; } = CalculatorState.Undefined;

		public CalculatorState MaxState { get; private set; } = CalculatorState.Undefined;

		public Function CombinedFunction { get; private set; }

		#endregion

		#region Public methods

		public void Start()
		{
			setState(CalculatorState.SetValues);
		}

		public void Next()
		{
			setState(State + 1);
		}

		public void Return(CalculatorState state)
		{
			if (state < State)
				setState(state);
		}

		public void Clear() => clear();

		public static XmlNode InsertNodeToXml(XmlNode parent, string name) => insertNodeToXml(parent, name);

		public IDictionary<int, DublicateIndex> GetDublicateIndexesOfSetVelues() => getDublicateIndexesOfSetVelues();

		#region Add/Remove

		public void AddSetValue(int value)
		{
			execute(() => _setValues.Add(value), CalculatorState.SetValues, Action.Add);
		}

		public void RemoveSetValue(int value)
		{
			execute(() => _setValues.Remove(value), CalculatorState.SetValues, Action.Remove);
		}

		public void ClearSetVelues() => ClearSetVelues(false);

		public void ClearSetVelues(bool ignoreState)
		{
			if (ignoreState)
				_setValues.Clear();
			else
				execute(() => _setValues.Clear(), CalculatorState.SetValues, Action.Clear);
		}

		public void ClearPlacement() => ClearPlacement(false);

		public void ClearPlacement(bool ignoreState)
		{
			if (ignoreState)
				Placement = 0;
			else
				execute(() => Placement = 0, CalculatorState.Placement, Action.Clear);
		}

		public void SetPlacement(int value)
		{
			execute(() => Placement = value, CalculatorState.Placement, Action.Set);
		}

		public void ClearFunctions() => ClearFunctions(false);

		public void ClearFunctions(bool ignoreState)
		{
			if (ignoreState)
				_functions.Clear();
			else
				execute(() => _functions.Clear(), CalculatorState.Functions, Action.Clear);
		}

		public void AddFunction(Function function)
		{	
			execute(() => _functions.Add(function), CalculatorState.Functions, Action.Add);
		}

		public void RemoveFunction(Function function)
		{
			execute(() => _functions.Remove(function), CalculatorState.Functions, Action.Remove);
		}

		public void ClearLimitedFunctions() => ClearLimitationFunctions(false);

		public void ClearLimitationFunctions(bool ignoreState)
		{
			if (ignoreState)
				_limitationFunctions.Clear();
			else
				execute(() => _limitationFunctions.Clear(), CalculatorState.LimitationFunctions, Action.Clear);
		}

		public void AddLimitationFunction(LimitationFunction function)
		{
			execute(() => _limitationFunctions.Add(function), CalculatorState.LimitationFunctions, Action.Add);
		}

		public void RemoveLimitedFunction(LimitationFunction function)
		{
			execute(() => _limitationFunctions.Remove(function), CalculatorState.LimitationFunctions, Action.Remove);
		}

		#endregion

		#endregion

		#region Events

		public event CalculatorStateEventHandler StateChanged;

		public event ErrorEventHandler OnError;

		#endregion

		#region Private methods

		private void clear()
		{
			State = MaxState = CalculatorState.Undefined;

			ClearSetVelues(true);
			ClearPlacement(true);
			ClearFunctions(true);
			ClearLimitationFunctions(true);
		}

		private void setState(CalculatorState state)
		{
			State = state;

			if (MaxState < State)
				MaxState = State;

			stateChanged();
		}

		private void stateChanged()
		{
			if (State == CalculatorState.LimitationFunctions)
				CombinedFunction = getCombinedFunction();

			if (State < CalculatorState.LimitationFunctions)
				CombinedFunction = null;
		
			if (StateChanged != null)
				StateChanged(this, new CalculatorStateEventArgs(this, State));
		}

		private static XmlNode insertNodeToXml(XmlNode parent, string name)
		{
			var element = ((XmlDocument)(parent.OwnerDocument == null ? parent : parent.OwnerDocument)).CreateElement(name);
			parent.AppendChild(element);

			return element;
		}

		private void execute(System.Action action, CalculatorState actionState, Action executionAction)
		{
			if (State == actionState)
				action();
			else
				throw new CalculatorWrongStateActionException(State, actionState, executionAction);
		}

		private Function getCombinedFunction()
		{
			Function combined = new Function();

			for (int i = 0; i < Placement; i++)
			{
				decimal value = 0;

				foreach (var function in Functions)
					if (function.Elements[i].Operator == FunctionOperator.Addition)
						value += function.Elements[i].Value;
					else
						value -= function.Elements[i].Value;

				value /= Functions.Count;

				combined.AddElement(FunctionOperator.Addition, value);
			}

			return combined;
		}

		public Dictionary<int, DublicateIndex> getDublicateIndexesOfSetVelues()
		{
			Dictionary<int, DublicateIndex> dublicates = new Dictionary<int, DublicateIndex>();

			for (int i = 0; i < SetValues.Count; i++)
				if (_setValues.IndexOf(SetValues[i]) < i)
					dublicates.Add(i, new DublicateIndex(i, _setValues.IndexOf(SetValues[i])));

			return dublicates;
		}

		#endregion

		#region Subsidiaries

		public enum Action
		{
			Clear, Add, Remove, Set
		}

		#endregion

		#region Commented

		#region Public XML methods

		//public void SaveToXml(XmlDocument xml) => saveToXml(xml);

		//public void LoadFromXml(string fileName) => loadFromXml(fileName);

		#endregion

		#region Save to XML

		//private delegate void SaveToXmlDelegate(XmlDocument xml);

		//private void saveToXml(XmlDocument xml)
		//{
		//	SaveToXmlDelegate[] saves = new SaveToXmlDelegate[] { saveRootToXml, saveSetValuesToXml, savePlacementToXml, saveFunctionsToXml, saveLimitationFunctionsToXml };

		//	foreach (var save in saves)
		//		save(xml);
		//}

		//private void saveRootToXml(XmlDocument xml)
		//{
		//	var nodeCalculator = insertNodeToXml(xml, "calculator");

		//	var attrState = xml.CreateAttribute("state");
		//	attrState.InnerText = State.ToString();

		//	var attrMaxState = xml.CreateAttribute("max-state");
		//	attrMaxState.InnerText = MaxState.ToString();
		//}

		//private void saveSetValuesToXml(XmlDocument xml)
		//{
		//	var nodeSetValues = InsertNodeToXml(xml.DocumentElement, "set-values");

		//	for (int i = 0; i < _setValuesReadOnly.Count; i++)
		//	{
		//		var nodeValue = insertNodeToXml(nodeSetValues, "value");
		//		nodeValue.InnerText = _setValuesReadOnly[i].ToString();
		//	}
		//}

		//private void savePlacementToXml(XmlDocument xml)
		//{
		//	var root = insertNodeToXml(xml.DocumentElement, "placement");
		//	root.InnerText = Placement.ToString();
		//}

		//private void saveFunctionsToXml(XmlDocument xml) => saveFunctionsToXml(xml, false);

		//private void saveLimitationFunctionsToXml(XmlDocument xml) => saveFunctionsToXml(xml, true);

		//private void saveFunctionsToXml(XmlDocument xml, bool isLimitation)
		//{
		//	var nodeFunctions = insertNodeToXml(xml.DocumentElement, isLimitation ? "limitation-functions" : "functions");

		//	for (int i = 0; i < (isLimitation ? LimitationFunctions : Functions).Count; i++)
		//	{
		//		var nodeFunction = insertNodeToXml(nodeFunctions, "function");
		//		var nodeElements = insertNodeToXml(nodeFunction, "elements");

		//		foreach (var element in (isLimitation ? LimitationFunctions : Functions)[i].Elements)
		//		{
		//			var nodeElement = insertNodeToXml(nodeElements, "element");

		//			var nodeOperator = insertNodeToXml(nodeElement, "operator");
		//			nodeOperator.InnerText = element.Operator.ToString();

		//			var nodeValue = insertNodeToXml(nodeElement, "value");
		//			nodeValue.InnerText = element.Value.ToString();
		//		}

		//		if (isLimitation)
		//		{
		//			var nodeLimitation = insertNodeToXml(nodeFunction, "limitation");

		//			var nodeCondition = insertNodeToXml(nodeLimitation, "condition");
		//			nodeCondition.InnerText = LimitationFunctions[i].Condition.ToString();

		//			var nodeConditionValue = insertNodeToXml(nodeLimitation, "value");
		//			nodeConditionValue.InnerText = LimitationFunctions[i].ConditionValue.ToString();
		//		}
		//	}
		//}

		#endregion

		#region Load from XML

		//private delegate bool LoadFromXmlDelegate(XmlDocument xml);

		//private bool loadFromXml(string fileName)
		//{
		//	XmlDocument xml = new XmlDocument();

		//	try { xml.Load(fileName); }
		//	catch
		//	{
		//		if (OnError != null)
		//			OnError(this, new ErrorEventArgs("Не вдалося завантажити файл. Файл пошкоджений або невірного формату."));

		//			return false;
		//	}

		//	Clear();

		//	LoadFromXmlDelegate[] loads = new LoadFromXmlDelegate[] { loadRootFromXml, loadSetValuesFromXml, loadPlacementFromXml, loadFunctionsFromXml, loadLimitationFunctionsFromXml };

		//	foreach(var load in loads)
		//		if (!load(xml))
		//			return false;

		//	return true;
		//}

		//private bool loadRootFromXml(XmlDocument xml)
		//{
		//	var nodeCalculator = xml.SelectSingleNode("calculator");

		//	if (nodeCalculator == null)
		//	{
		//		if (OnError != null)
		//			OnError(this, new ErrorEventArgs("Не вдалося прочитати корінний елемент."));

		//		return false;
		//	}

		//	string stateValue = null;

		//	try { stateValue = nodeCalculator.Attributes["state"].InnerText; }
		//	catch
		//	{
		//		if (OnError != null)
		//			OnError(this, new ErrorEventArgs("Не вдалося отримати значення стану."));

		//		return false;
		//	}

		//	if (!Enum.TryParse<CalculatorState>(stateValue, out CalculatorState state))
		//	{
		//		if (OnError != null)
		//			OnError(this, new ErrorEventArgs("Не вдалося прочитати значення стану."));

		//		return false;
		//	}

		//	string maxStateValue = null;

		//	try { maxStateValue = nodeCalculator.Attributes["max-state"].InnerText; }
		//	catch
		//	{
		//		if (OnError != null)
		//			OnError(this, new ErrorEventArgs("Не вдалося отримати значення максимального стану."));

		//		return false;
		//	}

		//	if (!Enum.TryParse<CalculatorState>(maxStateValue, out CalculatorState maxState))
		//	{
		//		if (OnError != null)
		//			OnError(this, new ErrorEventArgs("Не вдалося прочитати значення максимального стану."));

		//		return false;
		//	}

		//	State = state;
		//	MaxState = state;

		//	return true;
		//}

		//private bool loadSetValuesFromXml(XmlDocument xml)
		//{
		//	CalculatorState state = State;
		//	State = CalculatorState.SetValues;

		//	var nodes = xml.SelectNodes("calculator/set-values/value");

		//	for (int i = 0; i < nodes.Count; i++)
		//	{
		//		if (!int.TryParse(nodes[i].InnerText, out int value))
		//		{
		//			if (OnError != null)
		//				OnError(this, new ErrorEventArgs("Не вдалося прочитати значення множини."));

		//			return false;
		//		}

		//		AddSetValue(value);
		//	}

		//	State = state;
		//	return true;
		//}

		//private bool loadPlacementFromXml(XmlDocument xml)
		//{
		//	CalculatorState state = State;
		//	State = CalculatorState.Placement;

		//	var nodePlacement = xml.SelectSingleNode("calculator/placement");

		//	if (nodePlacement == null)
		//	{
		//		if (OnError != null)
		//			OnError(this, new ErrorEventArgs("Не вдалося отримати розміщення."));

		//		return false;
		//	}

		//	if (!int.TryParse(nodePlacement.InnerText, out int value))
		//	{
		//		if (OnError != null)
		//			OnError(this, new ErrorEventArgs("Не вдалося прочитати розміщення."));

		//		return false;
		//	}

		//	SetPlacement(value);

		//	State = state;
		//	return true;
		//}

		//private bool loadFunctionsFromXml(XmlDocument xml) => loadFunctionsFromXml(xml, false);

		//private bool loadLimitationFunctionsFromXml(XmlDocument xml) => loadFunctionsFromXml(xml, true);

		//private bool loadFunctionsFromXml(XmlDocument xml, bool isLimitation)
		//{
		//	CalculatorState state = State;
		//	State = isLimitation ? CalculatorState.LimitationFunctions : CalculatorState.Functions;

		//	string xPath = isLimitation ? "calculator/limitation-functions/function" : "calculator/functions/function";

		//	var nodes = xml.SelectNodes(xPath);

		//	for (int i = 0; i < nodes.Count; i++)
		//	{
		//		var function = isLimitation ? new LimitationFunction() : new Function();

		//		var nodesElement = nodes[i].SelectNodes("elements/element");

		//		foreach (XmlNode nodeElement in nodesElement)
		//		{
		//			var nodeOperator = nodeElement.SelectSingleNode("operator");

		//			if (nodeOperator == null)
		//			{
		//				if (OnError != null)
		//					OnError(this, new ErrorEventArgs($"Не вдалося отримати оператор елемента {(isLimitation ? "обмежувальної " : null)}функції."));

		//				return false;
		//			}

		//			if (!Enum.TryParse<FunctionOperator>(nodeOperator.InnerText, out FunctionOperator oper))
		//			{
		//				if (OnError != null)
		//					OnError(this, new ErrorEventArgs($"Не вдалося прочитати оператор елемента {(isLimitation ? "обмежувальної " : null)}функції."));

		//				return false;
		//			}

		//			var nodeValue = nodeElement.SelectSingleNode("value");

		//			if (nodeValue == null)
		//			{
		//				if (OnError != null)
		//					OnError(this, new ErrorEventArgs($"Не вдалося отримати значення елемента {(isLimitation ? "обмежувальної " : null)}функції."));

		//				return false;
		//			}

		//			if (!int.TryParse(nodeValue.InnerText, out int value))
		//			{
		//				if (OnError != null)
		//					OnError(this, new ErrorEventArgs($"Не вдалося прочитати значення елемента {(isLimitation ? "обмежувальної " : null)}функції."));

		//				return false;
		//			}

		//			function.AddElement(oper, value);
		//		}

		//		if (isLimitation)
		//		{
		//			var nodeLimitationCondition = nodes[i].SelectSingleNode("limitation/condition");

		//			if (nodeLimitationCondition == null)
		//			{
		//				if (OnError != null)
		//					OnError(this, new ErrorEventArgs($"Не вдалося отримати умову обмеження обмежувальної функції."));

		//				return false;
		//			}

		//			if (!Enum.TryParse<LimitationFunctionCondition>(nodeLimitationCondition.InnerText, out LimitationFunctionCondition condition))
		//			{
		//				if (OnError != null)
		//					OnError(this, new ErrorEventArgs($"Не вдалося прочитати умову обмеження обмежувальної функції."));

		//				return false;
		//			}

		//			var nodeLimitationValue = nodes[i].SelectSingleNode("limitation/value");

		//			if (nodeLimitationValue == null)
		//			{
		//				if (OnError != null)
		//					OnError(this, new ErrorEventArgs($"Не вдалося отримати значення обмеження обмежувальної функції."));

		//				return false;
		//			}

		//			if (!int.TryParse(nodeLimitationValue.InnerText, out int conditionValue))
		//			{
		//				if (OnError != null)
		//					OnError(this, new ErrorEventArgs($"Не вдалося прочитати значення обмеження обмежувальної функції."));

		//				return false;
		//			}

		//			((LimitationFunction)function).Condition = condition;
		//			((LimitationFunction)function).ConditionValue = conditionValue;

		//			AddLimitationFunction((LimitationFunction)function);
		//		}
		//		else
		//			AddFunction(function);
		//	}

		//	State = state;
		//	return true;
		//}

		#endregion

		#endregion

		#region Commented

		//private XmlDocument getDataXml()
		//{
		//	if (MaxState > CalculatorState.ValueSet)
		//	{
		//		XmlDocument xml = new XmlDocument();
		//		var nodeCalculator = insertNodeToXml(xml, "calculator");

		//		writeSetToXml(nodeCalculator);
		//		writePlacementToXml(nodeCalculator);
		//		writeFunctionsToXml(nodeCalculator);
		//		writeLimitedFunctionsToXml(nodeCalculator);

		//		return xml;
		//	}

		//	return null;
		//}

		//private void writeSetToXml(XmlNode nodeCalculator)
		//{
		//	var root = insertNodeToXml(nodeCalculator, "set-values");

		//	for (int i = 0; i < ValuesSet.Count; i++)
		//	{
		//		var nodeValue = insertNodeToXml(root, "value");
		//		nodeValue.InnerText = ValuesSet[i].ToString();
		//	}
		//}

		//private void writePlacementToXml(XmlNode nodeCalculator)
		//{
		//	var root = insertNodeToXml(nodeCalculator, "placement");
		//	root.InnerText = Placement.ToString();
		//}

		//private void writeFunctionsToXml(XmlNode nodeCalculator)
		//{
		//	var root = insertNodeToXml(nodeCalculator, "functions");

		//	for (int i = 0; i < Functions.Count; i++)
		//	{
		//		var nodeFunction = insertNodeToXml(root, "function");
		//		var nodeElements = insertNodeToXml(nodeFunction, "elements");

		//		foreach (var element in Functions[i].Elements)
		//		{ 
		//			var nodeElement= insertNodeToXml(nodeElements, "element");

		//			var nodeOperator = insertNodeToXml(nodeElement, "operator");
		//			nodeOperator.InnerText = element.Operator.ToString();

		//			var nodeValue = insertNodeToXml(nodeElement, "value");
		//			nodeValue.InnerText = element.Value.ToString();
		//		}
		//	}
		//}

		//private void writeLimitedFunctionsToXml(XmlNode nodeCalculator)
		//{
		//	var root = insertNodeToXml(nodeCalculator, "limited-functions");

		//	for (int i = 0; i < LimitationFunctions.Count; i++)
		//	{
		//		var nodeFunction = insertNodeToXml(root, "function");
		//		var nodeElements = insertNodeToXml(nodeFunction, "elements");

		//		foreach (var element in LimitationFunctions[i].Elements)
		//		{
		//			var nodeElement = insertNodeToXml(nodeElements, "element");

		//			var nodeOperator = insertNodeToXml(nodeElement, "operator");
		//			nodeOperator.InnerText = element.Operator.ToString();

		//			var nodeValue = insertNodeToXml(nodeElement, "value");
		//			nodeValue.InnerText = element.Value.ToString();
		//		}

		//		var nodeLimitation = insertNodeToXml(nodeFunction, "limitation");

		//		var nodeCondition = insertNodeToXml(nodeLimitation, "condition");
		//		nodeCondition.InnerText = LimitationFunctions[i].Condition.ToString();

		//		var nodeConditionValue = insertNodeToXml(nodeLimitation, "value");
		//		nodeConditionValue.InnerText = LimitationFunctions[i].ConditionValue.ToString();

		//	}
		//}

		#endregion
	}
}
