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
	/// Interaction logic for StateActionUserControl.xaml
	/// </summary>
	public partial class ActionsUserControl : UserControl
	{
		#region Private fields

		private CalculatorState _targetState = CalculatorState.Undefined;
		private AddButtonMode _addingMode = AddButtonMode.Enabled;
		private bool _mustApprove;
		private bool _pressed;

		#endregion

		#region Constructors

		public ActionsUserControl()
		{
			InitializeComponent();
		}

		#endregion

		#region Public Properties

		public AddButtonMode AddingMode
		{
			get { return _addingMode; }
			set
			{
				_addingMode = value;
				checkButtonAdd();
			}
		}

		public bool MustApprove 
		{ 
			get { return _mustApprove; }
			set
			{
				_mustApprove = value;
				checkButtonApprove();
			}
		}

		#endregion

		#region Public events

		public event EventHandler OnAdd;

		public event ApproveEventHandler OnApprove;

		#endregion

		#region Public methods

		#endregion

		#region Interface implementations

		public CalculatorState TargetState
		{
			get { return _targetState; }
			set 
			{ 
				_targetState = value;
				checkButtons();
			}
		}

		#endregion

		#region Overridies

		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);

			checkButtons();

			Calculator.Current.StateChanged += calculator_StateChanged;
		}

		#endregion

		#region Private methods

		private void checkButtons()
		{
			checkButtonAdd();
			checkButtonApprove();
			checkButtonEdit();
		}

		private void checkButtonAdd()
		{
			if (Calculator.Current.State == TargetState)
			{
				switch (AddingMode)
				{
					case AddButtonMode.Hidden:
						imageAdd.Visibility = Visibility.Collapsed;
						break;
					case AddButtonMode.Enabled:
						imageAdd.Source = new BitmapImage(new Uri($"pack://application:,,,/Telesyk.GraphCalculator.WPF.UserControls;component/images/icon-add.png"));
						imageAdd.IsEnabled = true;
						break;
					case AddButtonMode.Disabled:
						imageAdd.Source = new BitmapImage(new Uri($"pack://application:,,,/Telesyk.GraphCalculator.WPF.UserControls;component/images/icon-add-disabled.png"));
						imageAdd.IsEnabled = false;
						break;
				}
			}
			else
			{

				imageAdd.Source = new BitmapImage(new Uri($"pack://application:,,,/Telesyk.GraphCalculator.WPF.UserControls;component/images/icon-add-disabled.png"));
				imageAdd.IsEnabled = false;
			}
		}

		private void checkButtonApprove()
		{
			if (Calculator.Current.State == TargetState && MustApprove)
			{
				imageApprove.Source = new BitmapImage(new Uri($"pack://application:,,,/Telesyk.GraphCalculator.WPF.UserControls;component/images/icon-approve.png"));
				imageApprove.IsEnabled = true;
			}
			else
			{

				imageApprove.Source = new BitmapImage(new Uri($"pack://application:,,,/Telesyk.GraphCalculator.WPF.UserControls;component/images/icon-approve-disabled.png"));
				imageApprove.IsEnabled = false;
			}
		}

		private void checkButtonEdit()
		{
			if (Calculator.Current.State > TargetState)
			{
				imageEdit.Source = new BitmapImage(new Uri($"pack://application:,,,/Telesyk.GraphCalculator.WPF.UserControls;component/images/icon-edit.png"));
				imageEdit.IsEnabled = true;
			}
			else
			{
				imageEdit.Source = new BitmapImage(new Uri($"pack://application:,,,/Telesyk.GraphCalculator.WPF.UserControls;component/images/icon-edit-disabled.png"));
				imageEdit.IsEnabled = true;
			}
		}

		#region Event handlers
		
		private void calculator_StateChanged(object sender, CalculatorStateEventArgs args) => checkButtons();

		private void image_MouseUp(object sender, MouseButtonEventArgs e)
		{
			var image = (Image)sender;
			image.Margin = new Thickness(0, 0, 2, 0);
		}

		private void image_MouseLeave(object sender, MouseEventArgs e)
		{
			if (_pressed)
			{
				var image = (Image)sender;
				image.Margin = new Thickness(0, 0, 2, 0);

				_pressed = false;
			}
		}

		private void image_MouseDown(object sender, MouseButtonEventArgs e)
		{
			var image = (Image)sender;

			if (image.IsEnabled)
			{ 
				image.Margin = new Thickness(2, 2, 0, 0);

				_pressed = true;
			}
		}

		private void imageAdd_MouseUp(object sender, MouseButtonEventArgs e)
		{
			image_MouseUp(sender, e);

			if (OnAdd != null)
				OnAdd(sender, e);
		}

		private void imageApprove_MouseUp(object sender, MouseButtonEventArgs e)
		{
			image_MouseUp(sender, e);

			var args = new ApproveEventArgs();

			if (OnApprove != null)
				OnApprove(this, args);

			if (args.IsValid && Calculator.Current.State == TargetState)
				Calculator.Current.Next();
		}

		private void imageEdit_MouseUp(object sender, MouseButtonEventArgs e)
		{
			image_MouseUp(sender, e);

			if (Calculator.Current.State > TargetState)
				Calculator.Current.Return(TargetState);
		}

		#endregion

		#endregion

		#region Child classes

		public delegate void ApproveEventHandler(object sender, ApproveEventArgs args);

		public class ApproveEventArgs
		{
			public ApproveEventArgs()
			{

			}

			public bool IsValid { get; set; }
		}

		public enum AddButtonMode
		{
			Hidden,
			Disabled,
			Enabled
		}

		#endregion
	}
}
