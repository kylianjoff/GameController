using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

using System.Text;

var claims = new[]
{
    new Claim(ClaimTypes.NameIdentifier, "8"),
    new Claim(ClaimTypes.Name, "Roger"),
    new Claim(ClaimTypes.Role, "Admin"),
};

SymmetricSecurityKey key = new SymmetricSecurityKey(
    Encoding.UTF8.GetBytes("TheSecretKeyThatShouldBeStoredInTheConfiguration")
);
SigningCredentials credentials = new SigningCredentials(
    key, 
    SecurityAlgorithms.HmacSha256
);


JwtSecurityToken token = new JwtSecurityToken(
    issuer: "localhost:5000", // Qui émet le token ici c'est notre API
    audience: "localhost:5000", // Qui peut utiliser le token ici c'est notre API
    claims: claims, // Les informations sur l'utilisateur
    expires: DateTime.Now.AddMinutes(3000), // Date d'expiration du token
    signingCredentials: credentials // La clé secrète
);

string tokenString = new JwtSecurityTokenHandler().WriteToken(token);
