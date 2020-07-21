using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Xml;

namespace Telesyk.GraphCalculator.WPF.UserControls
{
	/// <summary>
	/// Interaction logic for ValueSetUserControl.xaml
	/// </summary>
	public partial class SetValuesUserControl : UserControl, ITabContent
	{
		#region Private fields

		private List<SetValuesElementUserControl> _elements = new List<SetValuesElementUserControl>();
		private IReadOnlyList<SetValuesElementUserControl> _elementsReadOnly;

		#endregion

		#region Constructors

		public SetValuesUserControl()
		{
			InitializeComponent();

			_elementsReadOnly = new ReadOnlyCollection<SetValuesElementUserControl>(_elements);
		}

		#endregion

		#region Public Properties

		public IReadOnlyList<SetValuesElementUserControl> Elements => _elementsReadOnly;

		#endregion

		#region Public methods

		public bool LoadFromXml(XmlDocument xml) => loadFromXml(xml);

		public void SaveToXml(XmlDocument xml) => saveToXml(xml);

		public void Clear() => clear();

		public bool Approve()
		{
			bool valid = approve();

			if (valid && Calculator.Current.State == CalculatorState.SetValues)
			{
				Calculator.Current.Next();
				return true;
			}

			return false;
		}

		#endregion

		#region Interface implementations

		public CalculatorState TargetState { get; } = CalculatorState.SetValues;

		public event ErrorEventHandler OnError;

		#endregion

		#region Overridies

		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);

			controlActions.TargetState = TargetState;
			controlActions.OnAdd += controlActions_OnAdd;
			controlActions.OnApprove += controlActions_OnApprove;
		}

		#endregion

		#region Private methods

		#region Handlers

		private void controlActions_OnAdd(object sender, EventArgs e) => addSetValue();

		private void element_OnDelete(object sender, EventArgs e)
		{
			var element = (SetValuesElementUserControl)sender;

			panelValues.Children.Remove(element);
			_elements.Remove(element);

			for (int i = 1; i < _elements.Count; i++)
				_elements[i].Position = i + 1;

			controlActions.MustApprove = isMustApprove();

			if (_elements.Count < 9)
				controlActions.AddingMode = ActionsUserControl.AddButtonMode.Enabled;
		}

		private void controlActions_OnApprove(object sender, ActionsUserControl.ApproveEventArgs args)
		{
			args.IsValid = approve();
		}

		private void element_ValueChanged(object sender, int e)
		{
			controlActions.MustApprove = isMustApprove();
		}

		#endregion

		#region XML

		private bool loadFromXml(XmlDocument xml)
		{
			clear();
		
			var nodes = xml.SelectNodes("calculator/set-values/value");

			for (int i = 0; i < nodes.Count; i++)
			{
				if (!int.TryParse(nodes[i].InnerText, out int value))
				{
					if (OnError != null)
						OnError(this, new ErrorEventArgs(Strings.FAILED_READ_SET_VALUE, Strings.ERROR_READ_FILE));

					return false;
				}

				addSetValue(value);
			}

			return true;
		}

		private void saveToXml(XmlDocument xml)
		{
			var nodeSetValues = Calculator.InsertNodeToXml(xml.DocumentElement, "set-values");

			foreach(var element in Elements)
			{
				var nodeValue = Calculator.InsertNodeToXml(nodeSetValues, "value");
				nodeValue.InnerText = element.Value.ToString();
			}
		}

		#endregion

		private bool approve()
		{
			bool valid = isMustApprove();

			if (valid)
				createCalculatorSetValues();

			return valid;
		}

		private void clear()
		{
			controlActions.AddingMode = ActionsUserControl.AddButtonMode.Enabled;
		
			foreach (var element in _elements)
				panelValues.Children.Remove(element);

			_elements.Clear();
			controlActions.MustApprove = false;
		}

		private void addSetValue(int value)
		{
			var element = addSetValue();
			element.Value = value;
		}

		private SetValuesElementUserControl addSetValue()
		{
			var element = new SetValuesElementUserControl(_elements.Count + 1);
			element.ValueChanged += element_ValueChanged;
			element.OnDelete += element_OnDelete;

			panelValues.Children.Add(element);
			_elements.Add(element);

			controlActions.MustApprove = isMustApprove();

			if (_elements.Count > 8)
				controlActions.AddingMode = ActionsUserControl.AddButtonMode.Disabled;

			return element;
		}

		//private void checkState()
		//{
		//	controlActions.MustApprove = isMustApprove();
		//	controlActions.AddingMode = Calculator.Current.State == CalculatorState.SetValues && _elements.Count < 9 ? 
		//		ActionsUserControl.AddButtonMode.Enabled : ActionsUserControl.AddButtonMode.Disabled;
		//}

		private void createCalculatorSetValues()
		{
			Calculator.Current.ClearSetVelues();

			foreach (var element in _elements)
				Calculator.Current.AddSetValue(element.Value);
		}

		private bool isMustApprove()
		{
			if (Calculator.Current.State != TargetState)
				return false;

			bool unvalid = false;

			foreach (var element in _elements)
				if (!element.IsValid)
				{
					unvalid = true;
					break;
				}

			return _elements.Count > 1 ? !unvalid : false;
		}

		#endregion
	}
}
