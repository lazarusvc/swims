# Social Welfare Information Management System (SWIMS)

## Overview
SWIMS is a digital case management and payments platform designed to streamline the intake, registration, assessment, validation, and ongoing case management of social welfare applicants in Dominica. Building on the existing Pension Assistance (65+) platform and interoperating with VCAS and Smart-Stream, SWIMS delivers shock-responsive social protection, automated workflows, and robust reporting to ensure timely and accurate support for the island’s most vulnerable populations.

---

## Background
Dominica’s Social Welfare Services Department (DSS) currently relies on largely paper-based processes and standalone digital tools for Public Assistance, Emergency Grants, Medical Support, Education Aid, and other ad hoc supports. Following extensive process re-engineering in 2024—including end-to-end mapping, pain-point analysis, digitized intake/registration forms, and draft SOPs—the Government of Dominica, supported by WFP, has embarked on Phase 2: full development of SWIMS.  

Key drivers include:
- **Improved Deduplication:** Ensure each beneficiary is uniquely identified and avoid duplicate applications.  
- **Workflow Automation:** Streamline approvals, status changes, and payment preparation.  
- **Reporting & Monitoring:** Real-time dashboards, automated report generation, and performance analytics.  
- **Interoperability:** Seamless integration with existing systems (VCAS, Smart-Stream) for payments and reconciliation.  
- **Shock Response:** Rapid activation of emergency modules following disasters (e.g., hurricanes, fires).  
- **Security & Access Control:** Role-based permissions to safeguard sensitive beneficiary data.

---

## Objectives
- **Digitalize Core Processes:** From intake → recommendation → validation → payments.  
- **Enable Case Management:** Track beneficiary journeys, referrals, reassessments, and exit strategies.  
- **Automate Reporting:** Generate payroll listings, financial upload files, and analytical reports.  
- **Facilitate Inter-Agency Coordination:** Data sharing across ministries, Village Councils, and WFP.  
- **Design for Scalability:** Modular architecture to onboard new programs (e-wallet, grievance redress, additional DSS units).

---

## Features
- **Applicant Intake & Registration**  
- **Automated Approval Workflows**  
- **Payment Preparation & Reconciliation**  
- **Dashboard & Reporting Module**  
- **Role-Based Access Control**  
- **Emergency Response Module**  
- **Interoperability Adapters (VCAS, Smart-Stream)**  
- **Audit Logging & Data Quality Controls**

---

## Architecture
1. **Frontend:** ASP.NET Core MVC / Razor Pages  
2. **Backend:** C# (.NET 8/9), Entity Framework Core  
3. **Database:** SQL Server (hosted on (Government) Ministry of Finance IT Unit)  
4. **Integration Layers:**  
   - VCAS bulk upload adapter  
   - Smart-Stream payment file generator  
5. **Hosting:** Secured on-premises IIS server (Ministry of Finance)  
6. **Digital Forms:** KoboToolbox integration for offline data capture

---

## Tech Stack
- **Language:** C# (.NET 8/9)  
- **Framework:** ASP.NET Core MVC / Razor Pages  
- **ORM:** Entity Framework Core  
- **UI:** Metro UI 5, Bootstrap, jQuery  
- **Forms:** KoboToolbox  
- **Database:** SQL Server  
- **CI/CD:** GitHub Actions → IIS Deployment  
- **Monitoring:** Application Insights / Custom Dashboards

---