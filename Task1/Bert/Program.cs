using NuGetAnswering;
class Program
{
    static async Task Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Please specify the file name");
            return;
        }

        string FilePath = args[0];

        string text = File.ReadAllText(FilePath);
        var cts = new CancellationTokenSource();
        var cancelToken = cts.Token;
        string modelUrl = "https://storage.yandexcloud.net/dotnet4/bert-large-uncased-whole-word-masking-finetuned-squad.onnx";
        string modelPath = "bert-large-uncased-whole-word-masking-finetuned-squad.onnx";
        var answerTask = new AnsweringComponent(modelUrl, modelPath, cancelToken);
        await answerTask.Create(new ConsoleProgress());
        var tasks = new List<Task>();
        while(!cancelToken.IsCancellationRequested)
        {
            Console.Write("Ask a question or press enter to exit: ");
            string question = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(question))
                cts.Cancel();

            var task = answerTask.GetAnswerAsync(text, question).ContinueWith(task=>{Console.WriteLine(question + " : " + task.Result);});
            tasks.Add(task);

        }
        await Task.WhenAll(tasks);
        
    
    }
    public class ConsoleProgress : IProgress<string>
    {
        public void Report(string message)
        {
            Console.WriteLine(message);
        }
    }
}
