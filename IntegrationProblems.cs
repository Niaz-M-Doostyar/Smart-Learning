using System;
using System.Collections.Generic;
using System.Data;

namespace AdvancedMathSolverWithIntegration
{
    class Program
    {
        static Dictionary<string, double> variables = new Dictionary<string, double>();
        static List<string> history = new List<string>();

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to the Advanced Math Solver with Integration!");
            Console.WriteLine("You can input mathematical expressions, use variables, and solve integrals.");
            Console.WriteLine("Supported operations: +, -, *, /, ^ (power), sqrt(), sin(), cos(), tan(), log(), exp()");
            Console.WriteLine("Supported constants: pi, e");
            Console.WriteLine("Type 'exit' to quit or 'history' to see previous calculations.\n");

            while (true)
            {
                Console.Write("\nEnter expression or integration (e.g., integrate(2*x, 0, 10)): ");
                string input = Console.ReadLine();

                if (input.ToLower() == "exit")
                {
                    Console.WriteLine("Exiting... Goodbye!");
                    break;
                }

                if (input.ToLower() == "history")
                {
                    ShowHistory();
                    continue;
                }

                try
                {
                    if (input.StartsWith("integrate"))
                    {
                        double result = EvaluateIntegration(input);
                        Console.WriteLine($"Result (Integration): {result}");
                        history.Add($"{input} = {result}");
                    }
                    else
                    {
                        double result = EvaluateExpression(input);
                        Console.WriteLine($"Result: {result}");
                        history.Add($"{input} = {result}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        // Function to show history
        static void ShowHistory()
        {
            if (history.Count == 0)
            {
                Console.WriteLine("No history yet.");
            }
            else
            {
                Console.WriteLine("History of calculations:");
                foreach (var item in history)
                {
                    Console.WriteLine(item);
                }
            }
        }

        // Evaluate the integration expression using Trapezoidal Rule
        static double EvaluateIntegration(string input)
        {
            // Extract the function, start and end points from the input, e.g., integrate(2*x, 0, 10)
            var match = System.Text.RegularExpressions.Regex.Match(input, @"integrate\((.+),\s*([\d.]+),\s*([\d.]+)\)");
            if (!match.Success)
                throw new ArgumentException("Invalid integration format. Use: integrate(function, start, end)");

            string function = match.Groups[1].Value;
            double start = double.Parse(match.Groups[2].Value);
            double end = double.Parse(match.Groups[3].Value);

            return TrapezoidalIntegration(function, start, end, 1000); // Using 1000 slices for good accuracy
        }

        // Numerical integration using the Trapezoidal rule
        static double TrapezoidalIntegration(string function, double a, double b, int n)
        {
            double h = (b - a) / n;
            double sum = 0.5 * (EvaluateFunction(function, a) + EvaluateFunction(function, b));

            for (int i = 1; i < n; i++)
            {
                double x = a + i * h;
                sum += EvaluateFunction(function, x);
            }

            return sum * h;
        }

        // Function to evaluate a mathematical expression at a given x (used for integration)
        static double EvaluateFunction(string function, double x)
        {
            // Replace the variable 'x' with its value in the function expression
            string expression = function.Replace("x", x.ToString());
            return EvaluateExpression(expression);
        }

        // Method to evaluate a general mathematical expression
        static double EvaluateExpression(string expression)
        {
            if (expression.Contains("="))
            {
                var parts = expression.Split('=');
                if (parts.Length != 2)
                    throw new ArgumentException("Invalid variable assignment format.");

                string varName = parts[0].Trim();
                double varValue = EvaluateExpression(parts[1].Trim());

                if (variables.ContainsKey(varName))
                    variables[varName] = varValue;
                else
                    variables.Add(varName, varValue);

                return varValue;
            }

            var tokens = TokenizeExpression(expression);
            var postfix = ConvertToPostfix(tokens);
            return EvaluatePostfix(postfix);
        }

        // Tokenize expression into elements
        static List<string> TokenizeExpression(string expression)
        {
            List<string> tokens = new List<string>();
            int index = 0;

            expression = expression.Replace("pi", Math.PI.ToString()).Replace("e", Math.E.ToString());

            while (index < expression.Length)
            {
                char current = expression[index];

                if (char.IsWhiteSpace(current))
                {
                    index++;
                    continue;
                }

                if (char.IsDigit(current) || current == '.')
                {
                    string number = "";
                    while (index < expression.Length && (char.IsDigit(expression[index]) || expression[index] == '.'))
                    {
                        number += expression[index];
                        index++;
                    }
                    tokens.Add(number);
                }
                else if (IsOperator(current.ToString()) || current == '(' || current == ')')
                {
                    tokens.Add(current.ToString());
                    index++;
                }
                else if (char.IsLetter(current))
                {
                    string word = "";
                    while (index < expression.Length && (char.IsLetter(expression[index])))
                    {
                        word += expression[index];
                        index++;
                    }

                    if (IsFunction(word))
                    {
                        tokens.Add(word);
                    }
                    else if (variables.ContainsKey(word))
                    {
                        tokens.Add(variables[word].ToString());
                    }
                    else
                    {
                        throw new InvalidOperationException($"Unknown function or variable: {word}");
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Invalid character in expression: {current}");
                }
            }

            return tokens;
        }

        // Convert infix to postfix (Reverse Polish Notation)
        static List<string> ConvertToPostfix(List<string> tokens)
        {
            Stack<string> operators = new Stack<string>();
            List<string> output = new List<string>();

            foreach (var token in tokens)
            {
                if (double.TryParse(token, out _))
                {
                    output.Add(token);
                }
                else if (IsFunction(token))
                {
                    operators.Push(token);
                }
                else if (token == "(")
                {
                    operators.Push(token);
                }
                else if (token == ")")
                {
                    while (operators.Peek() != "(")
                    {
                        output.Add(operators.Pop());
                    }
                    operators.Pop();
                    if (operators.Count > 0 && IsFunction(operators.Peek()))
                    {
                        output.Add(operators.Pop());
                    }
                }
                else if (IsOperator(token))
                {
                    while (operators.Count > 0 && GetPrecedence(operators.Peek()) >= GetPrecedence(token))
                    {
                        output.Add(operators.Pop());
                    }
                    operators.Push(token);
                }
            }

            while (operators.Count > 0)
            {
                output.Add(operators.Pop());
            }

            return output;
        }

        // Evaluate postfix expression
        static double EvaluatePostfix(List<string> postfix)
        {
            Stack<double> operands = new Stack<double>();

            foreach (var token in postfix)
            {
                if (double.TryParse(token, out double number))
                {
                    operands.Push(number);
                }
                else if (IsOperator(token))
                {
                    double b = operands.Pop();
                    double a = operands.Pop();
                    double result = PerformOperation(a, b, token);
                    operands.Push(result);
                }
                else if (IsFunction(token))
                {
                    double a = operands.Pop();
                    double result = PerformFunction(token, a);
                    operands.Push(result);
                }
            }

            return operands.Pop();
        }

        // Supported operators
        static bool IsOperator(string token)
        {
            return token == "+" || token == "-" || token == "*" || token == "/" || token == "^";
        }

        // Supported mathematical functions
        static bool IsFunction(string token)
        {
            return token == "sin" || token == "cos" || token == "tan" || token == "sqrt" || token == "log" || token == "exp";
        }

        // Perform arithmetic operations
        static double PerformOperation(double a, double b, string operation)
        {
            return operation switch
            {
                "+" => a + b,
                "-" => a - b,
                "*" => a * b,
                "/" when b != 0 => a / b,
                "/" => throw new DivideByZeroException("Cannot divide by zero."),
                "^" => Math.Pow(a, b),
                _ => throw new InvalidOperationException($"Unknown operator: {operation}")
            };
        }

        // Perform mathematical functions
        static double PerformFunction(string function, double a)
        {
            return function switch
            {
                "sin" => Math.Sin(a),
                "cos" => Math.Cos(a),
                "tan" => Math.Tan(a),
                "sqrt" => Math.Sqrt(a),
                "log" => Math.Log(a),
                "exp" => Math.Exp(a),
                _ => throw new InvalidOperationException($"Unknown function: {function}")
            };
        }

        // Get precedence of operators
        static int GetPrecedence(string op)
        {
            return op switch
            {
                "+" or "-" => 1,
                "*" or "/" => 2,
                "^" => 3,
                _ => 0
            };
        }
    }
}
