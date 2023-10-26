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
using System.Runtime.CompilerServices;
using DataBase;
using Microsoft.EntityFrameworkCore;

namespace WpfAnswerNet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int tabCounter = 1;
       
        string modelUrl = "https://storage.yandexcloud.net/dotnet4/bert-large-uncased-whole-word-masking-finetuned-squad.onnx";
        string modelPath = "bert-large-uncased-whole-word-masking-finetuned-squad.onnx";
        AnsweringComponent answerTask; 

        public MainWindow()
        {
            InitializeComponent();
            CancellationTokenSource cts = new CancellationTokenSource();

            answerTask = new AnsweringComponent(modelUrl, modelPath, cts.Token);

            _ = answerTask.Create(new ConsoleProgress());
            LoadTabTextsFromDatabase();
            CreateNewTabElem(answerTask, cts);
        }
        public class ConsoleProgress : IProgress<string>
        {
            public void Report(string message)
            {
                MessageBox.Show(message);
            }
        }
        private void NewTabButton_Click(object sender, RoutedEventArgs e)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            CreateNewTabElem(answerTask, cts);
        }

        private void CreateNewTabElem(AnsweringComponent answerTask, CancellationTokenSource cts)
        {
            TabItem newTab = new TabItem();
            newTab.Header = "Tab " + tabCounter;
            tabCounter++;
            newTab.Content = new TabElement(answerTask, cts);
            TabControl1.Items.Add(newTab);

            newTab.MouseDoubleClick += (s, e) => CloseTab(newTab);

        }

   
        private void CloseTab(TabItem tabItem)
        {
            TabControl1.Items.Remove(tabItem);
        }

        private void LoadTabTextsFromDatabase()
        {
            using (var context = new AppDbContext()) 
            {
                var texts = context.TabTexts.ToList();

                foreach (var text in texts)
                {
                    AddNewTabWithText(text.Text);
                }
            }
        }

        private void AddNewTabWithText(string text)
        {
            TabItem tabItem = new TabItem();
            tabItem.Header = "Tab " + tabCounter;
            tabCounter++;
            CancellationTokenSource cts = new CancellationTokenSource();
            tabItem.Content = new TabElement(answerTask, cts, text);
            TabControl1.Items.Add(tabItem);

            tabItem.MouseDoubleClick += (s, e) => CloseTab(tabItem);
           
        }

        private void ClearDatabase_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите очистить базу данных?", "Подтверждение", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                using (var context = new AppDbContext())
                {
                    context.TabTexts.RemoveRange(context.TabTexts);
                    context.QuestionAnswers.RemoveRange(context.QuestionAnswers);
                    context.SaveChanges();
                }
            }
        }
    }
}
