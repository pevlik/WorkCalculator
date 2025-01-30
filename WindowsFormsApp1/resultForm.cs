using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace EffortCalculator
{
    public partial class ResultForm : Form
    {
        private Label formulaLabel;
        private Label resultLabel;
        private Label ResCoeffLabel;
        private Label totalCostLabel;
        private DataGridView sketchProjectGrid, technicalProjectGrid, pdsProjectGrid, developmentDocGrid;
        private ComboBox formatComboBox;
        private Button saveButton;

        public ResultForm(
            string formula,
            double totalResult,
            double totalCost,
            double baseResult,
            (string Name, double Percentage, double Hours, double Price)[] sketchProjectResults,
            (string Name, double Percentage, double Hours, double Price)[] technicalProjectResults,
            (string Name, double Percentage, double Hours, double Price)[] pdsProjectResults,
            (string Name, double Percentage, double Hours, double Price)[] developmentDocResults
        )
        {
            InitializeComponent(
                formula,
                totalResult,
                totalCost,
                baseResult,
                sketchProjectResults,
                technicalProjectResults,
                pdsProjectResults,
                developmentDocResults
            );
        }

        private void InitializeComponent(
            string formula,
            double totalResult,
            double totalCost,
            double baseResult,
            (string Name, double Percentage, double Hours, double Price)[] sketchProjectResults,
            (string Name, double Percentage, double Hours, double Price)[] technicalProjectResults,
            (string Name, double Percentage, double Hours, double Price)[] pdsProjectResults,
            (string Name, double Percentage, double Hours, double Price)[] developmentDocResults
        )
        {
            this.Text = "Результаты расчета";
            this.ClientSize = new System.Drawing.Size(800, 900); // Увеличен размер формы

            // Поля результатов расчета
            formulaLabel = new Label // Поле для формулы
            {
                Text = $"Формула расчета: {formula}",
                AutoSize = true,
                Location = new System.Drawing.Point(10, 10),
            };

            resultLabel = new Label // Поле для общего результата
            {
                Text = $"Общее время: {baseResult:F2} часов",
                AutoSize = true,
                Location = new System.Drawing.Point(10, 30),
            };

            ResCoeffLabel = new Label // Поле для общего результата + коэф
            {
                Text = $"Общее время + коэффициент: {totalResult:F2} часов",
                AutoSize = true,
                Location = new System.Drawing.Point(10, 50),
            };

            totalCostLabel = new Label // Лейбл для общей стоимости
            {
                Text = $"Общая стоимость: {totalCost:F2}",
                AutoSize = true,
                Location = new System.Drawing.Point(10, 70),
            };

            // Таблица эскизного проекта
            sketchProjectGrid = CreateDataGridView("Эскизный проект");
            sketchProjectGrid.Location = new System.Drawing.Point(10, 130);

            // Таблица технического проекта
            technicalProjectGrid = CreateDataGridView("Технический проект");
            technicalProjectGrid.Location = new System.Drawing.Point(10, 300);

            // Таблица ПДСП
            pdsProjectGrid = CreateDataGridView("ПДСП");
            pdsProjectGrid.Location = new System.Drawing.Point(10, 470);

            // Таблица разработки рабочей конструкторской документации
            developmentDocGrid = CreateDataGridView("Разработка РКД");
            developmentDocGrid.Location = new System.Drawing.Point(10, 640);

            // Заполнение таблиц данными
            FillDataGrid(sketchProjectGrid, sketchProjectResults);
            FillDataGrid(technicalProjectGrid, technicalProjectResults);
            FillDataGrid(pdsProjectGrid, pdsProjectResults);
            FillDataGrid(developmentDocGrid, developmentDocResults);

            // Выбор формата сохранения
            formatComboBox = new ComboBox
            {
                Items = { "PDF", "Word", "Excel", "CSV" },
                Location = new System.Drawing.Point(10, 810),
                Width = 100,
                DropDownStyle = ComboBoxStyle.DropDownList,
            };

            // Кнопка для сохранения
            saveButton = new Button
            {
                Text = "Сохранить",
                Location = new System.Drawing.Point(150, 810),
                Width = 100,
            };
            saveButton.Click += SaveButton_Click;

            // Добавление контролов на форму
            this.Controls.Add(formulaLabel);
            this.Controls.Add(resultLabel);
            this.Controls.Add(ResCoeffLabel);
            this.Controls.Add(totalCostLabel);
            this.Controls.Add(sketchProjectGrid);
            this.Controls.Add(technicalProjectGrid);
            this.Controls.Add(pdsProjectGrid);
            this.Controls.Add(developmentDocGrid);
            this.Controls.Add(formatComboBox);
            this.Controls.Add(saveButton);
        }

        private DataGridView CreateDataGridView(string headerText)
        {
            var dataGridView = new DataGridView
            {
                Size = new System.Drawing.Size(660, 150),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                ReadOnly = false,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
            };

            dataGridView.Columns.Add(
                new DataGridViewCheckBoxColumn
                {
                    Name = "include",
                    HeaderText = "Включить",
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                    Width = 80,
                }
            );
            dataGridView.Columns.Add("name", "Наименование работы");
            dataGridView.Columns.Add("percentage", "Процент");
            dataGridView.Columns.Add("hours", "Часы");
            dataGridView.Columns.Add("price", "Стоимость");

            var header = new Label
            {
                Text = headerText,
                AutoSize = true,
                Location = new System.Drawing.Point(0, -20),
            };
            dataGridView.Controls.Add(header);

            return dataGridView;
        }

        private void FillDataGrid(DataGridView grid, (string Name, double Percentage, double Hours, double Price)[] detailedResults)
        {
            foreach (var result in detailedResults)
            {
                grid.Rows.Add(
                    true, // По умолчанию все строки выбраны
                    result.Name,
                    $"{result.Percentage:P1}",
                    $"{result.Hours:F2}",
                    $"{result.Price:F2}"
                );
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (formatComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите формат для сохранения.", "Ошибка");
                return;
            }

            string format = formatComboBox.SelectedItem.ToString();

            // Список выбранных работ для всех таблиц
            var selectedWorks =
                new List<(string Name, string Percentage, string Hours, string Price)>();
            AddSelectedWorks(sketchProjectGrid, selectedWorks);
            AddSelectedWorks(technicalProjectGrid, selectedWorks);
            AddSelectedWorks(pdsProjectGrid, selectedWorks);
            AddSelectedWorks(developmentDocGrid, selectedWorks);

            if (!selectedWorks.Any())
            {
                MessageBox.Show("Выберите хотя бы одну работу для сохранения.", "Ошибка");
                return;
            }

            string message = "Вы выбрали следующие работы для сохранения:\n";
            foreach (var work in selectedWorks)
            {
                message += $"- {work.Name}: {work.Hours} часов, {work.Price}.\n";
            }

            message += $"Формат сохранения: {format}";

            MessageBox.Show(message, "Данные для сохранения");
            // Здесь вы можете реализовать реальное сохранение данных в выбранном формате.
        }

        private void AddSelectedWorks(DataGridView grid, List<(string Name, string Percentage, string Hours, string Price)> selectedWorks)
        {
            foreach (DataGridViewRow row in grid.Rows)
            {
                var isSelected = Convert.ToBoolean(row.Cells["include"].Value);
                if (isSelected)
                {
                    selectedWorks.Add(
                        (
                            row.Cells["name"].Value?.ToString(),
                            row.Cells["percentage"].Value?.ToString(),
                            row.Cells["hours"].Value?.ToString(),
                            row.Cells["price"].Value?.ToString()
                        )
                    );
                }
            }
        }
    }
}
