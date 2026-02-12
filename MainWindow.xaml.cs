using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace EngineeringCalculator
{
    public partial class MainWindow : Window
    {
        private readonly ExpressionParser _parser = new();
        private bool _isNewCalculation = true;
        private string _currentExpression = "";
        private string _history = "";

        public MainWindow()
        {
            InitializeComponent();
            DisplayTextBlock.Text = "0";
            SciDisplayTextBlock.Text = "0";

            // Инициализация кнопки максимизации
            UpdateMaximizeButton();

            // Подписываемся на изменение состояния окна
            StateChanged += MainWindow_StateChanged;
        }
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (e.ClickCount == 2)
                {
                    AdjustWindowSize();
                }
                else
                {
                    DragMove();
                }
            }
        }

        private void AdjustWindowSize()
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Maximized;
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            UpdateMaximizeButton();
        }

        // Методы для кастомного заголовка
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ToggleMaximize();
            }
            else
            {
                DragMove();
            }
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleMaximize();
        }

        private void ToggleMaximize()
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Maximized;
            }
        }

        private void UpdateMaximizeButton()
        {

        }

        // Остальные методы калькулятора...
        private void UpdateDisplay(string text)
        {
            DisplayTextBlock.Text = text;
            SciDisplayTextBlock.Text = text;

            if (text.Length > 15)
            {
                DisplayTextBlock.FontSize = 42;
                SciDisplayTextBlock.FontSize = 30;
            }
            else if (text.Length > 10)
            {
                DisplayTextBlock.FontSize = 48;
                SciDisplayTextBlock.FontSize = 36;
            }
            else
            {
                DisplayTextBlock.FontSize = 64;
                SciDisplayTextBlock.FontSize = 48;
            }
        }

        private void UpdateHistory(string expression, string result)
        {
            _history = $"{expression} = {result}\n{_history}";
            HistoryTextBlock.Text = _history.Length > 100 ? _history.Substring(0, 100) + "..." : _history;
            SciHistoryTextBlock.Text = _history.Length > 100 ? _history.Substring(0, 100) + "..." : _history;
        }

        private void ClearHistory()
        {
            _history = "";
            HistoryTextBlock.Text = "";
            SciHistoryTextBlock.Text = "";
        }

        private void NumberButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string number = button.Content.ToString();

                if (_isNewCalculation)
                {
                    _currentExpression = number;
                    UpdateDisplay(number);
                    _isNewCalculation = false;
                }
                else
                {
                    _currentExpression += number;
                    UpdateDisplay(_currentExpression);
                }
            }
        }

        private void AppendButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string operation = button.Content.ToString();

                string parsedOperation = operation switch
                {
                    "×" => "*",
                    "÷" => "/",
                    "x^y" => "^",
                    _ => operation
                };

                if (_isNewCalculation)
                {
                    if (!string.IsNullOrEmpty(_currentExpression) && _currentExpression != "0")
                    {
                        _currentExpression += parsedOperation;
                    }
                    else
                    {
                        _currentExpression = "0" + parsedOperation;
                    }
                }
                else
                {
                    _currentExpression += parsedOperation;
                }

                UpdateDisplay(_currentExpression);
                _isNewCalculation = false;
            }
        }

        private void FunctionButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string function = button.Content.ToString();
                string parsedFunction = function switch
                {
                    "x²" => "^2",
                    "√x" => "sqrt(",
                    "1/x" => "1/(",
                    "|x|" => "abs(",
                    "sin⁻¹" => "asin(",
                    "cos⁻¹" => "acos(",
                    "n!" => "!",
                    "e^x" => "exp(",
                    "10^x" => "10^(",
                    "ln" => "ln(",
                    "log" => "log(",
                    "sin" => "sin(",
                    "cos" => "cos(",
                    "tan" => "tan(",
                    _ => function + "("
                };

                if (_isNewCalculation || string.IsNullOrEmpty(_currentExpression) || _currentExpression == "0")
                {
                    _currentExpression = parsedFunction;
                }
                else
                {
                    _currentExpression += parsedFunction;
                }

                UpdateDisplay(_currentExpression);
                _isNewCalculation = false;
            }
        }

        private void ConstantButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string constant = button.Content.ToString();

                if (_isNewCalculation || string.IsNullOrEmpty(_currentExpression) || _currentExpression == "0")
                {
                    _currentExpression = constant;
                }
                else
                {
                    _currentExpression += constant;
                }

                UpdateDisplay(_currentExpression);
                _isNewCalculation = false;
            }
        }

        private void DecimalButton_Click(object sender, RoutedEventArgs e)
        {
            if (CanAddDecimal())
            {
                _currentExpression += CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
                UpdateDisplay(_currentExpression);
                _isNewCalculation = false;
            }
        }

        private bool CanAddDecimal()
        {
            if (string.IsNullOrEmpty(_currentExpression))
                return true;

            for (int i = _currentExpression.Length - 1; i >= 0; i--)
            {
                char c = _currentExpression[i];
                if (!char.IsDigit(c) && c != '.' && c != ',')
                {
                    return true;
                }
                if (c == '.' || c == ',')
                {
                    return false;
                }
            }
            return true;
        }

        private void NegateButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_currentExpression) && _currentExpression != "0")
            {
                if (_currentExpression.StartsWith("-"))
                {
                    _currentExpression = _currentExpression.Substring(1);
                }
                else
                {
                    _currentExpression = "-" + _currentExpression;
                }
                UpdateDisplay(_currentExpression);
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            _currentExpression = "";
            UpdateDisplay("0");
            _isNewCalculation = true;
            ClearHistory();
        }

        private void BackspaceButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_currentExpression) && _currentExpression.Length > 0)
            {
                _currentExpression = _currentExpression.Substring(0, _currentExpression.Length - 1);
                UpdateDisplay(string.IsNullOrEmpty(_currentExpression) ? "0" : _currentExpression);
            }
            else
            {
                UpdateDisplay("0");
                _isNewCalculation = true;
            }
        }

        private void EqualsButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentExpression) || _currentExpression == "0")
            {
                UpdateDisplay("0");
                return;
            }

            try
            {
                bool degreesMode = DegreesToggle?.IsChecked ?? true;
                string expressionToEvaluate = _currentExpression;

                expressionToEvaluate = expressionToEvaluate
                    .Replace("×", "*")
                    .Replace("÷", "/");

                double result = _parser.Evaluate(expressionToEvaluate, degreesMode);

                string resultText = FormatResult(result);

                UpdateHistory(_currentExpression, resultText);
                UpdateDisplay(resultText);

                _currentExpression = resultText;
                _isNewCalculation = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка вычисления",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                _currentExpression = "";
                UpdateDisplay("0");
                _isNewCalculation = true;
            }
        }

        private string FormatResult(double result)
        {
            if (double.IsInfinity(result) || double.IsNaN(result))
                return "Ошибка";

            if (Math.Abs(result) < 1e-10 && result != 0)
                return "0";

            string format = Math.Abs(result) >= 1e10 || (Math.Abs(result) < 1e-3 && result != 0)
                ? "E6"
                : "G10";

            return result.ToString(format, CultureInfo.CurrentCulture);
        }

        private void PlotButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string function = FunctionTextBox.Text;
                double xMin = double.Parse(XMinTextBox.Text, CultureInfo.InvariantCulture);
                double xMax = double.Parse(XMaxTextBox.Text, CultureInfo.InvariantCulture);

                if (xMin >= xMax)
                    throw new ArgumentException("X min должен быть меньше X max");

                var (xValues, yValues) = GraphPlotter.Plot(function, xMin, xMax, 200);
                DrawGraph(xValues, yValues, xMin, xMax);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка построения графика: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DrawGraph(double[] xValues, double[] yValues, double xMin, double xMax)
        {
            GraphCanvas.Children.Clear();

            GraphPlaceholder.Visibility = Visibility.Collapsed;

            double canvasWidth = GraphCanvas.ActualWidth;
            double canvasHeight = GraphCanvas.ActualHeight;

            if (canvasWidth <= 0 || canvasHeight <= 0)
            {
                canvasWidth = 400;
                canvasHeight = 300;
            }

            var validYValues = yValues.Where(y => !double.IsNaN(y) && !double.IsInfinity(y)).ToArray();
            if (validYValues.Length == 0)
            {
                ShowGraphMessage("Не удалось построить график для указанной функции");
                return;
            }

            double yMin = validYValues.Min();
            double yMax = validYValues.Max();

            double yRange = yMax - yMin;
            if (yRange > 0)
            {
                yMin -= yRange * 0.1;
                yMax += yRange * 0.1;
            }
            else
            {
                yMin -= 1;
                yMax += 1;
            }

            DrawCoordinateSystem(canvasWidth, canvasHeight, xMin, xMax, yMin, yMax);
            DrawFunction(xValues, yValues, canvasWidth, canvasHeight, xMin, xMax, yMin, yMax);
        }

        private void ShowGraphMessage(string message)
        {
            TextBlock messageBlock = new TextBlock
            {
                Text = message,
                Foreground = Brushes.Gray,
                FontSize = 14,
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            GraphCanvas.Children.Add(messageBlock);
            Canvas.SetLeft(messageBlock, (GraphCanvas.ActualWidth - 200) / 2);
            Canvas.SetTop(messageBlock, GraphCanvas.ActualHeight / 2);
        }

        private void DrawCoordinateSystem(double width, double height, double xMin, double xMax, double yMin, double yMax)
        {
            double yZero = Map(0, yMin, yMax, height, 0);
            if (yZero >= 0 && yZero <= height)
            {
                Line xAxis = new Line
                {
                    X1 = 0,
                    Y1 = yZero,
                    X2 = width,
                    Y2 = yZero,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };
                GraphCanvas.Children.Add(xAxis);
            }

            double xZero = Map(0, xMin, xMax, 0, width);
            if (xZero >= 0 && xZero <= width)
            {
                Line yAxis = new Line
                {
                    X1 = xZero,
                    Y1 = 0,
                    X2 = xZero,
                    Y2 = height,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };
                GraphCanvas.Children.Add(yAxis);
            }
        }

        private void DrawGrid(double width, double height, double xMin, double xMax, double yMin, double yMax)
        {
            int xGridLines = 10;
            for (int i = 1; i < xGridLines; i++)
            {
                double xValue = xMin + (xMax - xMin) * i / xGridLines;
                double x = Map(xValue, xMin, xMax, 0, width);

                Line gridLine = new Line
                {
                    X1 = x,
                    Y1 = 0,
                    X2 = x,
                    Y2 = height,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 0.5,
                    StrokeDashArray = new DoubleCollection { 2, 2 }
                };
                GraphCanvas.Children.Add(gridLine);
            }

            int yGridLines = 10;
            for (int i = 1; i < yGridLines; i++)
            {
                double yValue = yMin + (yMax - yMin) * i / yGridLines;
                double y = Map(yValue, yMin, yMax, height, 0);

                Line gridLine = new Line
                {
                    X1 = 0,
                    Y1 = y,
                    X2 = width,
                    Y2 = y,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 0.5,
                    StrokeDashArray = new DoubleCollection { 2, 2 }
                };
                GraphCanvas.Children.Add(gridLine);
            }
        }

        private void DrawFunction(double[] xValues, double[] yValues, double width, double height,
                                double xMin, double xMax, double yMin, double yMax)
        {
            Polyline polyline = new Polyline
            {
                Stroke = new SolidColorBrush(Color.FromRgb(0, 122, 255)),
                StrokeThickness = 2,
                StrokeLineJoin = PenLineJoin.Round,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round
            };

            for (int i = 0; i < xValues.Length; i++)
            {
                if (!double.IsNaN(yValues[i]) && !double.IsInfinity(yValues[i]))
                {
                    double x = Map(xValues[i], xMin, xMax, 0, width);
                    double y = Map(yValues[i], yMin, yMax, height, 0);
                    polyline.Points.Add(new Point(x, y));
                }
            }

            GraphCanvas.Children.Add(polyline);
        }

        private double Map(double value, double fromMin, double fromMax, double toMin, double toMax)
        {
            return (value - fromMin) * (toMax - toMin) / (fromMax - fromMin) + toMin;
        }
    }
}