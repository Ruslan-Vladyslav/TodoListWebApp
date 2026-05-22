using Microsoft.EntityFrameworkCore;
using TodoListApp.Services.Database.Entities;
using TodoListApp.Services.Database.Entity;

namespace TodoListApp.Services.Database
{
    public class TodoListDbContext : DbContext
    {
        public TodoListDbContext(DbContextOptions<TodoListDbContext> options)
            : base(options)
        {
        }

        public DbSet<TodoListEntity> TodoLists => this.Set<TodoListEntity>();

        public DbSet<TodoTaskEntity> TodoTasks => this.Set<TodoTaskEntity>();

        public DbSet<TodoTagEntity> TodoTags => this.Set<TodoTagEntity>();

        public DbSet<TodoCommentEntity> TodoComments => this.Set<TodoCommentEntity>();

        public DbSet<TodoListAccessEntity> TodoListAccesses => this.Set<TodoListAccessEntity>();

        public DbSet<TodoInvitationEntity> TodoInvitations => this.Set<TodoInvitationEntity>();

        public DbSet<NotificationEntity> Notifications => this.Set<NotificationEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            _ = modelBuilder?.Entity<TodoListAccessEntity>()
                .HasIndex(x => new { x.TodoListId, x.TargetUserId })
                .IsUnique();

            _ = modelBuilder?.Entity<TodoInvitationEntity>()
                .HasIndex(x => new { x.TodoListId, x.ReceiverUserId });

            _ = modelBuilder?.Entity<TodoTaskEntity>()
                .HasOne(t => t.TodoList)
                .WithMany(l => l.TodoTasks)
                .HasForeignKey(t => t.TodoListId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
