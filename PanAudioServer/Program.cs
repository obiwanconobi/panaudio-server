using Microsoft.EntityFrameworkCore;
using PanAudioServer.Data;
using PanAudioServer.Helper;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.WebHost.UseSentry(options =>
{
    options.Dsn = Environment.GetEnvironmentVariable("SentryDsn");
    options.SendDefaultPii = true; // Adds request URL and headers, IP and name for users, etc.
    options.SetBeforeSend((@event, hint) =>
    {
        // Never report server names
        @event.ServerName = null;
        return @event;
    });
});
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
{
    builder.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader();
}));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddDbContext<SqliteContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("SqliteDB")));

builder.Services.AddSingleton<HttpClient>();
builder.Services.AddScoped<SqliteHelper>();
builder.Services.AddScoped<ConfigHelper>();
builder.Services.AddScoped<DirectoryHelper>();
builder.Services.AddScoped<DatabaseHelper>();
builder.Services.AddScoped<ImageHelper>();
builder.Services.AddScoped<MusicBrainzHelper>();
builder.Services.AddScoped<PlaybackHelper>();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SqliteContext>();
    db.Database.Migrate();
    db.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL; PRAGMA busy_timeout=5000;");
}
app.Run();


