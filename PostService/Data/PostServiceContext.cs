using Microsoft.EntityFrameworkCore;

namespace PostService.Data;

public class PostServiceContext : DbContext
{
    public PostServiceContext(DbContextOptions<PostServiceContext> options)
        : base(options)
    {
    }

    public DbSet<Entities.Post> Post { get; set; }
    public DbSet<Entities.User> User { get; set; }
}
