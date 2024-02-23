var builder = WebApplication.CreateBuilder(args);

var startup = new WebStartup(builder.Configuration);

startup.ConfigureServices(builder);

var app = builder.Build();

startup.ConfigureApp(app);

app.MapFallbackToFile("/index.html");

app.Run();

