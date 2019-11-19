///
/// Program: HTMLTags Checker
/// 
/// Author: Huy Mac
/// Date:   November 14, 2019
/// 
/// Purpose:  This program reads the any html data file by loading that file, and filter out html
///           opening and closing tags, displaying it on the textbox. It also check tags balance
///           or not.
///
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HTMLTagChecker
{
    public partial class Form1 : Form
    {
        string fileContent = "";        // A variable store data content from file.txt 
        string filePath = "";           // A file.txt path from computer
        string FileName = "";           // A variable store the file.txt of name
        public Form1()
        {
            InitializeComponent();
            processToolStripMenuItem.Enabled = false;
        }
        /// <summary>
        /// This method opens file Dialog from the from computer and displays the file name in the label.
        /// </summary>
        /// <param name="sender">The Object sender</param>
        /// <param name="e">The Event Argument</param>
        private void LoadFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            OpenFileDialog openFileDialog = new OpenFileDialog(); // Create openFileDialog Object.
            openFileDialog.InitialDirectory = "C:\\";             // Initial to C: Directory.
            openFileDialog.Filter = "html files (*.html)|*.html"; // Display only html file name extension.
            // Open the Dialog and Press "Ok" for specific html file, read all content of the file to the end.
            DialogResult dialogResult = openFileDialog.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {
                processToolStripMenuItem.Enabled = true;
                filePath = openFileDialog.FileName;
                StreamReader reader = new StreamReader(filePath);
                fileContent = reader.ReadToEnd();

                // Display file name in Label
                FileName = Path.GetFileName(filePath);
                loadLabel.Text = "Loaded: " + FileName;
            } else if (dialogResult == DialogResult.Cancel)
            {
                processToolStripMenuItem.Enabled = false;
            }

        }
        /// <summary>
        /// This method exits the application.
        /// </summary>
        /// <param name="sender">The Object sender</param>
        /// <param name="e">The Event Argument</param>
        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Pop up the Message Box ask user want to close the form or not.
            if (MessageBox.Show("Are you want to exit?", "Exit", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                Application.Exit();
            }
        }
        /// <summary>
        /// This method checks opening and closing tags.
        /// </summary>
        /// <param name="tag">The HTML tags</param>
        private void checkOpeningClosing(string tag)
        {
            // Recognize Opening and Closing tags display suitable text.
            if (tag.Contains("/"))
            {
                htmlTextBox.Text += "Found closing tag: " + tag + "\r\n";
            }
            else
            {
                htmlTextBox.Text += "Found opening tag: " + tag + "\r\n";
            }
        }
        /// <summary>
        /// This method checks all tag in HTML file and displays on the TextBox.
        /// </summary>
        /// <param name="sender">The Object sender</param>
        /// <param name="e">The Event Argument</param>
        private void CheckTagToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Record the number of characters
            int numChars = 0;
            string displayedResult = "";

            // This Regex will match all opening and closing and closing tags
            Match match = Regex.Match(fileContent, @"<(\/)?\w+");

            // Initialize a stack to store opening tags and
            // then compare the opening tag on top of it with any closing tag found
            Stack<string> stack = new Stack<string>();          // Create stack object

            do
            {
                // Current tag found from "match"
                string currTag = match.ToString();

                // [Optional] This is intended to ignore everything between <style> and </style>
                // or <script> and </script>
                // Used to check balanced tags for websites that use client-site frameworks like React or Angular
                if (stack.Any() &&
                    Regex.IsMatch(stack.Peek(), @"^(style|script)$", RegexOptions.IgnoreCase))
                {
                    if (Regex.IsMatch(currTag, @"^(</style|</script)$", RegexOptions.IgnoreCase))
                    {
                        displayedResult += $"{AddLeadingSpaces(stack.Count)}Found closing tag: {currTag}>\r\n";
                        stack.Pop();
                    }

                    // Find the next tag
                    match = match.NextMatch();
                    continue;
                }

                // If Regex found a non-container tag, it would append the element to the displayed result
                if (Regex.IsMatch(currTag, @"^(<br|<hr|<img|<link|<col|<row|<input|<meta)$", RegexOptions.IgnoreCase))
                    displayedResult += $"{AddLeadingSpaces(stack.Count)}Found non-container tag: {currTag}>\r\n";

                // If Regex found an opening tag, it would push the non-alphabetical version of the opening tag
                // on top of the stack and append the element to the displayed result
                else if (Regex.IsMatch(currTag, @"<\w+", RegexOptions.IgnoreCase))
                {
                    stack.Push(currTag.Remove(0, 1));
                    displayedResult += $"{AddLeadingSpaces(stack.Count)}Found opening tag: {currTag}>\r\n";
                }
                // If Regex found a closing tag, it would compare it with the opening tag on top of the stack and
                // append the element to the displayed result

                else
                {
                    displayedResult += $"{AddLeadingSpaces(stack.Count)}Found closing tag: {currTag}>\r\n";

                    string sanitizedOpeningTag = stack.Pop().ToLower();
                    // If they don't match, then this html file is not balanced
                    if (!currTag.ToLower().Contains(sanitizedOpeningTag))
                    {
                        charactersLabel.Text = $"Number of characters found: {numChars + 1}";
                        DisplayFailureMessage($"<{sanitizedOpeningTag}>", $"{currTag}>", $"</{sanitizedOpeningTag}>");
                        htmlTextBox.Text = displayedResult;
                        return;
                    }
                }

                // Find the next tag
                match = match.NextMatch();

                // Update number of characters
                numChars = match.Index > numChars ? match.Index : numChars;
            } while (stack.Any());

            DisplaySuccessMessage();
            htmlTextBox.Text = displayedResult;
            charactersLabel.Text = $"Number of characters found: {numChars + 1}";
            processToolStripMenuItem.Enabled = false;
        }
        private static string AddLeadingSpaces(int tagsStackSize)
        {
            return new string(' ', tagsStackSize * 2);
        }

        /// <summary>
        /// This method displays a meaningful message on the form if tags aren't balanced
        /// </summary>
        private void DisplayFailureMessage(string openingTag, string closingTag, string expectedTag)
        {
            // Clean the list box
            htmlTextBox.Clear();

            // Assign a success message to the result label on the form
            loadLabel.Text = $"{this.FileName} DOES NOT have balanced tags (expected {expectedTag} for {openingTag} but found {closingTag})";

            // Make the label distinct by change the color of the text inside
            loadLabel.ForeColor = Color.Red;
            return;
        }

        /// <summary>
        /// This method displays a meaningful message on the form if tags are balanced
        /// </summary>
        private void DisplaySuccessMessage()
        {
            // Clean the list box
            htmlTextBox.Clear();

            // Assign a success message to the result label on the form
            loadLabel.Text = String.Format("{0} has balanced tags",
                this.FileName);

            // Make the label distinct by change the color of the text inside
            loadLabel.ForeColor = Color.Blue;
        }

        /// <summary>
        /// This method uses to clear data from TextBox and change back default label
        /// </summary>
        /// <param name="sender">The Object sender</param>
        /// <param name="e">The Event Argument</param>
        private void FileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            htmlTextBox.Clear();
            charactersLabel.Text = "Number of characters found: ";
            loadLabel.Text = "No File Loaded";
            loadLabel.ForeColor = Color.Black;
        }
    }
}
