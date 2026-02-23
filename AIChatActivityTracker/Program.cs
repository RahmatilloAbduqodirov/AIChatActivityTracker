using AIChatActivityTracker.Plugins;
using AIChatActivityTracker.Services;
using Microsoft.SemanticKernel;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();

// Register Activity Service
builder.Services.AddSingleton<IActivityService, ActivityService>();

// Configure Semantic Kernel
var openAiConfig = builder.Configuration.GetSection("OpenAI");
var modelId = openAiConfig["ModelId"] ?? "gpt-4o-mini";
var apiKey = openAiConfig["ApiKey"]
    ?? throw new InvalidOperationException(
        "OpenAI API key is not configured. Set OpenAI:ApiKey in appsettings.json or user secrets.");

builder.Services.AddSingleton<Kernel>(sp =>
{
    var kernelBuilder = Kernel.CreateBuilder();
    kernelBuilder.AddOpenAIChatCompletion(modelId, apiKey);

    var activityService = sp.GetRequiredService<IActivityService>();
    kernelBuilder.Plugins.AddFromObject(new ActivityPlugin(activityService), "ActivityPlugin");

    return kernelBuilder.Build();
});

builder.Services.AddSingleton(sp =>
{
    var kernel = sp.GetRequiredService<Kernel>();
    return kernel.GetRequiredService<Microsoft.SemanticKernel.ChatCompletion.IChatCompletionService>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
