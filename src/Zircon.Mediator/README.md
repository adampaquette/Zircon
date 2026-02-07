# Zircon.Mediator

Mediator extensions and behaviors including performance metric collection and dependency injection utilities for Zircon applications.

Uses [martinothamar/Mediator](https://github.com/martinothamar/Mediator) (source-generated, MIT license).

## Installation

```bash
dotnet add package Zircon.Mediator
```

## Quick Start

```csharp
using Zircon.Mediator.DependencyInjection;

services.AddZirconMediator(options =>
{
    options.MeterName = "MyApp";
});
```

## Features

### Performance Metric Collection

The `HandlerPerformanceMetricBehaviour` automatically tracks execution time for all Mediator handlers via `System.Diagnostics.Metrics`.

### Configurable Meter Name

Use `PerformanceMetricOptions` to configure the meter name for your application:

```csharp
services.AddZirconMediator(options =>
{
    options.MeterName = "MyApp.Api";
});
```

## License

This project is licensed under the MIT License - see the LICENSE file for details.
