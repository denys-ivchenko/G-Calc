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
	/// Interaction logic for CalculationUserControl.xaml
	/// </summary>
	public partial class CalculationUserControl : UserControl, ITabContent
	{
		#region Private fields

		#endregion

		#region Constructors

		public CalculationUserControl()
		{
			InitializeComponent();
		}

		#endregion

		#region Public Properties

		#endregion

		#region Public methods

		#endregion

		#region Interface implementations

		public CalculatorState TargetState { get; } = CalculatorState.Calculating;

		public event ErrorEventHandler OnError;

		#endregion

		#region Overridies

		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);
		}

		#endregion

		#region Private methods

		private void button_MouseDown(object sender, MouseButtonEventArgs e)
		{
			
		}

		private void button_MouseLeave(object sender, MouseEventArgs e)
		{
			mouseOut(sender);
		}

		private void button_MouseUp(object sender, MouseButtonEventArgs e)
		{
			mouseOut(sender);
		}

		private void mouseOut(object sender)
		{
			var button = (Button)sender;
			button.Margin = new Thickness(5, 9, 5, 1);
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			var button = (Button)sender;
			button.Margin = new Thickness(6, 10, 4, 0);
		}

		#endregion
	}
}
