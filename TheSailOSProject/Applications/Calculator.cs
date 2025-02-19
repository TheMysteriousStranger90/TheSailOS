using System;
using System.Collections.Generic;
using TheSailOSProject.Styles;

namespace TheSailOSProject.Applications
{
    public class Calculator
    {
        public static void Run()
        {
            Console.Clear();
            ConsoleManager.WriteLineColored("Advanced Calculator", ConsoleStyle.Colors.Primary);
            ConsoleManager.WriteLineColored("Enter expression (e.g., 1 + 2 * 3) or 'exit':", ConsoleStyle.Colors.Accent);

            while (true)
            {
                ConsoleManager.WriteColored(">", ConsoleStyle.Colors.Primary);
                string input = Console.ReadLine();

                if (input.ToLower() == "exit")
                {
                    break;
                }

                try
                {
                    double result = Evaluate(input);
                    ConsoleManager.WriteLineColored($"Result: {result}", ConsoleStyle.Colors.Success);
                }
                catch (Exception ex)
                {
                    ConsoleManager.WriteLineColored($"Error: {ex.Message}", ConsoleStyle.Colors.Error);
                }
            }

            ConsoleManager.WriteLineColored("Calculator closed.", ConsoleStyle.Colors.Primary);
            Console.ReadKey();
        }

        private static double Evaluate(string expression)
        {
            try
            {
                return EvaluateExpression(expression);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Invalid expression: {ex.Message}");
            }
        }

        private static double EvaluateExpression(string expression)
        {
            expression = expression.Replace(" ", "");

            if (string.IsNullOrEmpty(expression))
            {
                throw new ArgumentException("Expression cannot be empty.");
            }

            return ParseAndEvaluate(expression);
        }

        private static double ParseAndEvaluate(string expression)
        {
            Stack<double> numbers = new Stack<double>();
            Stack<char> operators = new Stack<char>();

            for (int i = 0; i < expression.Length; i++)
            {
                if (char.IsDigit(expression[i]) || expression[i] == '.')
                {
                    string num = "";
                    while (i < expression.Length && (char.IsDigit(expression[i]) || expression[i] == '.'))
                    {
                        num += expression[i];
                        i++;
                    }
                    i--;
                    numbers.Push(double.Parse(num));
                }
                else if (expression[i] == '(')
                {
                    operators.Push(expression[i]);
                }
                else if (expression[i] == ')')
                {
                    while (operators.Peek() != '(')
                    {
                        numbers.Push(ApplyOperator(operators.Pop(), numbers.Pop(), numbers.Pop()));
                    }
                    operators.Pop();
                }
                else if (IsOperator(expression[i]))
                {
                    while (operators.Count > 0 && Precedence(expression[i]) <= Precedence(operators.Peek()))
                    {
                        numbers.Push(ApplyOperator(operators.Pop(), numbers.Pop(), numbers.Pop()));
                    }
                    operators.Push(expression[i]);
                }
            }

            while (operators.Count > 0)
            {
                numbers.Push(ApplyOperator(operators.Pop(), numbers.Pop(), numbers.Pop()));
            }

            return numbers.Pop();
        }

        private static bool IsOperator(char c)
        {
            return c == '+' || c == '-' || c == '*' || c == '/' || c == '%' || c == '^';
        }

        private static int Precedence(char op)
        {
            switch (op)
            {
                case '+':
                case '-':
                    return 1;
                case '*':
                case '/':
                case '%':
                    return 2;
                case '^':
                    return 3;
                default:
                    return 0;
            }
        }

        private static double ApplyOperator(char op, double b, double a)
        {
            switch (op)
            {
                case '+':
                    return a + b;
                case '-':
                    return a - b;
                case '*':
                    return a * b;
                case '/':
                    if (b == 0)
                    {
                        throw new DivideByZeroException("Cannot divide by zero.");
                    }
                    return a / b;
                case '%':
                    if (b == 0)
                    {
                        throw new DivideByZeroException("Cannot divide by zero.");
                    }
                    return a % b;
                case '^':
                    return Math.Pow(a, b);
                default:
                    throw new ArgumentException("Invalid operator.");
            }
        }
    }
}