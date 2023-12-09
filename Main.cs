using CalcMatrix.Data;
using Matrix32;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CalcMatrix
{
    public partial class Main : Form
    {
        public static BindingSource BindingSource { get; set; }
        public static CourseMatrixContext dbContext { get; set; }
        public Main()
        {
            InitializeComponent();
        }
        private void Main_Load(object sender, EventArgs e)
        {
            BindingSource = new BindingSource();
            dbContext = new CourseMatrixContext();

            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;

            SetControlAvailabilityA(false);
            SetControlAvailabilityB(false);

            NumerForMatrix.Maximum = decimal.MaxValue;
            NumerForMatrix.Minimum = decimal.MinValue;
            NumberForMatrixB.Maximum = decimal.MaxValue;
            NumberForMatrixB.Minimum = decimal.MinValue;
        }

        MatrixCourse matrixB;
        MatrixCourse matrixA;
        bool fl = false; // флаг заполнения двух матриц А и В
        private string currentOperation = "Неизвестная операция";
        private bool isSingleMatrixOperation = false;
        private bool isMatrixA = false;
        string filename;

        /*-------------------------Доп. функции-----------------------------*/
        // Установка видимости кнопок
        private void SetControlAvailabilityA(bool isEnabled)
        {
            Transpose.Enabled = isEnabled;
            AddNumber.Enabled = isEnabled;
            MultiNumber.Enabled = isEnabled;
            AddTwoMatrix.Enabled = isEnabled;
            MultiTwoMatrix.Enabled = isEnabled;
            DiffTwoMatrix.Enabled = isEnabled;
            DiffNumber.Enabled = isEnabled;
            ReMatrix.Enabled = isEnabled;
        }
        private void SetControlAvailabilityB(bool isEnabled)
        {
            TransposeB.Enabled = isEnabled;
            AddNumberB.Enabled = isEnabled;
            MultiNumberB.Enabled = isEnabled;
            AddTwoMatrix.Enabled = isEnabled;
            MultiTwoMatrix.Enabled = isEnabled;
            DiffTwoMatrix.Enabled = isEnabled;
            DiffNumberB.Enabled = isEnabled;
            ReMatrixB.Enabled = isEnabled;
        }
        // Заполнение матрицы, проверка корректности значений
        private MatrixCourse PopulateMatrixFromDataGridView(DataGridView dataGridView)
        {
            int rows = dataGridView.RowCount;
            int columns = dataGridView.ColumnCount;
            MatrixCourse matrix = new MatrixCourse(rows, columns);

            int indI = 0;
            int indJ = 0;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    indI = i;
                    indJ = j;
                    if (double.TryParse(dataGridView[j, i].Value?.ToString(), out double value))
                    {
                        matrix.SetElement(i, j, value);
                    }
                    else
                    {
                        MessageBox.Show("В матрице введено некорректное значение!");
                        // Выделение ячейки, где возникла ошибка
                        dataGridView1.CurrentCell = dataGridView1.Rows[indI].Cells[indJ];
                        return null;
                    }
                }
            }
            return matrix;
        }
        // Обработка ошибок в файл log
        static void LogErrorToFile(Exception ex)
        {
            try
            {
                string logDirectory = Path.Combine(Environment.CurrentDirectory, "log");

                // Проверяем, существует ли папка "log", если нет - создаем
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                string filePath = Path.Combine(logDirectory, "logXM.txt");

                // Запись ошибки в файл
                using (FileStream file = new FileStream(filePath, FileMode.Append))
                using (StreamWriter writer = new StreamWriter(file))
                {
                    writer.WriteLine("* --------------------------------------------------------------------- *");
                    writer.WriteLine(DateTime.Now.ToString() + ex.ToString());
                }
            }
            catch (Exception ex2)
            {
                AddInDB(Convert.ToString(ex2.Message), Convert.ToString(ex2.TargetSite), DateTime.Now);
            }

        }
        // Заполнение матрицы результатов
        private void FillDataGridViewFromMatrix(DataGridView dataGridView, MatrixCourse matrix)
        {
            dataGridView.RowCount = matrix.GetRows();
            dataGridView.ColumnCount = matrix.GetColumns();

            for (int i = 0; i < matrix.GetRows(); i++)
            {
                for (int j = 0; j < matrix.GetColumns(); j++)
                {
                    dataGridView.Rows[i].HeaderCell.Value = (i + 1).ToString();
                    dataGridView.Columns[j].HeaderText = (j + 1).ToString();
                    dataGridView[j, i].Value = matrix.GetElement(i, j);
                }
            }
        }
        // Расчёт определителя
        private async Task<string> CalculateDeterminantAsync(MatrixCourse matrix)
        {
            if (matrix.GetColumns() == matrix.GetRows())
            {
                double determinant = await Task.Run(() => MatrixOperations.CalculateDeterminant(matrix));
                return determinant.ToString();
            }
            else
            {
                return "Не квадратная";
            }

        }
        // Вывод результата в файл
        private void PrintGeneratedFile()
        {
            try
            {
                saveFileDialog1.Filter = "Text files(*, txt, pdf)|*.txt|All files(*.*)|*.*";
                // Если не нажата "Отмена"
                if (saveFileDialog1.ShowDialog() != DialogResult.Cancel)
                {
                    filename = saveFileDialog1.FileName;

                    string operationDescription = currentOperation;
                    string dataToSave;
                    if (isSingleMatrixOperation)
                    {
                        if (isMatrixA)
                        {
                            dataToSave = $"Матрица:\n{DisplayFormatted(dataGridView1)}\n\n" +
                                        $"{operationDescription}:\n{DisplayFormatted(dataGridView3)}";
                        }
                        else
                        {
                            dataToSave = $"Матрица:\n{DisplayFormatted(dataGridView2)}\n\n" +
                                        $"{operationDescription}:\n{DisplayFormatted(dataGridView3)}";
                        }
                        System.IO.File.WriteAllText(filename, dataToSave);

                    }
                    else
                    {
                        dataToSave = $"Матрица А:\n{DisplayFormatted(dataGridView1)}\n\n" +
                                        $"Матрица B:\n{DisplayFormatted(dataGridView2)}\n\n" +
                                        $"{operationDescription}:\n{DisplayFormatted(dataGridView3)}";
                        System.IO.File.WriteAllText(filename, dataToSave);
                    }
                }
            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);
                AddInDB(Convert.ToString(ex.Message), Convert.ToString(ex.TargetSite), DateTime.Now);
            }

        }
        // Форматный дисплей вывода в файле
        private string DisplayFormatted(DataGridView dataGridView)
        {
            StringBuilder data = new StringBuilder();

            for (int i = 0; i < dataGridView.Rows.Count; i++)
            {
                for (int j = 0; j < dataGridView.Columns.Count; j++)
                {
                    data.AppendFormat("{0,15}", dataGridView[j, i].Value?.ToString() ?? "null");
                }
                data.AppendLine();
            }

            return data.ToString();
        }
        // Вывод фала на печать
        private void PrintFile(string filePath)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();

                if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
                {
                    psi.FileName = "xdg-open"; // Для Linux и macOS
                }
                else if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    psi.FileName = "notepad.exe"; // Для Windows
                }
                else
                {
                    MessageBox.Show("Не удалось");
                    throw new InvalidOperationException("Не удалось определить операционную систему.");

                }

                psi.Arguments = $"\"{filePath}\"";
                psi.UseShellExecute = true;
                psi.CreateNoWindow = true;

                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при печати файла: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                AddInDB(Convert.ToString(ex.Message), Convert.ToString(ex.TargetSite), DateTime.Now);
            }
        }
        // Метод заполнения DGV через файл
        private void LoadMatrixFromFile(string filePath, DataGridView dataGridView)
        {
            try
            {
                string[] lines = File.ReadAllLines(filePath);

                // Предполагаем, что матрица прямоугольная
                int rows = lines.Length;
                int columns = lines[0].Split(' ').Length;

                MatrixCourse matrix = new MatrixCourse(rows, columns);

                for (int i = 0; i < rows; i++)
                {
                    string[] values = lines[i].Split(' ');
                    for (int j = 0; j < columns; j++)
                    {
                        if (double.TryParse(values[j], out double value))
                        {
                            matrix.SetElement(i, j, value);
                        }
                        else
                        {
                            MessageBox.Show("Ошибка чтения файла. Некорректное значение в матрице.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                }

                numericUpDown1.Value = rows;
                numericUpDown2.Value = columns;

                // Заполняем DataGridView
                dataGridView.Rows.Clear();
                dataGridView.Columns.Clear();

                for (int i = 0; i < columns; i++)
                {
                    dataGridView.Columns.Add($"{i + 1}", $"{i + 1}");
                }

                for (int i = 0; i < rows; i++)
                {
                    dataGridView.Rows.Add();
                    for (int j = 0; j < columns; j++)
                    {
                        dataGridView[j, i].Value = matrix.GetElement(i, j);
                        dataGridView1.Rows[i].HeaderCell.Value = (i + 1).ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке матрицы из файла: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                AddInDB(Convert.ToString(ex.Message), Convert.ToString(ex.TargetSite), DateTime.Now);
            }
        }
        // Добавление записи в БД
        public static void AddInDB(string m, string t, DateTime d)
        {
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    ExceptionCourse Exception1 = new ExceptionCourse { Message = m, TargetSite = t, DateTimeExc = d };
                    dbContext.ExceptionCourses.Add(Exception1);
                    dbContext.SaveChanges();

                    transaction.Commit(); // Фиксация транзакции
                    BindingSource.Add(Exception1);
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    MessageBox.Show("Ошибка запроса к базе данных!!!");
                }
            }
        }

        /*-------------------------Элементы формы-------------------------*/
        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                PrintGeneratedFile();
                MessageBox.Show("Файл сохранен");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении файла: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void очиститьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();
            dataGridView2.Rows.Clear();
            dataGridView2.Columns.Clear();
            dataGridView3.Rows.Clear();
            dataGridView3.Columns.Clear();

            numericUpDown1.Value = numericUpDown1.Minimum;
            numericUpDown2.Value = numericUpDown2.Minimum;
            numericUpDown3.Value = numericUpDown3.Minimum;
            numericUpDown4.Value = numericUpDown4.Minimum;

            NumerForMatrix.Value = 0;
            NumberForMatrixB.Value = 0;

            DetA.Text = "";
            DetB.Text = "";
            DetC.Text = "";
        }
        private void печатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                PrintGeneratedFile();
                PrintFile(filename);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при печати файла: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            SetControlAvailabilityA(false);
        }
        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            SetControlAvailabilityA(false);
        }
        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            SetControlAvailabilityB(false);
        }
        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            SetControlAvailabilityB(false);
        }

        /*-------------Создание матриц--------------*/
        private void createMatrixA_Click(object sender, EventArgs e)
        {
            try
            {
                Transpose.Enabled = true;
                AddNumber.Enabled = true;
                DiffNumber.Enabled = true;
                MultiNumber.Enabled = true;
                ReMatrix.Enabled = true;
                if (fl)
                {
                    AddTwoMatrix.Enabled = true;
                    MultiTwoMatrix.Enabled = true;
                    DiffTwoMatrix.Enabled = true;
                }
                int rows = Convert.ToInt32(numericUpDown1.Value);
                int columns = Convert.ToInt32(numericUpDown2.Value);
                MatrixManager matrixCreateA = new MatrixManager(rows, columns);
                // Авто заполнение
                if (comboBox1.SelectedIndex == 1)
                {
                    matrixCreateA.FillRandom();
                    dataGridView1.RowCount = rows;
                    dataGridView1.ColumnCount = columns;
                    matrixA = matrixCreateA.GetMatrix();
                    for (int i = 0; i < rows; i++)
                    {
                        for (int j = 0; j < columns; j++)
                        {
                            dataGridView1.Columns[j].Width = 60;
                            dataGridView1.Rows[i].HeaderCell.Value = (i + 1).ToString();
                            dataGridView1.Columns[j].HeaderText = (j + 1).ToString();
                            dataGridView1.Rows[i].Cells[j].Value = matrixA.GetElement(i, j);
                        }
                    }
                }
                // Ручное заполнение
                else if (comboBox1.SelectedIndex == 0)
                {
                    dataGridView1.RowCount = rows;
                    dataGridView1.ColumnCount = columns;
                    matrixA = matrixCreateA.GetMatrix();
                    for (int i = 0; i < rows; i++)
                    {
                        for (int j = 0; j < columns; j++)
                        {
                            dataGridView1.Columns[j].Width = 60;
                            dataGridView1.Rows[i].HeaderCell.Value = (i + 1).ToString();
                            dataGridView1.Columns[j].HeaderText = (j + 1).ToString();
                        }
                    }
                }
                // Заполнение через файл
                else if (comboBox1.SelectedIndex == 2)
                {
                    openFileDialog1.Filter = "Text files(*.txt)|*.txt|All files(*.*)|*.*";

                    // Если не нажата "Отмена"
                    if (openFileDialog1.ShowDialog() != DialogResult.Cancel)
                    {
                        string filename = openFileDialog1.FileName;

                        // Загрузить матрицу из файла в dataGridView1
                        LoadMatrixFromFile(filename, dataGridView1);
                        Transpose.Enabled = true;
                        AddNumber.Enabled = true;
                        DiffNumber.Enabled = true;
                        MultiNumber.Enabled = true;
                        ReMatrix.Enabled = true;
                        if (fl)
                        {
                            AddTwoMatrix.Enabled = true;
                            MultiTwoMatrix.Enabled = true;
                            DiffTwoMatrix.Enabled = true;
                        }
                    }
                }
                fl = true;
            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);
                AddInDB(Convert.ToString(ex.Message), Convert.ToString(ex.TargetSite), DateTime.Now);
            }


        }
        private void createMatrixB_Click(object sender, EventArgs e)
        {
            try
            {
                MultiNumberB.Enabled = true;
                AddNumberB.Enabled = true;
                TransposeB.Enabled = true;
                DiffNumberB.Enabled = true;
                ReMatrixB.Enabled = true;
                if (fl)
                {
                    AddTwoMatrix.Enabled = true;
                    MultiTwoMatrix.Enabled = true;
                    DiffTwoMatrix.Enabled = true;
                }

                int rows = Convert.ToInt32(numericUpDown3.Value);
                int columns = Convert.ToInt32(numericUpDown4.Value);
                MatrixManager matrixCreateB = new MatrixManager(rows, columns);
                if (comboBox2.SelectedIndex == 1)
                {
                    matrixCreateB.FillRandom();
                    dataGridView2.RowCount = rows;
                    dataGridView2.ColumnCount = columns;
                    matrixB = matrixCreateB.GetMatrix();
                    for (int i = 0; i < rows; i++)
                    {
                        for (int j = 0; j < columns; j++)
                        {
                            dataGridView2.Columns[j].Width = 60;
                            dataGridView2.Rows[i].HeaderCell.Value = (i + 1).ToString();
                            dataGridView2.Columns[j].HeaderText = (j + 1).ToString();
                            dataGridView2.Rows[i].Cells[j].Value = matrixB.GetElement(i, j);
                        }
                    }

                }
                else if (comboBox2.SelectedIndex == 0)
                {
                    dataGridView2.RowCount = rows;
                    dataGridView2.ColumnCount = columns;
                    for (int i = 0; i < rows; i++)
                    {
                        for (int j = 0; j < columns; j++)
                        {
                            dataGridView2.Columns[j].Width = 60;
                            dataGridView2.Rows[i].HeaderCell.Value = (i + 1).ToString();
                            dataGridView2.Columns[j].HeaderText = (j + 1).ToString();
                        }
                    }
                }
                fl = true;
            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);
                AddInDB(Convert.ToString(ex.Message), Convert.ToString(ex.TargetSite), DateTime.Now);
            }
        }

        /*-------------Транспонирование матриц--------------*/
        private async void Transpose_Click(object sender, EventArgs e)
        {
            try
            {
                int rows = Convert.ToInt32(numericUpDown1.Value);
                int columns = Convert.ToInt32(numericUpDown2.Value);
                MatrixManager newMatrix = new MatrixManager(rows, columns);

                MatrixCourse result = newMatrix.GetMatrix();
                result = await Task.Run(() => PopulateMatrixFromDataGridView(dataGridView1));
                if (result != null)
                {
                    DetA.Text = await Task.Run(() => CalculateDeterminantAsync(result));
                    MatrixCourse tr = MatrixOperations.Transpose(result);
                    FillDataGridViewFromMatrix(dataGridView3, tr);
                    DetC.Text = await Task.Run(() => CalculateDeterminantAsync(tr));
                    currentOperation = "Транспонированная матрица";
                    isSingleMatrixOperation = true;
                    isMatrixA = true;
                }
            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);
                AddInDB(Convert.ToString(ex.Message), Convert.ToString(ex.TargetSite), DateTime.Now);
            }
        }
        private async void TransposeB_Click(object sender, EventArgs e)
        {
            try
            {
                int rows = Convert.ToInt32(numericUpDown3.Value);
                int columns = Convert.ToInt32(numericUpDown4.Value);
                MatrixManager newMatrix = new MatrixManager(rows, columns);

                MatrixCourse result = newMatrix.GetMatrix();
                result = await Task.Run(() => PopulateMatrixFromDataGridView(dataGridView2));
                if (result != null)
                {
                    DetB.Text = await Task.Run(() => CalculateDeterminantAsync(result));
                    MatrixCourse tr = MatrixOperations.Transpose(result);
                    FillDataGridViewFromMatrix(dataGridView3, tr);
                    DetC.Text = await Task.Run(() => CalculateDeterminantAsync(tr));
                    currentOperation = "Транспонированная матрица";
                    isSingleMatrixOperation = true;
                    isMatrixA = false;
                }
            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);
                AddInDB(Convert.ToString(ex.Message), Convert.ToString(ex.TargetSite), DateTime.Now);
            }
        }

        /*-------------Сложение матриц--------------*/
        private async void AddNumber_Click(object sender, EventArgs e)
        {
            try
            {
                int rows = Convert.ToInt32(numericUpDown1.Value);
                int columns = Convert.ToInt32(numericUpDown2.Value);
                MatrixManager newMatrix = new MatrixManager(rows, columns);

                MatrixCourse result = newMatrix.GetMatrix();
                result = await Task.Run(() => PopulateMatrixFromDataGridView(dataGridView1));
                if (result != null)
                {
                    DetA.Text = await Task.Run(() => CalculateDeterminantAsync(result));
                    int scalar = Convert.ToInt32(NumerForMatrix.Value);
                    MatrixCourse addMatrix = MatrixOperations.Add(result, scalar);
                    FillDataGridViewFromMatrix(dataGridView3, addMatrix);
                    DetC.Text = await Task.Run(() => CalculateDeterminantAsync(addMatrix));
                    currentOperation = $"Сложение матрицы с константой = {NumerForMatrix.Value}";
                    isSingleMatrixOperation = true;
                    isMatrixA = true;
                }
            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);
                AddInDB(Convert.ToString(ex.Message), Convert.ToString(ex.TargetSite), DateTime.Now);
            }
        }
        private async void AddNumberB_Click(object sender, EventArgs e)
        {
            try
            {
                int rows = Convert.ToInt32(numericUpDown3.Value);
                int columns = Convert.ToInt32(numericUpDown4.Value);
                MatrixManager newMatrix = new MatrixManager(rows, columns);

                MatrixCourse result = newMatrix.GetMatrix();
                result = await Task.Run(() => PopulateMatrixFromDataGridView(dataGridView2));
                if (result != null)
                {
                    DetB.Text = await Task.Run(() => CalculateDeterminantAsync(result));
                    int scalar = Convert.ToInt32(NumberForMatrixB.Value);
                    MatrixCourse addMatrix = MatrixOperations.Add(result, scalar);
                    FillDataGridViewFromMatrix(dataGridView3, addMatrix);
                    DetC.Text = await Task.Run(() => CalculateDeterminantAsync(addMatrix));
                    currentOperation = $"Сложение матрицы с константой = {NumberForMatrixB.Value}";
                    isSingleMatrixOperation = true;
                    isMatrixA = false;
                }
            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);
                AddInDB(Convert.ToString(ex.Message), Convert.ToString(ex.TargetSite), DateTime.Now);
            }
        }
        private async void AddTwoMatrix_Click(object sender, EventArgs e)
        {
            try
            {
                int rowsA = Convert.ToInt32(numericUpDown1.Value);
                int columnsA = Convert.ToInt32(numericUpDown2.Value);
                int rowsB = Convert.ToInt32(numericUpDown3.Value);
                int columnsB = Convert.ToInt32(numericUpDown4.Value);

                if (rowsA == rowsB && columnsA == columnsB)
                {
                    MatrixManager newMatrixA = new MatrixManager(rowsA, columnsA);
                    MatrixCourse matrixA = newMatrixA.GetMatrix();
                    matrixA = await Task.Run(() => PopulateMatrixFromDataGridView(dataGridView1));

                    MatrixManager newMatrixB = new MatrixManager(rowsB, columnsB);
                    MatrixCourse matrixB = newMatrixB.GetMatrix();
                    matrixB = await Task.Run(() => PopulateMatrixFromDataGridView(dataGridView2));
                    if (matrixA != null && matrixB != null)
                    {
                        DetA.Text = await Task.Run(() => CalculateDeterminantAsync(matrixA));
                        DetB.Text = await Task.Run(() => CalculateDeterminantAsync(matrixB));
                        MatrixCourse addTwoMatrix = MatrixOperations.Add(matrixA, matrixB);
                        FillDataGridViewFromMatrix(dataGridView3, addTwoMatrix);
                        DetC.Text = await Task.Run(() => CalculateDeterminantAsync(addTwoMatrix));
                        currentOperation = "Результат сложения матриц А + В";
                        isSingleMatrixOperation = false;
                    }
                }
                else
                {
                    MessageBox.Show("Размерность матриц (шхв) должна совпадать!");
                }

            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);
                AddInDB(Convert.ToString(ex.Message), Convert.ToString(ex.TargetSite), DateTime.Now);
            }
        }

        /*-------------Вычитание матриц--------------*/
        private async void DiffNumber_Click(object sender, EventArgs e)
        {
            try
            {
                int rows = Convert.ToInt32(numericUpDown1.Value);
                int columns = Convert.ToInt32(numericUpDown2.Value);
                MatrixManager newMatrix = new MatrixManager(rows, columns);

                MatrixCourse result = newMatrix.GetMatrix();
                result = await Task.Run(() => PopulateMatrixFromDataGridView(dataGridView1));
                if (result != null)
                {
                    DetA.Text = await Task.Run(() => CalculateDeterminantAsync(result));
                    int scalar = Convert.ToInt32(NumerForMatrix.Value);
                    MatrixCourse diffMatrix = MatrixOperations.Difference(result, scalar);
                    FillDataGridViewFromMatrix(dataGridView3, diffMatrix);
                    DetC.Text = await Task.Run(() => CalculateDeterminantAsync(diffMatrix));
                    currentOperation = $"Разность матрицы с константой = {NumerForMatrix.Value}";
                    isSingleMatrixOperation = true;
                    isMatrixA = true;
                }
            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);
                AddInDB(Convert.ToString(ex.Message), Convert.ToString(ex.TargetSite), DateTime.Now);
            }
        }
        private async void DiffNumberB_Click(object sender, EventArgs e)
        {
            try
            {
                int rows = Convert.ToInt32(numericUpDown3.Value);
                int columns = Convert.ToInt32(numericUpDown4.Value);
                MatrixManager newMatrix = new MatrixManager(rows, columns);

                MatrixCourse result = newMatrix.GetMatrix();
                result = await Task.Run(() => PopulateMatrixFromDataGridView(dataGridView2));
                if (result != null)
                {
                    DetB.Text = await Task.Run(() => CalculateDeterminantAsync(result));
                    int scalar = Convert.ToInt32(NumberForMatrixB.Value);
                    MatrixCourse diffMatrix = MatrixOperations.Difference(result, scalar);
                    FillDataGridViewFromMatrix(dataGridView3, diffMatrix);
                    DetC.Text = await Task.Run(() => CalculateDeterminantAsync(diffMatrix));
                    currentOperation = $"Разность матрицы с константой = {NumberForMatrixB.Value}";
                    isSingleMatrixOperation = true;
                    isMatrixA = false;
                }
            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);
                AddInDB(Convert.ToString(ex.Message), Convert.ToString(ex.TargetSite), DateTime.Now);
            }
        }
        private async void DiffTwoMatrix_Click(object sender, EventArgs e)
        {
            try
            {
                int rowsA = Convert.ToInt32(numericUpDown1.Value);
                int columnsA = Convert.ToInt32(numericUpDown2.Value);
                int rowsB = Convert.ToInt32(numericUpDown3.Value);
                int columnsB = Convert.ToInt32(numericUpDown4.Value);

                if (rowsA == rowsB && columnsA == columnsB)
                {
                    MatrixManager newMatrixA = new MatrixManager(rowsA, columnsA);
                    MatrixCourse matrixA = newMatrixA.GetMatrix();
                    matrixA = await Task.Run(() => PopulateMatrixFromDataGridView(dataGridView1));

                    MatrixManager newMatrixB = new MatrixManager(rowsB, columnsB);
                    MatrixCourse matrixB = newMatrixB.GetMatrix();
                    matrixB = await Task.Run(() => PopulateMatrixFromDataGridView(dataGridView2));
                    if (matrixA != null && matrixB != null)
                    {
                        DetA.Text = await Task.Run(() => CalculateDeterminantAsync(matrixA));
                        DetB.Text = await Task.Run(() => CalculateDeterminantAsync(matrixB));
                        MatrixCourse diffTwoMatrix = MatrixOperations.Difference(matrixA, matrixB);
                        FillDataGridViewFromMatrix(dataGridView3, diffTwoMatrix);
                        DetC.Text = await Task.Run(() => CalculateDeterminantAsync(diffTwoMatrix));
                        currentOperation = "Результат разности матриц А - В";
                        isSingleMatrixOperation = false;
                    }
                }
                else
                {
                    MessageBox.Show("Размерность матриц (шхв) должна совпадать!");
                }

            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);
                AddInDB(Convert.ToString(ex.Message), Convert.ToString(ex.TargetSite), DateTime.Now);
            }
        }

        /*-------------Умножение матриц--------------*/
        private async void MultiNumber_Click(object sender, EventArgs e)
        {
            try
            {
                int rows = Convert.ToInt32(numericUpDown1.Value);
                int columns = Convert.ToInt32(numericUpDown2.Value);
                MatrixManager newMatrix = new MatrixManager(rows, columns);

                MatrixCourse result = newMatrix.GetMatrix();
                result = await Task.Run(() => PopulateMatrixFromDataGridView(dataGridView1));
                if (result != null)
                {
                    DetA.Text = await Task.Run(() => CalculateDeterminantAsync(result));
                    int scalar = Convert.ToInt32(NumerForMatrix.Value);
                    MatrixCourse multiMatrix = MatrixOperations.MultiplyConst(result, scalar);
                    FillDataGridViewFromMatrix(dataGridView3, multiMatrix);
                    DetC.Text = await Task.Run(() => CalculateDeterminantAsync(multiMatrix));
                    currentOperation = $"Произведение матрицы с константой = {NumerForMatrix.Value}";
                    isSingleMatrixOperation = true;
                    isMatrixA = true;
                }
            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);
                AddInDB(Convert.ToString(ex.Message), Convert.ToString(ex.TargetSite), DateTime.Now);
            }
        }
        private async void MultiNumberB_Click(object sender, EventArgs e)
        {
            try
            {
                int rows = Convert.ToInt32(numericUpDown3.Value);
                int columns = Convert.ToInt32(numericUpDown4.Value);
                MatrixManager newMatrix = new MatrixManager(rows, columns);

                MatrixCourse result = newMatrix.GetMatrix();
                result = await Task.Run(() => PopulateMatrixFromDataGridView(dataGridView2));
                if (result != null)
                {
                    DetB.Text = await Task.Run(() => CalculateDeterminantAsync(result));
                    int scalar = Convert.ToInt32(NumberForMatrixB.Value);
                    MatrixCourse multiMatrix = MatrixOperations.MultiplyConst(result, scalar);
                    FillDataGridViewFromMatrix(dataGridView3, multiMatrix);
                    DetC.Text = await Task.Run(() => CalculateDeterminantAsync(multiMatrix));
                    currentOperation = $"Произведение матрицы с константой = {NumberForMatrixB.Value}";
                    isSingleMatrixOperation = true;
                    isMatrixA = false;
                }
            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);
                AddInDB(Convert.ToString(ex.Message), Convert.ToString(ex.TargetSite), DateTime.Now);
            }
        }
        private async void MultiTwoMatrix_Click(object sender, EventArgs e)
        {
            try
            {
                int rowsA = Convert.ToInt32(numericUpDown1.Value);
                int columnsA = Convert.ToInt32(numericUpDown2.Value);
                int rowsB = Convert.ToInt32(numericUpDown3.Value);
                int columnsB = Convert.ToInt32(numericUpDown4.Value);

                if (columnsA == rowsB)
                {
                    MatrixManager newMatrixA = new MatrixManager(rowsA, columnsA);
                    MatrixCourse matrixA = newMatrixA.GetMatrix();
                    matrixA = await Task.Run(() => PopulateMatrixFromDataGridView(dataGridView1));

                    MatrixManager newMatrixB = new MatrixManager(rowsB, columnsB);
                    MatrixCourse matrixB = newMatrixB.GetMatrix();
                    matrixB = await Task.Run(() => PopulateMatrixFromDataGridView(dataGridView2));
                    if (matrixA != null && matrixB != null)
                    {
                        DetA.Text = await Task.Run(() => CalculateDeterminantAsync(matrixA));
                        DetB.Text = await Task.Run(() => CalculateDeterminantAsync(matrixB));
                        MatrixCourse multiTwoMatrix = MatrixOperations.Multiply(matrixA, matrixB);
                        FillDataGridViewFromMatrix(dataGridView3, multiTwoMatrix);
                        DetC.Text = await Task.Run(() => CalculateDeterminantAsync(multiTwoMatrix));
                        currentOperation = "Результат умножения матриц А Х В";
                        isSingleMatrixOperation = false;
                    }
                }
                else
                {
                    MessageBox.Show("Количество столбцов матрицы А должно\nсовпадатьс количеством строк матрицы В!");
                }

            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);
                AddInDB(Convert.ToString(ex.Message), Convert.ToString(ex.TargetSite), DateTime.Now);
            }
        }

        /*-------------Обратная матрица-------------*/
        private async void ReMatrix_Click(object sender, EventArgs e)
        {
            try
            {
                int rows = Convert.ToInt32(numericUpDown1.Value);
                int columns = Convert.ToInt32(numericUpDown2.Value);
                if (rows == columns && (rows != 1 && columns != 1))
                {
                    MatrixManager newMatrix = new MatrixManager(rows, columns);
                    MatrixCourse result = newMatrix.GetMatrix();

                    result = await Task.Run(() => PopulateMatrixFromDataGridView(dataGridView1));
                    if (result != null)
                    {
                        DetA.Text = await Task.Run(() => CalculateDeterminantAsync(result));
                        MatrixCourse tr = MatrixOperations.Inverse(result);
                        FillDataGridViewFromMatrix(dataGridView3, tr);
                        DetC.Text = await Task.Run(() => CalculateDeterminantAsync(tr));
                        currentOperation = $"Обратная матрица";
                        isSingleMatrixOperation = true;
                        isMatrixA = true;
                    }
                }
                else
                {
                    MessageBox.Show("Матрица должна быть квадратной\nи не должна быть числом!");
                }

            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);
                AddInDB(Convert.ToString(ex.Message), Convert.ToString(ex.TargetSite), DateTime.Now);
            }
        }
        private async void ReMatrixB_Click(object sender, EventArgs e)
        {
            try
            {
                int rows = Convert.ToInt32(numericUpDown3.Value);
                int columns = Convert.ToInt32(numericUpDown4.Value);
                if (rows == columns && (rows != 1 && columns != 1))
                {
                    MatrixManager newMatrix = new MatrixManager(rows, columns);
                    MatrixCourse result = newMatrix.GetMatrix();

                    result = await Task.Run(() => PopulateMatrixFromDataGridView(dataGridView2));
                    if (result != null)
                    {
                        DetB.Text = await Task.Run(() => CalculateDeterminantAsync(result));
                        MatrixCourse tr = MatrixOperations.Inverse(result);
                        FillDataGridViewFromMatrix(dataGridView3, tr);
                        DetC.Text = await Task.Run(() => CalculateDeterminantAsync(tr));
                        currentOperation = $"Обратная матрица";
                        isSingleMatrixOperation = true;
                        isMatrixA = false;
                    }
                }
                else
                {
                    MessageBox.Show("Матрица должна быть квадратной\nи не должна быть числом!");
                }

            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);
                AddInDB(Convert.ToString(ex.Message), Convert.ToString(ex.TargetSite), DateTime.Now);
            }
        }
    }
}
