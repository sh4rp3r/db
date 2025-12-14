using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.EntityFrameworkCore;

namespace BD
{
    /// <summary>
    /// Логика взаимодействия для CompositeFormWindow.xaml
    /// </summary>
    public partial class CompositeFormWindow : Window
    {
        private DatabaseContext _context;
        private ObservableCollection<ResultItem> _results;

        public CompositeFormWindow(DatabaseContext context)
        {
            InitializeComponent();
            _context = context;
            _results = new ObservableCollection<ResultItem>();

            LoadComboBoxData();
            ResultsDataGrid.ItemsSource = _results;
            BirthDatePicker.SelectedDate = DateTime.Now.AddYears(-20);
            UpdateResultsCount();
        }

        private void LoadComboBoxData()
        {
            // Загружаем страны
            CountryComboBox.ItemsSource = _context.Countries.ToList();

            // Загружаем виды спорта
            var sports = _context.Sports.ToList();
            SportComboBox.ItemsSource = sports;
            ResultSportComboBox.ItemsSource = sports;

            // Устанавливаем значения по умолчанию
            if (sports.Any())
            {
                SportComboBox.SelectedIndex = 0;
                ResultSportComboBox.SelectedIndex = 0;
            }

            if (_context.Countries.Any())
            {
                CountryComboBox.SelectedIndex = 0;
            }
        }

        private void AddResultButton_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (ResultSportComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите вид спорта для результата.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedSport = (Sports)ResultSportComboBox.SelectedItem;

            // Проверка на дубликат вида спорта
            if (_results.Any(r => r.SportId == selectedSport.SportId))
            {
                MessageBox.Show("Результат для этого вида спорта уже добавлен.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int? place = null;
            if (!string.IsNullOrWhiteSpace(PlaceTextBox.Text))
            {
                if (!int.TryParse(PlaceTextBox.Text, out int placeValue) || placeValue <= 0)
                {
                    MessageBox.Show("Место должно быть положительным числом.", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                place = placeValue;
            }

            if (string.IsNullOrWhiteSpace(ScoreTextBox.Text))
            {
                MessageBox.Show("Введите результат.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!decimal.TryParse(ScoreTextBox.Text, out decimal score) || score < 0)
            {
                MessageBox.Show("Результат должен быть неотрицательным числом.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Добавляем результат в список
            _results.Add(new ResultItem
            {
                SportId = selectedSport.SportId,
                SportName = selectedSport.Name,
                Place = place,
                Score = score
            });

            // Очищаем поля
            PlaceTextBox.Clear();
            ScoreTextBox.Text = "0";

            UpdateResultsCount();
            MessageBox.Show("Результат добавлен в список.", "Успех",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RemoveResultButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var resultItem = button?.DataContext as ResultItem;

            if (resultItem != null)
            {
                _results.Remove(resultItem);
                UpdateResultsCount();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Валидация основных данных
            if (string.IsNullOrWhiteSpace(FullNameTextBox.Text))
            {
                MessageBox.Show("Введите полное имя участника.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (BirthDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату рождения.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (CountryComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите страну.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (SportComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите основной вид спорта.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var fullName = FullNameTextBox.Text.Trim();
                var countryId = (int)CountryComboBox.SelectedValue;

                // Конвертируем дату в UTC (ИСПРАВЛЕНИЕ!)
                var birthDate = BirthDatePicker.SelectedDate.Value;
                if (birthDate.Kind == DateTimeKind.Unspecified)
                {
                    birthDate = DateTime.SpecifyKind(birthDate, DateTimeKind.Utc);
                }

                // Проверяем уникальность (country_id, full_name)
                var existingParticipant = _context.Participants
                    .FirstOrDefault(p => p.CountryId == countryId && p.FullName == fullName);

                if (existingParticipant != null)
                {
                    MessageBox.Show(
                        "Участник '" + fullName + "' из этой страны уже существует!\n" +
                        "Измените имя или выберите другую страну.",
                        "Дубликат участника",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                // Создаем участника (таблица 1)
                var participant = new Participants
                {
                    FullName = fullName,
                    BirthDate = birthDate,  // Используем конвертированную дату
                    CountryId = countryId,
                    SportId = (int)SportComboBox.SelectedValue,
                    Gender = ((ComboBoxItem)GenderComboBox.SelectedItem).Tag.ToString()
                };

                _context.Participants.Add(participant);
                _context.SaveChanges(); // Сохраняем, чтобы получить ParticipantId

                // Добавляем результаты (таблица 2 - связь 1:M)
                foreach (var resultItem in _results)
                {
                    var result = new Results
                    {
                        ParticipantId = participant.ParticipantId,
                        SportId = resultItem.SportId,
                        Place = resultItem.Place,
                        Score = resultItem.Score
                    };
                    _context.Results.Add(result);
                }

                _context.SaveChanges();

                MessageBox.Show(
                    "Участник '" + participant.FullName + "' успешно добавлен!\n" +
                    "Добавлено результатов: " + _results.Count.ToString(),
                    "Успех",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (DbUpdateException dbEx)
            {
                string errorMessage = "Ошибка при сохранении в базу данных:\n\n";

                Exception innerEx = dbEx.InnerException;
                while (innerEx != null)
                {
                    errorMessage += innerEx.Message + "\n\n";
                    innerEx = innerEx.InnerException;
                }

                MessageBox.Show(errorMessage, "Ошибка БД",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                // Откатываем изменения
                var entries = _context.ChangeTracker.Entries().ToList();
                foreach (var entry in entries)
                {
                    entry.State = EntityState.Detached;
                }
            }
            catch (Exception ex)
            {
                string errorMessage = "Произошла ошибка:\n\n";
                errorMessage += ex.Message + "\n\n";

                if (ex.InnerException != null)
                {
                    errorMessage += "Детали: " + ex.InnerException.Message;
                }

                MessageBox.Show(errorMessage, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                // Откатываем изменения
                var entries = _context.ChangeTracker.Entries().ToList();
                foreach (var entry in entries)
                {
                    entry.State = EntityState.Detached;
                }
            }
        }


        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void UpdateResultsCount()
        {
            ResultsCountText.Text = "(" + _results.Count.ToString() + ")";
        }
    }

    // Вспомогательный класс для отображения результатов
    public class ResultItem
    {
        public int SportId { get; set; }
        public string SportName { get; set; }
        public int? Place { get; set; }
        public decimal Score { get; set; }
    }
}