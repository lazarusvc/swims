// ==================== SwRole.cs ====================
// -------------------------------------------------------------------
// File:    SwRole.cs
// Author:  N/A
// Created: N/A
// Purpose: ASP.NET Core Identity role entity for SWIMS.
// Dependencies:
//   - Microsoft.AspNetCore.Identity.IdentityRole<int>
// -------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;


namespace SWIMS.Models;

/// <summary>
/// Represents a role within the SWIMS application.
/// Inherits from <c>IdentityRole&lt;int&gt;</c> to include standard identity properties.
/// </summary>
public class SwRole : IdentityRole<int>
{
   
}
