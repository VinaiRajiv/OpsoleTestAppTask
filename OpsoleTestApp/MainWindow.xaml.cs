using OpsoleTestApp.Helpers;
using OpsoleTestApp.Services;
using System.Windows;

namespace OpsoleTestApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ExecutionService _executionService;

        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            _executionService = new ExecutionService(Append);
        }

        /// <summary>
        /// Execute all the operation on start button click
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="e">RoutedEventArgs</param>
        private async void OnStartBtnClick(object sender, RoutedEventArgs e)
        {
            await _executionService.RunAll();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        private void Append(string msg)
        {
            Dispatcher.Invoke(() =>
            {
                OutputBox.AppendText(msg + Environment.NewLine);
            });
        }
    }
}