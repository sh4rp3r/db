using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;

namespace BD
{
    public partial class AgeReportWindow : Window
    {
        private DatabaseContext _context;

        public AgeReportWindow(DatabaseContext context)
        {
            InitializeComponent();
            _context = context;
        }

        private void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получаем фильтр
                if (!int.TryParse(MinParticipantsTextBox.Text, out int minParticipants) || minParticipants < 1)
                {
                    MessageBox.Show("Введите корректное минимальное количество участников (≥1).", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Базовый запрос с JOIN по таблицам participants и sports, группировка по виду спорта
                var query = from p in _context.Participants
                            join s in _context.Sports on p.SportId equals s.SportId
                            group new { p, s } by new { s.SportId, s.Name, s.IsTeam } into g
                            select new
                            {
                                SportId = g.Key.SportId,
                                SportName = g.Key.Name,
                                IsTeam = g.Key.IsTeam,
                                ParticipantsCount = g.Count(),
                                Participants = g.Select(x => x.p).ToList()
                            };

                // Фильтр по типу спорта
                var sportTypeTag = ((ComboBoxItem)SportTypeFilterComboBox.SelectedItem).Tag.ToString();
                if (sportTypeTag == "team")
                {
                    query = query.Where(s => s.IsTeam == true);
                }
                else if (sportTypeTag == "individual")
                {
                    query = query.Where(s => s.IsTeam == false);
                }

                // Фильтр по минимальному количеству участников
                query = query.Where(s => s.ParticipantsCount >= minParticipants);

                var data = query.ToList();

                // Вычисляем возраст для каждого спортсмена
                var reportData = data.Select(s => new AgeReportItem
                {
                    SportName = s.SportName,
                    IsTeam = s.IsTeam,
                    ParticipantsCount = s.ParticipantsCount,
                    AverageAge = s.Participants.Average(p => CalculateAge(p.BirthDate)),
                    MinAge = s.Participants.Min(p => CalculateAge(p.BirthDate)),
                    MaxAge = s.Participants.Max(p => CalculateAge(p.BirthDate))
                }).ToList();

                // Применяем сортировку
                var sortTag = ((ComboBoxItem)SortComboBox.SelectedItem).Tag.ToString();
                reportData = sortTag switch
                {
                    "age_desc" => reportData.OrderByDescending(x => x.AverageAge).ToList(),
                    "age_asc" => reportData.OrderBy(x => x.AverageAge).ToList(),
                    "name" => reportData.OrderBy(x => x.SportName).ToList(),
                    "count_desc" => reportData.OrderByDescending(x => x.ParticipantsCount).ToList(),
                    _ => reportData.OrderByDescending(x => x.AverageAge).ToList()
                };

                // Отображаем данные
                ReportDataGrid.ItemsSource = reportData;

                // Итоговые данные
                SportsCountText.Text = reportData.Count.ToString();
                TotalParticipantsText.Text = reportData.Sum(r => r.ParticipantsCount).ToString();
                OverallAvgAgeText.Text = reportData.Count > 0
                    ? reportData.Average(r => r.AverageAge).ToString("F2")
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

        private int CalculateAge(DateTime birthDate)
        {
            int age = DateTime.Now.Year - birthDate.Year;
            if (DateTime.Now.DayOfYear < birthDate.DayOfYear)
                age--;
            return age;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public class AgeReportItem
    {
        public string SportName { get; set; }
        public bool IsTeam { get; set; }
        public int ParticipantsCount { get; set; }
        public double AverageAge { get; set; }
        public int MinAge { get; set; }
        public int MaxAge { get; set; }

        public string SportTypeDisplay => IsTeam ? "Командный" : "Индивидуальный";
    }
}
