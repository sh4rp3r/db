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
using Microsoft.EntityFrameworkCore;

namespace BD
{
    /// <summary>
    /// Логика взаимодействия для Views.xaml
    /// </summary>
    public partial class Views : Page
    {
        private DatabaseContext _context;
        public Views()
        {
            InitializeComponent();
            _context = new DatabaseContext();
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        private void MedalReport_Click(object sender, RoutedEventArgs e)
        {
            var reportWindow = new MedalReportWindow(_context);
            reportWindow.ShowDialog();
        }

        private void AthleteReport_Click(object sender, RoutedEventArgs e)
        {
            var reportWindow = new AthleteReportWindow(_context);
            reportWindow.ShowDialog();
        }

        private void AgeReport_Click(object sender, RoutedEventArgs e)
        {
            var reportWindow = new AgeReportWindow(_context);
            reportWindow.ShowDialog();
        }

        private void ScheduleReport_Click(object sender, RoutedEventArgs e)
        {
            var reportWindow = new ScheduleReportWindow(_context);
            reportWindow.ShowDialog();
        }
    }
}
