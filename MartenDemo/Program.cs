using JasperFx.CodeGeneration.Frames;
using Marten;
using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics;
using Weasel.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
string connectionString = "Server=127.0.0.1;Port=5432;Userid=postgres;Password=admin;Database=testjsonb";


builder.Services.AddMarten(o =>
{
    o.Connection(connectionString);
    o.AutoCreateSchemaObjects = Weasel.Core.AutoCreate.All;
});


var store = DocumentStore.For(_ =>
{
    _.Connection(connectionString);
    // This creates
    _.Schema.For<TestMarten>().Index(x => x.Speed);
    _.Schema.For<TestMarten>().Index(x => x.Category);
});

   




var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
app.UseSwagger();
app.UseSwaggerUI();
}
app.UseHttpsRedirection();
var summaries = new[]
{
"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};
app.MapGet("/weatherforecast", async (IDocumentStore doc) =>
{
    /*var rand = new Random();
    for (int i = 0; i < 30000; i++)
    {
        doc.Insert(
            new TestMarten(
                Guid.NewGuid(),
                $"Test_{i}",
                rand.Next(1, 4) switch
                {
                    1 => "cars",
                    2 => "planes",
                    3 => "helis",
                    4 => "boats",
                    _ => "cars"
                },
                (decimal)rand.NextDouble() * (200 - 100) + 100,
                rand.Next(1, 4)
            ));
    }
    doc.SaveChanges();*/

    var sw = new Stopwatch();
    sw.Start();
    using (var session = store.QuerySession())
    {
        var res = await session.Query<TestMarten>()
           .Where(x => x.Speed > 150 && x.Category == "cars")
           .ToListAsync();
        sw.Stop();
        Console.WriteLine("Elapsed: " + sw.ElapsedMilliseconds);
    }
    /*var res = await doc.Query<TestMarten>()
    .Where(x => x.Speed > 150 && x.Category == "cars")
        .ToListAsync();*/
    sw.Stop();
    return sw.ElapsedMilliseconds;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public record TestMarten(
    Guid Id,
    string Title,
    string Category,
    decimal Speed,
    int Capacity);