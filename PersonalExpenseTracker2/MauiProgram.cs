using Microsoft.Extensions.Logging;
using PersonalExpenseTracker2.service;
using MudBlazor.Services;
using PersonalExpenseTracker2.model;

namespace PersonalExpenseTracker2
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();
            

#if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
            //Use Userservice as a singleton
			builder.Services.AddSingleton<UserService>();
            //Add store to use as 
            builder.Services.AddScoped<Store>();
			builder.Logging.AddDebug();

            //Adding external library mudblazor service to our main program
			builder.Services.AddMudServices();
#endif

			return builder.Build();
        }
    }
}
