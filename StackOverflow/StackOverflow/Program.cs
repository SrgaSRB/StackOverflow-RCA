using StackOverflow.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Dodaj CORS podršku
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Dodajte Azure Storage service
var connectionString = builder.Configuration.GetConnectionString("AzureStorage");
builder.Services.AddSingleton(new VoteService(connectionString!));
builder.Services.AddSingleton(new UserService(connectionString!));
builder.Services.AddSingleton(provider => 
{
    var voteService = provider.GetRequiredService<VoteService>();
    return new QuestionService(connectionString!, voteService);
});
builder.Services.AddSingleton(provider => 
{
    var userService = provider.GetRequiredService<UserService>();
    var voteService = provider.GetRequiredService<VoteService>();
    return new CommentService(connectionString!, userService, voteService);
});

var app = builder.Build();

// Aktiviraj CORS
app.UseCors();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();