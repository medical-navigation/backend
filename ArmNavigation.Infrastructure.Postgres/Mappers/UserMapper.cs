using ArmNavigation.Domain.Models;
using ArmNavigation.Infrastructure.Postgres.Repositories;

namespace ArmNavigation.Infrastructure.Postgres.Mappers
{
    public static class UserMapper
    {
        public static User MapToUser(User row) => new()
        {
            UserId = row.UserId,
            Login = row.Login,
            PasswordHash = row.PasswordHash,
            MedInstitutionId = row.MedInstitutionId,
            Role = row.Role,
            IsRemoved = row.IsRemoved
        };
    }
}
