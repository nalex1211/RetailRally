using Microsoft.EntityFrameworkCore;
using RetailRally.Contexts;
using RetailRally.Interfaces;
using RetailRally.Models;

namespace RetailRally.Repositories;

public class CommentRepository(HubContextClass _db) : ICommentRepository
{
    public async Task<bool> AddCommentAsync(Comment comment)
    {
        await _db.Comments.AddAsync(comment);
        return await SaveChangesAsync();
    }

    public async Task<bool> DeleteCommentAsync(int id)
    {
        var comment = await _db.Comments.FindAsync(id);
        _db.Entry(comment).State = EntityState.Deleted;
        return await SaveChangesAsync();
    }

    public async Task<bool> SaveChangesAsync()
    {
        var saved = await _db.SaveChangesAsync();
        return saved > 0;
    }
}
