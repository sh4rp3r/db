using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;

namespace BD
{
    public partial class AthleteReportWindow : Window
    {
        private DatabaseContext _context;

        public AthleteReportWindow(DatabaseContext context)
        {
            InitializeComponent();
            _context = context;
            LoadFilters();
        }

        private void LoadFilters()
        {
            // Загружаем страны
            var countries = _context.Countries.ToList();
            countries.Insert(0, new Countries { CountryId = 0, Name = "Все страны" });
            CountryFilterComboBox.ItemsSource = countries;
            CountryFilterComboBox.SelectedIndex = 0;

            // Загружаем виды спорта
            var sports = _context.Sports.ToList();
            sports.Insert(0, new Sports { SportId = 0, Name = "Все виды спорта" });
            SportFilterComboBox.ItemsSource = sports;
            SportFilterComboBox.SelectedIndex = 0;
        }

        private void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Базовый запрос с JOIN по 3 таблицам: results, participants, countries, sports
                var query = from r in _context.Results
                            join p in _context.Participants on r.ParticipantId equals p.ParticipantId
                            join c in _context.Countries on p.CountryId equals c.CountryId
                            join s in _context.Sports on r.SportId equals s.SportId
                            select new AthleteReportItem
                            {
                                FullName = p.FullName,
                                CountryName = c.Name,
                                SportName = s.Name,
                                Gender = p.Gender,
                                BirthDate = p.BirthDate,
                                Place = r.Place,
                                Score = r.Score
                            };

                // Применяем фильтры
                int countryId = (int)CountryFilterComboBox.SelectedValue;
                if (countryId > 0)
                {
                    query = query.Where(a => a.CountryName == CountryFilterComboBox.Text);
                }

                int sportId = (int)SportFilterComboBox.SelectedValue;
                if (sportId > 0)
                {
                    query = query.Where(a => a.SportName == SportFilterComboBox.Text);
                }

                string gender = ((ComboBoxItem)GenderFilterComboBox.SelectedItem).Tag.ToString();
                if (!string.IsNullOrEmpty(gender))
                {
                    query = query.Where(a => a.Gender == gender);
                }

                // Применяем сортировку
                var sortTag = ((ComboBoxItem)SortComboBox.SelectedItem).Tag.ToString();
                query = sortTag switch
                {
                    "name" => query.OrderBy(a => a.FullName),
                    "country" => query.OrderBy(a => a.CountryName).ThenBy(a => a.FullName),
                    "place" => query.OrderBy(a => a.Place).ThenBy(a => a.FullName),
                    "score" => query.OrderByDescending(a => a.Score).ThenBy(a => a.FullName),
                    _ => query.OrderBy(a => a.FullName)
                };

                var reportData = query.ToList();

                // Вычисляем возраст (вычисляемое поле)
                foreach (var item in reportData)
                {
                    item.Age = DateTime.Now.Year - item.BirthDate.Year;
                    if (DateTime.Now.DayOfYear < item.BirthDate.DayOfYear)
                        item.Age--;
                }

                // Отображаем данные
                ReportDataGrid.ItemsSource = reportData;

                // Итоговые данные
                TotalAthletesText.Text = reportData.Count.ToString();
                AvgAgeText.Text = reportData.Count > 0
                    ? reportData.Average(a => a.Age).ToString("F1")
                    : "0";
                BestScoreText.Text = reportData.Count > 0
                    ? reportData.Max(a => a.Score).ToString("F2")
                    : "0";

                if (reportData.Count == 0)
                {
                    MessageBox.Show("Нет данных для отображения с указанными фильтрами.", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка формирования отчета: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public class AthleteReportItem
    {
        public string FullName { get; set; }
        public string CountryName { get; set; }
        public string SportName { get; set; }
        public string Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public int? Place { get; set; }
        public decimal Score { get; set; }

        // Вычисляемое поле
        public int Age { get; set; }

        public string GenderDisplay => Gender == "M" ? "М" : "Ж";
    }
}
