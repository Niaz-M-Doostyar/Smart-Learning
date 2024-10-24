using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AdvancedMathSolver
{
    class Program
    {
        // Dictionary to store variables
        static Dictionary<string, double> variables = new Dictionary<string, double>();
        // History of expressions
        static List<string> history = new List<string>();

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to the Advanced Computational Math Solver!");
            Console.WriteLine("You can input mathematical expressions, use variables, and define functions.");
            Console.WriteLine("Supported operations: +, -, *, /, ^ (power), sqrt(), sin(), cos(), tan(), log(), exp()");
            Console.WriteLine("Supported constants: pi, e");
            Console.WriteLine("Type 'exit' to quit or 'history' to see previous calculations.\n");

            while (true)
            {
                Console.Write("\nEnter expression: ");
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
                    double result = EvaluateExpression(input);
                    Console.WriteLine($"Result: {result}");
                    history.Add($"{input} = {result}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        // Function to display the history of previous expressions
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

        // Method to evaluate the mathematical expression
        static double EvaluateExpression(string expression)
        {
            // Handle variable assignment (e.g., x = 5)
            if (expression.Contains('='))
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

        // Tokenize the expression into numbers, operators, and functions
        static List<string> TokenizeExpression(string expression)
        {
            List<string> tokens = new List<string>();
            int index = 0;

            // Add support for constants like pi and e
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
                    else if (variables.ContainsKey(word)) // Variable substitution
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

        // Convert infix notation to postfix (Reverse Polish Notation)
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
                    operators.Pop(); // Remove '('
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

        // Evaluate the postfix expression
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

        // Check if a token is a supported operator
        static bool IsOperator(string token)
        {
            return token == "+" || token == "-" || token == "*" || token == "/" || token == "^";
        }

        // Check if a token is a mathematical function
        static bool IsFunction(string token)
        {
            return token == "sin" || token == "cos" || token == "tan" || token == "sqrt" || token == "log" || token == "exp";
        }

        // Perform basic arithmetic operations
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
                _ => throw new InvalidOperationException($"Unknown operator: {operation}"),
            };
        }

        // Perform supported mathematical functions
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

        // Get precedence of operators (for order of operations)
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
