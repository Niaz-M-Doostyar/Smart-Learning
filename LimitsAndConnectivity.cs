using System;
using System.Collections.Generic;
using System.Data;

namespace LimitsAndContinuitySolver
{
    class Program
    {
        static Dictionary<string, double> variables = new Dictionary<string, double>();
        static List<string> history = new List<string>();

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to the Limits and Continuity Solver!");
            Console.WriteLine("Supported operations: +, -, *, /, ^ (power), sqrt(), sin(), cos(), tan(), log(), exp()");
            Console.WriteLine("You can calculate limits and check for continuity.");
            Console.WriteLine("Usage examples:");
            Console.WriteLine("  limit(2*x^2 + 3*x, 0)      -> Calculate limit as x -> 0");
            Console.WriteLine("  limit(1/x, 0, 'left')      -> Left-hand limit");
            Console.WriteLine("  continuity(1/x, 0)         -> Check if function is continuous at x = 0");
            Console.WriteLine("Type 'exit' to quit or 'history' to see previous calculations.\n");

            while (true)
            {
                Console.Write("\nEnter command: ");
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
                    if (input.StartsWith("limit"))
                    {
                        double result = EvaluateLimit(input);
                        Console.WriteLine($"Limit Result: {result}");
                        history.Add($"{input} = {result}");
                    }
                    else if (input.StartsWith("continuity"))
                    {
                        bool result = CheckContinuity(input);
                        Console.WriteLine($"Is the function continuous at the point? {result}");
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

        // Evaluate the limit expression
        static double EvaluateLimit(string input)
        {
            // Example format: limit(f(x), point, side)
            var match = System.Text.RegularExpressions.Regex.Match(input, @"limit\((.+),\s*([\d.]+),\s*'(left|right)?'\)");
            if (!match.Success)
                match = System.Text.RegularExpressions.Regex.Match(input, @"limit\((.+),\s*([\d.]+)\)");

            if (!match.Success)
                throw new ArgumentException("Invalid limit format. Use: limit(function, point, 'left' or 'right')");

            string function = match.Groups[1].Value;
            double point = double.Parse(match.Groups[2].Value);
            string side = match.Groups.Count > 3 ? match.Groups[3].Value : "";

            if (side == "left")
            {
                return CalculateLimit(function, point, -1); // Approach from the left
            }
            else if (side == "right")
            {
                return CalculateLimit(function, point, 1);  // Approach from the right
            }
            else
            {
                return CalculateLimit(function, point, 0);  // General limit (both sides)
            }
        }

        // Method to calculate the limit numerically
        static double CalculateLimit(string function, double point, int direction)
        {
            double h = 1e-5; // A very small value to approximate the limit
            double x = point;

            if (direction == -1)
            {
                // Left-hand limit (approach from values less than the point)
                return EvaluateFunction(function, x - h);
            }
            else if (direction == 1)
            {
                // Right-hand limit (approach from values greater than the point)
                return EvaluateFunction(function, x + h);
            }
            else
            {
                // General limit (both sides)
                double leftLimit = EvaluateFunction(function, x - h);
                double rightLimit = EvaluateFunction(function, x + h);

                if (Math.Abs(leftLimit - rightLimit) < 1e-5) // Close enough to be considered equal
                {
                    return leftLimit;
                }
                else
                {
                    throw new InvalidOperationException("The limit does not exist (left and right limits are not equal).");
                }
            }
        }

        // Method to check for continuity
        static bool CheckContinuity(string input)
        {
            // Example format: continuity(f(x), point)
            var match = System.Text.RegularExpressions.Regex.Match(input, @"continuity\((.+),\s*([\d.]+)\)");

            if (!match.Success)
                throw new ArgumentException("Invalid continuity format. Use: continuity(function, point)");

            string function = match.Groups[1].Value;
            double point = double.Parse(match.Groups[2].Value);

            // Check 3 conditions for continuity:
            // 1. The function is defined at x = point.
            double functionValue = EvaluateFunction(function, point);

            // 2. The limit exists at x -> point.
            double limitValue = CalculateLimit(function, point, 0);

            // 3. The function value equals the limit value at x = point.
            return Math.Abs(functionValue - limitValue) < 1e-5;
        }

        // Function to evaluate a mathematical expression at a given x (used for limits and continuity)
        static double EvaluateFunction(string function, double x)
        {
            // Replace the variable 'x' with its value in the function expression
            string expression = function.Replace("x", x.ToString());
            return EvaluateExpression(expression);
        }

        // General method to evaluate a mathematical expression
        static double EvaluateExpression(string expression)
        {
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
