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
using System.Xml;

namespace Telesyk.GraphCalculator.WPF.UserControls
{
	/// <summary>
	/// Interaction logic for PlacementsUserControl.xaml
	/// </summary>
	public partial class PlacementUserControl : UserControl, ITabContent
	{
		#region Private fields

		#endregion

		#region Constructors

		public PlacementUserControl()
		{
			InitializeComponent();
		}

		#endregion

		#region Public Properties

		public int Value { get; private set; }

		#endregion

		#region Public methods

		public bool LoadFromXml(XmlDocument xml) => loadFromXml(xml);

		public void SaveToXml(XmlDocument xml) => saveToXml(xml);

		public void SetPlacement(int value) => setPlacement(value);

		public bool Approve()
		{
			bool valid = approve();

			if (valid && Calculator.Current.State == CalculatorState.Placement)
			{
				Calculator.Current.Next(); 
				return true;
			}

			return false;
		}

		#endregion

		#region Interface implementations

		public CalculatorState TargetState { get; } = CalculatorState.Placement;

		public event ErrorEventHandler OnError;

		#endregion

		#region Overridies

		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);

			controlActions.TargetState = TargetState;
			controlActions.AddingMode = ActionsUserControl.AddButtonMode.Hidden;

			controlActions.OnApprove += controlActions_OnApprove;

			Calculator.Current.StateChanged += calculator_StateChanged;
			listValue.SelectionChanged += listValue_SelectionChanged;
		}

		#endregion

		#region Private methods

		private bool loadFromXml(XmlDocument xml)
		{
			var nodePlacement = xml.SelectSingleNode("calculator/placement");

			if (nodePlacement == null)
			{
				if (OnError != null)
					OnError(this, new ErrorEventArgs(Strings.FAILED_GET_PLACEMENT, Strings.ERROR_READ_FILE));

				return false;
			}

			if (!int.TryParse(nodePlacement.InnerText, out int value))
			{
				if (OnError != null)
					OnError(this, new ErrorEventArgs(Strings.FAILED_READ_PLACEMENT, Strings.ERROR_READ_FILE));

				return false;
			}

			setPlacement(value);

			return true;
		}

		private void saveToXml(XmlDocument xml)
		{
			var nodePlacement = Calculator.InsertNodeToXml(xml.DocumentElement, "placement");
			nodePlacement.InnerText = Value.ToString();
		}

		private bool approve()
		{
			if (Calculator.Current.State != TargetState)
				return false;

			if (Value > 0)
			{ 
				Calculator.Current.SetPlacement(Value);
				return true;
			}

			return false;
		}

		private void setPlacement(int value)
		{
			Value = value;
			textValue.Text = value.ToString();

			if (ensureList())
				controlActions.MustApprove = (Calculator.Current.State == CalculatorState.Placement);
			else
				controlActions.MustApprove = false;
		}

		private void checkState()
		{
			if (ensureList())
				controlActions.MustApprove = (Calculator.Current.State == CalculatorState.Placement);
			else
				controlActions.MustApprove = false;
		}

		private bool ensureList()
		{
			bool mustApprove = false;

			for (int i = 2; i <= Calculator.Current.SetValues.Count; i++)
			{
				ComboBoxItem item = null;

				if (listValue.Items.Count < i - 1)
				{

					item = new ComboBoxItem();
					item.Content = i.ToString();
					listValue.Items.Add(item);
				}
				else
					item = (ComboBoxItem)listValue.Items[i - 2];

				if (i == Value)
				{
					item.IsSelected = true;
					mustApprove = true;
				}
			}

			while (listValue.Items.Count > Calculator.Current.SetValues.Count - 1 && listValue.Items.Count != 0)
				listValue.Items.RemoveAt(listValue.Items.Count - 1);

			return mustApprove;
		}

		#region Handlers

		private void controlActions_OnApprove(object sender, ActionsUserControl.ApproveEventArgs args)
		{
			args.IsValid = approve();
		}

		private void listValue_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var item = (ComboBoxItem)listValue.SelectedItem;
			int.TryParse(item?.Content?.ToString(), out int value);
			Value = value;
			textValue.Text = Value.ToString();

			controlActions.MustApprove = true;
		}

		private void calculator_StateChanged(object sender, CalculatorStateEventArgs args)
		{
			checkState();
		}

		private void stackPanel_MouseMove(object sender, MouseEventArgs e)
		{
			if (Calculator.Current.State == TargetState)
			{
				textValue.Visibility = Visibility.Collapsed;
				listValue.Visibility = Visibility.Visible;
			}
		}

		private void stackPanel_MouseLeave(object sender, MouseEventArgs e)
		{
			textValue.Visibility = Visibility.Visible;
			listValue.Visibility = Visibility.Collapsed;
		}

		#endregion

		#endregion
	}
}
