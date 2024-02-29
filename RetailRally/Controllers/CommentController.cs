using Microsoft.AspNetCore.Mvc;
using RetailRally.Interfaces;
using RetailRally.Models;
using RetailRally.ViewModels;

namespace RetailRally.Controllers;
public class CommentController(ICommentRepository _repository) : Controller
{
    [HttpPost]
    public async Task<IActionResult> AddComment(ProductVm comment)
    {
        await _repository.AddCommentAsync(comment.NewComment);
        return RedirectToAction("GoToDetailsPage", "Product", new { productId = comment.Product.Id });
    }


    [HttpPost]
    public async Task<IActionResult> DeleteComment(int commentId, int productId)
    {
        await _repository.DeleteCommentAsync(commentId);
        return RedirectToAction("GoToDetailsPage", "Product", new { productId = productId });
    }
}
