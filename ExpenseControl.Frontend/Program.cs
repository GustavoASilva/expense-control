using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ExpenseControl.Frontend;
using ExpenseControl.Frontend.Services;
using Refit;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddRefitClient<IApiService>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("http://localhost:5293"));

builder.Services.AddScoped<ApiService>();

await builder.Build().RunAsync();