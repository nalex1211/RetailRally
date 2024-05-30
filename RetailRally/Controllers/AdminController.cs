using Microsoft.AspNetCore.Mvc;
using RetailRally.Interfaces;
using RetailRally.Models;

namespace RetailRally.Controllers;
public class AdminController(IAdminRepository _repository) : Controller
{
    public async Task<IActionResult> GoToAllUsersPage()
    {
        var users = await _repository.GetAllUsersAsync();
        return View("AllUsersPage", users);
    }

    [HttpPost]
    [ValidateAntiForgeryToken] 
    public async Task<IActionResult> DeleteUser(string userId)
    {
        await _repository.DeleteUserAsync(userId);
        return RedirectToAction("GoToAllUsersPage");
    }

    public async Task<IActionResult> GoToAllShippingTypesPage()
    {
        var shippingTypes = await _repository.GetAllShippingTypesAsync();
        return View("AllShippingTypesPage", shippingTypes);
    }

    [HttpPost]
    public async Task<IActionResult> AddShippingType([FromBody] ShippingType shippingType)
    {
        if (shippingType.Name == "")
        {
            return Json(new { success = false });
        }

        await _repository.AddShippingTypeAsync(shippingType);

        return Json(new { success = true, id = shippingType.Id, name = shippingType.Name });
    }

    public async Task<IActionResult> GoToAllPaymentTypesPage()
    {
        var paymentTypes = await _repository.GetAllPaymentTypesAsync();
        return View("AllPaymentTypesPage", paymentTypes);
    }

    [HttpPost]
    public async Task<IActionResult> AddPaymentType([FromBody] PaymentType paymentType)
    {
        if (paymentType.Name == "")
        {
            return Json(new { success = false });
        }

        await _repository.AddPaymentTypeAsync(paymentType);

        return Json(new { success = true, id = paymentType.Id, name = paymentType.Name });
    }
}
