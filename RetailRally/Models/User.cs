﻿using Microsoft.AspNetCore.Identity;

namespace RetailRally.Models;

public class User : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? PictureUrl { get; set; }
    public Cart? Cart { get; set; }
    public List<Product>? Products { get; set; } = new List<Product>();
    public List<Comment>? Comments { get; set; } = new List<Comment>();
}
