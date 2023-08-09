using Dapper;
using dapper_crud.Models;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("DefaultConnection") ??
                                throw new ApplicationException("Incorrect connection strings");
    return new SqlConnectionFactory(connectionString);
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
var group = app.MapGroup("products");

group.MapPost("products", async (Product product, SqlConnectionFactory sqlConnectionFactory) =>
{
    using var connection = sqlConnectionFactory.GetConnection();
    const string sql = """
    INSERT INTO Products (Name,Price,Tags)
    VALUES (@Name,@Price,@Tags)
    """;
    await connection.ExecuteAsync(sql, product);

    return Results.Ok();
});

group.MapGet("products", async (SqlConnectionFactory sqlConnectionFactory) =>
{
    using var connection = sqlConnectionFactory.GetConnection();
    const string sql = "SELECT * FROM Products";

    var products = await connection.QueryAsync<Product>(sql);

    return Results.Ok(products);
});

group.MapGet("products/{id}", async (int id, SqlConnectionFactory sqlConnectionFactory) =>
{
    using var connection = sqlConnectionFactory.GetConnection();
    const string sql = """SELECT * FROM Products WHERE Id=@ProductId""";

    var product = await connection.QuerySingleOrDefaultAsync<Product>(sql, new { ProductId = id });

    return product is not null ? Results.Ok(product) : Results.NotFound();
});

group.MapPut("products/{id}", async (int id, Product product, SqlConnectionFactory sqlConnectionFactory) =>
{
    using var connection = sqlConnectionFactory.GetConnection();
    product.Id = id;
    const string sql = """
    UPDATE Products 
    SET Name = @Name, 
        Price = @Price, 
        Tags = @Tags
    WHERE Id = @Id
    """;

    await connection.ExecuteAsync(sql, product);
    return Results.NoContent();
});

group.MapDelete("products/{id}", async (int id, SqlConnectionFactory sqlConnectionFactory) =>
{
    using var connection = sqlConnectionFactory.GetConnection();
    const string sql = "DELETE FROM Products WHERE Id = @ProductId";
    await connection.ExecuteAsync(sql, new { ProductId = id });
    return Results.NoContent();
});

app.Run();
