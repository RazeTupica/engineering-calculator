using System.Globalization;
using System.Text;

namespace EngineeringCalculator
{
    public class ExpressionParser
    {
        private static readonly Dictionary<string, int> OperatorPrecedence = new()
        {
            { "+", 1 }, { "-", 1 },
            { "~", 1 },
            { "*", 2 }, { "/", 2 },
            { "^", 3 },
            { "!", 4 },
            { "sin", 5 }, { "cos", 5 }, { "tan", 5 }, { "cot", 5 }, { "ctg", 5 },
            { "asin", 5 }, { "acos", 5 }, { "atan", 5 }, { "acot", 5 }, { "actg", 5 },
            { "sinh", 5 }, { "cosh", 5 }, { "tanh", 5 },
            { "ln", 5 }, { "log", 5 },
            { "sqrt", 5 }, { "abs", 5 }, { "exp", 5 }, { "10^", 5 }
        };

        public double Evaluate(string expression, bool degreesMode = true)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return 0;

            try
            {
                // Замена констант и специальных символов
                expression = expression.Replace("π", Math.PI.ToString(CultureInfo.InvariantCulture))
                                       .Replace("pi", Math.PI.ToString(CultureInfo.InvariantCulture))
                                       .Replace("e", Math.E.ToString(CultureInfo.InvariantCulture))
                                       .Replace("×", "*")
                                       .Replace("÷", "/")
                                       .Replace("x²", "^2")
                                       .Replace("√x", "sqrt")
                                       .Replace("|x|", "abs");

                var rpn = ConvertToRPN(expression);
                return EvaluateRPN(rpn, degreesMode);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Ошибка парсинга выражения: {ex.Message}", ex);
            }
        }

        private List<string> ConvertToRPN(string expression)
        {
            var output = new List<string>();
            var operators = new Stack<string>();
            var token = new StringBuilder();

            // Предварительная обработка выражения
            expression = expression.Trim();

            for (int i = 0; i < expression.Length; i++)
            {
                char c = expression[i];

                if (char.IsWhiteSpace(c))
                    continue;

                // 1. Обработка функций (sin, cos, tan, ln, log, sqrt, abs, asin, acos, exp)
                if (char.IsLetter(c))
                {
                    token.Clear();
                    while (i < expression.Length && (char.IsLetter(expression[i]) || expression[i] == '⁻' || expression[i] == '¹'))
                    {
                        token.Append(expression[i]);
                        i++;
                    }
                    i--; // Возвращаемся на один символ назад

                    string func = token.ToString();

                    // Обработка специальных функций
                    if (func.Contains("sin⁻¹")) func = "asin";
                    else if (func.Contains("cos⁻¹")) func = "acos";
                    else if (func.Contains("10^")) func = "10^";

                    operators.Push(func);
                    continue;
                }

                // 2. Обработка чисел (целые, десятичные)
                if (char.IsDigit(c) || c == '.' || c == ',')
                {
                    token.Clear();
                    bool hasDecimal = false;

                    while (i < expression.Length &&
                           (char.IsDigit(expression[i]) ||
                            expression[i] == '.' || expression[i] == ','))
                    {
                        if (expression[i] == '.' || expression[i] == ',')
                        {
                            if (hasDecimal) break;
                            hasDecimal = true;
                            token.Append(CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator);
                        }
                        else
                        {
                            token.Append(expression[i]);
                        }
                        i++;
                    }
                    i--; // Возвращаемся на один символ назад

                    output.Add(token.ToString());
                    continue;
                }

                // 3. Обработка операторов (+, -, *, /, ^)
                if (IsOperator(c.ToString()))
                {
                    string currentOperator = c.ToString();

                    // Обработка унарного минуса
                    if (currentOperator == "-")
                    {
                        if (i == 0 || expression[i - 1] == '(' || IsOperator(expression[i - 1].ToString()))
                        {
                            // Это унарный минус
                            output.Add("0");
                            operators.Push("~");
                            continue;
                        }
                    }

                    while (operators.Count > 0 &&
                           operators.Peek() != "(" &&
                           HasHigherPrecedence(operators.Peek(), currentOperator))
                    {
                        output.Add(operators.Pop());
                    }

                    operators.Push(currentOperator);
                }
                // 4. Обработка факториала
                else if (c == '!')
                {
                    operators.Push("!");
                }
                // 5. Открывающая скобка
                else if (c == '(')
                {
                    operators.Push("(");
                }
                // 6. Закрывающая скобка
                else if (c == ')')
                {
                    // Выталкиваем все операторы до открывающей скобки
                    while (operators.Count > 0 && operators.Peek() != "(")
                    {
                        output.Add(operators.Pop());
                    }

                    if (operators.Count == 0)
                        throw new ArgumentException("Несбалансированные скобки");

                    operators.Pop(); // Удаляем "("

                    // Если на вершине функция - добавляем в вывод
                    if (operators.Count > 0 && IsFunction(operators.Peek()))
                    {
                        output.Add(operators.Pop());
                    }
                }
                // 7. Обработка символа '^' для степени
                else if (c == '^')
                {
                    while (operators.Count > 0 &&
                           operators.Peek() != "(" &&
                           OperatorPrecedence.TryGetValue(operators.Peek(), out int prec1) &&
                           prec1 >= OperatorPrecedence["^"])
                    {
                        output.Add(operators.Pop());
                    }
                    operators.Push("^");
                }
                else
                {
                    throw new ArgumentException($"Неизвестный символ: {c}");
                }
            }

            // 8. Выталкиваем оставшиеся операторы
            while (operators.Count > 0)
            {
                var op = operators.Pop();
                if (op == "(" || op == ")")
                    throw new ArgumentException("Несбалансированные скобки");
                output.Add(op);
            }

            return output;
        }

        private double EvaluateRPN(List<string> rpn, bool degreesMode)
        {
            var stack = new Stack<double>();

            foreach (var token in rpn)
            {
                if (double.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out double number))
                {
                    stack.Push(number);
                }
                else if (token == "~") // Унарный минус
                {
                    if (stack.Count < 1)
                        throw new InvalidOperationException("Недостаточно операндов для унарного оператора.");

                    double a = stack.Pop();
                    stack.Push(-a);
                }
                else if (IsOperator(token) || token == "^")
                {
                    if (stack.Count < 2)
                        throw new InvalidOperationException($"Недостаточно операндов для оператора '{token}'.");

                    double b = stack.Pop();
                    double a = stack.Pop();
                    stack.Push(ApplyOperator(token, a, b));
                }
                else if (token == "!") // Факториал
                {
                    if (stack.Count < 1)
                        throw new InvalidOperationException("Недостаточно операндов для факториала.");

                    double a = stack.Pop();
                    stack.Push(CalculatorEngine.Factorial(a));
                }
                else // Функция
                {
                    if (stack.Count < 1)
                        throw new InvalidOperationException($"Недостаточно операндов для функции '{token}'.");

                    double a = stack.Pop();
                    stack.Push(ApplyFunction(token, a, degreesMode));
                }
            }

            return stack.Count == 1 ? stack.Pop() : 0;
        }

        private double ApplyOperator(string op, double a, double b)
        {
            return op switch
            {
                "+" => CalculatorEngine.Add(a, b),
                "-" => CalculatorEngine.Subtract(a, b),
                "*" => CalculatorEngine.Multiply(a, b),
                "/" => CalculatorEngine.Divide(a, b),
                "^" => CalculatorEngine.Power(a, b),
                _ => throw new ArgumentException($"Неизвестный оператор: {op}")
            };
        }

        private double ApplyFunction(string func, double a, bool degreesMode)
        {
            return func.ToLower() switch
            {
                "sin" => CalculatorEngine.Sin(a, degreesMode),
                "cos" => CalculatorEngine.Cos(a, degreesMode),
                "tan" => CalculatorEngine.Tan(a, degreesMode),
                "cot" or "ctg" => CalculatorEngine.Cot(a, degreesMode),
                "asin" => CalculatorEngine.Asin(a, degreesMode),
                "acos" => CalculatorEngine.Acos(a, degreesMode),
                "atan" => CalculatorEngine.Atan(a, degreesMode),
                "acot" or "actg" => CalculatorEngine.Acot(a, degreesMode),
                "sinh" => CalculatorEngine.Sinh(a),
                "cosh" => CalculatorEngine.Cosh(a),
                "tanh" => CalculatorEngine.Tanh(a),
                "ln" => CalculatorEngine.Ln(a),
                "log" => CalculatorEngine.Log10(a),
                "sqrt" => CalculatorEngine.Sqrt(a),
                "abs" => CalculatorEngine.Modulus(a),
                "exp" => CalculatorEngine.Exp(a),
                "10^" => CalculatorEngine.Power10(a),
                _ => throw new ArgumentException($"Неизвестная функция: {func}")
            };
        }

        // Вспомогательные методы
        private bool HasHigherPrecedence(string op1, string op2)
        {
            if (!OperatorPrecedence.ContainsKey(op1) || !OperatorPrecedence.ContainsKey(op2))
                return false;

            return OperatorPrecedence[op1] >= OperatorPrecedence[op2];
        }

        private bool IsFunction(string token)
        {
            return token == "sin" || token == "cos" || token == "tan" ||
                   token == "cot" || token == "ctg" ||
                   token == "ln" || token == "log" || token == "sqrt" ||
                   token == "abs" || token == "asin" || token == "acos" ||
                   token == "atan" || token == "acot" || token == "actg" ||
                   token == "sinh" || token == "cosh" || token == "tanh" ||
                   token == "exp" || token == "10^";
        }

        private bool IsOperator(string token)
        {
            return token == "+" || token == "-" || token == "*" || token == "/";
        }
    }
}