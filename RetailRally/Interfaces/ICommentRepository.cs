using RetailRally.Models;

namespace RetailRally.Interfaces;

public interface ICommentRepository
{
    Task<bool> AddCommentAsync(Comment comment);
    Task<bool> DeleteCommentAsync(int id);
    Task<bool> SaveChangesAsync();
}
