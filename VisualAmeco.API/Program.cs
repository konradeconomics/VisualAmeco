using Application.Interfaces;
using Application.Services;
using Microsoft.EntityFrameworkCore;
using VisualAmeco.Application.Interfaces;
using VisualAmeco.Application.Services;
using VisualAmeco.Core.Interfaces;
using VisualAmeco.Data.Contexts;
using VisualAmeco.Data.Repositories;
using VisualAmeco.Parser.Parsers;
using VisualAmeco.Parser.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<VisualAmecoDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddScoped<IIndicatorService, IndicatorService>();
builder.Services.AddScoped<ILookupService, LookupService>();

builder.Services.AddScoped<IChapterRepository, ChapterRepository>();
builder.Services.AddScoped<ISubchapterRepository, SubchapterRepository>();
builder.Services.AddScoped<IVariableRepository, VariableRepository>();
builder.Services.AddScoped<ICountryRepository, CountryRepository>();
builder.Services.AddScoped<IValueRepository, ValueRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IAmecoCsvParser, AmecoCsvParser>();
builder.Services.AddScoped<IAmecoEntitySaver, AmecoEntitySaver>();

builder.Services.AddScoped<ICsvRowMapper, CsvRowMapper>();
builder.Services.AddScoped<ICsvFileReader, CsvFileReader>();
builder.Services.AddScoped<ICsvHeaderValidator, CsvHeaderValidator>();


var app = builder.Build();

// Middleware setup
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers(); // <- Use your [ApiController] controllers here

app.Run();
