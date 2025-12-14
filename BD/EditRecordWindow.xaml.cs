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
using System.Windows.Shapes;

namespace BD
{
    /// <summary>
    /// Логика взаимодействия для EditRecordWindow.xaml
    /// </summary>
    public partial class EditRecordWindow : Window
    {
        private int _tableNumber;
        private object _record;
        private DatabaseContext _context;
        private Dictionary<string, TextBox> _fieldTextBoxes = new Dictionary<string, TextBox>();
        private Dictionary<string, ComboBox> _fieldComboBoxes = new Dictionary<string, ComboBox>();
        private bool _isNewRecord;

        public EditRecordWindow(int tableNumber, object record, DatabaseContext context)
        {
            InitializeComponent();

            _tableNumber = tableNumber;
            _record = record;
            _context = context;
            _isNewRecord = (record == null);

            WindowTitle.Text = _isNewRecord ? "Добавление записи" : "Редактирование записи";

            BuildFields();
        }

        private void BuildFields()
        {
            switch (_tableNumber)
            {
                case 1: // Countries
                    if (_isNewRecord)
                        _record = new Countries();
                    AddField("Name", "Название страны", ((Countries)_record).Name);
                    break;

                case 2: // Sports
                    if (_isNewRecord)
                        _record = new Sports();
                    AddField("Name", "Название вида спорта", ((Sports)_record).Name);
                    AddCheckBox("IsTeam", "Командный вид спорта", ((Sports)_record).IsTeam);
                    AddField("Description", "Описание", ((Sports)_record).Description);
                    break;

                case 3: // Venues
                    if (_isNewRecord)
                        _record = new Venues();
                    AddField("Name", "Название места", ((Venues)_record).Name);
                    AddField("Location", "Местоположение", ((Venues)_record).Location);
                    break;

                case 4: // Participants
                    if (_isNewRecord)
                        _record = new Participants { BirthDate = DateTime.Now.AddYears(-20) };

                    AddComboBoxField("CountryId", "Страна", ((Participants)_record).CountryId,
                        _context.Countries, "CountryId", "Name");
                    AddComboBoxField("SportId", "Вид спорта", ((Participants)_record).SportId,
                        _context.Sports, "SportId", "Name");
                    AddField("FullName", "Полное имя", ((Participants)_record).FullName);
                    AddField("BirthDate", "Дата рождения (ГГГГ-ММ-ДД)",
                        ((Participants)_record).BirthDate.ToString("yyyy-MM-dd"));
                    AddComboBoxField("Gender", "Пол", ((Participants)_record).Gender,
                        new List<GenderOption> {
                            new GenderOption { Code = "M", Name = "Мужской" },
                            new GenderOption { Code = "F", Name = "Женский" }
                        }, "Code", "Name");
                    break;

                case 5: // Schedule
                    if (_isNewRecord)
                        _record = new Schedule { StartDate = DateTime.Now, StartTime = TimeSpan.FromHours(10) };

                    AddComboBoxField("SportId", "Вид спорта", ((Schedule)_record).SportId,
                        _context.Sports, "SportId", "Name");
                    AddComboBoxField("VenueId", "Место проведения", ((Schedule)_record).VenueId,
                        _context.Venues, "VenueId", "Name");
                    AddField("StartDate", "Дата начала (ГГГГ-ММ-ДД)",
                        ((Schedule)_record).StartDate.ToString("yyyy-MM-dd"));
                    AddField("StartTime", "Время начала (ЧЧ:ММ)",
                        ((Schedule)_record).StartTime.ToString(@"hh\:mm"));
                    break;

                case 6: // Results
                    if (_isNewRecord)
                        _record = new Results { Score = 0 };

                    AddComboBoxField("SportId", "Вид спорта", ((Results)_record).SportId,
                        _context.Sports, "SportId", "Name");
                    AddComboBoxField("ParticipantId", "Участник", ((Results)_record).ParticipantId,
                        _context.Participants, "ParticipantId", "FullName");
                    AddField("Place", "Место", ((Results)_record).Place?.ToString() ?? "");
                    AddField("Score", "Результат", ((Results)_record).Score.ToString());
                    break;
            }
        }

        private void AddField(string propertyName, string label, string value)
        {
            var stackPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 15) };

            var textBlock = new TextBlock
            {
                Text = label,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 5)
            };

            var textBox = new TextBox
            {
                Text = value ?? "",
                Height = 35,
                Padding = new Thickness(8),
                VerticalContentAlignment = VerticalAlignment.Center,
                FontSize = 14
            };

            stackPanel.Children.Add(textBlock);
            stackPanel.Children.Add(textBox);
            FieldsPanel.Children.Add(stackPanel);

            _fieldTextBoxes[propertyName] = textBox;
        }

        private void AddCheckBox(string propertyName, string label, bool value)
        {
            var stackPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 15) };

            var checkBox = new CheckBox
            {
                Content = label,
                IsChecked = value,
                FontWeight = FontWeights.SemiBold,
                VerticalContentAlignment = VerticalAlignment.Center
            };

            stackPanel.Children.Add(checkBox);
            FieldsPanel.Children.Add(stackPanel);

            // Сохраняем в словарь через временный TextBox для единообразия
            var hiddenTextBox = new TextBox { Visibility = Visibility.Collapsed };
            _fieldTextBoxes[propertyName] = hiddenTextBox;
            checkBox.Checked += (s, e) => hiddenTextBox.Text = "true";
            checkBox.Unchecked += (s, e) => hiddenTextBox.Text = "false";
            hiddenTextBox.Text = value.ToString().ToLower();
        }

        private void AddComboBoxField(string propertyName, string label, object selectedValue,
            System.Collections.IEnumerable items, string valueMemberPath, string displayMemberPath)
        {
            var stackPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 15) };

            var textBlock = new TextBlock
            {
                Text = label,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 5)
            };

            var comboBox = new ComboBox
            {
                ItemsSource = items,
                SelectedValuePath = valueMemberPath,
                DisplayMemberPath = displayMemberPath,
                Height = 35,
                Padding = new Thickness(8, 0, 8, 0),
                VerticalContentAlignment = VerticalAlignment.Center,
                FontSize = 14
            };

            if (selectedValue != null && (int)selectedValue > 0)
            {
                comboBox.SelectedValue = selectedValue;
            }

            stackPanel.Children.Add(textBlock);
            stackPanel.Children.Add(comboBox);
            FieldsPanel.Children.Add(stackPanel);

            _fieldComboBoxes[propertyName] = comboBox;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                switch (_tableNumber)
                {
                    case 1: // Countries
                        var country = (Countries)_record;
                        country.Name = _fieldTextBoxes["Name"].Text;

                        if (_isNewRecord)
                            _context.Countries.Add(country);
                        break;

                    case 2: // Sports
                        var sport = (Sports)_record;
                        sport.Name = _fieldTextBoxes["Name"].Text;
                        sport.IsTeam = bool.Parse(_fieldTextBoxes["IsTeam"].Text);
                        sport.Description = _fieldTextBoxes["Description"].Text;

                        if (_isNewRecord)
                            _context.Sports.Add(sport);
                        break;

                    case 3: // Venues
                        var venue = (Venues)_record;
                        venue.Name = _fieldTextBoxes["Name"].Text;
                        venue.Location = _fieldTextBoxes["Location"].Text;

                        if (_isNewRecord)
                            _context.Venues.Add(venue);
                        break;

                    case 4: // Participants
                        var participant = (Participants)_record;
                        participant.CountryId = (int)_fieldComboBoxes["CountryId"].SelectedValue;
                        participant.SportId = (int)_fieldComboBoxes["SportId"].SelectedValue;
                        participant.FullName = _fieldTextBoxes["FullName"].Text;
                        participant.BirthDate = DateTime.Parse(_fieldTextBoxes["BirthDate"].Text);
                        participant.Gender = ((GenderOption)_fieldComboBoxes["Gender"].SelectedItem)?.Code;

                        if (_isNewRecord)
                            _context.Participants.Add(participant);
                        break;

                    case 5: // Schedule
                        var schedule = (Schedule)_record;
                        schedule.SportId = (int)_fieldComboBoxes["SportId"].SelectedValue;
                        schedule.VenueId = (int)_fieldComboBoxes["VenueId"].SelectedValue;
                        schedule.StartDate = DateTime.Parse(_fieldTextBoxes["StartDate"].Text);
                        schedule.StartTime = TimeSpan.Parse(_fieldTextBoxes["StartTime"].Text);

                        if (_isNewRecord)
                            _context.Schedule.Add(schedule);
                        break;

                    case 6: // Results
                        var result = (Results)_record;
                        result.SportId = (int)_fieldComboBoxes["SportId"].SelectedValue;
                        result.ParticipantId = (int)_fieldComboBoxes["ParticipantId"].SelectedValue;
                        result.Place = string.IsNullOrWhiteSpace(_fieldTextBoxes["Place"].Text)
                            ? (int?)null
                            : int.Parse(_fieldTextBoxes["Place"].Text);
                        result.Score = decimal.Parse(_fieldTextBoxes["Score"].Text);

                        if (_isNewRecord)
                            _context.Results.Add(result);
                        break;
                }

                _context.SaveChanges();

                MessageBox.Show("Запись успешно сохранена!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    // Вспомогательный класс для выбора пола
    public class GenderOption
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }

}
