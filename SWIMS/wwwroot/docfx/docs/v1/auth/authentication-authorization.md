## **Authentication & Authorization Implementation**

**Project:** SWIMS

**Module:** Authentication & Authorization

**Date:** 2025-07-01

**Author:** Jaimon Orlé

---

## Introduction

This document covers the detailed progress, decisions, and rationale behind the implementation of the Authentication and Authorization system within the project, from inception through to the successful LDAP Authentication commit. The goal of this documentation is to clearly outline the evolution of the authentication strategy, the rationale behind architectural decisions, and ensure future maintainability, scalability, and security.

---

## Initial Project Setup and Rationale

### Original Structure and Intent

Initially, we started with custom-defined models (`SwUsers` and `SwRoles`) to manage user authentication and authorization. The intent was to build a tailored, lightweight identity system customized to our specific business logic.

### Limitations of Original Approach

After further consideration and initial testing, several critical limitations emerged:

- **Security Concerns**: Custom models require extensive additional security checks and validation that are inherently handled by established frameworks.
- **Scalability Issues**: Maintaining custom authentication logic could significantly complicate future scalability.
- **Future-Proofing**: Reliance on custom logic could pose risks for compatibility with future upgrades, new identity providers, or authentication standards.

Due to these identified limitations, the decision was made to pivot toward leveraging a standard and proven identity system.

---

## Transition to ASP.NET Core Identity

### Decision to Use ASP.NET Core Identity

We refactored the project to use ASP.NET Core Identity for the following strategic advantages:

- **Security**: ASP.NET Core Identity provides built-in robust security features (password hashing, token management, secure storage).
- **Scalability and Maintainability**: It offers easier management, seamless integration with external providers, and simplified scaling.
- **Extensibility**: Identity is designed with future integrations in mind, such as OAuth, OpenID Connect, and LDAP.
- **Community and Documentation**: Comprehensive documentation, active community support, and regular updates from Microsoft ensure stability and longevity.

### Refactoring the Models

The custom-defined `SwUsers` and `SwRoles` models were refactored into the standard `IdentityUser` and `IdentityRole` provided by the ASP.NET Core Identity framework. This transition simplified our database schema and aligned the application with recognized industry standards.

**Note:** The only functionality not fully configured as part of this identity integration is the SMTP service for email support (password reset, account confirmation). All other features, including two-factor authentication with QR codes, have been configured and successfully tested.

---

## LDAP Authentication Integration

### Reasoning for LDAP Integration

LDAP (Lightweight Directory Access Protocol) was integrated due to:

- **Centralized User Management**: Leveraging existing centralized user databases.
- **Enhanced Security**: Secure authentication via encrypted LDAP (LDAPS).
- **Enterprise Compatibility**: Support for enterprise environments with standardized directory services.

The requirement for LDAP integration was specifically designed to match integration functionality of other related projects (e.g., VCAS), enabling both regular identity users and LDAP users to utilize a single login form seamlessly.

### Implementation Details

We implemented an `LdapAuthService` as a dedicated authentication handler to abstract LDAP interactions:

- Initial support for LDAPS (Secure LDAP) was successfully tested and integrated over port `636`.
- We enforced the use of LDAPv3 and clearly structured logic for SecureSocketLayer and StartTLS configurations to ensure secure, consistent communication.

### Configuration Management

We structured LDAP settings through clearly documented and environment-specific configuration files:

- **Development Configuration** (`appsettings.Development.json`):

```
{
  "LdapSettings": {
    "Server": "clva-s01.orleindustries.com",
    "Port": 389,
    "UseSsl": false
  }
}
```

This demo configuration points to a local Active Directory environment within the developer's personal development setup, providing an easy-to-configure, non-secure setup ideal for rapid local testing and debugging.

- **Production Configuration Template** (`appsettings.json`):

```
{
  "LdapSettings": {
    "Server": "clva-s01.orleindustries.com",
    "Port": 636,
    "UseSsl": true
  }
}
```

This template guides secure production deployments, ensuring encrypted communication by default.

---

## Known Issues and Considerations

At the time of writing, one known bug has been identified:

### LDAP User Identity Exception

An exception occurs when an LDAP user accesses the Identity Management (Manage) pages, caused by the cookie storing the LDAP distinguished name (DN) as the `NameIdentifier` instead of the expected integer ID.

### Exception details:

```
System.ArgumentException: CN=Lorem Ipsum,OU=SwUsers,DC=orleindustries,DC=com is not a valid value for Int32.
```

### Recommended Solution:

Auto-provision local Identity user records upon successful LDAP authentication. This approach ensures that the cookie uses the appropriate integer-based identifier, resolving the issue and seamlessly integrating LDAP-authenticated users with ASP.NET Identity functionalities.

---

## Commit Reference

The milestone commit capturing successful LDAP authentication implementation:

```
feat(ldap): initial working LDAPS auth via LdapAuthService

- First successful LDAP login over LDAPS (port 636)
- Enforce LDAPv3 and streamline SecureSocketLayer/StartTLS logic
- Updated appsettings.Development.json with demo LDAP config (clva-s01.orleindustries.com:389, UseSsl=false)
- Added blank appsettings.json template for production (clva-s01.orleindustries.com:636, UseSsl=true)
```

---

## Future-Proofing and Scalability

The decisions made throughout this implementation process significantly enhance the project's ability to adapt and grow:

- **Integration Readiness**: Built-in readiness for OAuth/OpenID integrations.
- **Scalable Authentication Strategies**: Easily extendable authentication providers (social logins, multifactor authentication).
- **Security Compliance**: Adherence to industry-standard authentication practices, reducing maintenance overhead.

---

## Conclusion

The detailed journey outlined above ensures clarity on decisions, streamlined collaboration, and comprehensive documentation for sustainable future development. Each choice was deliberately aligned with strategic goals of scalability, security, and long-term maintainability, positioning the project effectively for robust future use cases.