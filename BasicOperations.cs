using System;
using System.Collections.Generic;
using System.Linq;

namespace MathSolver
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to the Computational Thinking Math Solver!");
            Console.WriteLine("You can input mathematical expressions with +, -, *, /, and parentheses.");

            while (true)
            {
                Console.WriteLine("\nPlease enter a math expression (or type 'exit' to quit): ");
                string input = Console.ReadLine();

                if (input.ToLower() == "exit")
                {
                    Console.WriteLine("Goodbye!");
                    break;
                }

                try
                {
                    double result = EvaluateExpression(input);
                    Console.WriteLine($"Result: {result}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        // EvaluateExpression method processes the entire mathematical expression
        static double EvaluateExpression(string expression)
        {
            var tokens = TokenizeExpression(expression);
            var postfix = ConvertToPostfix(tokens);
            return EvaluatePostfix(postfix);
        }

        // Tokenize the input expression into individual elements (numbers, operators, parentheses)
        static List<string> TokenizeExpression(string expression)
        {
            List<string> tokens = new List<string>();
            int index = 0;

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
                else
                {
                    throw new InvalidOperationException($"Invalid character in expression: {current}");
                }
            }

            return tokens;
        }

        // Convert the list of tokens from infix notation to postfix (Reverse Polish Notation)
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

        // Evaluate the postfix expression to compute the result
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
            }

            return operands.Pop();
        }

        // Check if the string is an operator
        static bool IsOperator(string token)
        {
            return token == "+" || token == "-" || token == "*" || token == "/";
        }

        // Perform the arithmetic operation for two operands
        static double PerformOperation(double a, double b, string operation)
        {
            return operation switch
            {
                "+" => a + b,
                "-" => a - b,
                "*" => a * b,
                "/" when b != 0 => a / b,
                "/" => throw new DivideByZeroException("Cannot divide by zero."),
                _ => throw new InvalidOperationException($"Unknown operator: {operation}"),
            };
        }

        // Get precedence of operators, higher value means higher precedence
        static int GetPrecedence(string op)
        {
            return op switch
            {
                "+" or "-" => 1,
                "*" or "/" => 2,
                _ => 0
            };
        }
    }
}
