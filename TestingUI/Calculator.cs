using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TestingUI
{
    public interface ICalculatorCommand
    {
        void Execute();
        void Unexecute();
    }
    public class Calculator
    {
        private string _expression = "";
        private string _lastEntry = "";
        private readonly Action<string> _updateDisplay;
        private readonly Action<string> _updateExpression;
        private Stack<ICalculatorCommand> undoStack = new Stack<ICalculatorCommand>();
        private Stack<ICalculatorCommand> redoStack = new Stack<ICalculatorCommand>();

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
            string[] parts = _expression.Split(new char[] { '+', '-', '*', '/', '^', '(', ')' }, StringSplitOptions.None);
            string lastNum = parts.Length > 0 ? parts.Last() : "";
           
            if (input == ".")
            {
                if (string.IsNullOrEmpty(lastNum) || lastNum.Contains("."))
                {
                    return; 
                }
            }

            if (input == "0")
            {
                if (lastNum == "0")
                {
                    return; 
                }
            }

            if (input == "00")
            {
                if (string.IsNullOrEmpty(lastNum) || lastNum == "0")
                {
                    return; 
                }
            }
            if (char.IsDigit(input[0]))
            {
                
                if (parts.Length > 0)
                {
                    string lastNumber = parts.Last();

                    if (lastNumber.StartsWith("0") && !lastNumber.Contains(".") && lastNumber.Length >= 1)
                    {
                        if (input != ".")
                        {
                            if (_expression.Length > 0)
                            {
                                char lastChar = _expression.Last();
                                if (!"+-*/^(".Contains(lastChar))
                                {
                                    return;
                                }
                            }
                            else return;
                        }
                    }
                }
            }
            string operators = "+-*/^";

            if (operators.Contains(input))
            {
                if (string.IsNullOrEmpty(_expression))
                {                  
                    if (input != "-") return;
                }
                else
                {
                    char lastChar = _expression.Last();
                    if (operators.Contains(lastChar))
                    {
                        return;
                    }
                }
            }
            _lastEntry = input;
            string actualInput = input;         
            AppendToExpression(input, actualInput);

        }

        private double EvaluateExpression(string expression)
        {
            Queue<string> postfix = ConvertToPostfix(expression);
            return EvaluatePostfix(postfix);
        }
        public void Undo_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine($"[Undo] undoStack.Count = {undoStack.Count}");

            if (undoStack.Count > 0)
            {
                var cmd = undoStack.Pop();
                Console.WriteLine("[Undo] Виконую Unexecute");
                cmd.Unexecute();
                redoStack.Push(cmd);
            }
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
                    else if (c == 'π' || c == 'e')
                    {
                        output.Enqueue(c.ToString());
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

        private bool IsOperator(string c) => "+-*/^√ln%".Contains(c);

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
        public void AppendToExpression(string displayInput, string actualInput, bool record = true)
        {
            Console.WriteLine($"[Append] Додаю в undoStack: '{actualInput}'");
            _expression += actualInput;
            _updateExpression(_expression);
            if (record)
            {
                undoStack.Push(new InputCommand(this, actualInput.Length));
                redoStack.Clear();
            }

        }

        public void RemoveLastInput(int length)
        {
            if (_expression.Length >= length)
            {
                _expression = _expression.Substring(0, _expression.Length - length);
                _updateExpression(_expression);
            }
        }
    }
    public class InputCommand : ICalculatorCommand
    {
        private Calculator _calculator;
        private int _length;

        public InputCommand(Calculator calculator, int length)
        {
            _calculator = calculator;
            _length = length;
        }

        public void Execute()
        {
            
        }

        public void Unexecute()
        {
            _calculator.RemoveLastInput(_length);
        }
    }
}
