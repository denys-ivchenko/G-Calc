using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
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
using Telesyk.GraphCalculator.WPF.UserControls;

namespace Telesyk.GraphCalculator.WPF.UserControls
{
	/// <summary>
	/// Interaction logic for UserControl1.xaml
	/// </summary>
	public partial class FunctionUserControl : UserControl
	{
		#region Private fields

		private List<FunctionElementUserControl> _elements = new List<FunctionElementUserControl>();
		private IReadOnlyList<FunctionElementUserControl> _elementsReadOnly;
		private int _position = 1;
		private WrapPanel _wrap = new WrapPanel();

		#endregion

		#region Constructors

		public FunctionUserControl() => new FunctionUserControl(false);

		public FunctionUserControl(bool isLimitation)
		{
			IsLimitation = isLimitation;

			InitializeComponent();

			_elementsReadOnly = new ReadOnlyCollection<FunctionElementUserControl>(_elements);

			_elements.Add(elementFirst);
			elementFirst.Function = this;
		}

		#endregion

		#region Public Properties

		public bool IsLimitation { get; private set; }

		public int Position
		{
			get { return _position; }
			set
			{
				_position = value;
				labelPosition.Content = $"{Position}:";
			}
		}

		public LimitationFunctionCondition Condition { get; private set; } = LimitationFunctionCondition.LessThan;

		public int ConditionValue { get; private set; }

		public bool IsValid { get { return _elements.Count == Calculator.Current.Placement; } }

		public IReadOnlyList<FunctionElementUserControl> Elements => _elementsReadOnly;

		public CalculatorState TargetState 
		{ 
			get { return IsLimitation ? CalculatorState.LimitationFunctions : CalculatorState.Functions; } 
		}

		#endregion

		#region Public methods

		public bool LoadFromXml(XmlNode nodeFunction) => loadFromXml(nodeFunction);

		public void SaveToXml(XmlNode nodeFunctions) => saveToXml(nodeFunctions);

		public void AddElement(FunctionOperator oper, int position) => addElement(oper, position);

		public void RemoveElement(int position) => removeElement(position);

		#endregion

		#region Public events

		public event FunctionEventHandler OnChanged;

		public event EventHandler OnDelete;

		public event ErrorEventHandler OnError;

		//public void SetLimitation(LimitationFunctionCondition condition, int conditionValue)
		//{
			
		//}

		#endregion

		#region Overridies

		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);

			Calculator.Current.StateChanged += calculator_StateChanged;

			if (IsLimitation)
			{
				listConditions.SelectionChanged += conditions_SelectionChanged;
				textConditionValue.TextChanged += conditionValue_TextChanged;
			
				panelLimitation.Visibility = Visibility.Visible;
			}
		}

		#endregion

		#region Private methods

		#region XML

		private bool loadFromXml(XmlNode nodeFunction)
		{
			var nodesElement = nodeFunction.SelectNodes("elements/element");

			for (int i = 0; i < nodesElement.Count; i++)
			{
				var nodeOperator = nodesElement[i].SelectSingleNode("operator");

				if (nodeOperator == null)
				{
					if (OnError != null)
						OnError(this, new ErrorEventArgs(IsLimitation ? Strings.FAILED_GET_LIMITATION_FUNCTION_ELEMENT_OPERATOR : Strings.FAILED_GET_FUNCTION_ELEMENT_OPERATOR, Strings.ERROR_READ_FILE));

					return false;
				}

				FunctionOperator oper = FunctionOperator.Addition;

				if (nodeOperator.InnerText != "+")
					if (nodeOperator.InnerText != "-")
					{
						if (OnError != null)
							OnError(this, new ErrorEventArgs(IsLimitation ? Strings.FAILED_READ_LIMITATION_FUNCTION_ELEMENT_OPERATOR : Strings.FAILED_READ_FUNCTION_ELEMENT_OPERATOR, Strings.ERROR_READ_FILE));

						return false;
					}
					else
						oper = FunctionOperator.Substraction;

				var nodeValue = nodesElement[i].SelectSingleNode("value");

				if (nodeValue == null)
				{
					if (OnError != null)
						OnError(this, new ErrorEventArgs(IsLimitation ? Strings.FAILED_GET_LIMITATION_FUNCTION_ELEMENT_VALUE : Strings.FAILED_GET_FUNCTION_ELEMENT_VALUE, Strings.ERROR_READ_FILE));

					return false;
				}

				if (!int.TryParse(nodeValue.InnerText, out int value))
				{
					if (OnError != null)
						OnError(this, new ErrorEventArgs(IsLimitation ? Strings.FAILED_GET_LIMITATION_FUNCTION_ELEMENT_VALUE : Strings.FAILED_GET_FUNCTION_ELEMENT_VALUE, Strings.ERROR_READ_FILE));

					return false;
				}

				if (i != 0)
					addElement(oper, i + 1);

				Elements[i].Value = value;
			}

			if (IsLimitation)
			{
				var nodeLimitationCondition = nodeFunction.SelectSingleNode("limitation/condition");

				if (nodeLimitationCondition == null)
				{
					if (OnError != null)
						OnError(this, new ErrorEventArgs(Strings.FAILED_GET_LIMITATION_FUNCTION_CONDITION, Strings.ERROR_READ_FILE));

					return false;
				}

				if (!Enum.TryParse<LimitationFunctionCondition>(nodeLimitationCondition.InnerText, out LimitationFunctionCondition condition))
				{
					if (OnError != null)
						OnError(this, new ErrorEventArgs(Strings.FAILED_READ_LIMITATION_FUNCTION_CONDITION, Strings.ERROR_READ_FILE));

					return false;
				}

				var nodeLimitationValue = nodeFunction.SelectSingleNode("limitation/value");

				if (nodeLimitationValue == null)
				{
					if (OnError != null)
						OnError(this, new ErrorEventArgs(Strings.FAILED_GET_LIMITATION_FUNCTION_CONDITION_VALUE, Strings.ERROR_READ_FILE));

					return false;
				}

				if (!int.TryParse(nodeLimitationValue.InnerText, out int conditionValue))
				{
					if (OnError != null)
						OnError(this, new ErrorEventArgs(Strings.FAILED_READ_LIMITATION_FUNCTION_CONDITION_VALUE, Strings.ERROR_READ_FILE));

					return false;
				}

				listConditions.SelectedIndex = condition == LimitationFunctionCondition.LessThan ? 0 : condition == LimitationFunctionCondition.LessThanOrEqual ? 1 : condition == LimitationFunctionCondition.GreaterThanOrEqual ? 2 : 3;
				labelConditionValue.Content = textConditionValue.Text = conditionValue.ToString();
			}

			return true;
		}

		private void saveToXml(XmlNode nodeFunctions)
		{
			var nodeFunction = Calculator.InsertNodeToXml(nodeFunctions, IsLimitation ? "limitation-function" : "function");
			var nodeElements = Calculator.InsertNodeToXml(nodeFunction, "elements");

			foreach (var element in Elements)
			{
				var nodeElement = Calculator.InsertNodeToXml(nodeElements, "element");
				
				var nodeOperator = Calculator.InsertNodeToXml(nodeElement, "operator");
				nodeOperator.InnerText = element.Operator == FunctionOperator.Addition ? "+" : "-";

				var nodeValue = Calculator.InsertNodeToXml(nodeElement, "value");
				nodeValue.InnerText = element.Value.ToString();
			}

			if (IsLimitation)
			{
				var nodeLimitation = Calculator.InsertNodeToXml(nodeFunction, "limitation");

				var nodeCondition = Calculator.InsertNodeToXml(nodeLimitation, "condition");
				nodeCondition.InnerText = Condition.ToString();

				var nodeValue = Calculator.InsertNodeToXml(nodeLimitation, "value");
				nodeValue.InnerText = ConditionValue.ToString();
			}
		}

		#endregion

		private void element_OnOperatorChanged(object sender, OperatorEventArgs args)
		{
			if (args.Position == 6)
				textSeparator.Text = args.Operator == FunctionOperator.Addition ? "+" : "-";
		}

		private void addElement(FunctionOperator oper, int position)
		{
			FunctionElementUserControl element = new FunctionElementUserControl();
			element.Position = position;
			element.Operator = oper;
			element.Function = this;
			element.OnOperatorChanged += element_OnOperatorChanged;

			(position > 5 ? panelWrapedElements : panelElements).Children.Insert(position > 5 ? position - 6 : position - 1, element);
			_elements.Insert(position - 1, element);

			if (_elements.Count == 6)
			{
				panelContainer.Children.Remove(imageDelete);
				panelContainer.Children.Remove(panelLimitation);

				panelWrapedContainer.Children.Add(panelLimitation);
				panelWrapedContainer.Children.Add(imageDelete);
			}

			if (_elements.Count > 5)
			{
				textSeparator.Text = _elements[5].Operator == FunctionOperator.Addition ? "+" : "-";
				textSeparator.Visibility = Visibility.Visible;
				panelWrapedContainer.Visibility = Visibility;
			}

			for (int i = 0; i < _elements.Count; i++)
				_elements[i].Position = i + 1;

			if (_elements.Count == 9)
				foreach (var item in _elements)
					item.DisableAdding();

			if (OnChanged != null)
				OnChanged(this, new FunctionEventArgs(IsValid));
		}

		private void removeElement(int position)
		{
			(position > 5 ? panelWrapedElements : panelElements).Children.RemoveAt(position > 5 ? position - 6 : position - 1);
			_elements.RemoveAt(position - 1);

			if (panelElements.Children.Count == 4 && panelWrapedElements.Children.Count > 0)
			{
				var elementControl = (FunctionElementUserControl)panelWrapedElements.Children[0];
				panelWrapedElements.Children.RemoveAt(0);
				panelElements.Children.Insert(4, elementControl);
			}

			if (_elements.Count == 5)
			{
				panelWrapedContainer.Children.Remove(imageDelete);
				panelWrapedContainer.Children.Remove(panelLimitation);

				panelContainer.Children.Add(panelLimitation);
				panelContainer.Children.Add(imageDelete);
			}

			if (_elements.Count < 6)
			{
				textSeparator.Visibility = Visibility.Collapsed;
				panelWrapedContainer.Visibility = Visibility.Collapsed;
			}	
			else
				textSeparator.Text = _elements[5].Operator == FunctionOperator.Addition ? "+" : "-";

			for (int i = 0; i < _elements.Count; i++)
				_elements[i].Position = i + 1;

			if (_elements.Count == 8)
				foreach (var item in _elements)
					item.EnableAdding();

			if (OnChanged != null)
				OnChanged(this, new FunctionEventArgs(IsValid));
		}

		private LimitationFunctionCondition parseLimitedCondition(string value)
		{
			switch (value)
			{
				case "&lt;":
				case "<":
					return LimitationFunctionCondition.LessThan;
				case "&lt;=":
				case "=&lt;":
				case "<=":
				case "=<":
					return LimitationFunctionCondition.LessThanOrEqual;
				case "&gt;":
				case ">":
					return LimitationFunctionCondition.GreaterThan;
				case "&gt;=":
				case "=&gt;":
				case ">=":
				case "=>":
					return LimitationFunctionCondition.LessThanOrEqual;
			}

			return LimitationFunctionCondition.GreaterThanOrEqual;
		}

		private void conditionValue_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (string.IsNullOrEmpty(textConditionValue.Text))
				textConditionValue.Text = "0";

			labelConditionValue.Content = textConditionValue.Text;

			int.TryParse(textConditionValue.Text, out int value);
			ConditionValue = value;
		}

		private void conditions_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var item = (ComboBoxItem)listConditions.SelectedItem;
			Condition = parseLimitedCondition(item.Content.ToString());
			labelCondition.Content = item.Content.ToString();

			//listConditions.Padding = new Thickness(listConditions.SelectedIndex == 0 || listConditions.SelectedIndex == 3 ? 7 : 3, 4, 0, 0);
			//labelCondition.Padding = new Thickness(listConditions.SelectedIndex == 0 || listConditions.SelectedIndex == 3 ? 8 : 2, 4, 0, 0);
		}

		private void conditionValue_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			Regex regex = new Regex("[^0-9]+");
			e.Handled = regex.IsMatch(e.Text);
		}

		private void imageDelete_MouseDown(object sender, MouseButtonEventArgs e)
		{
			var image = (Image)sender;
			image.Margin = new Thickness(5, 3, 0, 0);
		}

		private void imageDelete_MouseUp(object sender, MouseButtonEventArgs e)
		{
			var image = (Image)sender;
			image.Margin = new Thickness(4, 2, 0, 0);

			if (OnDelete != null)
				OnDelete(this, EventArgs.Empty);
		}

		private void imageDelete_MouseLeave(object sender, MouseEventArgs e)
		{
			var image = (Image)sender;
			image.Margin = new Thickness(4, 2, 0, 0);
		}

		private void calculator_StateChanged(object sender, CalculatorStateEventArgs args)
		{
			imageDelete.Visibility = Calculator.Current.State == TargetState ? Visibility.Visible : Visibility.Collapsed;
		
			//listConditions.Visibility = imageDelete.Visibility = Calculator.Current.State == CalculatorState.Functions ? Visibility.Visible : Visibility.Collapsed;
			//listConditions.Visibility = labelConditionValue.Visibility = Calculator.Current.State != CalculatorState.Functions ? Visibility.Visible : Visibility.Collapsed;
		}

		private void textConditionValue_LostFocus(object sender, RoutedEventArgs e)
		{
			var textBox = (TextBox)sender;

			if (string.IsNullOrEmpty(textBox.Text))
				textBox.Text = "0";
		}

		private void panelLimitation_MouseMove(object sender, MouseEventArgs e)
		{
			if (Calculator.Current.State == TargetState)
			{ 
				listConditions.Visibility = textConditionValue.Visibility = Visibility.Visible;
				labelCondition.Visibility = labelConditionValue.Visibility = Visibility.Collapsed;
			}
		}

		private void panelLimitation_MouseLeave(object sender, MouseEventArgs e)
		{
			listConditions.Visibility = textConditionValue.Visibility = Visibility.Collapsed;
			labelCondition.Visibility = labelConditionValue.Visibility = Visibility.Visible;
		}

		#endregion
	}
}