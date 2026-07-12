
using BLL.Services.Implementation;
using BLL.Services.Interface;
using DAL;
using DAL.Repository.Class;
using DAL.Repository.Interface;
using DAL.UnitOfWork;
using EduManage.API.Error;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace TicketPluse
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ================= Database =================
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var configuration = ConfigurationOptions.Parse(builder.Configuration.GetConnectionString("Redis"), true);
                return ConnectionMultiplexer.Connect(configuration);
            });
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();
            builder.Services.AddTransient(typeof(IGenaricRePo<>), typeof(GenaricRebo<>));
            builder.Services.AddScoped<IBookingService, BookingService>();

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();
            var app = builder.Build();
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                // MapOpenApi مكتوبة مرة واحدة فقط هنا!
                app.MapOpenApi();

                // تشغيل واجهة الـ UI
                app.UseSwaggerUI(options => {
                    options.SwaggerEndpoint("/openapi/v1.json", "TicketPulse API v1");
                });
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseExceptionHandler();

            app.MapControllers();

            app.Run();
        }
    }
}
