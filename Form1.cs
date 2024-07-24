using System;
using System.Data;
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
                if (textBox.Text.Length == 0 || IsOperator(lastChar))
                {
                    textBox.Text += "0.";
                    return;
                }
                else
                {
                    // Check if the current number segment already has a decimal point
                    int lastOperatorIndex = textBox.Text.LastIndexOfAny(new char[] { '+', '-', '*', '/' });
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
            if (textBox.Text.Length == 1 && buttonChar == '-')
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
            // Get the equation from the textbox
            string equation = textBox.Text;
            // Simplify consecutive operators
            equation = HandleConsecutiveOperators(equation);

            if (equation.Contains("0/0"))
            {
                MessageBox.Show("Result is undefined", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                decimalCount = 0;
                textBox.Text = "0";
                textBox.SelectionStart = textBox.Text.Length;

                clear_Click(sender, e);
                return;
            }

            // Check for division by zero
            if (equation.Contains("/0"))
            {
                MessageBox.Show("Division by zero is not allowed", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                decimalCount = 0;
                textBox.Text = "0";
                textBox.SelectionStart = textBox.Text.Length;

                clear_Click(sender, e);
                return;
            }

            // Check for equation length
            if (equation.Length <= 1)
            {
                clear_Click(sender, e);
                return;
            }

            // Check if the equation ends with an operator
            if (IsOperator(equation.Last()))
            {
                return;
            }

            // Use DataTable to compute the result
            var result = new DataTable().Compute(equation, null);
            // Change the decimal separator to "."
            textBox.Text = result.ToString().Replace(',', '.');
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

        // Handle key presses
        private void textBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            char lastChar = textBox.Text.Length > 0 ? textBox.Text.Last() : '\0';

            // Clear initial "0" when a number is pressed
            if (textBox.Text == "0" && char.IsDigit(e.KeyChar))
            {
                textBox.Clear();
            }

            // Allow minus as the first character
            if (textBox.Text.Length == 1 && e.KeyChar == '-')
            {
                textBox.Clear();
                textBox.Text += e.KeyChar;
                e.Handled = true;
            }

            // Avoid consecutive operators
            if (IsOperator(lastChar) && IsOperator(e.KeyChar))
            {
                // If the last character is "-" and the length is 1, don't replace it
                if (textBox.Text.Length == 1 && lastChar == '-')
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
                if (textBox.Text.Length == 0 || IsOperator(lastChar))
                {
                    textBox.Text += "0.";
                    e.Handled = true;
                }
                else
                {
                    // Check if the current number segment already has a decimal point
                    int lastOperatorIndex = textBox.Text.LastIndexOfAny(new char[] { '+', '-', '*', '/' });
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