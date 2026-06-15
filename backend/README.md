# Barber Shop API

Production-oriented multi-tenant Barber Shop Management System built with ASP.NET Core Web API, Entity Framework Core and PostgreSQL.

The platform allows multiple barber shops to operate on the same application instance while maintaining strict tenant isolation and ownership boundaries.

---

# Features

## Authentication & Authorization

### Authentication

* JWT Authentication
* Refresh Tokens
* Refresh Token Rotation
* User Registration
* User Login
* User Logout
* Current User Endpoint (`/me`)
* Forgot Password
* Reset Password
* Change Password

### Authorization

* Role-Based Authorization
* Multi-Role Support
* CLIENT
* EMPLOYEE
* OWNER

### Security Features

* Refresh Token Revocation
* Password Reset Tokens
* Password Reset Email Flow
* Progressive Account Lockout
* Built-in Rate Limiting
* Security Audit Logging
* Centralized Exception Handling
* Production Configuration Separation

---

# Multi-Tenant SaaS Architecture

The application is designed as a multi-tenant SaaS platform.

Multiple barber shops can use the same backend and database while remaining completely isolated from one another.

Each shop has its own:

* Employees
* Services
* Service Categories
* Appointments
* Reviews
* Inventory
* Notifications
* Working Hours
* Time Off Requests

Tenant isolation is enforced across all modules.

---

# Appointment Management

## Appointment Statuses

* PENDING
* CONFIRMED
* COMPLETED
* CANCELLED
* NO_SHOW

## Status Flow

```text
PENDING
 ├──> CONFIRMED
 └──> CANCELLED

CONFIRMED
 ├──> COMPLETED
 ├──> CANCELLED
 └──> NO_SHOW
```

## Features

* Appointment Booking
* Available Slots Calculation
* Appointment Search
* Appointment Details
* Status Management
* Cancellation Rules
* Ownership Validation
* Employee Assignment
* Snapshot Pricing
* Snapshot Duration
* Booking Spam Protection
* Tenant Isolation

---

# Client Portal

## Dashboard

* Next Appointment
* Popular Services
* Notification Summary

## Appointments

* Upcoming Appointments
* Completed Appointments
* Cancelled Appointments

## Reviews

* My Reviews
* Pending Reviews

## Favorites

* Add Favorite Service
* Remove Favorite Service
* List Favorite Services

## Profile

* Profile Update
* Change Password

---

# Public Shop API

Public endpoints allow customers to browse shop information before authentication.

Available features:

* Shop Information
* Service Categories
* Services
* Employees
* Popular Services
* Available Appointment Slots

Public API supports explicit shop context for multi-tenant frontends.

---

# Employee Portal

## Dashboard

* Today's Appointments
* Schedule Summary
* Assigned Appointments
* Time Off Summary

## Appointments

Employees can:

* View their own appointments
* Confirm appointments
* Complete appointments
* Mark appointments as NO_SHOW
* Cancel their own appointments

Employees cannot:

* Access other employees' appointments
* Modify appointments belonging to another employee

## Time Off Workflow

Employees can:

* Create Time Off Requests
* View Their Own Requests

Statuses:

* PENDING
* APPROVED
* REJECTED

---

# Owner Portal

## Dashboard

* Today's Appointments
* Employee Summary
* Revenue Overview
* Low Stock Overview
* Review Overview

## Management

Owners can manage:

* Employees
* Services
* Working Hours
* Time Off Requests
* Inventory
* Reviews
* Shop Settings

## Analytics

* Occupancy Rate
* Returning Client Rate
* Most Popular Service
* Revenue Overview

---

# Time Off Approval Workflow

Production-ready request/approval workflow.

Employee:

```text
Create Request
      ↓
    PENDING
```

Owner:

```text
PENDING
 ├──> APPROVED
 └──> REJECTED
```

Rules:

* Only APPROVED requests block booking
* PENDING requests do not block booking
* APPROVED and REJECTED are terminal states
* Audit information is stored for reviews

Audit fields:

* ReviewedByUserId
* ReviewedAt
* ReviewNote

---

# Reviews

Rules:

* One review per appointment
* Rating range: 1–5
* Comment length up to 1000 characters
* Reviews allowed only for COMPLETED appointments

---

# Inventory Management

Owner-only module.

Features:

* Inventory CRUD
* Low Stock Endpoint
* Unique Item Name Per Shop

---

# Notifications

## In-App Notifications

### Client

* Appointment Booked
* Appointment Confirmed
* Appointment Cancelled
* Appointment Completed
* Appointment No Show

### Employee

* Appointment Booked
* Appointment Cancelled

### Owner

* Appointment Booked
* Appointment Cancelled

---

# Email Notifications

Implemented using:

* MailKit
* MimeKit
* SMTP

Emails are currently sent for:

* Password Reset
* Appointment Booking
* Appointment Confirmation
* Appointment Cancellation

The email subsystem is designed as a best-effort service and never blocks core business flows.

---

# Architecture

The application uses a generic service architecture designed for maintainability and scalability.

## Generic Components

* BaseSearchObject
* PagedResult
* IBaseService
* IBaseCRUDService
* BaseService
* BaseCRUDService
* BaseController
* BaseCRUDController

---

# Design Patterns

## Template Method Pattern

Used in:

* BaseService
* BaseCRUDService

## Facade Pattern

Used in:

* AppointmentBookingFacade

## Strategy Pattern

Used in:

* Notification Delivery

## Factory Pattern

Used in:

* Notification Creation

---

# Security

## Authentication

* JWT Bearer Authentication
* Refresh Tokens
* Refresh Token Rotation

## Protection Mechanisms

* Ownership Validation
* Tenant Isolation
* Password Reset Tokens
* Refresh Token Revocation
* Rate Limiting
* Progressive Account Lockout
* Security Audit Logging

---

# Exception Handling

Centralized exception handling with:

* Standardized Error Responses
* Trace Identifiers
* Secure 500 Responses
* Structured Logging

---

# Health Checks

Endpoint:

```http
GET /health
```

Checks:

* API Health
* PostgreSQL Connectivity

Example:

```json
{
  "status": "Healthy",
  "checks": {
    "api": "Healthy",
    "database": "Healthy"
  }
}
```

---

# API Documentation

Swagger/OpenAPI is available in Development mode.

Features:

* JWT Bearer Integration
* Authorize Button Support
* Interactive API Testing

---

# Configuration

Environment-specific configuration:

* appsettings.json
* appsettings.Development.json
* appsettings.Production.json

Sensitive values should be provided through environment variables.

Examples:

```text
ConnectionStrings__DefaultConnection
Jwt__Key
EmailSettings__Host
EmailSettings__Username
EmailSettings__Password
```

---

# Technology Stack

## Backend

* ASP.NET Core Web API
* Entity Framework Core
* PostgreSQL

## Security

* JWT Authentication
* Refresh Tokens

## Email

* MailKit
* MimeKit

## Documentation

* Swagger / OpenAPI

---

# Running Locally

## Requirements

* .NET
* PostgreSQL
* Docker (optional)

## Database

Apply migrations:

```bash
dotnet ef database update
```

Run API:

```bash
dotnet run
```

Swagger:

```text
https://localhost:{PORT}/swagger
```

Health Check:

```text
https://localhost:{PORT}/health
```

---

# Project Status

Current status:

Production-oriented multi-tenant backend with completed:

* Authentication
* Authorization
* Appointment Management
* Client Portal API
* Employee Portal API
* Owner Portal API
* Public Shop API
* Inventory Management
* Reviews
* Notifications
* Time Off Approval Workflow
* Tenant Isolation
* Security Hardening

The project is ready for frontend integration and end-to-end testing.
