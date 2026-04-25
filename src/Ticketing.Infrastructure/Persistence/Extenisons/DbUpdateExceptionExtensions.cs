using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Ticketing.Infrastructure.Persistence.Extenisons
{
    public static class DbUpdateExceptionExtensions
    {
        public static bool IsUniqueConstraintViolation(this DbUpdateException ex) =>
                ex.InnerException switch
                {
                    PostgresException pg => pg.SqlState == "23505",
                    _ => false
                };
    }
}
