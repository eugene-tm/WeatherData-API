




using ClimateDataAPI.Repository;
using ClimateDataAPI.Services;
using ClimateDataAPI.Settings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
	// path.combine allows the program to work on any OS
	c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "ClimateDataAPI.xml"));
});



// This line extracts the config data from the appsettings folder and stores it in a config object
builder.Services.Configure<DefaultMongoConnection>(builder.Configuration.GetSection("DefaultMongoConnection"));
// A request creates a copy of this connection service and reuses it for similar requests
builder.Services.AddScoped<MongoConnection>();

builder.Services.AddScoped<IClimateRepository, MongoClimateRepository>();
builder.Services.AddScoped<IUserRepository, MongoUserRepository>();


builder.Services.AddCors(c => c.AddPolicy("GooglePolicy", d =>
{
    d.WithOrigins("https://www.google.com.au", "https://www.google.com");
    d.AllowAnyHeader();
    d.WithMethods("GET", "PUT", "POST", "PATCH", "DELETE");
}));



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseCors();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
