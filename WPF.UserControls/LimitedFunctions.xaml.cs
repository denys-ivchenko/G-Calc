using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Telesyk.GraphCalculator.WPF.UserControls
{
	/// <summary>
	/// Interaction logic for LimitedFunctionsUserControl.xaml
	/// </summary>
	public partial class LimitedFunctionsUserControl : UserControl, ITabContent
	{
		#region Private fields

		private List<FunctionUserControl> _functions = new List<FunctionUserControl>();

		#endregion

		#region Constructors

		public LimitedFunctionsUserControl()
		{
			InitializeComponent();
		}

		#endregion

		#region Public Properties

		#endregion

		#region Public methods

		#endregion

		#region Overridies

		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);

			controlActions.TargetState = TargetState;

			Calculator.Current.StateChanged += calculator_StateChanged;

			controlActions.OnAdd += controlActions_OnAdd;
			controlActions.OnApprove += controlActions_OnApprove;
		}

		#endregion

		#region Private methods

		private void controlActions_OnApprove(object sender, ActionsUserControl.ApproveEventArgs args)
		{
			var valid = args.IsValid = isMustApprove();

			if (valid)
			{
				Calculator.Current.ClearFunctions();

				foreach (var control in _functions)
				{
					var function = new Function();

					foreach (var element in control.Elements)
						function.AddElement(element.Operator);

					Calculator.Current.AddFunction(function);
				}
			}
		}

		private void controlActions_OnAdd(object sender, EventArgs e)
		{
			var function = new FunctionUserControl();
			function.OnChanged += function_OnChanged;
			function.OnDelete += function_OnDelete;

			_functions.Add(function);
			panelFunctions.Children.Add(function);

			function.Position = _functions.Count;

			controlActions.MustApprove = false;
		}

		private void function_OnDelete(object sender, EventArgs e)
		{
			var function = (FunctionUserControl)sender;

			_functions.Remove(function);
			panelFunctions.Children.Remove(function);

			controlActions.MustApprove = isMustApprove();
		}

		private void function_OnChanged(object sender, FunctionEventArgs args)
		{
			controlActions.MustApprove = isMustApprove();
		}

		private bool isMustApprove()
		{
			bool unvalid = false;

			foreach (var function in _functions)
				if (!function.IsValid)
				{
					unvalid = true;
					break;
				}

			return _functions.Count > 0 && !unvalid;
		}

		private void calculator_StateChanged(object sender, CalculatorStateEventArgs args)
		{
			controlActions.MustApprove = isMustApprove();
		}

		#endregion

		#region Interface implementations

		public CalcutatorState TargetState { get; } = CalcutatorState.LimitedFunctions;

		#endregion
	}
}
