using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Telesyk.GraphCalculator;

namespace Telesyk.GraphCalculator.WPF.UserControls
{
	/// <summary>
	/// Interaction logic for FunctionElementUserControl.xaml
	/// </summary>
	public partial class FunctionElementUserControl : UserControl
	{
		#region Private fields

		private int _position;
		private FunctionOperator _operator = FunctionOperator.Addition;
		private bool _contextMenuOpened;
		private int _value;

		#endregion

		#region Constructors

		public FunctionElementUserControl()
		{
			InitializeComponent();

			Position = 1;
			Operator = FunctionOperator.Addition;
			Value = 1;
		}

		#endregion

		#region Public Properties

		public FunctionUserControl Function { get; internal set; }

		public int Position
		{
			get { return _position; }
			set
			{
				_position = value;
				textPosition.Text = Position.ToString();

				textOperation.Visibility = Position == 1 ? Visibility.Collapsed : Visibility.Visible;
				menuDelete.IsEnabled = Position != 1;
				menuChange.IsEnabled = Position != 1;
				//Width = Position == 1 ? 34 : 43;
			}
		}

		public int Value
		{
			get { return _value; }
			set
			{
				_value = value;

				if (Value == 1)
				{ 
					textValue.Text = "";

					if (textEdit.Text != "1" && textEdit.Text != "")
						textEdit.Text = "";
				} 
				else if (Value == -1)
				{
					textValue.Text = "-";

					if (textEdit.Text != "-1" && textEdit.Text != "-")
						textEdit.Text = "-";
				}
				else
					textValue.Text = textEdit.Text = Value.ToString();
			}
		}

		public FunctionOperator Operator
		{
			get { return _operator; }
			set
			{
				_operator = value;

				string symbol = null;

				switch (Operator)
				{
					case FunctionOperator.Substraction:
						symbol = "-";
						break;
					case FunctionOperator.Addition:
						symbol = "+";
						break;
				}

				textOperation.Text = symbol;

				menuChangeAddtition.IsEnabled = Operator == FunctionOperator.Substraction;
				menuChangeSubstraction.IsEnabled = Operator == FunctionOperator.Addition;
			}
		}

		public bool IsFirst { get; set; }

		#endregion

		#region Public methods

		#endregion

		#region Internal methods

		internal void DisableAdding()
		{
			menuAdd.IsEnabled = false;
		}

		internal void EnableAdding()
		{
			menuAdd.IsEnabled = true;
		}

		#endregion

		#region Events

		public event OperatorEventHandler OnOperatorChanged;

		public event EventHandler OnChanged;

		#endregion

		#region Overridies

		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);
		}

		//private void calculator_StateChanged(object sender, CalculatorStateEventArgs args)
		//{
			
		//}

		#endregion

		#region Private methods

		private void panelXnOver(bool contextMenu)
		{
			_contextMenuOpened = contextMenu;

			panelXn.Background = new SolidColorBrush(Color.FromRgb(0xa0, 0xd6, 0xfb));
			((Border)panelXn.Parent).BorderBrush = new SolidColorBrush(Color.FromRgb(0x1e, 0x95, 0xe7));
		}

		private void panelXnOut(bool contextMenu)
		{
			if (contextMenu)
				_contextMenuOpened = false;

			if (!_contextMenuOpened)
			{
				panelXn.Background = Brushes.Transparent;
				((Border)panelXn.Parent).BorderBrush = Brushes.Transparent;
			}

			if (textEdit.Text == "-1")
				textEdit.Text = "-";

			if (textEdit.Text == "1")
				textEdit.Text = "";
		}

		#region Handlers

		private void panelXn_MouseMove(object sender, MouseEventArgs e)
		{
			panelXnOver(false);

			if (Calculator.Current.State == (Function.IsLimitation ? CalculatorState.LimitationFunctions : CalculatorState.Functions))
			{ 
				textValue.Visibility = Visibility.Collapsed;
				textEdit.Visibility = Visibility.Visible;

				textEdit.Focus();
			}
		}

		private void panelXn_MouseLeave(object sender, MouseEventArgs e)
		{
			var text = textEdit.Text;

			if (text == "-")
				text = "-1";

			if (!int.TryParse(text, out int value))
				value = 1;
		
			Value = value;
			panelXnOut(false);

			textValue.Visibility = Visibility.Visible;
			textEdit.Visibility = Visibility.Collapsed;
			textEdit.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
		}

		private void panelXn_ContextMenuOpening(object sender, ContextMenuEventArgs e)
		{
			e.Handled = (Calculator.Current.State != Function.TargetState);
		
			panelXnOver(true);
		}

		private void panelXn_ContextMenuClosing(object sender, ContextMenuEventArgs e)
		{
			panelXnOut(true);
		}

		private void panelXn_MouseDown(object sender, MouseButtonEventArgs e)
		{
			menuContext.IsOpen = Calculator.Current.State == Function.TargetState;

			panelXnOver(true);
		}

		private void menuAddition_Click(object sender, RoutedEventArgs e)
		{
			Function.AddElement(FunctionOperator.Addition, Position + 1);
		}

		private void menuSubstraction_Click(object sender, RoutedEventArgs e)
		{
			Function.AddElement(FunctionOperator.Substraction, Position + 1);
		}

		private void menuDelete_Click(object sender, RoutedEventArgs e)
		{
			Function.RemoveElement(Position);
		}

		private void menuChangeAddition_Click(object sender, RoutedEventArgs e)
		{
			Operator = FunctionOperator.Addition;

			if (OnOperatorChanged != null)
				OnOperatorChanged(this, new OperatorEventArgs(Position, Operator));
		}

		private void menuChangeSubstraction_Click(object sender, RoutedEventArgs e)
		{
			Operator = FunctionOperator.Substraction;

			if (OnOperatorChanged != null)
				OnOperatorChanged(this, new OperatorEventArgs(Position, Operator));
		}

		private void textValue_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			textEdit.Text = textValue.Text;
			textEdit.SelectAll();
		}

		private void textEdit_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			Regex regex = new Regex("[^0-9]+");

			e.Handled = regex.IsMatch(e.Text) && e.Text != "-";

			if (e.Text.StartsWith("-"))
				textEdit.MaxLength = 3;
		}

		private void textEdit_TextChanged(object sender, TextChangedEventArgs e)
		{
			while (textEdit.Text.LastIndexOf('-') > 0)
				textEdit.Text = textEdit.Text.Remove(textEdit.Text.LastIndexOf('-'), 1);

			if (textEdit.Text == "-")
				_value = -1;
			else if (!int.TryParse(textEdit.Text, out int value))
				_value = 1;
			else
				Value = value;

			textEdit.MaxLength = Value < 0 ? 3 : 2;
			labelBracketOpen.Visibility = labelBracketClose.Visibility = Value < 0 ? Visibility.Visible : Visibility.Collapsed;

			if (OnChanged != null)
				OnChanged(this, EventArgs.Empty);
		}

		#endregion

		#endregion
	}
}
