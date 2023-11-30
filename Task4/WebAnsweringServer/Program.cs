using NuGetAnswering;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
CancellationTokenSource cts = new CancellationTokenSource();
string modelUrl = "https://storage.yandexcloud.net/dotnet4/bert-large-uncased-whole-word-masking-finetuned-squad.onnx";
string modelPath = "bert-large-uncased-whole-word-masking-finetuned-squad.onnx";
AnsweringComponent answerTask;
answerTask = new AnsweringComponent(modelUrl, modelPath, cts.Token);
_ = answerTask.Create(new ConsoleProgress());

Dictionary<string, string> Texts = new Dictionary<string, string>();
builder.Services.AddSingleton<AnsweringComponent>(answerTask);
builder.Services.AddSingleton<Dictionary<string, string>>(Texts);


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

//app.UseAuthorization();
app.UseCors(builder =>
{
    builder
    .WithOrigins("*")
    .WithHeaders("*")
    .WithMethods("*");
   

    });

app.MapControllers();

app.Run();


public class ConsoleProgress : IProgress<string>
{
    public void Report(string message)
    {
        Console.WriteLine(message);
    }
}

public partial class Program
{

}