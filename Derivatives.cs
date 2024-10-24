using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DerivativesSolver
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to the Derivatives Solver!");
            Console.WriteLine("Supported operations: +, -, *, /, ^ (power), sqrt(), sin(), cos(), tan(), log(), exp()");
            Console.WriteLine("You can calculate symbolic derivatives and perform numerical differentiation.");
            Console.WriteLine("Usage examples:");
            Console.WriteLine("  derivative(2*x^2 + 3*x, 'x')       -> First derivative with respect to x");
            Console.WriteLine("  derivative(sin(x) + x^3, 'x', 2)   -> Second derivative with respect to x");
            Console.WriteLine("Type 'exit' to quit or 'history' to see previous calculations.\n");

            List<string> history = new List<string>();

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
                    ShowHistory(history);
                    continue;
                }

                try
                {
                    if (input.StartsWith("derivative"))
                    {
                        var match = Regex.Match(input, @"derivative\((.+),\s*'(\w+)',\s*(\d+)?\)");
                        if (!match.Success)
                            throw new ArgumentException("Invalid derivative format. Use: derivative(function, 'variable', [order])");

                        string function = match.Groups[1].Value;
                        string variable = match.Groups[2].Value;
                        int order = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : 1;

                        string result = CalculateDerivative(function, variable, order);
                        Console.WriteLine($"Derivative Result: {result}");
                        history.Add($"{input} = {result}");
                    }
                    else
                    {
                        Console.WriteLine("Invalid command. Please try again.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        // Function to show calculation history
        stati
