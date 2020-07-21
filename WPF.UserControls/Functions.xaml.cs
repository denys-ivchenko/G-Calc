using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Telesyk.GraphCalculator.WPF.UserControls
{
	/// <summary>
	/// Interaction logic for FunctionsUserControl.xaml
	/// </summary>
	public partial class FunctionsUserControl : UserControl, ITabContent
	{
		#region Private fields

		private List<FunctionUserControl> _functions = new List<FunctionUserControl>();
		private IReadOnlyList<FunctionUserControl> _functionsReadOnly;

		#endregion

		#region Constructors

		public FunctionsUserControl()
		{
			InitializeComponent();

			_functionsReadOnly = new ReadOnlyCollection<FunctionUserControl>(_functions);
		}

		#endregion

		#region Public Properties

		public IReadOnlyList<FunctionUserControl> Functions => _functionsReadOnly;
		public bool IsLimitation { get; set; }

		#endregion

		#region Public methods
		
		public bool LoadFromXml(XmlDocument xml) => loadFromXml(xml);
		
		public void SaveToXml(XmlDocument xml) => saveToXml(xml);

		public bool Approve()
		{
			bool valid = approve();

			if (valid && Calculator.Current.State == TargetState)
			{
				Calculator.Current.Next();
				return true;
			}

			return false;
		}

		public void Clear() => clear();

		#endregion

		#region Interface implementations

		public CalculatorState TargetState { get => IsLimitation ? CalculatorState.LimitationFunctions : CalculatorState.Functions; }

		public event ErrorEventHandler OnError;

		#endregion

		#region Overridies

		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);

			Calculator.Current.StateChanged += calculator_StateChanged;

			controlActions.OnAdd += controlActions_OnAdd;
			controlActions.OnApprove += controlActions_OnApprove;
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			if (IsLimitation)
				textTitle.Text = Strings.LIMITATION_FUNCTIONS;

			controlActions.TargetState = IsLimitation ? CalculatorState.LimitationFunctions : CalculatorState.Functions;
		}

		#endregion

		#region Private methods

		private bool loadFromXml(XmlDocument xml)
		{
			clear();
		
			var nodes = xml.SelectNodes(IsLimitation ? "calculator/limitation-functions/limitation-function" : "calculator/functions/function");

			for (int i = 0; i < nodes.Count; i++)
			{
				var function = addFunction();

				if (!function.LoadFromXml(nodes[i]))
					return false;
			}

			return true;
		}

		private void saveToXml(XmlDocument xml)
		{
			var nodeFunctions = Calculator.InsertNodeToXml(xml.DocumentElement, IsLimitation ? "limitation-functions" : "functions");

			foreach (var function in Functions)
				function.SaveToXml(nodeFunctions);
		}

		private void clear()
		{
			controlActions.AddingMode = ActionsUserControl.AddButtonMode.Enabled;

			foreach (var function in _functions)
				panelFunctions.Children.Remove(function);

			_functions.Clear();
		}

		private FunctionUserControl addFunction()
		{
			var function = new FunctionUserControl(IsLimitation);
			function.OnChanged += function_OnChanged;
			function.OnDelete += function_OnDelete;
			function.OnError += function_OnError;

			_functions.Add(function);
			panelFunctions.Children.Add(function);

			function.Position = _functions.Count;
			controlActions.MustApprove = false;

			if (_functions.Count > 8)
				controlActions.AddingMode = ActionsUserControl.AddButtonMode.Disabled;

			return function;
		}

		private void deleteFunction(FunctionUserControl controlFunction)
		{
			//if (_functions.Count == 5)
			//	panelFunctions.Children.RemoveAt(5);

			_functions.Remove(controlFunction);
			panelFunctions.Children.Remove(controlFunction);

			for (int i = 0; i < _functions.Count; i++)
				_functions[i].Position = i + 1;

			controlActions.MustApprove = isMustApprove();
			controlActions.AddingMode = ActionsUserControl.AddButtonMode.Enabled;
		}

		private void createCalculatorFunctions()
		{
			if (IsLimitation)
				Calculator.Current.ClearLimitedFunctions();
			else
				Calculator.Current.ClearFunctions();

			foreach (var control in _functions)
			{
				Function function = IsLimitation ? new LimitationFunction() : new Function();

				if (IsLimitation)
				{
					((LimitationFunction)function).Condition = control.Condition;
					((LimitationFunction)function).ConditionValue = control.ConditionValue;
				}

				foreach (var element in control.Elements)
					function.AddElement(element.Operator, element.Value);

				if (IsLimitation)
					Calculator.Current.AddLimitationFunction((LimitationFunction)function);
				else
					Calculator.Current.AddFunction(function);
			}
		}

		private bool approve()
		{
			bool valid = isMustApprove();

			if (valid)
				createCalculatorFunctions();

			return valid;
		}

		private bool isMustApprove()
		{
			if (Calculator.Current.State != TargetState)
				return false;
		
			bool unvalid = false;

			foreach (var function in _functions)
				if (!function.IsValid)
				{
					unvalid = true;
					break;
				}

			return _functions.Count > 0 && !unvalid;
		}

		#region Handlers

		private void controlActions_OnAdd(object sender, EventArgs e) => addFunction();

		private void function_OnDelete(object sender, EventArgs e) => deleteFunction((FunctionUserControl)sender);

		private void function_OnChanged(object sender, FunctionEventArgs args)
		{
			controlActions.MustApprove = isMustApprove();
		}

		private void calculator_StateChanged(object sender, CalculatorStateEventArgs args)
		{
			controlActions.MustApprove = isMustApprove();
		}

		private void function_OnError(object sender, ErrorEventArgs args)
		{
			if (OnError != null)
				OnError(sender, args);
		}

		private void controlActions_OnApprove(object sender, ActionsUserControl.ApproveEventArgs args)
		{
			args.IsValid = approve();
		}

		#endregion

		#endregion
	}
}
