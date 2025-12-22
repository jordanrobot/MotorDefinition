---
title: "ADR-0009: Logging and Error Handling Policy"
status: "Accepted"
date: "2025-12-10"
authors: "mjordan"
tags: ["architecture", "logging", "error-handling", "serilog"]
supersedes: ""
superseded_by: ""
---

## Status

Accepted

## Context

The Curve Editor uses Serilog for structured logging, with sinks configured for file and console output. Phase 1.7 of the roadmap defined goals for logging and exception handling:

- Centralized logging with context (file paths, operations, etc.).
- Global exception handling with user-friendly error dialogs.
- Logs written to a known location under the user's AppData folder.

As the application grows, we need a concise policy so new code paths log consistently and handle errors in a user-appropriate way.

## Decision

We adopt the following logging and error-handling policy:

1. **Serilog as the single logging mechanism**
- All application logging uses Serilog; ad-hoc `Console.WriteLine` or other logging frameworks are not used.
- Loggers are obtained via dependency injection or static accessors configured at startup.

2. **Structured logging with contextual properties**
- Log events include contextual properties such as:
  - `FilePath` for file operations.
  - `MotorName`, `DriveName`, `Voltage` where relevant.
  - `Operation` (e.g., `"LoadMotor"`, `"SaveMotor"`, `"EditPoint"`).
- This enables filtering and correlation in logs without relying solely on message text.

3. **Global exception handling**
- A top-level exception handler captures unhandled exceptions, logs them at `Error` level with context, and shows a user-friendly dialog.
- The dialog should:
  - Indicate that an unexpected error occurred.
  - Offer to open the logs folder or copy error details if feasible.
  - Avoid exposing raw stack traces by default.

4. **Error handling at feature boundaries**
- Service methods (file I/O, schema validation, curve generation) catch expected exceptions, log them appropriately, and surface clear error messages to the UI.
- Business logic should not swallow exceptions silently.

## Consequences

### Positive

- **POS-001**: Provides consistent, structured diagnostic information for debugging and support.
- **POS-002**: Ensures users see clear, friendly error messages instead of application crashes or silent failures.
- **POS-003**: Simplifies future observability improvements (e.g., additional sinks, telemetry) by standardizing on Serilog.

### Negative

- **NEG-001**: Requires discipline to include meaningful context in log events; minimal messages are less useful.
- **NEG-002**: Over-logging can create noisy logs; contributors must choose log levels carefully.

## Alternatives Considered

### ALT-001: Minimal logging

- **ALT-001**: **Description**: Only log high-level events or unhandled exceptions, with limited context.
- **ALT-002**: **Rejection Reason**: Makes troubleshooting difficult, especially for complex undo/redo and file operations.

### ALT-003: Mixed logging frameworks

- **ALT-003**: **Description**: Allow different subsystems to use different logging abstractions.
- **ALT-004**: **Rejection Reason**: Complicates configuration and makes correlating events across components harder.

## Implementation Notes

- **IMP-001**: Ensure Serilog is configured during application startup with file and console sinks, and that the log file location is documented in the UI (e.g., via a "Open Logs Folder" menu item).
- **IMP-002**: When adding new operations, include contextual properties in log events and choose appropriate log levels (Debug/Information/Warning/Error).
- **IMP-003**: Use try/catch at service boundaries to turn exceptions into logged events and user-facing messages, not as a replacement for validation.

## References

- **REF-001**: `src/MotorEditor.Avalonia/Program.cs` and logging configuration.
- **REF-002**: `src/MotorEditor.Avalonia/Services` – file and curve generation services using Serilog.
- **REF-003**: Roadmap Phase 1.7 – Logging and Exception Handling.
