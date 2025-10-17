// -------------------------------------------------------------------
// File:    SwUser.cs
// Author:  N/A
// Created: N/A
// Purpose: ASP.NET Core Identity user entity with extended profile data for SWIMS.
// Dependencies:
//   - Microsoft.AspNetCore.Identity.IdentityUser<int>
//   - System.ComponentModel.DataAnnotations.Schema
// -------------------------------------------------------------------

using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;


namespace SWIMS.Models;

/// <summary>
/// Represents a user within the SWIMS application.
/// Extends <c>IdentityUser&lt;int&gt;</c> to include profile fields.
/// </summary>
public class SwUser : IdentityUser<int>
{
    /// <summary>
    /// Gets or sets the user's first name.
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Gets or sets the user's last name.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Gets the user's full name by combining first and last names.
    /// </summary>
    [NotMapped]
    public string FullName => $"{FirstName} {LastName}".Trim();

}
