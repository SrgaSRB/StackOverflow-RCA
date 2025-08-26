using StackOverflow.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Dodajte Azure Storage service
var connectionString = builder.Configuration.GetConnectionString("AzureStorage");
builder.Services.AddSingleton(new UserService(connectionString!));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    //app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Test Azure Storage connection
try 
{
    using var scope = app.Services.CreateScope();
    var userService = scope.ServiceProvider.GetRequiredService<UserService>();
    
    // Pročitajte korisnika sa RowKey "user1"
    var user = await userService.GetUserAsync("user1");
    if (user != null)
    {
        Console.WriteLine($"✅ Korisnik pronađen: {user.FullName}, Email: {user.Email}");
    }
    else
    {
        Console.WriteLine("⚠️ Korisnik sa RowKey 'user1' nije pronađen");
    }
    
    // Ili sve korisnike
    var allUsers = await userService.GetAllUsersAsync();
    Console.WriteLine($"📊 Ukupno korisnika u bazi: {allUsers.Count}");
    
    foreach (var u in allUsers)
    {
        Console.WriteLine($"   - {u.FullName} ({u.Email})");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Greška pri povezivanju sa Azure Storage: {ex.Message}");
}

app.Run();