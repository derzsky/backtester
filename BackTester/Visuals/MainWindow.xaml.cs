using ScottPlot;
using System.Windows;

namespace Visuals
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		StrategyViewModel _viewModel;
		public MainWindow()
		{
			InitializeComponent();

			_viewModel = (StrategyViewModel)DataContext;

			_viewModel.WindowPlot = WpfPlot1;
			_viewModel.DrawPlot();

			
		}
	}
}