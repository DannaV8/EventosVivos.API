namespace EventosVivos.Application.Common.Interfaces;
public interface ITokenGenerator
{
    string GenerateToken(string userId, string email, string role);
}
