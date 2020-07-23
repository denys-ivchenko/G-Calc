using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
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

			Calculator.Current.StateChanged += calculator_StateChanged;
		}

		#endregion

		#region Public Properties

		#endregion

		#region Public methods

		public void Clear() => clear();

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

		private void clear()
		{
			panelResults.Children.Clear();

			textConbinations.Text = string.Empty;
			textDuration.Text = string.Empty;
			textResult.Text = string.Empty;
			textResultCombinations.Text = string.Empty;

			scrollResults.Visibility = Visibility.Collapsed;
			panelResultInfo.Visibility = Visibility.Collapsed;
		}

		private void calculated(Task<Processor> task)
		{
			scrollResults.Visibility = Visibility.Visible;
			panelResultInfo.Visibility = Visibility.Visible;

			textConbinations.Text = task.Result.Combinations.ToString();
			textDuration.Text = $"{task.Result.Duration.Hours.ToString("00")}:{task.Result.Duration.Minutes.ToString("00")}:{task.Result.Duration.Seconds.ToString("00")}.{(task.Result.Duration.Milliseconds == 0 ? "001" : task.Result.Duration.Milliseconds.ToString("000"))}";
			textResult.Text = task.Result.ResultValue.ToString();
			textResultCombinations.Text = task.Result.Results.Count.ToString();

			//addResultTextBlock(panelResultInfo, "Тривалість", $"{task.Result.Duration.Hours.ToString("00")}:{task.Result.Duration.Minutes.ToString("00")}:{task.Result.Duration.Seconds.ToString("00")}.{task.Result.Duration.Milliseconds}");
			//addResultTextBlock(panelResultInfo, "Комбінацій", task.Result.Combinations.ToString());
			//addResultTextBlock(panelResultInfo, "Результат", task.Result.ResultValue.ToString());
			//addResultTextBlock(panelResultInfo, "Комбінацій результату", task.Result.Results.Count.ToString());

			for (int i = 0; i < task.Result.Results.Count; i++)
				addResult(task.Result.Results[i].Key);
		}

		private void addResult(string value)
		{
			TextBlock text = new TextBlock();
			panelResults.Children.Add(text);
			text.Foreground = Brushes.White;
			text.Margin = new Thickness(5, 3, 5, 3);
			text.Text = value;

			//Run runTitle = new Run();
			//textDuration.Inlines.Add(runTitle);
			//runTitle.Text = title + value ?? ": ";

			//Run runValue = new Run();
			//textDuration.Inlines.Add(runValue);
			//runValue.Foreground = new SolidColorBrush(Color.FromRgb(0x02, 0x48, 0x84));
			//runValue.Text = value;
		}

		#region Handlers

		private void button_Click(object sender, RoutedEventArgs e)
		{
			clear();

			var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
			Processor processor = new Processor(Calculator.Current);
			Task task = processor.Calculate().ContinueWith((t) => calculated(t), scheduler);
		}

		private void calculator_StateChanged(object sender, CalculatorStateEventArgs args)
		{
			clear();

			buttonStart.IsEnabled = Calculator.Current.State > CalculatorState.LimitationFunctions;
		}

		#region Commented

		//private void button_MouseDown(object sender, MouseButtonEventArgs e)
		//{
		//	var button = (Button)sender;
		//	button.Margin = new Thickness(6, 10, 4, 0);
		//}

		//private void button_MouseLeave(object sender, MouseEventArgs e)
		//{
		//	mouseOut(sender);
		//}

		//private void button_MouseUp(object sender, MouseButtonEventArgs e)
		//{
		//	mouseOut(sender);
		//}

		//private void mouseOut(object sender)
		//{
		//	var button = (Button)sender;
		//	button.Margin = new Thickness(5, 9, 5, 1);
		//}

		#endregion

		#endregion

		#endregion
	}
}
