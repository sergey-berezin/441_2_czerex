using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using NuGetAnswering;
using static WpfAnswerNet.MainWindow;

namespace WpfAnswerNet
{
    /// <summary>
    /// Логика взаимодействия для TabElement.xaml
    /// </summary>
    public partial class TabElement : UserControl
    {
        CancellationTokenSource cts;
        AnsweringComponent answerTask;

        public TabElement(AnsweringComponent answerTaskTemp, CancellationTokenSource ctsTemp)
        {
            InitializeComponent();
            answerTask = answerTaskTemp;
            cts = ctsTemp;
            cancelButton.IsEnabled = false;
        }
        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                string fileContents = File.ReadAllText(filePath);  
                fileContentBlock.Text = fileContents;
                
            }
        }
        private void CancelClick(object sender, RoutedEventArgs e)
        {
           cts.Cancel();
        }

        private async void GetAnswerClick(object sender, RoutedEventArgs e)
        {
            cancelButton.IsEnabled = true;
            answerButton.IsEnabled = false;
            string text = fileContentBlock.Text;
            string question = questionBox.Text;
            if (text.Length == 0)
            {
                MessageBox.Show("Choose text");
            }
            else if (question.Length == 0)
            {
                MessageBox.Show("Ask a question");
            }
            else
            {
                var answer = await answerTask.GetAnswerAsync(text, question, cts.Token);
                answerBlock.Text = answer;
            }
            cancelButton.IsEnabled = false;
            answerButton.IsEnabled =true;
   
        }
      
    }
}
