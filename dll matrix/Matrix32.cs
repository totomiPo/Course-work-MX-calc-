using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Matrix32
{
    // Класс упраления матрицой
    public class MatrixManager
    {
        public MatrixCourse matrix;
        public MatrixManager(int rows, int columns)
        {
            matrix = new MatrixCourse(rows, columns);
        }
        public void InputMatrix()
        {
            for (int i = 0; i < matrix.GetRows(); i++)
            {
                for (int j = 0; j < matrix.GetColumns(); j++)
                {
                    Console.Write($"Введите значение для ячейки [{i},{j}]: ");
                    if (double.TryParse(Console.ReadLine(), out double value))
                    {
                        matrix.SetElement(i, j, value);
                    }
                    else
                    {
                        Console.WriteLine("Некорректный ввод. Введите число.");
                        j--; // Повторить ввод для той же ячейки
                    }
                }
            }
        }
        public void Display()
        {
            Console.WriteLine("----------------------");
            Console.WriteLine("Матрица имеет вид:");
            Console.WriteLine("----------------------");
            for (int i = 0; i < matrix.GetRows(); i++)
            {
                Console.Write("| ");
                for (int j = 0; j < matrix.GetColumns(); j++)
                {
                    Console.Write(matrix.GetElement(i, j).ToString().PadLeft(4) + " ");
                }
                Console.Write(" |");
                Console.WriteLine();
            }
        }
        public void FillRandom()
        {
            Random random = new Random();
            for (int i = 0; i < matrix.GetRows(); i++)
            {
                for (int j = 0; j < matrix.GetColumns(); j++)
                {
                    matrix.SetElement(i, j, random.Next(1, 101));
                }
            }
        }
        public void SetElement(int row, int column, double value)
        {
            if (row < 0 || row >= matrix.GetRows() || column < 0 || column >= matrix.GetColumns())
            {
                throw new ArgumentOutOfRangeException("Invalid row or column index.");
            }

            matrix.SetElement(row, column, value);
        }
        public MatrixCourse GetMatrix()
        {
            return matrix;
        }
    }

    // Основной класс - матрица
    public class MatrixCourse
    {
        private double[,] data;
        private int rows;
        private int columns;

        #region Конструкторы

        // Конструктор по умолчанию
        public MatrixCourse()
        {
            rows = 1;
            columns = 1;
            data = new double[rows, columns];
        }

        // Конструктор с двумя параметрами
        public MatrixCourse(int rows, int columns)
        {
            if (rows >= 1 && columns >= 1)
            {
                this.rows = rows;
                this.columns = columns;
                data = new double[rows, columns];
            }
            else
            {
                throw new ArgumentException("Rows and columns must be greater than or equal to 2");
            }
        }

        // Конструктор с одним параметром (квадратная матрица)
        public MatrixCourse(int dimension)
        {
            if (dimension >= 1)
            {
                this.rows = dimension;
                this.columns = dimension;
                data = new double[rows, columns];
            }
            else
            {
                throw new ArgumentException("Rows and columns must be greater than or equal to 2");
            }
        }

        // Инициализация матрицы сцществующим двумерным массивом
        public MatrixCourse(double[,] array)
        {
            rows = array.GetLength(0);
            columns = array.GetLength(1);
            data = new double[rows, columns];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    data[i, j] = array[i, j];
                }
            }
        }
        #endregion

        #region Setter-Getter 
        public int GetRows()
        {
            return rows;
        }

        public int GetColumns()
        {
            return columns;
        }

        public double GetElement(int i, int j)
        {
            if (i >= 0 && i < rows && j >= 0 && j < columns)
            {
                return data[i, j];
            }
            else
            {
                throw new IndexOutOfRangeException("Index is out of range");
            }
        }

        public void SetElement(int i, int j, double value)
        {
            if (i >= 0 && i < rows && j >= 0 && j < columns)
            {
                data[i, j] = value;
            }
            else
            {
                throw new IndexOutOfRangeException("Index is out of range");
            }
        }

        public double[,] GetData()
        {
            return data;
        }
        #endregion
    }

    public class MatrixOperations
    {
        public static MatrixManager ConvertToMatrixManager(MatrixCourse matrixCourse)
        {
            int rows = matrixCourse.GetRows();
            int columns = matrixCourse.GetColumns();

            MatrixManager manager = new MatrixManager(rows, columns);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    manager.SetElement(i, j, matrixCourse.GetElement(i, j));
                }
            }

            return manager;
        }

        #region Сложение 
        public static MatrixCourse Add(MatrixCourse matrix1, MatrixCourse matrix2)
        {
            if (matrix1.GetRows() != matrix2.GetRows() || matrix1.GetColumns() != matrix2.GetColumns())
            {
                throw new ArgumentException("Matrix dimensions are not compatible for addition.");
            }

            MatrixCourse result = new MatrixCourse(matrix1.GetRows(), matrix1.GetColumns());
            for (int i = 0; i < matrix1.GetRows(); i++)
            {
                for (int j = 0; j < matrix1.GetColumns(); j++)
                {
                    result.SetElement(i, j, matrix1.GetElement(i, j) + matrix2.GetElement(i, j));
                }
            }
            return result;
        }

        public static MatrixCourse Add(MatrixCourse matrix, double scalar)
        {
            MatrixCourse result = new MatrixCourse(matrix.GetRows(), matrix.GetColumns());
            for (int i = 0; i < matrix.GetRows(); i++)
            {
                for (int j = 0; j < matrix.GetColumns(); j++)
                {
                    result.SetElement(i, j, matrix.GetElement(i, j) + scalar);
                }
            }
            return result;
        }
        #endregion

        #region Вычитание
        public static MatrixCourse Difference(MatrixCourse matrix1, MatrixCourse matrix2)
        {
            if (matrix1.GetRows() != matrix2.GetRows() || matrix1.GetColumns() != matrix2.GetColumns())
            {
                throw new ArgumentException("Matrix dimensions are not compatible for subtraction.");
            }

            MatrixCourse result = new MatrixCourse(matrix1.GetRows(), matrix1.GetColumns());
            for (int i = 0; i < matrix1.GetRows(); i++)
            {
                for (int j = 0; j < matrix1.GetColumns(); j++)
                {
                    double difference = matrix1.GetElement(i, j) - matrix2.GetElement(i, j);
                    result.SetElement(i, j, difference);
                }
            }
            return result;
        }

        public static MatrixCourse Difference(MatrixCourse matrix, double scalar)
        {
            MatrixCourse result = new MatrixCourse(matrix.GetRows(), matrix.GetColumns());
            for (int i = 0; i < matrix.GetRows(); i++)
            {
                for (int j = 0; j < matrix.GetColumns(); j++)
                {
                    result.SetElement(i, j, matrix.GetElement(i, j) - scalar);
                }
            }
            return result;
        }

        #endregion

        #region Умножение
        public static MatrixCourse Multiply(MatrixCourse matrix1, MatrixCourse matrix2)
        {
            int rows1 = matrix1.GetRows();
            int columns1 = matrix1.GetColumns();
            int rows2 = matrix2.GetRows();
            int columns2 = matrix2.GetColumns();

            if (columns1 != rows2)
            {
                throw new ArgumentException("Number of columns in the first matrix must be equal to the number of rows in the second matrix.");
            }

            MatrixCourse resultData = new MatrixCourse(rows1, columns2);
            for (int i = 0; i < rows1; i++)
            {
                for (int j = 0; j < columns2; j++)
                {
                    double sum = 0;

                    for (int k = 0; k < columns1; k++)
                    {
                        sum += matrix1.GetElement(i, k) * matrix2.GetElement(k, j);
                    }

                    resultData.SetElement(i, j, sum);
                }
            }
            return resultData;
        }

        public static MatrixCourse MultiplyConst(MatrixCourse matrix1, double num)
        {
            int rows = matrix1.GetRows();
            int columns = matrix1.GetColumns();

            MatrixCourse resultData = new MatrixCourse(rows, columns);
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    resultData.SetElement(i, j, matrix1.GetElement(i, j) * num);
                }
            }
            return resultData;
        }

        #endregion

        #region Транспонирование
        public static MatrixCourse Transpose(MatrixCourse matrix1)
        {
            int rows = matrix1.GetRows();
            int columns = matrix1.GetColumns();
            double[,] transposedData = new double[columns, rows];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    transposedData[j, i] = matrix1.GetElement(i, j);
                }
            }
            return new MatrixCourse(transposedData);
        }
        #endregion

        #region Минор матрицы
        // Метод для нахождения минора матрицы
        public static MatrixCourse GetMinor(MatrixCourse matrixCourse, int row, int col)
        {
            int rows = matrixCourse.GetRows();
            int columns = matrixCourse.GetColumns();
            double[,] matrixData = matrixCourse.GetData();

            if (row < 0 || row > rows || col < 0 || col > columns)
            {
                throw new ArgumentOutOfRangeException("Invalid row or column index.");
            }

            double[,] minorData = new double[rows - 1, columns - 1];

            for (int i = 0, m = 0; i < rows; i++)
            {
                if (i == row)
                    continue;

                for (int j = 0, n = 0; j < columns; j++)
                {
                    if (j == col)
                        continue;

                    minorData[m, n] = matrixData[i, j];
                    n++;
                }
                m++;
            }
            return new MatrixCourse(minorData);
        }

        #endregion

        #region Определитель матрицы

        /*public static double CalculateDeterminant(MatrixCourse matrix)
        {
            int rows = matrix.GetRows();
            int columns = matrix.GetColumns();

            if (rows != columns)
                throw new ArgumentException("Matrix must be square.");

            if (rows == 1)
                return matrix.GetElement(0, 0);

            if (rows == 2)
                return matrix.GetElement(0, 0) * matrix.GetElement(1, 1) - matrix.GetElement(0, 1) * matrix.GetElement(1, 0);

            double determinant = 0;

            for (int j = 0; j < columns; j++)
            {
                double sign = (j % 2 == 0) ? 1.0 : -1.0;
                MatrixCourse minotMatrix = GetMinor(matrix, 0, j);
                double minorDeterminant = CalculateDeterminant(minotMatrix);
                determinant += sign * matrix.GetElement(0, j) * minorDeterminant;
            }

            return determinant;
        }*/
        
        // Копирование текущей матрицы
        public static MatrixCourse Clone(MatrixCourse matrix)
        {
            int rows = matrix.GetRows();
            int columns = matrix.GetColumns();
            MatrixCourse cloneMatrix = new MatrixCourse(rows, columns);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    cloneMatrix.SetElement(i, j, matrix.GetElement(i, j));
                }
            }

            return cloneMatrix;
        }
        // Смена рядов
        public static void SwapRows(int row1, int row2, MatrixCourse matrix)
        {
            int columns = matrix.GetColumns();
            for (int j = 0; j < columns; j++)
            {
                double temp = matrix.GetElement(row1, j);
                matrix.SetElement(row1, j, matrix.GetElement(row2, j));
                matrix.SetElement(row2, j, temp);
            }
        }

        // Вычисление определителя методом Гаусса
        public static double CalculateDeterminant(MatrixCourse matrix)
        {
            int rows = matrix.GetRows();
            int columns = matrix.GetColumns();
            if (rows != columns)
            {
                throw new ArgumentException("Matrix must be square.");
            }
            double determinant = 1.0;
            // Создаем копию матрицы для избежания изменения оригинальной матрицы
            MatrixCourse workingMatrix = Clone(matrix);
            for (int i = 0; i < rows - 1; i++)
            {
                // Поиск максимального элемента в столбце
                int maxRowIndex = i;
                double maxElement = Math.Abs(workingMatrix.GetElement(i, i));

                for (int k = i + 1; k < rows; k++)
                {
                    double currentElement = Math.Abs(workingMatrix.GetElement(k, i));
                    if (currentElement > maxElement)
                    {
                        maxElement = currentElement;
                        maxRowIndex = k;
                    }
                }
                // Перестановка строк для обеспечения максимального элемента на главной диагонали
                if (maxRowIndex != i)
                {
                    SwapRows(i, maxRowIndex, workingMatrix);
                    determinant *= -1; // Меняем знак определителя при перестановке строк
                }
                // Приведение матрицы к верхнетреугольному виду с использованием элементарных преобразований
                for (int j = i + 1; j < rows; j++)
                {
                    double factor = workingMatrix.GetElement(j, i) / workingMatrix.GetElement(i, i);
                    for (int k = i; k < columns; k++)
                    {
                        double currentValue = workingMatrix.GetElement(j, k);
                        double subtractValue = factor * workingMatrix.GetElement(i, k);
                        workingMatrix.SetElement(j, k, currentValue - subtractValue);
                    }
                }
            }
            // Умножение элементов на главной диагонали для получения определителя
            for (int i = 0; i < rows; i++)
            {
                determinant *= workingMatrix.GetElement(i, i);
            }
            return determinant;
        }
        #endregion

        #region Алгебраическое дополнение
        public static double GetAlgebraicCofactor(MatrixCourse matrix, int row, int col)
        {
            int sign = ((row + col) % 2 == 0) ? 1 : -1;
            MatrixCourse minor = GetMinor(matrix, row, col);
            MatrixOperations.ConvertToMatrixManager(minor).Display();
            double minorDeterminant = CalculateDeterminant(minor);

            return sign * minorDeterminant;
        }
        #endregion

        #region Обратная матрица
        public static MatrixCourse Inverse(MatrixCourse matrixCourse)
        {
            int rows = matrixCourse.GetRows();
            int columns = matrixCourse.GetColumns();
            double[,] matrix = matrixCourse.GetData();

            if (rows != columns)
                throw new ArgumentException("Matrix must be square.");

            double determinant = CalculateDeterminant(matrixCourse);

            if (determinant == 0)
                throw new InvalidOperationException("Matrix is singular; it doesn't have an inverse.");

            double[,] inverseMatrix = new double[rows, columns];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    double algebraicCofactor = GetAlgebraicCofactor(matrixCourse, i, j);
                    inverseMatrix[j, i] = (int)(algebraicCofactor / determinant * 100000) / 100000.0; // Два знака после запятой
                }
            }

            return new MatrixCourse(inverseMatrix);
        }
        #endregion
    }
}
