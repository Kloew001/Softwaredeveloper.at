var builder = WebApplication.CreateBuilder(args);
var startup = new WebStartup(builder);
startup.Run();
