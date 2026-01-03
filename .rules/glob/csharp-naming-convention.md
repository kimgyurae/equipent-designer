
# C# Naming Conventions

## Overview

This guide outlines the most widely adopted naming conventions in C# development, based on Microsoft's official guidelines and industry best practices.

## General Rules

- Use meaningful and descriptive names
- Avoid abbreviations except for well-known ones (e.g., `Id`, `Xml`, `Html`)
- Do not use Hungarian notation or type prefixes
- Avoid underscores, hyphens, or other non-alphanumeric characters (except for private fields)
- Use English language for all identifiers

## Technical Naming Rules

These are compiler-enforced or compiler-reserved naming constraints:

- **Identifier Characters**: Must start with a letter or underscore (`_`), followed by Unicode letters, digits, connecting characters, combining characters, or formatting characters
- **Avoid Consecutive Underscores**: Never use consecutive underscores (e.g., `__value`, `my__field`) - these are reserved for compiler-generated identifiers
- **Keyword Matching**: Use the `@` prefix when an identifier needs to match a C# keyword (e.g., `@if`, `@class`, `@event`)

```csharp
// ✅ Valid identifiers
private int _value;
var @class = "Biology";  // Using keyword as identifier

// ❌ Invalid - consecutive underscores
private int __value;
private string my__field;
```

## Casing Styles

### PascalCase
First letter of each word is capitalized.
```csharp
CustomerAccount, ProductInventory
```

### camelCase
First letter is lowercase, subsequent words are capitalized.
```csharp
customerAccount, productInventory
```

### _camelCase (with underscore prefix)
Used for private fields.
```csharp
_customerAccount, _productInventory
```

## Naming by Type

### Classes and Structs
**Convention:** PascalCase

```csharp
public class CustomerAccount { }
public struct Point { }
```

### Attributes
**Convention:** PascalCase with `Attribute` suffix

```csharp
[AttributeUsage(AttributeTargets.Class)]
public class ValidationAttribute : Attribute 
{
    // Implementation
}

[AttributeUsage(AttributeTargets.Property)]
public class RequiredAttribute : Attribute { }

// Usage (Attribute suffix is optional in usage)
[Validation]
[Required]
public class Customer { }
```

### Interfaces
**Convention:** PascalCase with `I` prefix

```csharp
public interface ICustomerRepository { }
public interface IPaymentProcessor { }
```

### Methods
**Convention:** PascalCase, use verbs or verb phrases

```csharp
public void CalculateTotal() { }
public string GetCustomerName() { }
public bool IsValid() { }
```

### Properties
**Convention:** PascalCase

```csharp
public string FirstName { get; set; }
public int TotalAmount { get; set; }
public bool IsActive { get; set; }
```

### Fields

#### Public/Protected Fields
**Convention:** PascalCase (though public fields are generally discouraged)

```csharp
public string CustomerName;
protected int MaxRetries;
```

#### Private Fields
**Convention:** _camelCase (with underscore prefix)

```csharp
private string _customerName;
private int _maxRetries;
```

#### Private Protected Fields
**Convention:** _camelCase (with underscore prefix) - follows private field convention

**Important:** `private protected` is a compound access modifier introduced in C# 7.2 that combines the restrictions of both `private` and `protected`. It is accessible only within derived classes AND within the same assembly, making it MORE restrictive than `protected` alone. For naming purposes, `private protected` fields follow the same convention as `private` fields.

```csharp
private protected ProcessBase _parentProcess;
private protected string _internalState;
```

#### Static Fields
**Convention:** Prefix with `s_` for static fields, `t_` for thread-static fields

```csharp
// Static fields
private static int s_instanceCount;
private static string s_defaultName;

// Thread-static fields
[ThreadStatic]
private static string t_currentContext;
[ThreadStatic]
private static int t_requestId;
```

#### Constants
**Convention:** PascalCase

**Important:** All constants use PascalCase, regardless of whether they are class-level fields or local constants within methods.

```csharp
// Class-level constants
public const int MaximumLoginAttempts = 3;
private const string DefaultCurrency = "USD";
protected const double Pi = 3.14159;

// Local constants also use PascalCase
public void ProcessOrder(int orderId, string customerName)
{
    const int MaxRetries = 3;
    const string DefaultStatus = "Pending";
    const decimal TaxRate = 0.08m;
}
```

### Local Variables and Parameters
**Convention:** camelCase

```csharp
public void ProcessOrder(int orderId, string customerName)
{
    int totalItems = 0;
    decimal totalPrice = 0m;
}
```

### Enums
**Convention:** PascalCase for enum type and values (use singular names unless bit flags)

```csharp
public enum OrderStatus
{
    Pending,
    Processing,
    Shipped,
    Delivered
}

// Bit flags use plural names
[Flags]
public enum FilePermissions
{
    Read = 1,
    Write = 2,
    Execute = 4
}
```

### Namespaces
**Convention:** PascalCase, use meaningful hierarchy with reverse domain notation

Use reverse domain notation to organize namespaces hierarchically, starting from your organization/company name down to specific features.

```csharp
namespace CompanyName.ProductName.Feature
{
    // Code here
}

// Example
namespace Contoso.Shopping.Cart
{
    // Code here
}
```

### Events
**Convention:** PascalCase, use verbs or verb phrases

```csharp
public event EventHandler OrderProcessed;
public event EventHandler<CustomerEventArgs> CustomerRegistered;
```

### Delegates
**Convention:** PascalCase, suffix with `EventHandler` for events, `Callback` for callbacks

```csharp
public delegate void OrderProcessedEventHandler(object sender, EventArgs e);
public delegate bool ValidationCallback(string input);
```

### Generic Type Parameters
**Convention:** PascalCase with `T` prefix for single parameters

```csharp
public class Repository<T> where T : class { }
public class Dictionary<TKey, TValue> { }
```

### Async Methods
**Convention:** PascalCase with `Async` suffix

```csharp
public async Task<Customer> GetCustomerAsync(int id) { }
public async Task ProcessOrderAsync(Order order) { }
```

## Acronyms and Abbreviations

### Two-letter Acronyms
Use all uppercase in PascalCase contexts:
```csharp
public class IOHelper { }
public string UIComponent { get; set; }
```

### Three+ letter Acronyms
Only capitalize the first letter:
```csharp
public class HtmlParser { }
public class XmlDocument { }
public string HttpClient { get; set; }
```

## Special Cases

### Boolean Properties and Methods
Prefix with `Is`, `Has`, `Can`, or `Should`:
```csharp
public bool IsValid { get; set; }
public bool HasPermission { get; set; }
public bool CanExecute() { }
public bool ShouldRetry() { }
```

### Collection Properties
Use plural nouns:
```csharp
public List<Customer> Customers { get; set; }
public IEnumerable<Order> Orders { get; set; }
```

### Private Methods
**Convention:** PascalCase (same as public methods)

```csharp
private void ValidateInput() { }
private string FormatAddress() { }
```

### Primary Constructors (C# 12+)
**Convention:** camelCase for class/struct parameters, PascalCase for record parameters

```csharp
// Class primary constructor - camelCase
public class Person(string firstName, string lastName)
{
    public string FullName => $"{firstName} {lastName}";
}

// Struct primary constructor - camelCase
public struct Point(int x, int y)
{
    public int X { get; } = x;
    public int Y { get; } = y;
}

// Record primary constructor - PascalCase
public record Customer(string FirstName, string LastName);
public record Product(string Name, decimal Price);
```

## File Organization

- One primary type per file
- File name should match the type name: `CustomerAccount.cs`
- Partial classes can use descriptive suffixes: `CustomerAccount.Designer.cs`

## Best Practices Summary

1. **Be Consistent** - Follow the same convention throughout your codebase
2. **Be Descriptive** - Names should clearly indicate purpose
3. **Avoid Single Letters** - Except for loop counters (`i`, `j`, `k`) and generic type parameters (`T`)
4. **Use Positive Names** - Prefer `IsEnabled` over `IsDisabled`
5. **Keep It Concise** - Balance between descriptive and overly verbose
6. **Follow Framework Guidelines** - Match the style of the .NET framework

## References

- Microsoft's official [.NET naming guidelines](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/naming-guidelines)
- C# Coding Conventions from Microsoft documentation