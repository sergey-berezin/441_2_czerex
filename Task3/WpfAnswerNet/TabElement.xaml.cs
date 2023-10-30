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
using DataBase;
using Microsoft.EntityFrameworkCore;

namespace WpfAnswerNet
{
    /// <summary>
    /// Логика взаимодействия для TabElement.xaml
    /// </summary>
    public partial class TabElement : UserControl
    {
        CancellationTokenSource cts;
        AnsweringComponent answerTask;

        public TabElement(AnsweringComponent answerTaskTemp, CancellationTokenSource ctsTemp, string text = null)
        {
            InitializeComponent();
            answerTask = answerTaskTemp;
            cts = ctsTemp;
            cancelButton.IsEnabled = false;
            if(text != null)
            {
                fileContentBlock.Text = text;
                using (var context = new AppDbContext())
                {
                    var existText = context.TabTexts.FirstOrDefault(t => t.Text == text);
                    var tabId = existText.Id;
                    var lastQuestionAnswer = context.QuestionAnswers.Where(qa => qa.TabId == tabId)
                        .OrderByDescending(qa => qa.Id)
                        .FirstOrDefault();
                    if (lastQuestionAnswer != null)
                    {
                        questionBox.Text = lastQuestionAnswer.Question;
                        answerBlock.Text = lastQuestionAnswer.Answer;
                    }
                }
            }
            
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
                using (var context = new AppDbContext())
                {   
                    var existText = context.TabTexts.FirstOrDefault(t=> t.Text == fileContents);
                    if (existText == null)
                    {
                        var tabText = new TabText() { Text = fileContents };
                        context.TabTexts.Add(tabText);
                        context.SaveChanges();

                    }
                }
                
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
                using (var context = new AppDbContext())
                {
                    var existText = context.TabTexts.FirstOrDefault(t => t.Text == text);
                    var tabId = existText.Id;
                    var existQuestion = context.QuestionAnswers.Where(qa=> qa.TabId == tabId && qa.Question == question).FirstOrDefault();
                    if (existQuestion != null)
                    {
                        questionBox.Text = existQuestion.Question;
                        answerBlock.Text = existQuestion.Answer;
                        //MessageBox.Show("Exist!");
                        context.QuestionAnswers.Remove(existQuestion);
                        var questionAnswer = new QuestionAnswer()
                        {
                            TabId = existQuestion.TabId,
                            Question = existQuestion.Question,
                            Answer = existQuestion.Answer,
                        };
                        context.QuestionAnswers.Add(questionAnswer);
                        context.SaveChanges();
                     
                    }
                    else
                    {
                        var answer = await answerTask.GetAnswerAsync(text, question, cts.Token);
                        answerBlock.Text = answer;



                        var questionAnswer = new QuestionAnswer()
                        {
                            TabId = tabId,
                            Question = question,
                            Answer = answer
                        };
                        context.QuestionAnswers.Add(questionAnswer);
                        context.SaveChanges();
                    }
                }
              }

            cancelButton.IsEnabled = false;
            answerButton.IsEnabled = true;

        }

     

    }
}
