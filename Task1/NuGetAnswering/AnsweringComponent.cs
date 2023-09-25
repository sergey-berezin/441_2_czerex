
using BERTTokenizers;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ML.Data;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
namespace NuGetAnswering{
    public static class StreamExtensions
    {
        public static async Task CopyToAsync(this Stream source, Stream destination, int bufferSize, IProgress<string> progress = null, CancellationToken cancellationToken = default) {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (!source.CanRead)
                throw new ArgumentException("Has to be readable", nameof(source));
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));
            if (!destination.CanWrite)
                throw new ArgumentException("Has to be writable", nameof(destination));
            if (bufferSize < 0)
                throw new ArgumentOutOfRangeException(nameof(bufferSize));

            var buffer = new byte[bufferSize];
            long totalBytesRead = 0;
            int bytesRead;
            long lastReportedMb = 0;

            while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) != 0) {
                await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
                totalBytesRead += bytesRead;
                long currentMb = totalBytesRead / 1000000; 
                if (currentMb > lastReportedMb) 
                { progress?.Report($"Read: {currentMb} MB"); 
                lastReportedMb = currentMb; }
            }
        }
    }

    public class AnsweringComponent
    {
        private static InferenceSession session;
        private static string modelUrl;
        private static string modelPath;
        private readonly object locker  = new object();
        IProgress<string>? progress;
        CancellationToken cancelToken;
        public AnsweringComponent(string modelUrlTemp, string modelPathTemp, CancellationToken cancelTokenTemp = default)
        {   
            modelPath = modelPathTemp;
            modelUrl = modelUrlTemp;
            cancelToken = cancelTokenTemp;
            
        }
        public async Task Create(IProgress<string> progressTemp){
            if (!File.Exists(modelPath))
            {
                progress = progressTemp;
                await DownloadModelWithRetryAsync(); 
            }
            session = new InferenceSession(modelPath);
        }


        public Task<string> GetAnswerAsync(string text, string question){
            return Task.Factory.StartNew(() => {
                try{
                    cancelToken.ThrowIfCancellationRequested();
                    var sentence = "{\"question\": \"" + question + "\", \"context\": \"@CTX\"}".Replace("@CTX", text);
                    var tokenizer = new BertUncasedLargeTokenizer();
                    var tokens = tokenizer.Tokenize(sentence);
                    var encoded = tokenizer.Encode(tokens.Count(), sentence);
                    var bertInput = new BertInput()
                    {
                        InputIds = encoded.Select(t => t.InputIds).ToArray(),
                        AttentionMask = encoded.Select(t => t.AttentionMask).ToArray(),
                        TypeIds = encoded.Select(t => t.TokenTypeIds).ToArray(),
                    };
                        
                    var input_ids = ConvertToTensor(bertInput.InputIds, bertInput.InputIds.Length);
                    var attention_mask = ConvertToTensor(bertInput.AttentionMask, bertInput.InputIds.Length);
                    var token_type_ids = ConvertToTensor(bertInput.TypeIds, bertInput.InputIds.Length);
                    var input = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("input_ids", input_ids), 
                                                            NamedOnnxValue.CreateFromTensor("input_mask", attention_mask), 
                                                            NamedOnnxValue.CreateFromTensor("segment_ids", token_type_ids) };

                    cancelToken.ThrowIfCancellationRequested();
                    IDisposableReadOnlyCollection<DisposableNamedOnnxValue>? output;
                    lock(locker){
                        output = session.Run(input);
                    }
                    cancelToken.ThrowIfCancellationRequested();
            
                    List<float> startLogits = (output.ToList().First().Value as IEnumerable<float>).ToList();
                    List<float> endLogits = (output.ToList().Last().Value as IEnumerable<float>).ToList();

                    var startIndex = startLogits.ToList().IndexOf(startLogits.Max()); 
                    var endIndex = endLogits.ToList().IndexOf(endLogits.Max());
                    var predictedTokens = tokens
                                .Skip(startIndex)
                                .Take(endIndex + 1 - startIndex)
                                .Select(o => tokenizer.IdToToken((int)o.VocabularyIndex))
                                .ToList();

                    var answer = String.Join(" ", predictedTokens);
                    cancelToken.ThrowIfCancellationRequested();
                    return answer;
                }
                catch (OperationCanceledException) {  return "Operation was cancelled"; } 
                catch (Exception ex)  {return ex.Message; }
            }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

    }
        public async Task DownloadModelWithRetryAsync()
    {
        int maxRetries = 5; 
        for (int retry = 0; retry < maxRetries; retry++)
        {
            try
            {
                await DownloadModelAsync();
                return; 
            }
            catch (Exception ex)
            {
                progress?.Report($"Error downloading model: {ex.Message}");
                continue;
            }
        }
        progress?.Report("Failed to download the model.");
    }

    public async Task DownloadModelAsync()
    {
        using (var client = new HttpClient())
        {  
            progress?.Report($"Downloading model: {modelUrl}");
            client.Timeout = TimeSpan.FromMinutes(5);
            var response = await client.GetAsync(modelUrl);
            response.EnsureSuccessStatusCode();
            progress?.Report("Got model successfully.");
            using (var modelStream = await response.Content.ReadAsStreamAsync())
            {
                using (var fileStream = new FileStream(modelPath, FileMode.Create))
                {
                    //await modelStream.CopyToAsync(fileStream);
                    await StreamExtensions.CopyToAsync(modelStream, fileStream, 16000, progress, cancelToken );
                }
            }
        }
    }

        public class BertInput
        {
            public long[] InputIds { get; set; }
            public long[] AttentionMask { get; set; }
            public long[] TypeIds { get; set; }
        }
        public static Tensor<long> ConvertToTensor(long[] inputArray, int inputDimension)
            {
                Tensor<long> input = new DenseTensor<long>(new[] { 1, inputDimension });
                for (var i = 0; i < inputArray.Length; i++)
                {
                    input[0,i] = inputArray[i];
                }
                return input;
            }
    }
}
