# Principles

## Table of Contents
- [SOLID Principles](#solid-principles)
  - [1. Single Responsibility Principle (SRP)](#1-single-responsibility-principle-srp)
  - [2. Open/Closed Principle (OCP)](#2-openclosed-principle-ocp)
  - [3. Liskov Substitution Principle (LSP)](#3-liskov-substitution-principle-lsp)
  - [4. Interface Segregation Principle (ISP)](#4-interface-segregation-principle-isp)
  - [5. Dependency Inversion Principle (DIP)](#5-dependency-inversion-principle-dip)
## SOLID Principles

SOLID is a set of five object-oriented design principles, popularized by Robert C. Martin ("Uncle Bob"). The goal is code that is easier to maintain, test, and extend without breaking existing behavior.

| Letter | Principle | One-line idea |
|---|---|---|
| **S** | Single Responsibility | A class should have one reason to change |
| **O** | Open/Closed | Open for extension, closed for modification |
| **L** | Liskov Substitution | Subtypes must be usable wherever their base type is |
| **I** | Interface Segregation | Prefer small, focused interfaces |
| **D** | Dependency Inversion | Depend on abstractions, not concrete classes |

## 1. Single Responsibility Principle (SRP)

A class should do one job. If it mixes unrelated concerns, a change to one forces a change to the other.

**Violation:**
```csharp
public class InvoiceService
{
    public void Calculate(Invoice invoice) { /* business logic */ }
    public void SaveToDatabase(Invoice invoice) { /* persistence */ }
    public void SendEmail(Invoice invoice) { /* notification */ }
}
```

**Better:** split by responsibility.
```csharp
public class InvoiceCalculator { public decimal Calculate(Invoice i) => /* ... */ 0; }
public class InvoiceRepository { public void Save(Invoice i) { /* ... */ } }
public class InvoiceNotifier   { public void Send(Invoice i) { /* ... */ } }
```

## 2. Open/Closed Principle (OCP)

Add new behavior by adding new code, not by editing tested code.

**Violation:** every new shape means editing the `if` chain.
```csharp
public decimal Area(object shape)
{
    if (shape is Circle c) return Math.PI * c.Radius * c.Radius;
    if (shape is Rectangle r) return r.Width * r.Height;
    throw new NotSupportedException();
}
```

**Better:** new shapes extend the system without touching existing code.
```csharp
public interface IShape { double Area(); }

public class Circle : IShape
{
    public double Radius { get; init; }
    public double Area() => Math.PI * Radius * Radius;
}

public class Rectangle : IShape
{
    public double Width { get; init; }
    public double Height { get; init; }
    public double Area() => Width * Height;
}
```

## 3. Liskov Substitution Principle (LSP)

If `B` inherits from `A`, you should be able to use a `B` anywhere an `A` is expected without surprises.

**Classic violation:** a `Square` that inherits from `Rectangle`.
```csharp
public class Rectangle
{
    public virtual int Width { get; set; }
    public virtual int Height { get; set; }
}

public class Square : Rectangle
{
    public override int Width  { set { base.Width = base.Height = value; } }
    public override int Height { set { base.Width = base.Height = value; } }
}

// Code expecting a Rectangle breaks:
rect.Width = 5; rect.Height = 10;
// rect.Width * rect.Height should be 50, but a Square gives 100
```

**Fix:** don't force an inheritance relationship that doesn't behave correctly. Use a shared interface (like `IShape`) instead. Also watch for subclasses that throw `NotImplementedException` or strengthen preconditions.

## 4. Interface Segregation Principle (ISP)

Don't force classes to implement methods they don't use.

**Violation:**
```csharp
public interface IWorker
{
    void Work();
    void Eat();
}

public class Robot : IWorker
{
    public void Work() { }
    public void Eat() => throw new NotSupportedException(); // meaningless
}
```

**Better:**
```csharp
public interface IWorkable { void Work(); }
public interface IFeedable { void Eat(); }

public class Human : IWorkable, IFeedable { /* ... */ }
public class Robot : IWorkable { /* ... */ }
```

## 5. Dependency Inversion Principle (DIP)

High-level code shouldn't depend on low-level details. Both should depend on abstractions, and dependencies should be passed in.

**Violation:**
```csharp
public class OrderService
{
    private readonly SqlOrderRepository _repo = new SqlOrderRepository(); // hard-wired
}
```

**Better:**
```csharp
public interface IOrderRepository { void Save(Order order); }

public class OrderService
{
    private readonly IOrderRepository _repo;
    public OrderService(IOrderRepository repo) => _repo = repo;
}

// Registered in ASP.NET Core:
builder.Services.AddScoped<IOrderRepository, SqlOrderRepository>();
```

This is what dependency injection in .NET is built around, and it's also what makes unit testing with mocks (or WireMock, as in your earlier examples) practical.

## Why they matter

- **Easier testing**: small classes with injected dependencies are simple to isolate
- **Safer changes**: modifying one thing is less likely to break another
- **Better readability**: each piece has a clear purpose
- **Flexibility**: swap implementations (database, email provider, API client) without rewriting logic

## Common pitfalls

- **Over-engineering.** Don't create an interface for every class or split everything into tiny pieces "just in case." Apply the principles when you feel real pain, like a class that keeps changing for different reasons.
- **Treating them as laws.** They're guidelines, and a small, simple app may not need all of them.
- **Confusing DIP with DI.** Dependency *Inversion* is the principle (depend on abstractions); Dependency *Injection* is one technique for achieving it.

## Quick memory aid

- **S**: one class, one job
- **O**: extend, don't edit
- **L**: subclasses must behave like their parent
- **I**: small interfaces
- **D**: depend on interfaces, inject them

Want me to walk through refactoring a single messy class step by step to apply all five?