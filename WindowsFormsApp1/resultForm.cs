using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
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
        private string formula;
        private double totalResult;
        private double totalCost;

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
            this.formula = formula;
            this.totalResult = totalResult;
            this.totalCost = totalCost;
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
                Location = new System.Drawing.Point(10, 810),
                Width = 100,
                DropDownStyle = ComboBoxStyle.DropDownList,
            };
            //formatComboBox.Items.AddRange(new[] { "PDF", "Word", "Excel", "CSV" });
            formatComboBox.Items.AddRange(new[] { "CSV" });

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

            dataGridView.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "include",
                HeaderText = "Включить",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                Width = 80
            }
            );
            dataGridView.Columns.Add("name", "Наименование работы");
            dataGridView.Columns.Add("percentage", "Процент");
            dataGridView.Columns.Add("hours", "Часы");
            dataGridView.Columns.Add("price", "Стоимость");

            // Настройка заголовка таблицы
            dataGridView.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Arial", 10);
            dataGridView.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            return dataGridView;
        }

        private void FillDataGrid(DataGridView grid, (string Name, double Percentage, double Hours, double Price)[] detailedResults)
        {
            foreach (var result in detailedResults)
            {
                // По умолчанию все строки выбраны
                grid.Rows.Add(true, result.Name, $"{result.Percentage:P1}", $"{result.Hours:F2}", $"{result.Price:F2}");
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

            // Диалог выбора пути сохранения
            using (var saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = GetSaveFileDialogFilter(format);
                saveFileDialog.Title = "Выберите место для сохранения файла";

                if (saveFileDialog.ShowDialog() != DialogResult.OK)
                {
                    return; // Пользователь отменил выбор
                }

                string fileName = saveFileDialog.FileName;

                // Получаем выбранные данные
                var selectedWorks = new List<(string Name, string Percentage, string Hours, string Price)>();
                AddSelectedWorks(sketchProjectGrid, selectedWorks);
                AddSelectedWorks(technicalProjectGrid, selectedWorks);
                AddSelectedWorks(pdsProjectGrid, selectedWorks);
                AddSelectedWorks(developmentDocGrid, selectedWorks);

                if (!selectedWorks.Any())
                {
                    MessageBox.Show("Выберите хотя бы одну работу для сохранения.", "Ошибка");
                    return;
                }

                // Сохраняем файл
                SaveToFile(selectedWorks, format, fileName);

                MessageBox.Show($"Данные успешно сохранены в файл {fileName}", "Успех");
            }
        }

        private string GetSaveFileDialogFilter(string format)
        {
            switch (format)
            {
                case "PDF":
                    return "PDF Files (*.pdf)|*.pdf";
                case "Word":
                    return "Word Files (*.docx)|*.docx";
                case "Excel":
                    return "Excel Files (*.xlsx)|*.xlsx";
                case "CSV":
                    return "CSV Files (*.csv)|*.csv";
                default:
                    return "All Files (*.*)|*.*";
            }
        }

        private void AddSelectedWorks(DataGridView grid, List<(string Name, string Percentage, string Hours, string Price)> selectedWorks)
        {
            foreach (DataGridViewRow row in grid.Rows)
            {
                if (row.Cells["include"] != null && Convert.ToBoolean(row.Cells["include"].Value))
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

        private void SaveToFile(List<(string Name, string Percentage, string Hours, string Price)> data, string format, string fileName)
        {
            try
            {
                switch (format)
                {
                    //case "PDF":
                    //	// Реализация экспорта в PDF
                    //	break;
                    //case "Word":
                    //	// Реализация экспорта в Word
                    //	break;
                    //case "Excel":
                    //	// Создание Excel-файла
                    //	using (var workbook = new XLWorkbook())
                    //	{
                    //		var worksheet = workbook.Worksheets.Add("Результаты");

                    //		// Добавление заголовков
                    //		worksheet.Cell(1, 1).Value = "Наименование работы";
                    //		worksheet.Cell(1, 2).Value = "Процент";
                    //		worksheet.Cell(1, 3).Value = "Часы";
                    //		worksheet.Cell(1, 4).Value = "Стоимость";

                    //		// Добавление данных
                    //		int row = 2;
                    //		foreach (var item in data)
                    //		{
                    //			worksheet.Cell(row, 1).Value = item.Name;
                    //			worksheet.Cell(row, 2).Value = item.Percentage;
                    //			worksheet.Cell(row, 3).Value = item.Hours;
                    //			worksheet.Cell(row, 4).Value = item.Price;
                    //			row++;
                    //		}

                    //		// Автоматическая настройка ширины столбцов
                    //		worksheet.Columns().AdjustToContents();

                    //		// Сохранение файла
                    //		workbook.SaveAs(fileName);
                    //	}

                    //	MessageBox.Show($"Данные успешно сохранены в файл {fileName}", "Успех");
                    //	break;

                    case "CSV":
                        // Сохранение в CSV
                        var csvData = "Наименование работы;Процент;Часы;Стоимость\n" +
                                      string.Join("\n", data.Select(d => $"{d.Name};{d.Percentage};{d.Hours};{d.Price}"));
                        File.WriteAllText(fileName, csvData, System.Text.Encoding.UTF8);
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении файла: {ex.Message}", "Ошибка");
            }
        }
    }
}
