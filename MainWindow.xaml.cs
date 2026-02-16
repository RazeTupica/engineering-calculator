using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace EngineeringCalculator
{
    public partial class MainWindow : Window
    {
        private readonly ExpressionParser _parser = new();
        private bool _isNewCalculation = true;
        private string _currentExpression = "";
        private string _history = "";

        // Поля для графика
        private double _currentXMin = -10;
        private double _currentXMax = 10;
        private double _currentYMin = -10;
        private double _currentYMax = 10;
        private string _currentFunction = "sin(x)";
        private Point _lastMousePosition;
        private bool _isDragging = false;
        private double[] _lastXValues;
        private double[] _lastYValues;

        public MainWindow()
        {
            InitializeComponent();
            DisplayTextBlock.Text = "0";
            SciDisplayTextBlock.Text = "0";

            // Инициализация истории
            UpdateHistoryListBox();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
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

        private void UpdateDisplay(string text)
        {
            DisplayTextBlock.Text = text;
            SciDisplayTextBlock.Text = text;

            if (text.Length > 15)
            {
                DisplayTextBlock.FontSize = 36;
                SciDisplayTextBlock.FontSize = 26;
            }
            else if (text.Length > 10)
            {
                DisplayTextBlock.FontSize = 42;
                SciDisplayTextBlock.FontSize = 30;
            }
            else
            {
                DisplayTextBlock.FontSize = 48;
                SciDisplayTextBlock.FontSize = 36;
            }
        }

        private void UpdateHistory(string expression, string result)
        {
            if (result.Contains("Ошибка"))
            {
                _history = $"{expression} = {result}\n{_history}";
            }
            else
            {
                _history = $"{expression} = {result}\n{_history}";
            }

            var historyLines = _history.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (historyLines.Length > 30)
            {
                _history = string.Join("\n", historyLines.Take(30));
            }

            UpdateHistoryListBox();
        }

        private void UpdateHistoryListBox()
        {
            var historyItems = _history.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            HistoryListBox.ItemsSource = null;
            HistoryListBox.ItemsSource = historyItems;

            if (HistoryCountText != null)
            {
                HistoryCountText.Text = historyItems.Length.ToString();
            }
        }

        private void ClearHistory()
        {
            _history = "";
            UpdateHistoryListBox();
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
                    "mod" => "%",
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
                    "x³" => "^3",
                    "√x" => "sqrt(",
                    "∛x" => "cuberoot(",
                    "1/x" => "1/(",
                    "|x|" => "abs(",
                    "n!" => "!",
                    "exp" => "exp(",
                    "2ˣ" => "2^(",
                    "10ˣ" => "10^(",
                    "ln" => "ln(",
                    "log" => "log(",
                    "sin" => "sin(",
                    "cos" => "cos(",
                    "tan" => "tan(",
                    "cot" => "cot(",
                    "sec" => "sec(",
                    "csc" => "csc(",
                    "asin" => "asin(",
                    "acos" => "acos(",
                    "atan" => "atan(",
                    "acot" => "acot(",
                    "asec" => "asec(",
                    "acsc" => "acsc(",
                    "sinh" => "sinh(",
                    "cosh" => "cosh(",
                    "tanh" => "tanh(",
                    "coth" => "coth(",
                    "sech" => "sech(",
                    "csch" => "csch(",
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
        }

        private void ClearHistoryButton_Click(object sender, RoutedEventArgs e)
        {
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
                    .Replace("÷", "/")
                    .Replace(",", ".");

                double result = _parser.Evaluate(expressionToEvaluate, degreesMode);

                string resultText = FormatResult(result);

                UpdateHistory(_currentExpression, resultText);
                UpdateDisplay(resultText);

                _currentExpression = resultText;
                _isNewCalculation = true;
            }
            catch (Exception ex)
            {
                string errorMessage = GetUserFriendlyErrorMessage(ex.Message);
                UpdateHistory(_currentExpression, $"Ошибка: {errorMessage}");
                UpdateDisplay(_currentExpression);
                _isNewCalculation = false;
            }
        }

        private string GetUserFriendlyErrorMessage(string technicalMessage)
        {
            if (technicalMessage.Contains("деление на ноль", StringComparison.OrdinalIgnoreCase) ||
                technicalMessage.Contains("divide by zero", StringComparison.OrdinalIgnoreCase))
            {
                return "Деление на ноль невозможно";
            }
            else if (technicalMessage.Contains("корень", StringComparison.OrdinalIgnoreCase) &&
                     technicalMessage.Contains("отрицательного", StringComparison.OrdinalIgnoreCase))
            {
                return "Корень из отрицательного числа не существует";
            }
            else if (technicalMessage.Contains("логарифм", StringComparison.OrdinalIgnoreCase) &&
                     (technicalMessage.Contains("положительных", StringComparison.OrdinalIgnoreCase) ||
                      technicalMessage.Contains("positive", StringComparison.OrdinalIgnoreCase)))
            {
                return "Логарифм определен только для положительных чисел";
            }
            else if (technicalMessage.Contains("факториал", StringComparison.OrdinalIgnoreCase) &&
                     technicalMessage.Contains("целых неотрицательных", StringComparison.OrdinalIgnoreCase))
            {
                return "Факториал определен только для целых неотрицательных чисел";
            }
            else if (technicalMessage.Contains("арксинус", StringComparison.OrdinalIgnoreCase) ||
                     technicalMessage.Contains("арккосинус", StringComparison.OrdinalIgnoreCase) ||
                     technicalMessage.Contains("asin", StringComparison.OrdinalIgnoreCase) ||
                     technicalMessage.Contains("acos", StringComparison.OrdinalIgnoreCase))
            {
                return "Арксинус и арккосинус определены для значений от -1 до 1";
            }
            else if (technicalMessage.Contains("скобк", StringComparison.OrdinalIgnoreCase) ||
                     technicalMessage.Contains("bracket", StringComparison.OrdinalIgnoreCase))
            {
                return "Несбалансированные скобки в выражении";
            }
            else if (technicalMessage.Contains("символ", StringComparison.OrdinalIgnoreCase) ||
                     technicalMessage.Contains("char", StringComparison.OrdinalIgnoreCase))
            {
                return "Выражение содержит недопустимые символы";
            }
            else if (technicalMessage.Contains("операнд", StringComparison.OrdinalIgnoreCase) ||
                     technicalMessage.Contains("operand", StringComparison.OrdinalIgnoreCase))
            {
                return "Недостаточно операндов для выполнения операции";
            }
            else if (technicalMessage.Contains("тангенс не определен", StringComparison.OrdinalIgnoreCase) ||
                     technicalMessage.Contains("tan", StringComparison.OrdinalIgnoreCase))
            {
                return "Тангенс не определен для данного угла";
            }
            else if (technicalMessage.Contains("котангенс не определен", StringComparison.OrdinalIgnoreCase) ||
                     technicalMessage.Contains("cot", StringComparison.OrdinalIgnoreCase))
            {
                return "Котангенс не определен для данного угла";
            }

            return "Некорректное выражение";
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

        // ========== МЕТОДЫ ДЛЯ ГРАФИКА ==========

        private void PlotButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _currentFunction = FunctionTextBox.Text;

                _currentFunction = _currentFunction.Replace("tg", "tan")
                                                  .Replace("ctg", "cot")
                                                  .Replace("arcsin", "asin")
                                                  .Replace("arccos", "acos")
                                                  .Replace("arctg", "atan")
                                                  .Replace("arcctg", "acot");

                _currentXMin = double.Parse(XMinTextBox.Text, CultureInfo.InvariantCulture);
                _currentXMax = double.Parse(XMaxTextBox.Text, CultureInfo.InvariantCulture);

                if (_currentXMin >= _currentXMax)
                    throw new ArgumentException("X min должен быть меньше X max");

                UpdateGraph();
                UpdateGraphInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка построения графика: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateGraph()
        {
            var (xValues, yValues) = GraphPlotter.Plot(_currentFunction, _currentXMin, _currentXMax, 500);
            _lastXValues = xValues;
            _lastYValues = yValues;

            var validYValues = yValues.Where(y => !double.IsNaN(y) && !double.IsInfinity(y)).ToArray();
            if (validYValues.Length > 0)
            {
                _currentYMin = validYValues.Min();
                _currentYMax = validYValues.Max();

                double yRange = _currentYMax - _currentYMin;
                if (yRange > 0)
                {
                    _currentYMin -= yRange * 0.1;
                    _currentYMax += yRange * 0.1;
                }
                else
                {
                    _currentYMin -= 1;
                    _currentYMax += 1;
                }
            }

            DrawGraph(xValues, yValues, _currentXMin, _currentXMax, _currentYMin, _currentYMax);
        }

        private void UpdateGraphInfo()
        {
            if (GraphInfoText != null)
            {
                GraphInfoText.Text = $"X:[{_currentXMin:F1}, {_currentXMax:F1}]  Y:[{_currentYMin:F1}, {_currentYMax:F1}]";
            }
        }

        private void ResetGraphView_Click(object sender, RoutedEventArgs e)
        {
            _currentXMin = -10;
            _currentXMax = 10;
            XMinTextBox.Text = "-10";
            XMaxTextBox.Text = "10";
            PlotButton_Click(sender, e);
        }

        private void ZoomInGraph_Click(object sender, RoutedEventArgs e)
        {
            double centerX = (_currentXMin + _currentXMax) / 2;
            double centerY = (_currentYMin + _currentYMax) / 2;

            double rangeX = (_currentXMax - _currentXMin) * 0.8;
            double rangeY = (_currentYMax - _currentYMin) * 0.8;

            _currentXMin = centerX - rangeX / 2;
            _currentXMax = centerX + rangeX / 2;
            _currentYMin = centerY - rangeY / 2;
            _currentYMax = centerY + rangeY / 2;

            XMinTextBox.Text = _currentXMin.ToString("F2", CultureInfo.InvariantCulture);
            XMaxTextBox.Text = _currentXMax.ToString("F2", CultureInfo.InvariantCulture);

            if (_lastXValues != null && _lastYValues != null)
            {
                DrawGraph(_lastXValues, _lastYValues, _currentXMin, _currentXMax, _currentYMin, _currentYMax);
            }
            UpdateGraphInfo();
        }

        private void ZoomOutGraph_Click(object sender, RoutedEventArgs e)
        {
            double centerX = (_currentXMin + _currentXMax) / 2;
            double centerY = (_currentYMin + _currentYMax) / 2;

            double rangeX = (_currentXMax - _currentXMin) * 1.25;
            double rangeY = (_currentYMax - _currentYMin) * 1.25;

            _currentXMin = centerX - rangeX / 2;
            _currentXMax = centerX + rangeX / 2;
            _currentYMin = centerY - rangeY / 2;
            _currentYMax = centerY + rangeY / 2;

            XMinTextBox.Text = _currentXMin.ToString("F2", CultureInfo.InvariantCulture);
            XMaxTextBox.Text = _currentXMax.ToString("F2", CultureInfo.InvariantCulture);

            if (_lastXValues != null && _lastYValues != null)
            {
                DrawGraph(_lastXValues, _lastYValues, _currentXMin, _currentXMax, _currentYMin, _currentYMax);
            }
            UpdateGraphInfo();
        }

        private void GraphCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
            _lastMousePosition = e.GetPosition(GraphCanvas);
            GraphCanvas.CaptureMouse();
            GraphCanvas.Cursor = Cursors.Hand;
        }

        private void GraphCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            GraphCanvas.ReleaseMouseCapture();
            GraphCanvas.Cursor = Cursors.Arrow;
        }

        private void GraphCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && GraphCanvas.Children.Count > 0 && _lastXValues != null && _lastYValues != null)
            {
                Point currentPosition = e.GetPosition(GraphCanvas);
                double deltaX = currentPosition.X - _lastMousePosition.X;
                double deltaY = currentPosition.Y - _lastMousePosition.Y;

                double rangeX = _currentXMax - _currentXMin;
                double rangeY = _currentYMax - _currentYMin;

                double deltaValueX = -deltaX * rangeX / GraphCanvas.ActualWidth;
                double deltaValueY = deltaY * rangeY / GraphCanvas.ActualHeight;

                _currentXMin += deltaValueX;
                _currentXMax += deltaValueX;
                _currentYMin += deltaValueY;
                _currentYMax += deltaValueY;

                XMinTextBox.Text = _currentXMin.ToString("F2", CultureInfo.InvariantCulture);
                XMaxTextBox.Text = _currentXMax.ToString("F2", CultureInfo.InvariantCulture);

                DrawGraph(_lastXValues, _lastYValues, _currentXMin, _currentXMax, _currentYMin, _currentYMax);
                UpdateGraphInfo();

                _lastMousePosition = currentPosition;
            }
        }

        private void GraphCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_lastXValues == null || _lastYValues == null) return;

            Point mousePos = e.GetPosition(GraphCanvas);
            double mouseXValue = Map(mousePos.X, 0, GraphCanvas.ActualWidth, _currentXMin, _currentXMax);
            double mouseYValue = Map(mousePos.Y, 0, GraphCanvas.ActualHeight, _currentYMax, _currentYMin);

            double zoomFactor = e.Delta > 0 ? 0.85 : 1.15;

            double leftDist = mouseXValue - _currentXMin;
            double rightDist = _currentXMax - mouseXValue;
            _currentXMin = mouseXValue - leftDist * zoomFactor;
            _currentXMax = mouseXValue + rightDist * zoomFactor;

            double topDist = mouseYValue - _currentYMin;
            double bottomDist = _currentYMax - mouseYValue;
            _currentYMin = mouseYValue - topDist * zoomFactor;
            _currentYMax = mouseYValue + bottomDist * zoomFactor;

            XMinTextBox.Text = _currentXMin.ToString("F2", CultureInfo.InvariantCulture);
            XMaxTextBox.Text = _currentXMax.ToString("F2", CultureInfo.InvariantCulture);

            DrawGraph(_lastXValues, _lastYValues, _currentXMin, _currentXMax, _currentYMin, _currentYMax);
            UpdateGraphInfo();

            e.Handled = true;
        }

        private void DrawGraph(double[] xValues, double[] yValues, double xMin, double xMax, double yMin, double yMax)
        {
            GraphCanvas.Children.Clear();
            GraphPlaceholder.Visibility = Visibility.Collapsed;

            double canvasWidth = GraphCanvas.ActualWidth;
            double canvasHeight = GraphCanvas.ActualHeight;

            if (canvasWidth <= 0 || canvasHeight <= 0)
            {
                canvasWidth = 350;
                canvasHeight = 250;
                GraphCanvas.Width = canvasWidth;
                GraphCanvas.Height = canvasHeight;
            }

            var validYValues = yValues.Where(y => !double.IsNaN(y) && !double.IsInfinity(y)).ToArray();
            if (validYValues.Length == 0)
            {
                ShowGraphMessage("Не удалось построить график для указанной функции");
                return;
            }

            // Отрисовка сетки
            DrawGrid(canvasWidth, canvasHeight, xMin, xMax, yMin, yMax);

            // Отрисовка осей координат с подписями
            DrawCoordinateSystem(canvasWidth, canvasHeight, xMin, xMax, yMin, yMax);

            // Отрисовка функции
            DrawFunction(xValues, yValues, canvasWidth, canvasHeight, xMin, xMax, yMin, yMax);

            // Добавление точек на график
            AddSamplePoints(xValues, yValues, canvasWidth, canvasHeight, xMin, xMax, yMin, yMax);
        }

        private void ShowGraphMessage(string message)
        {
            TextBlock messageBlock = new TextBlock
            {
                Text = message,
                Foreground = Brushes.Gray,
                FontSize = 12,
                TextAlignment = TextAlignment.Center
            };

            GraphCanvas.Children.Add(messageBlock);
            Canvas.SetLeft(messageBlock, (GraphCanvas.ActualWidth - 180) / 2);
            Canvas.SetTop(messageBlock, GraphCanvas.ActualHeight / 2);
        }

        private void DrawCoordinateSystem(double width, double height, double xMin, double xMax, double yMin, double yMax)
        {
            // Ось X (горизонтальная)
            double yZero = Map(0, yMin, yMax, height, 0);
            if (yZero >= 0 && yZero <= height)
            {
                Line xAxis = new Line
                {
                    X1 = 0,
                    Y1 = yZero,
                    X2 = width,
                    Y2 = yZero,
                    Stroke = new SolidColorBrush(Color.FromRgb(100, 150, 200)),
                    StrokeThickness = 2
                };
                GraphCanvas.Children.Add(xAxis);

                // Стрелка на оси X (справа)
                Polygon xArrow = new Polygon
                {
                    Points = new PointCollection
                    {
                        new Point(width - 10, yZero - 5),
                        new Point(width, yZero),
                        new Point(width - 10, yZero + 5)
                    },
                    Fill = new SolidColorBrush(Color.FromRgb(100, 150, 200))
                };
                GraphCanvas.Children.Add(xArrow);

                // Подпись X
                TextBlock xAxisLabel = new TextBlock
                {
                    Text = "X",
                    Foreground = new SolidColorBrush(Color.FromRgb(100, 150, 200)),
                    FontSize = 12,
                    FontWeight = FontWeights.Bold
                };
                Canvas.SetLeft(xAxisLabel, width - 20);
                Canvas.SetTop(xAxisLabel, yZero - 25);
                GraphCanvas.Children.Add(xAxisLabel);
            }

            // Ось Y (вертикальная)
            double xZero = Map(0, xMin, xMax, 0, width);
            if (xZero >= 0 && xZero <= width)
            {
                Line yAxis = new Line
                {
                    X1 = xZero,
                    Y1 = 0,
                    X2 = xZero,
                    Y2 = height,
                    Stroke = new SolidColorBrush(Color.FromRgb(100, 150, 200)),
                    StrokeThickness = 2
                };
                GraphCanvas.Children.Add(yAxis);

                // Стрелка на оси Y (сверху)
                Polygon yArrow = new Polygon
                {
                    Points = new PointCollection
                    {
                        new Point(xZero - 5, 10),
                        new Point(xZero, 0),
                        new Point(xZero + 5, 10)
                    },
                    Fill = new SolidColorBrush(Color.FromRgb(100, 150, 200))
                };
                GraphCanvas.Children.Add(yArrow);

                // Подпись Y
                TextBlock yAxisLabel = new TextBlock
                {
                    Text = "Y",
                    Foreground = new SolidColorBrush(Color.FromRgb(100, 150, 200)),
                    FontSize = 12,
                    FontWeight = FontWeights.Bold
                };
                Canvas.SetLeft(yAxisLabel, xZero + 10);
                Canvas.SetTop(yAxisLabel, 5);
                GraphCanvas.Children.Add(yAxisLabel);
            }

            // Подпись начала координат (0)
            if (xZero >= 0 && xZero <= width && yZero >= 0 && yZero <= height)
            {
                Ellipse origin = new Ellipse
                {
                    Width = 6,
                    Height = 6,
                    Fill = new SolidColorBrush(Color.FromRgb(255, 200, 0)),
                    Stroke = new SolidColorBrush(Colors.White),
                    StrokeThickness = 1
                };
                Canvas.SetLeft(origin, xZero - 3);
                Canvas.SetTop(origin, yZero - 3);
                GraphCanvas.Children.Add(origin);

                TextBlock originLabel = new TextBlock
                {
                    Text = "0",
                    Foreground = new SolidColorBrush(Color.FromRgb(255, 200, 0)),
                    FontSize = 10,
                    FontWeight = FontWeights.Bold
                };
                Canvas.SetLeft(originLabel, xZero + 8);
                Canvas.SetTop(originLabel, yZero - 15);
                GraphCanvas.Children.Add(originLabel);
            }
        }

        private void DrawGrid(double width, double height, double xMin, double xMax, double yMin, double yMax)
        {
            int xLines = 10;
            int yLines = 8;

            // Вертикальные линии сетки
            for (int i = 0; i <= xLines; i++)
            {
                double xValue = xMin + (xMax - xMin) * i / xLines;
                double x = Map(xValue, xMin, xMax, 0, width);

                Line gridLine = new Line
                {
                    X1 = x,
                    Y1 = 0,
                    X2 = x,
                    Y2 = height,
                    Stroke = new SolidColorBrush(Color.FromArgb(30, 255, 255, 255)),
                    StrokeThickness = 0.5
                };
                GraphCanvas.Children.Add(gridLine);

                // Подпись значения X
                if (i % 2 == 0 || i == xLines)
                {
                    TextBlock xLabel = new TextBlock
                    {
                        Text = Math.Round(xValue, 2).ToString(CultureInfo.InvariantCulture),
                        Foreground = new SolidColorBrush(Color.FromArgb(150, 255, 255, 255)),
                        FontSize = 8,
                        Background = new SolidColorBrush(Color.FromArgb(100, 21, 27, 30))
                    };

                    Canvas.SetLeft(xLabel, x - 12);
                    Canvas.SetTop(xLabel, height - 18);
                    GraphCanvas.Children.Add(xLabel);
                }
            }

            // Горизонтальные линии сетки
            for (int i = 0; i <= yLines; i++)
            {
                double yValue = yMin + (yMax - yMin) * i / yLines;
                double y = Map(yValue, yMin, yMax, height, 0);

                Line gridLine = new Line
                {
                    X1 = 0,
                    Y1 = y,
                    X2 = width,
                    Y2 = y,
                    Stroke = new SolidColorBrush(Color.FromArgb(30, 255, 255, 255)),
                    StrokeThickness = 0.5
                };
                GraphCanvas.Children.Add(gridLine);

                // Подпись значения Y
                if (i % 2 == 0 || i == yLines)
                {
                    TextBlock yLabel = new TextBlock
                    {
                        Text = Math.Round(yValue, 2).ToString(CultureInfo.InvariantCulture),
                        Foreground = new SolidColorBrush(Color.FromArgb(150, 255, 255, 255)),
                        FontSize = 8,
                        Background = new SolidColorBrush(Color.FromArgb(100, 21, 27, 30))
                    };

                    Canvas.SetLeft(yLabel, 5);
                    Canvas.SetTop(yLabel, y - 10);
                    GraphCanvas.Children.Add(yLabel);
                }
            }
        }

        private void DrawFunction(double[] xValues, double[] yValues, double width, double height,
                         double xMin, double xMax, double yMin, double yMax)
        {
            Polyline polyline = new Polyline
            {
                Stroke = new SolidColorBrush(Color.FromRgb(0, 200, 255)),
                StrokeThickness = 2.5,
                StrokeLineJoin = PenLineJoin.Round,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round
            };

            PointCollection points = new PointCollection();

            for (int i = 0; i < xValues.Length; i++)
            {
                if (!double.IsNaN(yValues[i]) && !double.IsInfinity(yValues[i]))
                {
                    double x = Map(xValues[i], xMin, xMax, 0, width);
                    double y = Map(yValues[i], yMin, yMax, height, 0);

                    // Проверка на резкие скачки (для функций с разрывами)
                    if (points.Count > 0)
                    {
                        Point lastPoint = points.Last();
                        if (Math.Abs(y - lastPoint.Y) > height * 0.3)
                        {
                            // Рисуем накопленные точки и начинаем новый сегмент
                            if (points.Count > 1)
                            {
                                polyline.Points = new PointCollection(points);
                                GraphCanvas.Children.Add(polyline);
                                polyline = new Polyline
                                {
                                    Stroke = new SolidColorBrush(Color.FromRgb(0, 200, 255)),
                                    StrokeThickness = 2.5
                                };
                            }
                            points.Clear();
                        }
                    }

                    points.Add(new Point(x, y));
                }
                else
                {
                    // Разрыв функции - рисуем накопленные точки
                    if (points.Count > 1)
                    {
                        polyline.Points = new PointCollection(points);
                        GraphCanvas.Children.Add(polyline);
                        polyline = new Polyline
                        {
                            Stroke = new SolidColorBrush(Color.FromRgb(0, 200, 255)),
                            StrokeThickness = 2.5
                        };
                    }
                    points.Clear();
                }
            }

            // Добавляем последний сегмент
            if (points.Count > 1)
            {
                polyline.Points = new PointCollection(points);
                GraphCanvas.Children.Add(polyline);
            }
        }

        private void AddSamplePoints(double[] xValues, double[] yValues, double width, double height,
                                    double xMin, double xMax, double yMin, double yMax)
        {
            int step = Math.Max(1, xValues.Length / 20); // Берем примерно 20 точек

            for (int i = 0; i < xValues.Length; i += step)
            {
                if (!double.IsNaN(yValues[i]) && !double.IsInfinity(yValues[i]))
                {
                    double x = Map(xValues[i], xMin, xMax, 0, width);
                    double y = Map(yValues[i], yMin, yMax, height, 0);

                    // Проверяем, что точка в пределах видимой области
                    if (x >= 0 && x <= width && y >= 0 && y <= height)
                    {
                        Ellipse point = new Ellipse
                        {
                            Width = 4,
                            Height = 4,
                            Fill = new SolidColorBrush(Color.FromRgb(255, 100, 100)),
                            Stroke = new SolidColorBrush(Colors.White),
                            StrokeThickness = 1,
                            ToolTip = $"x = {Math.Round(xValues[i], 3)}, y = {Math.Round(yValues[i], 3)}"
                        };

                        Canvas.SetLeft(point, x - 2);
                        Canvas.SetTop(point, y - 2);
                        GraphCanvas.Children.Add(point);
                    }
                }
            }
        }

        private double Map(double value, double fromMin, double fromMax, double toMin, double toMax)
        {
            return (value - fromMin) * (toMax - toMin) / (fromMax - fromMin) + toMin;
        }

        // Метод для закрытия всплывающих меню
        private void ClosePopupButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string menuName)
            {
                switch (menuName)
                {
                    case "Trigonometry":
                        TrigonometryMenuButton.IsChecked = false;
                        break;
                    case "Functions":
                        FunctionsMenuButton.IsChecked = false;
                        break;
                    case "Constants":
                        ConstantsMenuButton.IsChecked = false;
                        break;
                }
            }
        }

        // Методы для панели истории
        private void HistoryItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem item && item.Content is string historyItem)
            {
                string[] parts = historyItem.Split('=');
                if (parts.Length > 0)
                {
                    string expression = parts[0].Trim();
                    _currentExpression = expression;
                    UpdateDisplay(_currentExpression);
                    _isNewCalculation = false;
                }
            }
        }

        private void CopyResult_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string historyItem)
            {
                string[] parts = historyItem.Split('=');
                if (parts.Length > 1)
                {
                    string result = parts[1].Trim();
                    Clipboard.SetText(result);

                    // Визуальное подтверждение
                    var tooltip = new ToolTip { Content = "Скопировано!", IsOpen = true };
                    button.ToolTip = tooltip;
                    var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
                    timer.Tick += (s, args) => { tooltip.IsOpen = false; timer.Stop(); };
                    timer.Start();
                }
            }
        }
    }
}