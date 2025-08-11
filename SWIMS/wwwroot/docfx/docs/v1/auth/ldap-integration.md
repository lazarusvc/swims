## LDAP and Identity Integration

**Project:** SWIMS

**Module:** Authentication & Authorization

**Date:** 2025-07-14

**Author:** Jaimon Orlé

---

# LDAP and Identity Integration - FINAL

When we first wired up LDAP in SWIMS, everything authenticated—but we ran into a few bumps:

- The Identity cookie was storing the full DN (e.g. `CN=Jane Doe,...`), which blew up trying to parse into an `int`.
- Account‑manage pages (change email/password/2FA) either crashed or let LDAP users edit settings they shouldn’t.

Over several tweaks we ended up with a smooth, unified experience where:

- Users log in with either their short AD name (`jdoe`) or full UPN/email.
- LDAP users get a local `SwUser` record created once, linked under the hood, and signed in with a normal Identity cookie.
- AD group memberships flow into ASP.NET Roles *before* sign‑in, so `[Authorize(Roles="Admin")]` works.
- Pages for email, password, and 2FA are automatically hidden/blocked for LDAP accounts.
- Logging out clears both your app cookie and any external‑login cookie.

---

## 1. Unified Login Form

**Challenge**: We wanted *one* login screen that serves both local Identity users and LDAP users.

**What changed**:

```diff
- [Required]
- [EmailAddress]
- public string Email { get; set; }
+ [Required]
+ [Display(Name = "Username or Email")]
+ public string Email { get; set; }

```

Now your `Login` page accepts:

- A short AD username (like `jdoe`)
- A UPN/email (like `jdoe@orleindustries.com`)
- Or a standard Identity email for local users

That single field covers every case.

---

## 2. Robust LDAP Bind & Lookup

**Goal**: Let users type `jdoe`, `DOMAIN\jdoe`, or `jdoe@example.org`, then reliably find them in AD.

**Key points**:

1. **Normalize input**: if there’s no `@` or `\`, tack on your configured UPN suffix.
2. **Extract `sAMAccountName`** for searching and as our canonical username.
3. **Return null only if no match**, not because of a misplaced `return` that made code unreachable.

**Snippet**:

```csharp
string bind = input.Contains("@") || input.Contains("\\")
    ? input
    : $"{input}@{upnSuffix}";

// Now 'bind' might be 'jdoe', 'DOMAIN\\jdoe', or 'jdoe@domain.com'
var sam = bind.Contains("@")
    ? bind.Split('@')[0]
    : bind.Split('\\').Last();

connection.Bind(new NetworkCredential(bind, password));
var response = (SearchResponse)connection.SendRequest(
    new SearchRequest(baseDn, $"(sAMAccountName={sam})", SearchScope.Subtree, attrs)
);
if (response.Entries.Count == 0) return null;

var entry = response.Entries[0];
return new LdapUser {
  Username          = entry.Attributes["sAMAccountName"][0].ToString(),
  DistinguishedName = entry.DistinguishedName,
  Groups            = entry.Attributes["memberOf"]
                        ?.GetValues(typeof(string)).Cast<string>().ToList()
                        ?? new()
};

```

That fixes the DN‑parsing bug and ensures we always work with the short name we can store locally.

---

## 3. One‑Time Provision & Link in Identity

**Why**: Identity needs an `int` primary key. We don’t want to provision new users on every login, just once.

**Flow**:

1. Try normal Identity login.
2. If it fails, attempt `AuthenticateAsync` against LDAP.
3. When LDAP succeeds:
    - Look up an external login entry `FindByLoginAsync("LDAP", samName)`.
    - If none, see if a local user by `UserName` exists.
    - If still none, create a new `SwUser`:
        
        ```csharp
        new SwUser {
          UserName       = samName,
          Email          = samName.Contains("@")
                             ? samName
                             : $"{samName}@ldap.orleindustries.fake",
          EmailConfirmed = true
        }
        
        ```
        
    - Call `AddLoginAsync(user, new UserLoginInfo("LDAP", samName, "LDAP"));`
    - Finally `SignInAsync(user)`, issuing an Identity cookie with the correct int ID.

That means every LDAP user ends up with a single local account and full access to Identity’s machinery.

---

## 4. Bring AD Groups into ASP.NET Roles

**Requirement**: `[Authorize(Roles = "Admin")]` should work out of the box.

**Trick**: Role claims are baked into the cookie at sign‑in. So sync *before* `SignInAsync`:

```csharp
foreach (var dn in ldapUser.Groups)
{
  var role = Regex.Match(dn, @"CN=([^,]+)").Groups[1].Value;
  if (!await roleManager.RoleExistsAsync(role))
    await roleManager.CreateAsync(new SwRole { Name = role });

  if (!await userManager.IsInRoleAsync(user, role))
    await userManager.AddToRoleAsync(user, role);
}

await signInManager.SignInAsync(user, isPersistent: rememberMe);

```

Now your cookie carries exactly the roles your AD groups dictate.

---

## 5. Lock Down Manage Pages for LDAP Users

Since email, password, and 2FA are managed by AD, we hide or block those sections for LDAP users.

**Helper** (`IdentityExtensions.cs`):

```csharp
public static async Task<bool> IsLdapUserAsync(
    this UserManager<SwUser> um, SwUser u)
    => (await um.GetLoginsAsync(u))
          .Any(l => l.LoginProvider == "LDAP");

```

**In each page model** (e.g. `Email.cshtml.cs`):

```csharp
var user = await userManager.GetUserAsync(User);
if (await userManager.IsLdapUserAsync(user))
  return Forbid();

```

**And in the nav bar** (`_ManageNav.cshtml`):

```
@inject UserManager<SwUser> UserManager
@{
  var u = await UserManager.GetUserAsync(User);
  var hide = await UserManager.IsLdapUserAsync(u);
}
@if (!hide)
{
  <a asp-page="./Email">Email</a>
  <a asp-page="./ChangePassword">Change Password</a>
  <a asp-page="./TwoFactorAuthentication">2FA</a>
}

```

This way, LDAP users neither see nor can navigate to those options.

---

## 6. Complete Sign‑Out

Finally, logging out must clear both cookies:

```csharp
// in Logout.cshtml.cs
await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
await signInManager.SignOutAsync();

```

That prevents any leftover external‑login session from persisting.

---

## Configuration Snippet

```json
"Ldap": {
  "Server":   "ldap.orleindustries.com",
  "Port":      389,
  "UseSsl":   false,
  "BaseDN":   "DC=orleindustries,DC=com",
  "UpnSuffix": "orleindustries.com"
}

```