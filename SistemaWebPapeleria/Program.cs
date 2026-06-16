using Microsoft.EntityFrameworkCore;
using SistemaWebPapeleria.Data;
using DinkToPdf;
using DinkToPdf.Contracts;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new SistemaWebPapeleria.Filters.SessionAuthFilter()); //aplicar filtros a todo los controladores
    options.Filters.Add(new SistemaWebPapeleria.Filters.GlobalExceptionFilter());   //para capturar errores (manejo de errores)
});
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("CadenaSQL41"));
});

// Registra el convertidor de HTML a PDF (DinkToPdf) como servicio único en la aplicación
builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseSession();
app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Login}/{id?}")
    .WithStaticAssets();


app.Run();
