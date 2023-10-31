using Authentication;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseMiddleware<AuthenticationMiddleware>();

app.MapGet("/", () => "Hello World!");

app.Run();