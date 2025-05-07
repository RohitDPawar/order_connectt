using AspnetCoreMvcFull.Data;
using Microsoft.EntityFrameworkCore;

namespace AspnetCoreMvcFull.Models
{
  public static class SeedData
  {
    public static void Initialize(IServiceProvider serviceProvider)
    {
      using (var context = new AspnetCoreMvcFullContext(
          serviceProvider.GetRequiredService<DbContextOptions<AspnetCoreMvcFullContext>>()))
      {
        if (context == null || context.Transactions == null)
        {
          throw new ArgumentNullException("Null AspnetCoreMvcFullContext");
        }

      }
    }
  }
}
