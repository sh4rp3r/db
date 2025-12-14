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

namespace BD
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void CountriesClick(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Tables(1));
        }

        private void SportsClick(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Tables(2));
        }

        private void ParticipantsClick(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Tables(3));
        }

        private void ScheduleClick(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Tables(4));
        }

        private void VenuesClick(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Tables(5));
        }

        private void ResultsClick(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Tables(6));
        }
        private void OpenCompositeFormClick(object sender, RoutedEventArgs e)
        {
            using (var context = new DatabaseContext())
            {
                var compositeForm = new CompositeFormWindow(context);
                compositeForm.ShowDialog();
            }
        }
        private void ViewsClick(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Views());
        }
    }
}