# Introduction

**SWIMS** (Social Welfare Information Management System) is a web application built for the **Government of the Commonwealth of Dominica** in partnership with the **World Food Programme (WFP)**. It digitises and centralises social welfare case management — covering beneficiary registration, application intake, programme-linked form submissions, a multi-level approval workflow, benefit period tracking, payments export, reporting, and system administration.

## Background

SWIMS was developed on top of the existing Pension Assistance (65+) platform managed by the Ministry of Finance IT unit. Phase 1 produced process documentation and redesigned end-to-end workflows. Phase 2 — the current codebase — delivers the SWIMS v1 platform with integrations to the Virtual Customer Assistance System (VCAS) and SmartStream.

## Technology Stack

| Layer | Technology |
|-------|-----------|
| Framework | ASP.NET Core 9 MVC + Razor Pages |
| Language | C# 13 (.NET 9) |
| ORM | Entity Framework Core 9 |
| Database | SQL Server |
| Authentication | ASP.NET Core Identity + LDAP (Active Directory) |
| Password Hashing | BCrypt (BCrypt.Net-Next) |
| Real-time | SignalR |
| Background Jobs | Hangfire 1.8 (SQL Server storage, `ops` schema) |
| Email | SMTP Profiles or Microsoft Graph (`Mail.Send`) |
| Web Push | Lib.Net.Http.WebPush (VAPID) |
| Workflow Engine | Elsa v3 (external service, HTTP integration) |
| Templating | Handlebars.Net (email templates) |
| Reporting | SQL Server Reporting Services (SSRS), reverse proxy |
| Logging | Serilog (Console + File sinks) |
| UI Theme | WowDash (Bootstrap-based admin theme) |
| Documentation | DocFX v2.78.3 (this site) |

## Current Version

**v1.6.0.0** — delivered with significant scope beyond the original concept note, including the Cases module, Notifications & Messaging, Observability, API Dashboard, Authorization Engine, and PWA/Web Push capabilities.

## Project Key Contacts

- **DSS Database Officer:** Courtney Warrington  
- **MoF Software Architect:** Austin Lazarus  
- **WFP IMS Expert:** Thomas Gabrielle  
- **WFP SP/BA Expert:** Gabrielle Viat  
- **Lead Developer:** Jaimon Orlé

## Licence & Confidentiality

This system and its documentation are confidential to the Government of the Commonwealth of Dominica and the World Food Programme. Not for public distribution.
