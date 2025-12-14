using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;

namespace BD
{
    public partial class MedalReportWindow : Window
    {
        private DatabaseContext _context;

        public MedalReportWindow(DatabaseContext context)
        {
            InitializeComponent();
            _context = context;
        }

        private void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получаем фильтр
                if (!int.TryParse(MinMedalsTextBox.Text, out int minMedals) || minMedals < 0)
                {
                    MessageBox.Show("Введите корректное минимальное количество медалей.", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Получаем данные из view с JOIN по таблицам countries, participants, results
                var query = from c in _context.Countries
                            join p in _context.Participants on c.CountryId equals p.CountryId into participants
                            from p in participants.DefaultIfEmpty()
                            join r in _context.Results on p.ParticipantId equals r.ParticipantId into results
                            from r in results.DefaultIfEmpty()
                            where r == null || r.Place <= 3
                            group r by new { c.CountryId, c.Name } into g
                            select new MedalReportItem
                            {
                                CountryName = g.Key.Name,
                                GoldMedals = g.Count(r => r != null && r.Place == 1),
                                SilverMedals = g.Count(r => r != null && r.Place == 2),
                                BronzeMedals = g.Count(r => r != null && r.Place == 3),
                                TotalMedals = g.Count(r => r != null && r.Place <= 3)
                            };

                // Применяем фильтр
                query = query.Where(m => m.TotalMedals >= minMedals);

                // Применяем сортировку
                var sortTag = ((ComboBoxItem)SortComboBox.SelectedItem).Tag.ToString();
                query = sortTag switch
                {
                    "country" => query.OrderBy(m => m.CountryName),
                    "total" => query.OrderByDescending(m => m.TotalMedals).ThenByDescending(m => m.GoldMedals),
                    "gold" => query.OrderByDescending(m => m.GoldMedals).ThenByDescending(m => m.SilverMedals),
                    _ => query.OrderBy(m => m.CountryName)
                };

                var reportData = query.ToList();

                // Отображаем данные
                ReportDataGrid.ItemsSource = reportData;

                // Вычисляем итоговые данные
                CountriesCountText.Text = reportData.Count.ToString();
                TotalGoldText.Text = reportData.Sum(m => m.GoldMedals).ToString();
                TotalSilverText.Text = reportData.Sum(m => m.SilverMedals).ToString();
                TotalBronzeText.Text = reportData.Sum(m => m.BronzeMedals).ToString();

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

    // Класс для отображения данных отчета
    public class MedalReportItem
    {
        public string CountryName { get; set; }
        public int GoldMedals { get; set; }
        public int SilverMedals { get; set; }
        public int BronzeMedals { get; set; }
        public int TotalMedals { get; set; }
    }
}
