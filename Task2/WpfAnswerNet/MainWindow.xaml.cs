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
    }
}
