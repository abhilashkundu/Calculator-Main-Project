using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class CalculatorController : MonoBehaviour
{
    [SerializeField] private Text outputText;

    private string expression = "";
    private bool resetOnNextInput = false;

    // Handles number button presses
    public void OnNumberPress(string number)
    {
        if (resetOnNextInput)
        {
            expression = "";
            resetOnNextInput = false;
        }

        if (number == "." && (expression.EndsWith(".") || HasMultipleDecimal()))
        {
            return;
        }

        expression += number;
        UpdateOutput();
    }

    // Handles operator button presses
    public void OnOperatorPress(string operatorSymbol)
    {
        if (string.IsNullOrEmpty(expression))
        {
            return;
        }

        if (IsLastCharacterOperator())
        {
            return; 
        }

        if (resetOnNextInput)
        {
            resetOnNextInput = false;
        }

        expression += " " + operatorSymbol + " ";
        UpdateOutput();
    }

    // Handles equals button press
    public void OnEqualsPress()
    {
        if (string.IsNullOrEmpty(expression))
        {
            return;
        }

        try
        {
            expression = RemoveLastOperator(expression);

            float result = Calculate(expression);
            expression = result.ToString();
            resetOnNextInput = true; // Flag
        }
        catch (Exception e)
        {
            expression = "Error";
            Debug.LogError(e.Message);
            resetOnNextInput = true;
        }

        UpdateOutput();
    }

    // Clears the current expression
    public void OnClearPress()
    {
        expression = "";
        resetOnNextInput = false; //flag
        UpdateOutput();
    }

    // Updates the displayed text on the calculator
    private void UpdateOutput()
    {
        if (string.IsNullOrEmpty(expression))
        {
            outputText.text = "0";
        }
        else
        {
            outputText.text = expression;
        }
    }

    // Evaluates the problem
    private float Calculate(string expression)
    {
        string[] tokens = expression.Split(' ');
        Stack<float> values = new Stack<float>();
        Stack<string> operators = new Stack<string>();

        foreach (string token in tokens)
        {
            if (float.TryParse(token, out float number))
            {
                values.Push(number);
            }
            else if (IsOperator(token))
            {
                while (operators.Count > 0 && Power(operators.Peek()) >= Power(token))
                {
                    float secondOperand = values.Pop();
                    float firstOperand = values.Pop();
                    string op = operators.Pop();
                    float result = ApplyOperator(op, firstOperand, secondOperand);
                    values.Push(result);
                }
                operators.Push(token);
            }
        }

        // Evaluate remaining operators
        while (operators.Count > 0)
        {
            float secondOperand = values.Pop();
            float firstOperand = values.Pop();
            string op = operators.Pop();
            float result = ApplyOperator(op, firstOperand, secondOperand);
            values.Push(result);
        }

        return values.Pop();
    }

    // Applies the specified operator to two operands
    private float ApplyOperator(string op, float a, float b)
    {
        if (op == "+")
        {
            return a + b;
        }
        else if (op == "-")
        {
            return a - b;
        }
        else if (op == "*")
        {
            return a * b;
        }
        else if (op == "/")
        {
            if (Mathf.Approximately(b, 0))
            {
                throw new DivideByZeroException("Cannot divide by zero");
            }
            return a / b;
        }
        else
        {
            throw new InvalidOperationException("Invalid operator: " + op);
        }
    }

    // Returns the power of operators
    private int Power(string op)
    {
        if (op == "+" || op == "-")
        {
            return 1; // Lower power operator
        }
        else if (op == "*" || op == "/")
        {
            return 2; // Higher power operator
        }
        return 0;
    }

    // Checks if a token is a valid operator
    private bool IsOperator(string token)
    {
        return token == "+" || token == "-" || token == "*" || token == "/"; //returns bool
    }

    // Checks if the last character in the expression is an operator
    private bool IsLastCharacterOperator()
    {
        if (string.IsNullOrEmpty(expression))
        {
            return false;
        }

        string[] tokens = expression.Trim().Split(' ');
        string lastToken = tokens[tokens.Length - 1];
        return IsOperator(lastToken);
    }

    // Checks if the current number has more than one decimal point - BUG 1
    private bool HasMultipleDecimal()
    {
        string[] tokens = expression.Split(' ');
        if (tokens.Length == 0)
        {
            return false;
        }

        string lastToken = tokens[tokens.Length - 1];
        int decimalCount = 0;

        foreach (char c in lastToken)
        {
            if (c == '.')
            {
                decimalCount++;
            }
            if (decimalCount > 1)
            {
                return true;
            }
        }

        return false;
    }

    private string RemoveLastOperator(string expression)
    {
        string[] tokens = expression.Trim().Split(' ');
        if (tokens.Length > 0 && IsOperator(tokens[tokens.Length - 1]))
        {
            return string.Join(" ", tokens, 0, tokens.Length - 1); // Remove last operator
        }
        return expression;
    }
}