using System;
using System.Linq;
using System.Windows.Forms;

namespace Calculator
{
    public partial class Form1 : Form
    {
        private int decimalCount = 0;

        public Form1()
        {
            InitializeComponent();
            this.ActiveControl = textBox;
            textBox.SelectionStart = textBox.Text.Length;
        }

        // Handle button clicks
        private void button_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            char buttonChar = button.Text[0];
            char lastChar = textBox.Text.Last();

            // Avoid consecutive operators
            if (IsOperator(lastChar) && IsOperator(buttonChar))
            {
                // If the last character is "-" and the length is 1, don't replace it
                if (textBox.Text.Length == 1 && lastChar == '-')
                {
                    return;
                }
                else
                {
                    // Replace the last operator with the new one
                    textBox.Text = textBox.Text.Substring(0, textBox.Text.Length - 1) + buttonChar;
                    return;
                }
            }

            // Prevent operators when first character is parentheses
            if (lastChar == '(' && IsParentheses(lastChar) && IsOperator(buttonChar))
            {
                // Allow minus as the first character
                if (textBox.Text == "(" && buttonChar == '-')
                {
                    textBox.Text += buttonChar;
                    return;
                }
                return;
            }

            // Add "*" when last character is ')' and clicked key is number or decimal
            if (lastChar == ')' && (char.IsDigit(buttonChar) || buttonChar == '.'))
            {
                textBox.Text += "*";
            }

            // Handle parentheses
            if (buttonChar == '(')
            {
                // Prevent decimals before parentheses
                if (lastChar == '.')
                {
                    textBox.Text = textBox.Text.Substring(0, textBox.Text.Length - 1);
                    return;
                }

                // Allow parentheses as the first character
                if (textBox.Text == "0")
                {
                    if (lastChar == '(')
                        return;
                    textBox.Text = textBox.Text.Substring(0, textBox.Text.Length - 1) + buttonChar;
                    return;
                }

                // Prevent multiple opening parentheses in a row
                if (lastChar == '(')
                {
                    return;
                }
                else
                {
                    // Handle cases where there is no text or the last character is an operator
                    if (textBox.Text.Length == 0 || IsOperator(lastChar))
                    {
                        textBox.Text += buttonChar;
                        //e.Handled = true;
                        //openParentheses++;
                    }
                    else
                    {
                        textBox.Text += "*" + buttonChar;
                        //e.Handled = true;
                        //openParentheses++;
                    }
                    return;
                }
            }

            if (buttonChar == ')')
            {
                // Handle cases where there are unbalanced parentheses
                int openParentheses = textBox.Text.Count(c => c == '(');
                int closeParentheses = textBox.Text.Count(c => c == ')');
                if (closeParentheses >= openParentheses)
                {
                    return;
                }

                // Prevent decimals before parentheses
                if (lastChar == '.')
                {
                    textBox.Text = textBox.Text.Substring(0, textBox.Text.Length - 1) + buttonChar;
                    return;
                }

                // Prevent multiple opening parentheses in a row
                if (IsParentheses(lastChar))
                {
                    return;
                }
                else
                {
                    // Prevent closing parenthesis if the last character is an operator
                    if (IsOperator(lastChar) || lastChar == '(' || !char.IsDigit(lastChar))
                    {
                        return;
                    }
                    else
                    {
                        textBox.Text += buttonChar;
                        return;
                        //closeParentheses++;
                    }
                }
            }

            // Reset decimalCount when an operator clicked
            if (IsOperator(buttonChar))
            {
                if (lastChar == '.')
                {
                    textBox.Text = textBox.Text.Substring(0, textBox.Text.Length - 1);
                }
                decimalCount = 0;
            }

            // Handle decimal point
            if (buttonChar == '.')
            {
                // If there is no text or the last character is an operator, prepend a "0."
                if (textBox.Text.Length == 0 || IsOperator(lastChar) || IsParentheses(lastChar))
                {
                    textBox.Text += "0.";
                    return;
                }
                else
                {
                    // Check if the current number segment already has a decimal point
                    int lastOperatorIndex = textBox.Text.LastIndexOfAny(new char[] { '+', '-', '*', '/', '(', ')' });
                    string lastSegment = lastOperatorIndex == -1 ? textBox.Text : textBox.Text.Substring(lastOperatorIndex + 1);
                    if (lastSegment.Contains('.'))
                    {
                        return;
                    }
                }
            }

            // Prevent multiple leading zeros
            if (textBox.Text == "0" && buttonChar == '0')
            {
                return;
            }

            // Allow minus as the first character
            if (textBox.Text == "0" && buttonChar == '-')
            {
                textBox.Clear();
                textBox.Text += buttonChar;
                return;
            }

            // Handle decimal point input when "0" is clicked
            if (textBox.Text == "0" && buttonChar == '.')
            {
                textBox.Text = "0.";
                decimalCount = 1;
                return;
            }

            // Clear initial "0" when a number is clicked
            if (textBox.Text == "0" && char.IsDigit(buttonChar))
            {
                textBox.Clear();
            }

            textBox.Text += buttonChar;
            textBox.SelectionStart = textBox.Text.Length;
        }

        // Handle result button click
        private void result_Click(object sender, EventArgs e)
        {
            string equation = textBox.Text;
            equation = HandleConsecutiveOperators(equation);

            // Check for unbalanced parentheses
            int openParentheses = equation.Count(c => c == '(');
            int closeParentheses = equation.Count(c => c == ')');
            if (openParentheses != closeParentheses)
            {
                return;
            }

            // Check for invalid operations
            if (equation.Contains("0/0"))
            {
                MessageBox.Show("Result is undefined", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                decimalCount = 0;
                textBox.Text = "0";
                return;
            }

            if (equation.Contains("/0"))
            {
                MessageBox.Show("Division by zero is not allowed", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                decimalCount = 0;
                textBox.Text = "0";
                return;
            }

            // Check if the equation ends with an operator
            if (IsOperator(equation.Last()) || equation.Last() == '(' || equation.Last() == '.')
            {
                return;
            }

            try
            {
                var expression = new NCalc.Expression(equation);
                var result = expression.Evaluate();
                textBox.Text = result.ToString().Replace(',', '.');
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox.Text = "0";
            }
            textBox.SelectionStart = textBox.Text.Length;
        }

        // Simplify consecutive operators in the equation
        private string HandleConsecutiveOperators(string equation)
        {
            char[] operators = { '+', '-', '*', '/', '.' };
            // Replace consecutive operators with a single one
            for (int i = 0; i < operators.Length; i++)
            {
                string consecutiveOperators = $"{operators[i]}{operators[i]}";

                while (equation.Contains(consecutiveOperators))
                {
                    equation = equation.Replace(consecutiveOperators, operators[i].ToString());
                }
            }
            return equation;
        }

        // Handle clear button click
        private void clear_Click(object sender, EventArgs e)
        {
            // Reset the text box
            if (textBox.Text.Length > 0)
            {
                textBox.Text = "0";
                decimalCount = 0;
                textBox.SelectionStart = textBox.Text.Length;
            }
        }

        // Check if the character is an operator
        private bool IsOperator(char character)
        {
            return (character == '+' || character == '-' || character == '*' || character == '/');
        }

        // Check if the character is an parentheses
        private bool IsParentheses(char character)
        {
            return (character == '(' || character == ')');
        }

        // Handle key presses
        private void textBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            char lastChar = textBox.Text.Length > 0 ? textBox.Text.Last() : '\0';
            char secondLastChar = textBox.Text.Length > 1 ? textBox.Text[textBox.Text.Length - 2] : '\0';

            // Clear initial "0" when a number is pressed
            if (textBox.Text == "0" && char.IsDigit(e.KeyChar))
            {
                textBox.Clear();
            }

            // Prevent operators when first character is parentheses
            if (lastChar == '(' && IsParentheses(lastChar) && IsOperator(e.KeyChar))
            {
                // Allow minus as the first character
                if (lastChar == '(' && e.KeyChar == '-')
                {
                    textBox.Text += e.KeyChar;
                    e.Handled = true;
                }
                e.Handled = true;
            }

            // Add "*" when last character is ')' and clicked key is number or decimal
            if (lastChar == ')' && (char.IsDigit(e.KeyChar) || e.KeyChar == '.'))
            {
                textBox.Text += "*";
            }

            // Handle parentheses
            if (e.KeyChar == '(')
            {
                // Prevent decimals before parentheses
                if (lastChar == '.')
                {
                    textBox.Text = textBox.Text.Substring(0, textBox.Text.Length - 1);
                    e.Handled = true;
                }

                // Allow parentheses as the first character
                if (textBox.Text == "0")
                {
                    if (lastChar == '(')
                        e.Handled = true;
                    textBox.Text = textBox.Text.Substring(0, textBox.Text.Length - 1);
                    e.Handled = true;
                }

                // Prevent multiple opening parentheses in a row
                if (lastChar == '(')
                {
                    e.Handled = true;
                }
                else
                {
                    // Handle cases where there is no text or the last character is an operator
                    if (textBox.Text.Length == 0 || IsOperator(lastChar))
                    {
                        textBox.Text += e.KeyChar;
                    }
                    else
                    {
                        textBox.Text += "*" + e.KeyChar;
                    }
                    e.Handled = true;
                }
            }

            if (e.KeyChar == ')')
            {
                // Handle cases where there are unbalanced parentheses
                int openParentheses = textBox.Text.Count(c => c == '(');
                int closeParentheses = textBox.Text.Count(c => c == ')');
                if (closeParentheses >= openParentheses)
                {
                    e.Handled = true;
                }

                // Prevent decimals before parentheses
                if (lastChar == '.')
                {
                    textBox.Text = textBox.Text.Substring(0, textBox.Text.Length - 1) + e.KeyChar;
                    e.Handled = true;
                }

                // Prevent multiple opening parentheses in a row
                if (IsParentheses(lastChar))
                {
                    e.Handled = true;
                }
                else
                {
                    // Prevent closing parenthesis if the last character is an operator
                    if (IsOperator(lastChar) || lastChar == '(' || !char.IsDigit(lastChar))
                    {
                        e.Handled = true;
                    }
                    else
                    {
                        textBox.Text += e.KeyChar;
                        e.Handled = true;
                    }
                }
            }

            // Prevent consecutive parentheses
            if (IsParentheses(lastChar) && IsParentheses(e.KeyChar))
            {
                e.Handled = true;
            }

            // Allow minus as the first character
            if (textBox.Text == "0" && e.KeyChar == '-')
            {
                textBox.Clear();
                textBox.Text += e.KeyChar;
                e.Handled = true;
            }

            // Avoid consecutive operators
            if (IsOperator(lastChar) && IsOperator(e.KeyChar))
            {
                // If the last character is "-" and the second last character is "(", don't replace it
                if (lastChar == '-' && secondLastChar == '(')
                {
                    e.Handled = true;
                }
                else
                {
                    // Replace the last operator with the new one
                    textBox.Text = textBox.Text.Substring(0, textBox.Text.Length - 1) + e.KeyChar;
                    e.Handled = true;
                }
            }

            // Handle decimal point
            if (e.KeyChar == '.')
            {
                // If there is no text or the last character is an operator, prepend a "0."
                if (textBox.Text.Length == 0 || IsOperator(lastChar) || IsParentheses(lastChar))
                {
                    textBox.Text += "0.";
                    e.Handled = true;
                }
                else
                {
                    // Check if the current number segment already has a decimal point
                    int lastOperatorIndex = textBox.Text.LastIndexOfAny(new char[] { '+', '-', '*', '/', '(', ')' });
                    string lastSegment = lastOperatorIndex == -1 ? textBox.Text : textBox.Text.Substring(lastOperatorIndex + 1);
                    if (lastSegment.Contains('.'))
                    {
                        e.Handled = true;
                    }
                }
            }

            // Reset decimalCount when an operator is clicked
            if (IsOperator(e.KeyChar))
            {
                // If the last character is '.', remove it
                if (textBox.Text.Length > 0 && lastChar == '.')
                {
                    textBox.Text = textBox.Text.Substring(0, textBox.Text.Length - 1);
                }
            }

            // Allow only numbers, operators, decimal point, and control chars
            if (!char.IsDigit(e.KeyChar) && !IsOperator(e.KeyChar)
                && e.KeyChar != '.'
                && e.KeyChar != (char)Keys.Back
                && e.KeyChar != (char)Keys.Enter
                && e.KeyChar != (char)Keys.Escape)
            {
                e.Handled = true;
            }

            // Handle backspace key
            if (e.KeyChar == (char)Keys.Back)
            {
                // If the removed char is '.', reset the decimalCount
                if (textBox.Text.Length > 0 && lastChar == '.')
                {
                    textBox.Text = textBox.Text.Substring(0, textBox.Text.Length - 1);
                }

                // If there is only one character, set it to '0' instead of deleting
                if (textBox.Text.Length == 1)
                {
                    textBox.Text = "0";
                    e.Handled = true;
                }
            }

            // Handle enter key pressed, show the result
            if (e.KeyChar == (char)Keys.Enter)
            {
                result_Click(sender, e);
                e.Handled = true;
            }

            // When escape key pressed, clear the textbox
            if (e.KeyChar == (char)Keys.Escape)
            {
                clear_Click(sender, e);
                e.Handled = true;
            }

            textBox.SelectionStart = textBox.Text.Length;
        }

        // Handle delete button click
        private void delete_Click(object sender, EventArgs e)
        {
            // Check if the last character is a decimal point
            if (textBox.Text.Length > 0 && textBox.Text[textBox.Text.Length - 1] == '.')
                // Reset decimalCount when a decimal point is deleted
                decimalCount = 0;

            // Deletion logic
            if (textBox.Text.Length > 1)
                textBox.Text = textBox.Text.Substring(0, textBox.Text.Length - 1);
            else
                clear_Click(sender, e);
        }
    }
}