using Authentication;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseMiddleware<AuthenticationMiddleware>();
app.UseMiddleware<UserIdMiddleware>();

app.MapGet("/", () => "Hello World!");

app.Run();