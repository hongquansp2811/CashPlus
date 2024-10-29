using LOYALTY.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;
using Microsoft.Extensions.Caching.Distributed;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using LOYALTY.Interfaces;
using LOYALTY.Helpers;
using LOYALTY.Extensions;
using LOYALTY.DataAccess;
using LOYALTY.CloudMessaging;
using LOYALTY.PaymentGate;
using System.Security.Claims;
using System.Net.Http;
using Microsoft.OpenApi.Models;
using LOYALTY.PaymentGate.Interface;
using LOYALTY.Services;

namespace LOYALTY
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            StaticConfig = configuration;
        }

        public IConfiguration Configuration { get; }
        public static IConfiguration StaticConfig { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Enable CORS
            services.AddCors(c =>
            {
                c.AddPolicy("AllowOrigin", options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            });

            // JSON Serializer
            //services.AddControllersWithViews().AddNewtonsoftJson(options =>
            //    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore)
            //    .AddNewtonsoftJson(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver());

            services.AddDbContext<DataContext.LOYALTYDapperContext>(options =>
                      options.UseSqlServer(
                          Configuration.GetConnectionString("DefaultConnection")));
            //Register dapper in scope  
            services.AddScoped<IDapper, Dapperr>();

            services.AddDbContext<LOYALTYContext>(options =>
                options.UseSqlServer(Configuration["ConnectionStrings:CoreDB"]));

            services.AddControllers();

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = "localhost";
                options.InstanceName = "LOYALTYInstance";
            });

            string key = Configuration["SecretToken"];

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key))
                };
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("WebAdminUser", policy => policy.RequireAssertion(context =>
                                    context.User.HasClaim(c => c.Type == ClaimTypes.NameIdentifier && c.Value == "web_admin")
                ));

                options.AddPolicy("WebPartnerUser", policy => policy.RequireAssertion(context =>
                                    context.User.HasClaim(c => c.Type == ClaimTypes.NameIdentifier && (c.Value == "web_partner"))
                ));

                options.AddPolicy("AppUser", policy => policy.RequireAssertion(context =>
                                    context.User.HasClaim(c => c.Type == ClaimTypes.NameIdentifier && c.Value == "customer")
                ));
            });

            services.AddHttpClient("HttpClientWithSSLUntrusted").ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ClientCertificateOptions = ClientCertificateOption.Manual,
                ServerCertificateCustomValidationCallback =
           (httpRequestMessage, cert, cetChain, policyErrors) =>
           {
               return true;
           }
            });

            // services.AddSwaggerGen(c =>
            // {
            //     c.SwaggerDoc("v1", new OpenApiInfo { Title = "API docs", Version = "v1", Description = "APis are built for project system by ultraneit" });
            //     c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
            //     {
            //         Name = "Authorization",
            //         Type = SecuritySchemeType.ApiKey,
            //         Scheme = "Bearer",
            //         BearerFormat = "JWT",
            //         In = ParameterLocation.Header,
            //         Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer token\"",
            //     });
            //     c.AddSecurityRequirement(new OpenApiSecurityRequirement
            //     {
            //         {
            //               new OpenApiSecurityScheme
            //                 {
            //                     Reference = new OpenApiReference
            //                     {
            //                         Type = ReferenceType.SecurityScheme,
            //                         Id = "Bearer"
            //                     }
            //                 },
            //                 new string[] {}
            //         }
            //     });
            // });


            // Extensions
            services.AddScoped<IEncryptData, EncryptData>();
            services.AddScoped<ExcuteStringQuery, ExcuteStringQuery>();
            services.AddScoped<FCMNotification, FCMNotification>();

            // Payment Gate
            services.AddScoped<RSASign, RSASign>();
            services.AddScoped<BKTransaction, BKTransaction>();

            // Authen
            services.AddSingleton<IJwtAuth>(new Authen(key));
            services.AddScoped<IUser, UserDataAccess>();
            services.AddScoped<IUserGroup, UserGroupDataAccess>();
            services.AddScoped<IAction, ActionDataAccess>();
            services.AddScoped<IFunction, FunctionDataAccess>();

            // Common
            services.AddScoped<ICommon, CommonDataAccess>();
            services.AddScoped<ICommonFunction, CommonFunction>();
            services.AddScoped<ILoggingHelpers, LoggingHelpers>();
            services.AddScoped<IEmailSender, EmailSender>();
            services.AddScoped<IOTPTransaction, OtpTransactionDataAccess>();
            services.AddScoped<ILogging, LoggingDataAccess>();
            services.AddScoped<SysTransDataAccess, SysTransDataAccess>();

            // Master Data
            services.AddScoped<IAppVersion, AppVersionDataAccess>();
            services.AddScoped<IBank, BankDataAccess>();
            services.AddScoped<IBanner, BannerDataAccess>();
            services.AddScoped<IBlogCategory, BlogCategoryDataAccess>();
            services.AddScoped<IBlog, BlogDataAccess>();
            services.AddScoped<IOtherList, OtherListDataAccess>();
            services.AddScoped<IOtherListType, OtherListTypeDataAccess>();
            services.AddScoped<IProvince, ProvinceDataAccess>();
            services.AddScoped<IProductGroup, ProductGroupDataAccess>();
            services.AddScoped<IProduct, ProductDataAccess>();
            services.AddScoped<IServiceType, ServiceTypeDataAccess>();
            services.AddScoped<IProductLabel, ProductLabelDataAccess>();
            services.AddScoped<INotificationType, NotificationTypeDataAccess>();
            services.AddScoped<INotification, NotificationDataAccess>();
            services.AddScoped<ICustomerRank, CustomerRankDataAccess>();
            services.AddScoped<IStaticPage, StaticPageDataAccess>();
            services.AddScoped<IAdminNotification, AdminNotificationDataAccess>();
            services.AddScoped<IOffending_Words, Offending_WordsDataAccess>(); 


            // Business
            services.AddScoped<IBankAccount, BankAccountDataAccess>();
            services.AddScoped<ICustomer, CustomerDataAccess>();
            services.AddScoped<IPartner, PartnerDataAccess>();
            services.AddScoped<IPartnerContract, PartnerContractDataAccess>();
            services.AddScoped<IAccumulatePointOrderRating, AccumulatePointOrderRatingDataAccess>();
            services.AddScoped<IAdminChangePointOrder, AdminChangePointDataAccess>();
            services.AddScoped<IAdminPartnerOrder, AdminPartnerOrderDataAccess>();
            services.AddScoped<IAdminAddPointOrder, AdminAddOrderDataAccess>();
            services.AddScoped<IRecallPointOrder, RecallPointOrderDataAccess>();
            services.AddScoped<IAdminAccumulatePointOrder, AdminAccumulateOrderDataAccess>();
            services.AddScoped<IComplainInfo, ComplainInfoDataAccess>();
            services.AddScoped<IAccumulatePointOrderComplain, AccumulatePointOrderComplainDataAccess>();

            // Config
            services.AddScoped<IAccumulatePointConfig, AccumulatePointConfigDataAccess>();
            services.AddScoped<IAffiliateConfig, AffiliateConfigDataAccess>();
            services.AddScoped<IBonusPointConfig, BonusPointConfigDataAccess>();
            services.AddScoped<ISettings, SettingsDataAccess>();
            services.AddScoped<INotiConfig, NotiConfigDataAccess>();
            services.AddScoped<IAppSuggestSearch, AppSuggestSearchDataAccess>();
            services.AddScoped<ISMSHistory, SMSHistoryDatatAccess>();

            // Report
            services.AddScoped<IAdminBaoKimTransaction, AdminBaoKimTransactionDataAccess>();

            // App Customer
            services.AddScoped<IAppAccumulatePointOrder, AppAccumulatePointOrderDataAccess>();
            services.AddScoped<IAppBlog, AppBlogDataAccess>();
            services.AddScoped<IAppChangePointOrder, AppChangePointDataAccess>();
            services.AddScoped<IAppCustomerBankAccount, AppCustomerBankAccountDataAccess>();
            services.AddScoped<IAppCustomerHome, AppCustomerHomeDataAccess>();
            services.AddScoped<IAppNotification, AppNotificationDataAccess>();
            services.AddScoped<IAppPartnerBag, AppPartnerBagDataAccess>();
            services.AddScoped<IAppPartnerOrder, AppPartnerOrderDataAccess>();

            // App partner
            services.AddScoped<IAppPartnerHome, AppPartnerHomeDataAccess>();
            services.AddScoped<IPartnerAccumulatePointOrder, AppPartnerAccumulatePointOrderDataAccess>();
            services.AddScoped<IPartnerAddPointOrder, AppAddPointOrderDataAccess>();
            services.AddScoped<IPartnerUser, AppPartnerUserDataAccess>();
            services.AddScoped<IPartnerOrder, AppPartnerOrder2DataAccess>();
            services.AddScoped<IAppPartnerContract, AppPartnerContractDataAccess>();
            services.AddScoped<IPartnerBankAccount, AppPartnerBankAccountDataAccess>();

            //SMSBrandName
            services.AddTransient<ISendSMSBrandName, SendSMSBrandName>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime, IDistributedCache cache)
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("NjM3NkAzMjMwMkUzNDJFMzBlTytqTTFkMDJ5RDJKMzZGUDdoRFJSWHdhQTdTVXVjOWZlRTkweWRIeEhNPQ==");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // app.UseSwagger();
            // app.UseSwaggerUI(c =>
            // {
            //     c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cashplus API By Digipro");
            // });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            // Enable CORS
            app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
