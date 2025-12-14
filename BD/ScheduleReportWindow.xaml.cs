using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace BD
{
    public partial class ScheduleReportWindow : Window
    {
        private DatabaseContext _context;

        public ScheduleReportWindow(DatabaseContext context)
        {
            InitializeComponent();
            _context = context;
            LoadFilters();
        }

        private void LoadFilters()
        {
            // Загружаем места проведения
            var venues = _context.Venues.ToList();
            venues.Insert(0, new Venues { VenueId = 0, Name = "Все места" });
            VenueFilterComboBox.ItemsSource = venues;
            VenueFilterComboBox.SelectedIndex = 0;

            // Устанавливаем даты по умолчанию
            StartDatePicker.SelectedDate = DateTime.Now.AddMonths(-1);
            EndDatePicker.SelectedDate = DateTime.Now.AddMonths(1);
        }

        private void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Базовый запрос с JOIN по 3 таблицам: schedule, venues, sports
                var query = from sch in _context.Schedule
                            join v in _context.Venues on sch.VenueId equals v.VenueId
                            join s in _context.Sports on sch.SportId equals s.SportId
                            select new
                            {
                                ScheduleId = sch.ScheduleId,
                                StartDate = sch.StartDate,
                                StartTime = sch.StartTime,
                                VenueId = v.VenueId,
                                VenueName = v.Name,
                                Location = v.Location,
                                SportName = s.Name
                            };

                // Применяем фильтры
                int venueId = (int)VenueFilterComboBox.SelectedValue;
                if (venueId > 0)
                {
                    query = query.Where(e => e.VenueId == venueId);
                }

                if (StartDatePicker.SelectedDate.HasValue)
                {
                    var startDate = StartDatePicker.SelectedDate.Value;
                    query = query.Where(e => e.StartDate >= startDate);
                }

                if (EndDatePicker.SelectedDate.HasValue)
                {
                    var endDate = EndDatePicker.SelectedDate.Value;
                    query = query.Where(e => e.StartDate <= endDate);
                }

                // Применяем сортировку
                var sortTag = ((ComboBoxItem)SortComboBox.SelectedItem).Tag.ToString();
                query = sortTag switch
                {
                    "date" => query.OrderBy(e => e.StartDate).ThenBy(e => e.StartTime),
                    "venue" => query.OrderBy(e => e.VenueName).ThenBy(e => e.StartDate),
                    "sport" => query.OrderBy(e => e.SportName).ThenBy(e => e.StartDate),
                    _ => query.OrderBy(e => e.StartDate).ThenBy(e => e.StartTime)
                };

                var data = query.ToList();

                // Создаем отчет с вычисляемым полем (день недели)
                var reportData = data.Select(e => new ScheduleReportItem
                {
                    StartDate = e.StartDate,
                    StartTime = e.StartTime.ToString(@"hh\:mm"),
                    VenueName = e.VenueName,
                    Location = e.Location,
                    SportName = e.SportName,
                    DayOfWeek = GetRussianDayOfWeek(e.StartDate)
                }).ToList();

                // Отображаем данные
                ReportDataGrid.ItemsSource = reportData;

                // Итоговые данные (группировка)
                TotalEventsText.Text = reportData.Count.ToString();
                VenuesUsedText.Text = data.Select(e => e.VenueId).Distinct().Count().ToString();
                SportsCountText.Text = data.Select(e => e.SportName).Distinct().Count().ToString();

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

        private string GetRussianDayOfWeek(DateTime date)
        {
            var culture = new CultureInfo("ru-RU");
            return culture.DateTimeFormat.GetDayName(date.DayOfWeek);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public class ScheduleReportItem
    {
        public DateTime StartDate { get; set; }
        public string StartTime { get; set; }
        public string VenueName { get; set; }
        public string Location { get; set; }
        public string SportName { get; set; }

        // Вычисляемое поле
        public string DayOfWeek { get; set; }
    }
}
