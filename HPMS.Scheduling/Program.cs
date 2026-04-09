using HPMS.Scheduling;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapSchedulingEndpoints();

app.Run();

