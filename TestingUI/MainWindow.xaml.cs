using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.TextFormatting;
namespace TestingUI
{
    public partial class MainWindow : Window
    {
        private Calculator _calculator;
        private bool _isExpanded = false;

        public MainWindow()
        {
            InitializeComponent();
            _calculator = new Calculator(UpdateDisplay, UpdateExpression);
            AttachButtonEvents();
            this.KeyDown += Window_KeyDown;
        }

        private void AttachButtonEvents()
        {
            foreach (UIElement el in ButtonGrid.Children)
            {
                if (el is Button button && button.Name != "BackspaceButton")
                {
                    button.Click += Button_Click;
                }
            }
        }

        private void UpdateDisplay(string value)
        {
            TextSize.Text = value;
        }

        private void UpdateExpression(string value)
        {
           TextLabel.Text = value;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string content = button.Name;               
                _calculator.ProcessInput(button.Content.ToString());
                
            }
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            _isExpanded = !_isExpanded;
            ExtraColumn.Width = _isExpanded ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
            MenuIcon.Kind = _isExpanded ? MaterialDesignThemes.Wpf.PackIconKind.ChevronLeft : MaterialDesignThemes.Wpf.PackIconKind.HamburgerMenu;

            foreach (UIElement element in ButtonGrid.Children)
            {
                if (Grid.GetColumn(element) == 4)
                    element.Visibility = _isExpanded ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.D0: case Key.NumPad0: _calculator.ProcessInput("0"); break;
                case Key.D1: case Key.NumPad1: _calculator.ProcessInput("1"); break;
                case Key.D2: case Key.NumPad2: _calculator.ProcessInput("2"); break;
                case Key.D3: case Key.NumPad3: _calculator.ProcessInput("3"); break;
                case Key.D4: case Key.NumPad4: _calculator.ProcessInput("4"); break;
                case Key.D5: case Key.NumPad5: _calculator.ProcessInput("5"); break;
                case Key.D6: case Key.NumPad6: _calculator.ProcessInput("6"); break;
                case Key.D7: case Key.NumPad7: _calculator.ProcessInput("7"); break;
                case Key.D8: case Key.NumPad8: _calculator.ProcessInput("8"); break;
                case Key.D9: case Key.NumPad9: _calculator.ProcessInput("9"); break;
                case Key.OemPeriod: case Key.Decimal: _calculator.ProcessInput("."); break;
                case Key.Add: case Key.OemPlus: _calculator.ProcessInput("+"); break;
                case Key.Subtract: case Key.OemMinus: _calculator.ProcessInput("-"); break;
                case Key.Multiply: _calculator.ProcessInput("*"); break;
                case Key.Divide: case Key.Oem2: _calculator.ProcessInput("/"); break;
                case Key.Enter: _calculator.ProcessInput("="); break;
                case Key.Back: _calculator.ProcessInput("Backspace"); break;
                case Key.Delete: _calculator.ProcessInput("C"); break;
                case Key.Escape: _calculator.ProcessInput("CE"); break;
                case Key.Z when Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl):
                    _calculator.ProcessInput("Undo");
                    break;
                case Key.P: _calculator.ProcessInput("π"); break;
                case Key.E: _calculator.ProcessInput("e"); break;
                case Key.L: _calculator.ProcessInput("ln"); break;
                case Key.OemOpenBrackets: _calculator.ProcessInput("("); break;
                case Key.OemCloseBrackets: _calculator.ProcessInput(")"); break;
            }
        }

        private void BackspaceButton_Click(object sender, RoutedEventArgs e)
        {
            _calculator.ProcessInput("Backspace");
        }
        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            _calculator.ProcessInput("Undo");
        }
    }

    public class Calculator
    {
        private string _expression = "";
        private string _lastEntry = "";
        private readonly Action<string> _updateDisplay;
        private readonly Action<string> _updateExpression;

        public Calculator(Action<string> updateDisplay, Action<string> updateExpression)
        {
            _updateDisplay = updateDisplay;
            _updateExpression = updateExpression;
        }

        public void ProcessInput(string input)
        {
            if (input == "C")
            {
                _expression = "";
                _updateExpression("");
                _updateDisplay("");
                return;
            }
            if (input == "CE")
            {
                int lastOpIndex = _expression.LastIndexOfAny(new char[] { '+', '-', '*', '/', '^' });
                _expression = lastOpIndex >= 0 ? _expression.Substring(0, lastOpIndex + 1) : "";
                _updateExpression(_expression);
                return;
            }
            if (input == "Backspace")
            {
                if (_expression.Length > 0)
                    _expression = _expression.Substring(0, _expression.Length - 1);
                _updateExpression(_expression);
                return;
            }
            if (input == "=")
            {
                try
                {
                    double result = EvaluateExpression(_expression);
                    _updateDisplay(result.ToString(CultureInfo.InvariantCulture));
                    _expression = result.ToString(CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    _updateDisplay("Помилка");
                }
                return;
            }
            if (input == "%")
            {
                _expression += "%";
                _updateExpression(_expression);
                return;
            }
            if (char.IsDigit(input[0]))
            {
                if (_expression.Length == 0 && input == "00")
                {
                    return; 
                }

                string[] parts = _expression.Split(new char[] { '+', '-', '*', '/', '^', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0)
                {
                    string lastNumber = parts.Last();
                    if (lastNumber == "0" && input == "00")
                    {
                        return; 
                    }
                }
            }


            if (input == "π")
                input = Math.PI.ToString(CultureInfo.InvariantCulture);
            else if (input == "e")
                input = Math.E.ToString(CultureInfo.InvariantCulture);
            

            _lastEntry = input;
            _expression += input;
            _updateExpression(_expression);

        }

        private double EvaluateExpression(string expression)
        {
            Queue<string> postfix = ConvertToPostfix(expression);
            return EvaluatePostfix(postfix);
        }

        private Queue<string> ConvertToPostfix(string infix)
        {
            Stack<string> operators = new Stack<string>();
            Queue<string> output = new Queue<string>();
            StringBuilder number = new StringBuilder();

            for (int i = 0; i < infix.Length; i++)
            {
                char c = infix[i];

                if (char.IsDigit(c) || c == '.')
                {
                    number.Append(c);
                }
                else
                {
                    if (number.Length > 0)
                    {
                        output.Enqueue(number.ToString());
                        number.Clear();
                    }

                    if (c == '(')
                    {
                        operators.Push(c.ToString());
                    }
                    else if (c == ')')
                    {
                        while (operators.Count > 0 && operators.Peek() != "(")
                            output.Enqueue(operators.Pop());
                        operators.Pop();
                    }
                    else if (c == 'l' && i + 1 < infix.Length && infix[i + 1] == 'n') 
                    {
                        operators.Push("ln");
                        i++;
                    }
                    else if (c == '√') 
                    {
                        operators.Push("√");
                    }
                    else if (IsOperator(c.ToString()))
                    {
                        while (operators.Count > 0 && Priority(operators.Peek()) >= Priority(c.ToString()))
                            output.Enqueue(operators.Pop());
                        operators.Push(c.ToString());
                    }
                }
            }

            if (number.Length > 0)
                output.Enqueue(number.ToString());

            while (operators.Count > 0)
                output.Enqueue(operators.Pop());

            return output;
        }

        private double EvaluatePostfix(Queue<string> postfix)
        {
            Stack<double> stack = new Stack<double>();
            double lastNumber = 0;

            while (postfix.Count > 0)
            {
                string token = postfix.Dequeue();
                if (double.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out double num))
                {
                    stack.Push(num);
                    lastNumber = num; 
                }
                else if (token == "π")
                {
                    stack.Push(Math.PI);
                }
                else if (token == "e")
                {
                    stack.Push(Math.E);
                }
                else if (token == "ln" || token == "√") 
                {
                    double b = stack.Pop(); 

                    if (token == "ln")
                    {
                        if (b <= 0)
                            throw new ArgumentException("Логарифм визначений тільки для x > 0.");
                        stack.Push(Math.Log(b));
                    }
                    else if (token == "√")
                    {
                        if (b < 0)
                            throw new ArgumentException("Корінь визначений тільки для x ≥ 0.");
                        stack.Push(Math.Sqrt(b));
                    }
                }              
                else if (token == "%") 
                {
                    if (stack.Count >= 2) 
                    {
                        double percentValue = stack.Pop();
                        double baseValue = stack.Peek();
                        stack.Push(baseValue * (percentValue / 100));
                    }
                    else if (stack.Count == 1) 
                    {
                        stack.Push(stack.Pop() / 100);
                    }
                }
                else // (+, -, *, /, ^)
                {
                    double b = stack.Pop();
                    double a = stack.Pop();
                    stack.Push(ApplyOperator(a, b, token));
                }
            }

            return stack.Pop();
        }

        private bool IsOperator(string c) => "+-*/^√lnπe%".Contains(c);

        private int Priority(string op)
        {
            return op switch
            {
                "+" or "-" => 1,
                "*" or "/" or "%" => 2,
                "^" => 3,
                "√" or "ln" => 4,
                
                _ => 0
            };
        }

        private double ApplyOperator(double a, double b, string op)
        {
            return op switch
            {
                "+" => a + b,
                "-" => a - b,
                "*" => a * b,
                "/" => b != 0 ? a / b : throw new DivideByZeroException(),
                "^" => Math.Pow(a, b),
                "√" => Math.Sqrt(b),
                "ln" => Math.Log(b),
                "π" => Math.PI,
                "e" => Math.E,
                "%" => a * (b / 100),
                _ => throw new InvalidOperationException()
            };
        }      
    }
}
