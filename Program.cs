using Microsoft.EntityFrameworkCore;
using WorkItemApi.Data;
using WorkItemApi.Middleware;
using WorkItemApi.Repositories;
using WorkItemApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    x => x.Key,
                    x => x.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(new
            {
                error = new
                {
                    code = "VALIDATION_ERROR",
                    message = "輸入資料驗證失敗。",
                    details = errors
                }
            });
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=workitems.db"));

builder.Services.AddScoped<IWorkItemRepository, WorkItemRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IStatusRepository, StatusRepository>();
builder.Services.AddScoped<WorkItemService>();
builder.Services.AddScoped<StatusService>();
builder.Services.AddScoped<AuthService>();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.RoutePrefix = "swagger";
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "WorkItem API v1");
});

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();
app.MapControllers();

await DatabaseInitializer.InitializeAsync(app.Services);

app.Run();
