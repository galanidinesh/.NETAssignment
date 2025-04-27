## Overview

This project simulates integrating a .NET Core service with an external system by fetching user data from the [ReqRes API](https://reqres.in/).  
It demonstrates **clean architecture principles**, **error handling**, **resilience (retry policies)**, **caching**, and **testability** — all focused on building a robust and production-ready service component.

---

## Table of Contents

- [Project Structure](#project-structure)
- [Core Requirements Implementation](#core-requirements-implementation)
- [Setup Instructions](#setup-instructions)
- [Building and Running](#building-and-running)
- [Unit Testing](#unit-testing)
- [Design Decisions](#design-decisions)
- [Bonus Features](#bonus-features)
- [Potential Improvements](#potential-improvements)

---

## Project Structure

- `ReqResApiClient` (Class Library)
  - Contains the core API client, service layer, models, configuration, and exceptions.
- `ReqResApiClient.Tests` (Unit Test Project)
  - Contains unit tests for service and client logic using mock objects.
- `ReqResApiClient.ConsoleDemo` (Console Application)
  - Demonstrates how to use the class library in a real application.

---

## Core Requirements Implementation

**1. API Client Implementation**
- `ReqResApiClient` uses `HttpClient` injected via `IHttpClientFactory`.
- Supports:
  - `GET /users?page={pageNumber}`
  - `GET /users/{userId}`
- Fully async/await based implementation.

**2. Data Modeling & Mapping**
- Models like `User`, `UserResponse`, and `UserListResponse` represent the API response.
- Mapped correctly using `Newtonsoft.Json`.

**3. Service Layer**
- `ExternalUserService`:
  - `Task<User> GetUserByIdAsync(int userId)`
  - `Task<IEnumerable<User>> GetAllUsersAsync()` (handles pagination internally)

**4. Configuration**
- Base URL and API key are configured using `appsettings.json` and `Options Pattern`.

**5. Error Handling & Basic Resilience**
- Graceful handling of:
  - API failures (timeouts, network issues)
  - HTTP errors (e.g., 404 Not Found throws `NotFoundException`)
  - JSON deserialization errors
- Specific custom exceptions like `ApiException` and `NotFoundException` are thrown.

**6. Retry Logic**
- Implemented using **Polly** for transient network errors with exponential backoff.
- Policies are configured during `HttpClient` registration.

---

## Setup Instructions

1. Clone the repository:
    ```bash
    git clone <your-repo-url>
    cd ReqResApiClient
    ```

2. Ensure you have `.NET 8.0 SDK` installed.

3. Restore dependencies:
    ```bash
    dotnet restore
    ```

4. Update configuration if needed:
    Edit `appsettings.json` (located in ConsoleDemo project) for API settings:
    ```json
    {
        "ApiSettings": {
            "BaseUrl": "https://reqres.in/api/",
            "ApiKey": "reqres-free-v1"
        },
        "RetryPolicySettings": {
            "RetryCount": 5,
            "RetryDelayMilliseconds": 600
        },
        "CacheSettings": {
            "ExpirationMinutes": 5
        }
    }
    ```

---

## Building and Running

To build the solution:
    ```bash
    dotnet build
    ```

---

## Unit Testing

Run all tests using the following command:
    ```bash
    dotnet test ReqResApiClient.Tests
    ```
Unit tests cover:
    Successful API data fetching
    API failures and correct exception handling
    Caching behavior
    Retry mechanism validation

## Design Decisions

- **Separation of Concerns**  
  API client, service, models, and configuration are split clearly into their respective layers for better maintainability.

- **Resilience**  
  Retry logic using Polly is implemented to handle transient network failures. Custom exception handling is added for different failure scenarios.

- **Testability**  
  The service and client logic are designed to be easily mockable by following interface-driven development (`IReqResApiClient`, `IExternalUserService`).

- **Async Everywhere**  
  All API interactions and service methods are fully asynchronous, leveraging `async/await` properly for non-blocking IO.

- **In-Memory Caching**  
  User list responses are cached using `IMemoryCache` with a 5-minute sliding expiration policy to reduce unnecessary API calls.

- **Options Pattern**  
  API configuration (like BaseUrl and API Key) is managed using strongly-typed settings classes via `IOptions<ApiSettings>`.


## Bonus Features

**Caching**  
- Implemented using `IMemoryCache`.  
- Configurable expiration (default set to 5 minutes).

**Polly Retry Logic**  
- Applied to `HttpClient` with exponential backoff strategy for transient failures.

**Advanced Configuration**  
- Used the **Options Pattern** (`IOptions<T>`) for loading API settings (e.g., BaseUrl, API Key).

**Clean Architecture Principles**  
- Proper separation of concerns:
  - **Clients** for direct API communication
  - **Services** for business logic
  - **Models** for data structure
  - **Exceptions** for specialized error handling
