using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var oauth = builder.Configuration.GetSection("OAuth");
var signingKey = oauth["SigningKey"] ?? throw new InvalidOperationException("OAuth:SigningKey is required.");
var issuer = oauth["Issuer"] ?? throw new InvalidOperationException("OAuth:Issuer is required.");
var audience = oauth["Audience"] ?? throw new InvalidOperationException("OAuth:Audience is required.");

builder.Services.AddControllers();
builder.Services
	.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
	{
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuerSigningKey = true,
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
			ValidateIssuer = true,
			ValidIssuer = issuer,
			ValidateAudience = true,
			ValidAudience = audience,
			ValidateLifetime = true
		};
	});
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();