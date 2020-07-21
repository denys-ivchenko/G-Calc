using System;
using System.Collections.Generic;
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
using System.Xml;

namespace Telesyk.GraphCalculator.WPF.UserControls
{
	/// <summary>
	/// Interaction logic for ValueSetElementUserControl.xaml
	/// </summary>
	public partial class SetValuesElementUserControl : UserControl
	{
		#region Private fields

		private int _position;

		#endregion

		#region Constructors

		public SetValuesElementUserControl (int position)
			: this()
		{
			Position = position;
			Value = Position;
		}
	
		private SetValuesElementUserControl()
		{
			InitializeComponent();
		}

		#endregion

		#region Public Properties

		public int Position
		{ 
			get { return _position; }
			set 
			{
				if (value < 1 || value > 9)
					throw new ArgumentOutOfRangeException("position", "1 .. 9");

				labelPosition.Content = $"{value}:";
				_position = value; 
			}
		}

		public int Value
		{
			get 
			{
				int.TryParse(textEdit.Text, out int value);
				return value;
			}
			set { textValue.Content = textEdit.Text = value.ToString(); }
		}

		public bool IsValid
		{
			get { return Value > 0 && Value < 1000; }
		}

		#endregion

		#region Public methods

		#endregion

		#region Public events

		public event EventHandler<int> ValueChanged;

		public event EventHandler OnDelete;

		#endregion

		#region Overridies

		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);

			Calculator.Current.StateChanged += calculator_StateChanged;

			checkState();
		}

		#endregion

		#region Private methods

		private void checkState()
		{
			imageDelete.Visibility = Calculator.Current.State == CalculatorState.SetValues ? Visibility.Visible : Visibility.Collapsed;
		}

		#region Handlers

		private void value_Changed(object sender, TextChangedEventArgs e)
		{
			textValue.Content = textEdit.Text;
			
			if (ValueChanged != null)
				ValueChanged(this, Value);
		}

		private void calculator_StateChanged(object sender, CalculatorStateEventArgs args)
		{
			checkState();
		}

		private void textEdit_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			Regex regex = new Regex("[^0-9]+");
			e.Handled = regex.IsMatch(e.Text);
		}

		private void imageDelete_MouseDown(object sender, MouseButtonEventArgs e)
		{
			var image = (Image)sender;
			image.Margin = new Thickness(1, 7, 0, 0);
		}

		private void imageDelete_MouseUp(object sender, MouseButtonEventArgs e)
		{
			var image = (Image)sender;
			image.Margin = new Thickness(0, 6, 0, 0);

			if (OnDelete != null)
				OnDelete(this, EventArgs.Empty);
		}

		private void imageDelete_MouseLeave(object sender, MouseEventArgs e)
		{
			var image = (Image)sender;
			image.Margin = new Thickness(0, 6, 0, 0);
		}

		private void stackPanel_MouseMove(object sender, MouseEventArgs e)
		{
			if (Calculator.Current.State == CalculatorState.SetValues)
			{
				textEdit.Visibility = Visibility.Visible;
				textValue.Visibility = Visibility.Collapsed;
			}
		}

		private void stackPanel_MouseLeave(object sender, MouseEventArgs e)
		{
			textEdit.Visibility = Visibility.Collapsed;
			textValue.Visibility = Visibility.Visible;
		}

		#endregion

		#endregion
	}
}
