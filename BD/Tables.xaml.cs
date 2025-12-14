using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
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
    /// Логика взаимодействия для Tables.xaml
    /// </summary>
    public partial class Tables : Page
    {
        private int _tableNumber;
        private DatabaseContext _context;
        private ICollectionView _collectionView;
        private List<object> _currentData;
        public Tables(int buttonNumber)
        {
            InitializeComponent();
            _tableNumber = buttonNumber;
            _context = new DatabaseContext();

            InitializePage();
            LoadData();
        }
        private void InitializePage()
        {
            switch (_tableNumber)
            {
                case 1:
                    TitleText.Text = "Страны";
                    SearchFieldComboBox.ItemsSource = new List<string> { "CountryId", "Name" };
                    SortFieldComboBox.ItemsSource = new List<string> { "CountryId", "Name" };
                    break;
                case 2:
                    TitleText.Text = "Виды спорта";
                    SearchFieldComboBox.ItemsSource = new List<string> { "SportId", "Name", "IsTeam", "Description" };
                    SortFieldComboBox.ItemsSource = new List<string> { "SportId", "Name", "IsTeam" };
                    break;
                case 3:
                    TitleText.Text = "Участники";
                    SearchFieldComboBox.ItemsSource = new List<string> { "ParticipantId", "FullName", "BirthDate", "Gender" };
                    SortFieldComboBox.ItemsSource = new List<string> { "ParticipantId", "FullName", "BirthDate", "Gender" };
                    break;
                case 4:
                    TitleText.Text = "Расписание стартов";
                    SearchFieldComboBox.ItemsSource = new List<string> { "ScheduleId", "StartDate", "StartTime" };
                    SortFieldComboBox.ItemsSource = new List<string> { "ScheduleId", "StartDate", "StartTime" };
                    break;
                case 5:
                    TitleText.Text = "Спортивные площадки";
                    SearchFieldComboBox.ItemsSource = new List<string> { "VenueId", "Name", "Location" };
                    SortFieldComboBox.ItemsSource = new List<string> { "VenueId", "Name", "Location" };
                    break;
                case 6:
                    TitleText.Text = "Результаты";
                    SearchFieldComboBox.ItemsSource = new List<string> { "ResultId", "Place", "Score" };
                    SortFieldComboBox.ItemsSource = new List<string> { "ResultId", "Place", "Score" };
                    break;
            }

            SearchFieldComboBox.SelectedIndex = 0;
            SortFieldComboBox.SelectedIndex = 0;
            DataGridView.SelectionChanged += DataGridView_SelectionChanged;
        }

        private void LoadData()
        {
            try
            {
                switch (_tableNumber)
                {
                    case 1:
                        _currentData = _context.Countries.ToList<object>();
                        break;
                    case 2:
                        _currentData = _context.Sports.ToList<object>();
                        break;
                    case 3:
                        _currentData = _context.Participants.ToList<object>();
                        break;
                    case 4:
                        _currentData = _context.Schedule.ToList<object>();
                        break;
                    case 5:
                        _currentData = _context.Venues.ToList<object>();
                        break;
                    case 6:
                        _currentData = _context.Results.ToList<object>();
                        break;
                }

                DataGridView.ItemsSource = _currentData;
                _collectionView = CollectionViewSource.GetDefaultView(DataGridView.ItemsSource);

                UpdateRecordCount();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new EditRecordWindow(_tableNumber, null, _context);
            if (editWindow.ShowDialog() == true)
            {
                RefreshButton_Click(sender, e);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridView.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите запись для редактирования.",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var editWindow = new EditRecordWindow(_tableNumber, DataGridView.SelectedItem, _context);
            if (editWindow.ShowDialog() == true)
            {
                RefreshButton_Click(sender, e);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridView.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите запись для удаления.",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show("Вы уверены, что хотите удалить эту запись?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var selectedItem = DataGridView.SelectedItem;

                    switch (_tableNumber)
                    {
                        case 1:
                            _context.Countries.Remove((Countries)selectedItem);
                            break;
                        case 2:
                            _context.Sports.Remove((Sports)selectedItem);
                            break;
                        case 3:
                            _context.Participants.Remove((Participants)selectedItem);
                            break;
                        case 4:
                            _context.Schedule.Remove((Schedule)selectedItem);
                            break;
                        case 5:
                            _context.Venues.Remove((Venues)selectedItem);
                            break;
                        case 6:
                            _context.Results.Remove((Results)selectedItem);
                            break;
                    }

                    _context.SaveChanges();
                    MessageBox.Show("Запись успешно удалена.", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    RefreshButton_Click(sender, e);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Clear();
            FilterStatusText.Text = "";
            LoadData();
        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                _collectionView.Filter = null;
                FilterStatusText.Text = "";
                UpdateRecordCount();
                return;
            }

            string searchText = SearchTextBox.Text.ToLower();
            string searchField = SearchFieldComboBox.SelectedItem?.ToString();
            var operatorItem = (ComboBoxItem)FilterOperatorComboBox.SelectedItem;
            string operatorType = operatorItem.Tag.ToString();

            _collectionView.Filter = item =>
            {
                if (item == null) return false;

                var property = item.GetType().GetProperty(searchField);
                if (property == null) return false;

                var value = property.GetValue(item);
                if (value == null) return false;

                try
                {
                    switch (operatorType)
                    {
                        case "LIKE":
                            return value.ToString().ToLower().Contains(searchText);

                        case "NOT_LIKE":
                            return !value.ToString().ToLower().Contains(searchText);

                        case "EQUALS":
                            return value.ToString().ToLower() == searchText;

                        case "NOT_EQUALS":
                            return value.ToString().ToLower() != searchText;

                        case "GREATER":
                            // Пробуем сравнить как числа
                            if (decimal.TryParse(value.ToString(), out decimal numValue) &&
                                decimal.TryParse(SearchTextBox.Text, out decimal numSearch))
                            {
                                return numValue > numSearch;
                            }
                            // Пробуем сравнить как даты
                            if (value is DateTime dateValue &&
                                DateTime.TryParse(SearchTextBox.Text, out DateTime dateSearch))
                            {
                                return dateValue > dateSearch;
                            }
                            // Сравниваем как строки
                            return string.Compare(value.ToString(), SearchTextBox.Text, StringComparison.Ordinal) > 0;

                        case "LESS":
                            // Пробуем сравнить как числа
                            if (decimal.TryParse(value.ToString(), out decimal numValue2) &&
                                decimal.TryParse(SearchTextBox.Text, out decimal numSearch2))
                            {
                                return numValue2 < numSearch2;
                            }
                            // Пробуем сравнить как даты
                            if (value is DateTime dateValue2 &&
                                DateTime.TryParse(SearchTextBox.Text, out DateTime dateSearch2))
                            {
                                return dateValue2 < dateSearch2;
                            }
                            // Сравниваем как строки
                            return string.Compare(value.ToString(), SearchTextBox.Text, StringComparison.Ordinal) < 0;

                        default:
                            return false;
                    }
                }
                catch
                {
                    return false;
                }
            };

            FilterStatusText.Text = $"Фильтр: {searchField} {operatorItem.Content} '{SearchTextBox.Text}'";
            UpdateRecordCount();
        }

        private void SortButton_Click(object sender, RoutedEventArgs e)
        {
            if (_collectionView == null) return;

            string sortField = SortFieldComboBox.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(sortField))
            {
                MessageBox.Show("Пожалуйста, выберите поле для сортировки.",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Получаем направление сортировки
            var selectedItem = (ComboBoxItem)SortDirectionComboBox.SelectedItem;
            var direction = selectedItem.Tag.ToString();

            ListSortDirection sortDirection = direction == "Ascending"
                ? ListSortDirection.Ascending
                : ListSortDirection.Descending;

            // Очищаем предыдущую сортировку
            _collectionView.SortDescriptions.Clear();

            // Добавляем новую сортировку
            _collectionView.SortDescriptions.Add(
                new SortDescription(sortField, sortDirection)
            );

            MessageBox.Show($"Сортировка применена по полю '{sortField}' " +
                (sortDirection == ListSortDirection.Ascending ? "по возрастанию" : "по убыванию"),
                "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DataGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataGridView.SelectedItem != null)
            {
                var idProperty = DataGridView.SelectedItem.GetType().GetProperty(
                    _tableNumber switch
                    {
                        1 => "CountryId",
                        2 => "SportId",
                        3 => "VenueId",
                        4 => "ParticipantId",
                        5 => "ScheduleId",
                        6 => "ResultId",
                        _ => "Id"
                    });
                var id = idProperty?.GetValue(DataGridView.SelectedItem);
                SelectedRecordText.Text = $"ID {id}";
            }
            else
            {
                SelectedRecordText.Text = "нет";
            }
        }

        private void UpdateRecordCount()
        {
            if (_collectionView != null)
            {
                int count = 0;
                foreach (var item in _collectionView)
                {
                    count++;
                }
                RecordCountText.Text = count.ToString();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
    }
}
