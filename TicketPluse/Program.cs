
using BLL.Services.Class;
using BLL.Services.Interface;
using DAL;
using DAL.Data.AuthModel;
using DAL.Repository.Class;
using DAL.Repository.Interface;
using DAL.UnitOfWork;
using EduManage.API.Error;
using EduMangment.Services.Classes;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;
using TicketPluse.Helper;

namespace TicketPluse
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var configuration = ConfigurationOptions.Parse(builder.Configuration.GetConnectionString("Redis"), true);
                return ConnectionMultiplexer.Connect(configuration);
            });
            builder.Services.AddIdentity<App_user, IdentityRole<Guid>>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.User.RequireUniqueEmail = true;
            })
         .AddEntityFrameworkStores<AppDbContext>()
         .AddDefaultTokenProviders();

            builder.Services.Configure<JWT>(
                builder.Configuration.GetSection("JWT"));

            builder.Services.Configure<Melseting>(
                builder.Configuration.GetSection("MailSettings"));

            var jwtKey = builder.Configuration["JWT:Key"]
                ?? throw new Exception("JWT Key is missing");
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero,

                    ValidIssuer = builder.Configuration["JWT:Issuer"],
                    ValidAudience = builder.Configuration["JWT:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey))
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (context.Request.Cookies.ContainsKey("jwtToken"))
                        {
                            context.Token = context.Request.Cookies["jwtToken"];
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy =>
                    policy.RequireRole("Admin"));
            });
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();
            builder.Services.AddTransient(typeof(IGenaricRePo<>), typeof(GenaricRebo<>));
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            // الـ Services الأساسية
            builder.Services.AddScoped<IAuthService, AuthServices>(); 
            builder.Services.AddScoped<IProfileService, ProfileService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IEventService, EventService>();
            builder.Services.AddScoped<IUser, UserService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IBookkingService, BookinService>();


            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();
            var app = builder.Build();
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();

                app.UseSwaggerUI(options => {
                    options.SwaggerEndpoint("/openapi/v1.json", "TicketPulse API v1");
                });
            }

            app.UseHttpsRedirection();

            app.UseAuthentication(); 

            app.UseAuthorization();

            app.UseExceptionHandler();

            app.MapControllers();

            app.Run();
        }
    }
}
