using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
	/// Interaction logic for TabMenuUserControl.xaml
	/// </summary>
	public partial class TabMenuUserControl : UserControl
	{
		private Image _currentTab = null;
		private string _currentTabName = null;
		private UserControl _currentTabContent;
		private ProcessStage _processStage = ProcessStage.Default;

		private Dictionary<string, UserControl> _contentControls = new Dictionary<string, UserControl>();

		public TabMenuUserControl()
		{
			InitializeComponent();
		}

		protected UserControl ValueSet { get; private set; }

		protected UserControl Function { get; private set; }

		protected UserControl LimitedFunction { get; private set; }

		protected UserControl Calculation { get; private set; }

		public void InitContentControls(params (string Name, UserControl Control)[] controls)
		{
			foreach(var control in controls)
				_contentControls.Add(control.Name, control.Control);
		}

		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);

			Calculator.Current.StateChanged += Current_StateChanged;
		}

		private void Current_StateChanged(object sender, CalculatorStateEventArgs args)
		{
			setCurrentTab();
		}

		private void imageTab_MouseMove(object sender, MouseEventArgs e)
		{
			var image = (Image)sender;
			var border = (Border)image.Parent;

			border.BorderBrush = new SolidColorBrush(Color.FromRgb(0x61, 0xBB, 0xF9));
		}

		private void imageTab_MouseLeave(object sender, MouseEventArgs e)
		{
			var image = (Image)sender;
			var border = (Border)image.Parent;

			border.BorderBrush = new SolidColorBrush(Color.FromRgb(0xF9, 0xEF, 0xB8));
		}

		private void imageTab_MouseDown(object sender, MouseButtonEventArgs e) => setCurrentTab((Image)sender);

		private void setCurrentTab()
		{
			Image image = null;
		
			switch (Calculator.Current.State)
			{
				case CalculatorState.SetValues:
					image = imageTabValueSet;
					break;
				case CalculatorState.Placement:
					image = imageTabPlacements;
					break;
				case CalculatorState.Functions:
					image = imageTabFunctions;
					break;
				case CalculatorState.LimitationFunctions:
					image = imageTabLimitedFunctions;
					break;
				case CalculatorState.Calculating:
					image = imageTabProcess;
					break;
				case CalculatorState.Calculated:
					image = imageTabProcess;
					break;
			}

			setCurrentTab(image);
		}


		private void setCurrentTab(Image image)
		{
			var border = (Border)image.Parent;

			string imageName = getTabImageName(false);

			//if (getStateByKey(_currentTabName) == Calculator.Current.State)
			//	imageName += "-current";

			if (_currentTab != null)
			{
				_currentTab.Source = new BitmapImage(new Uri($"pack://application:,,,/Telesyk.GraphCalculator.WPF.UserControls;component/images/{imageName}.png"));
				((Border)_currentTab.Parent).Background = new SolidColorBrush(Color.FromRgb(0xF9, 0xEF, 0xB8));

				if (_currentTabContent != null)
					_currentTabContent.Visibility = Visibility.Collapsed;
			}

			_currentTab = image;
			_currentTabName = (string)image.Tag;
			_currentTabContent = _contentControls[_currentTabName];

			if (_currentTabContent != null)
				_currentTabContent.Visibility = Visibility.Visible;

			imageName = getTabImageName(true);

			//if (getStateByKey(_currentTabName) == Calculator.Current.State)
			//	imageName += "-current";

			border.Background = new SolidColorBrush(Color.FromRgb(0x42, 0x43, 0x44));
			image.Source = new BitmapImage(new Uri($"pack://application:,,,/Telesyk.GraphCalculator.WPF.UserControls;component/images/{imageName}.png"));
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			_processStage = ProcessStage.Started;
			imageTabProcess.Source = new BitmapImage(new Uri($"pack://application:,,,/calculation-do.png"));
		}

		private CalculatorState getStateByKey(string key)
		{
			switch (key)
			{
				case "value-set":
					return CalculatorState.SetValues;
				case "placements":
					return CalculatorState.Placement;
				case "functions":
					return CalculatorState.Placement;
				case "limited-functions":
					return CalculatorState.Placement;
				case "calculation":
					return CalculatorState.Calculating;
			}

			return CalculatorState.Calculated;
		}

		private string getKeyByState(CalculatorState state)
		{
			switch (state)
			{
				case CalculatorState.SetValues:
					return "value-set";
				case CalculatorState.Placement:
					return "placements";
				case CalculatorState.Functions:
					return "functions";
				case CalculatorState.LimitationFunctions:
					return "limited-functions";
				case CalculatorState.Calculating:
					return "calculation";
			}

			return "calculated";
		}

		private string getTabImageName(bool isAlt)
		{
			string imageName = null;

			if (_currentTabName == "calculation")
			{
				switch (_processStage)
				{
					case ProcessStage.Started:
						imageName = "calculation-do";
						break;
					case ProcessStage.Ready:
						imageName = "calculation-go";
						break;
					case ProcessStage.Unvalid:
						imageName = "calculation-idle";
						break;
					default:
						imageName = isAlt ? "calculation-alt" : "calculation";
						break;
				}
			}
			else
			{
				imageName = $"{_currentTabName}";

				if (isAlt)
					imageName += "-alt";
			}

			return imageName;
		}
	}
}
