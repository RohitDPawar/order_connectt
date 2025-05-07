using Microsoft.EntityFrameworkCore;

namespace AspnetCoreMvcFull.Data
{
  public class AspnetCoreMvcFullContext : DbContext
  {
    public AspnetCoreMvcFullContext(DbContextOptions<AspnetCoreMvcFullContext> options)
        : base(options)
    {
    }
    public DbSet<AspnetCoreMvcFull.Models.Transactions> Transactions { get; set; } = default!;
  }
}
