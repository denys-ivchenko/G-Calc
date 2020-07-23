using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Xml;

namespace Telesyk.GraphCalculator.WPF
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		#region Private fields

		#endregion

		#region Constructors

		public MainWindow()
		{
			InitializeComponent();
		}

		#endregion

		#region Overridies

		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);
			
			controlTabMenu.InitContentControls((Name: "value-set", Control: controlSetValues),
				(Name: "placements", Control: controlPlacement),
				(Name: "functions", Control: controlFunctions),
				(Name: "limited-functions", Control: controlLimitationFunctions),
				(Name: "calculation", Control: controlCalculation));

			controlSetValues.OnError += on_error;
			controlPlacement.OnError += on_error;
			controlFunctions.OnError += on_error;
			controlLimitationFunctions.OnError += on_error;
			controlCalculation.OnError += on_error;

			Calculator.Current.StateChanged += calculator_StateChanged;
			Calculator.Current.OnError += on_error;

			Calculator.Current.Start();
		}

		#endregion

		#region Private methods

		private void openFile()
		{
			var dialog = new OpenFileDialog();
			dialog.DefaultExt = "*.gcd";
			dialog.Filter = "Graph calculator data files (*.gcd)|*.gcd|All files (*.*)|*.*";

			if ((bool)dialog.ShowDialog())
				if (loadFromXml(dialog.FileName, out CalculatorState state, out CalculatorState stateMax))
				{
					Calculator.Current.Clear();
					Calculator.Current.Start();

					approveToMaxState(state, stateMax);

					if (Calculator.Current.State != state)
						Calculator.Current.Return(state);
				}
				else
					clear();
		}

		private void clear()
		{
			Calculator.Current.Clear();
			Calculator.Current.Start();

			controlSetValues.Clear();
			controlPlacement.SetPlacement(0);
			controlFunctions.Clear();
			controlLimitationFunctions.Clear();
			controlCalculation.Clear();
		}

		private void saveFile()
		{
			var dialog = new SaveFileDialog();
			dialog.DefaultExt = "*.gcd";
			dialog.Filter = "Graph calculator data files (*.gcd)|*.gcd|All files (*.*)|*.*";

			if ((bool)dialog.ShowDialog())
			{
				XmlDocument xml = new XmlDocument();
				saveToXml(xml);

				xml.Save(dialog.FileName);
			}
		}

		private void approveToMaxState(CalculatorState state, CalculatorState stateMax)
		{
			if (Calculator.Current.State == stateMax)
				return;

			controlSetValues.Approve();

			if (Calculator.Current.State == stateMax)
				return;

			controlPlacement.Approve();

			if (Calculator.Current.State == stateMax)
				return;

			controlFunctions.Approve();

			if (Calculator.Current.State == stateMax)
				return;

			controlLimitationFunctions.Approve();
		}

		#region Load from XML

		private bool loadFromXml(string fileName, out CalculatorState state, out CalculatorState stateMax)
		{
			state = CalculatorState.Undefined;
			stateMax = CalculatorState.Undefined;

			XmlDocument xml = new XmlDocument();

			try { xml.Load(fileName); }
			catch
			{
				MessageBox.Show(Strings.FAILED_READ_FILE, Strings.ERROR_READ_FILE);
				return false;
			}

			if (!loadRootFromXml(xml, out state, out stateMax))
				return false;

			if (!controlSetValues.LoadFromXml(xml))
				return false;

			if (!controlPlacement.LoadFromXml(xml))
				return false;

			if (!controlFunctions.LoadFromXml(xml))
				return false;

			if (!controlLimitationFunctions.LoadFromXml(xml))
				return false;

			return true;
		}

		private bool loadRootFromXml(XmlDocument xml, out CalculatorState state, out CalculatorState stateMax)
		{
			state = CalculatorState.Undefined;
			stateMax = CalculatorState.Undefined;
		
			var nodeCalculator = xml.SelectSingleNode("calculator");

			if (nodeCalculator == null)
			{
				MessageBox.Show(Strings.FAILED_READ_ROOT, Strings.ERROR_READ_FILE);
				return false;
			}

			string stateValue = null;

			try { stateValue = nodeCalculator.Attributes["state"].InnerText; }
			catch
			{
				MessageBox.Show(Strings.FAILED_GET_STATE, Strings.ERROR_READ_FILE);
				return false;
			}

			if (!Enum.TryParse<CalculatorState>(stateValue, out state))
			{
				MessageBox.Show(Strings.FAILED_READ_STATE, Strings.ERROR_READ_FILE);
				return false;
			}

			string maxStateValue = null;

			try { maxStateValue = nodeCalculator.Attributes["max-state"].InnerText; }
			catch
			{
				MessageBox.Show(Strings.FAILED_GET_MAX_STATE, Strings.ERROR_READ_FILE);
				return false;
			}

			if (!Enum.TryParse<CalculatorState>(maxStateValue, out stateMax))
			{
				MessageBox.Show(Strings.FAILED_READ_MAX_STATE, Strings.ERROR_READ_FILE);
				return false;
			}

			return true;
		}

		#endregion		
		
		#region Save to XML

		private void saveToXml(XmlDocument xml)
		{
			saveRootToXml(xml);

			controlSetValues.SaveToXml(xml);
			controlPlacement.SaveToXml(xml);
			controlFunctions.SaveToXml(xml);
			controlLimitationFunctions.SaveToXml(xml);
		}

		private void saveRootToXml(XmlDocument xml)
		{
			var nodeCalculator = Calculator.InsertNodeToXml(xml, "calculator");

			var attrState = xml.CreateAttribute("state");
			nodeCalculator.Attributes.Append(attrState);
			attrState.InnerText = Calculator.Current.State.ToString();

			var attrMaxState = xml.CreateAttribute("max-state");
			nodeCalculator.Attributes.Append(attrMaxState);
			attrMaxState.InnerText = Calculator.Current.MaxState.ToString();
		}

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

		#region Handlers

		private void on_error(object sender, ErrorEventArgs args)
		{
			MessageBox.Show(args.Message, args.Title == null ? Strings.ERROR : args.Title);
		}

		private void calculator_StateChanged(object sender, CalculatorStateEventArgs args)
		{
			switch (Calculator.Current.State)
			{
				case CalculatorState.SetValues:
					textState.Text = "Множина значень";
					break;
				case CalculatorState.Placement:
					textState.Text = "Розміщення";
					break;
				case CalculatorState.Functions:
					textState.Text = "Функції";
					break;
				case CalculatorState.LimitationFunctions:
					textState.Text = "Обмежувальні функції";
					break;
				default:
					textState.Text = "Обчислення";
					break;
			}
		}

		private void menuQuit_Click(object sender, RoutedEventArgs e)
		{
			App.Current.Shutdown();
		}

		private void menuOpen_Click(object sender, RoutedEventArgs e) => openFile();

		private void menuSave_Click(object sender, RoutedEventArgs e) => saveFile();

		private void menuClear_Click(object sender, RoutedEventArgs e) => clear();

		#endregion

		#endregion
	}
}
