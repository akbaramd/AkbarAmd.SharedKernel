# IBusinessRule vs Specification: Architectural Analysis

## Problem Understanding

Both `IBusinessRule` and `ISpecification<T>` are domain-driven design patterns that encapsulate business logic, but they serve **fundamentally different purposes** and operate at different levels of abstraction.

**Scope:**
- **IBusinessRule**: In-memory validation constraints that enforce domain invariants
- **ISpecification<T>**: Query criteria and reusable business predicates for filtering and selection

**Out of Scope:**
- Application-level validation (DTO validation)
- Infrastructure concerns (database-specific optimizations)
- Cross-cutting concerns (logging, caching)

---

## Findings: Key Architectural Differences

### 1. **Purpose & Intent**

| Aspect | IBusinessRule | ISpecification<T> |
|--------|---------------|-------------------|
| **Primary Purpose** | Enforce domain invariants and constraints | Express query criteria and selection rules |
| **Evaluation Context** | Single entity/value object validation | Collection filtering and query building |
| **Failure Behavior** | Throws exception (fail-fast) | Returns boolean (non-throwing) |
| **Query Translation** | Not applicable (in-memory only) | Converts to `Expression<Func<T, bool>>` for EF Core |

### 2. **Interface Contract**

**IBusinessRule:**
```csharp
namespace AkbarAmd.SharedKernel.Domain.Contracts.BusinessRules;

public interface IBusinessRule
{
    bool IsSatisfied();  // No parameters - evaluates internal state
    string Message { get; }  // Error message for violations
}
```

**BaseBusinessRule:**
```csharp
namespace AkbarAmd.SharedKernel.Domain.BusinessRules;

public abstract class BaseBusinessRule : IBusinessRule
{
    public abstract bool IsSatisfied();
    public abstract string Message { get; }
    public override string ToString() => $"{GetType().Name}: {Message}";
}
```

**ISpecification<T>:**
```csharp
public interface ISpecification<T>
{
    Expression<Func<T, bool>>? Criteria { get; }  // Query expression
    bool IsSatisfiedBy(T candidate);  // Takes candidate as parameter
    Expression<Func<T, bool>>? ToExpression();  // For repository queries
}
```

### 3. **Usage Patterns**

**IBusinessRule** is used for:
- **Invariant enforcement** during entity construction/modification
- **Pre-condition checks** before state transitions
- **Domain validation** that must throw exceptions when violated
- **Single-instance validation** (one entity at a time)

**ISpecification<T>** is used for:
- **Repository queries** (filtering collections)
- **Reusable business predicates** (composable criteria)
- **In-memory validation** (non-throwing checks)
- **Query optimization** (EF Core expression translation)

### 4. **Composability**

- **IBusinessRule**: Not composable by design (each rule is independent)
- **ISpecification<T>**: Highly composable (AND, OR, NOT operations)

### 5. **Exception Handling**

- **IBusinessRule**: Violations throw `DomainBusinessRuleValidationException`
- **ISpecification<T>**: Returns boolean, never throws for validation failures

---

## Proposed Architecture: When to Use Each

### Use IBusinessRule When:

1. **Enforcing Invariants During Construction**
   ```csharp
   public class Order : Entity<Guid>
   {
       private decimal _totalAmount;
       
       public Order(Guid id, decimal totalAmount, Customer customer)
           : base(id)
       {
           // Business rule: Order total must be positive
           CheckRule(new OrderTotalMustBePositiveRule(totalAmount));
           
           // Business rule: Customer must be active
           CheckRule(new CustomerMustBeActiveRule(customer));
           
           _totalAmount = totalAmount;
       }
   }
   
   using AkbarAmd.SharedKernel.Domain.BusinessRules;
   
   public class OrderTotalMustBePositiveRule : BaseBusinessRule
   {
       private readonly decimal _totalAmount;
       
       public OrderTotalMustBePositiveRule(decimal totalAmount)
       {
           _totalAmount = totalAmount;
       }
       
       public override bool IsSatisfied() => _totalAmount > 0;
       public override string Message => "Order total amount must be greater than zero.";
   }
   ```

2. **State Transition Validation**
   ```csharp
   public class Order : Entity<Guid>
   {
       private OrderStatus _status;
       
       public void Cancel()
       {
           // Business rule: Only pending orders can be cancelled
           CheckRule(new OrderCanBeCancelledRule(_status));
           
           _status = OrderStatus.Cancelled;
           AddDomainEvent(new OrderCancelledEvent(Id));
       }
   }
   
   using AkbarAmd.SharedKernel.Domain.BusinessRules;
   
   public class OrderCanBeCancelledRule : BaseBusinessRule
   {
       private readonly OrderStatus _status;
       
       public OrderCanBeCancelledRule(OrderStatus status)
       {
           _status = status;
       }
       
       public override bool IsSatisfied() => _status == OrderStatus.Pending;
       public override string Message => "Only pending orders can be cancelled.";
   }
   ```

3. **Value Object Validation**
   ```csharp
   public class Email : ValueObject
   {
       private readonly string _value;
       
       public Email(string value)
       {
           ValidateNotEmpty(value, nameof(value));
           
           // Business rule: Email must be in valid format
           CheckRule(new EmailFormatRule(value));
           
           _value = value;
       }
       
       protected override IEnumerable<object> GetEqualityComponents()
       {
           yield return _value;
       }
   }
   
   using AkbarAmd.SharedKernel.Domain.BusinessRules;
   
   public class EmailFormatRule : BaseBusinessRule
   {
       private readonly string _email;
       
       public EmailFormatRule(string email)
       {
           _email = email;
       }
       
       public override bool IsSatisfied() => 
           !string.IsNullOrWhiteSpace(_email) && 
           _email.Contains('@') && 
           _email.Contains('.');
       
       public override string Message => "Email address must be in valid format.";
   }
   ```

### Use ISpecification<T> When:

1. **Repository Query Filtering**
   ```csharp
   // Find all active products in a category
   var spec = new ActiveProductsSpecification()
       .And(new ProductsByCategorySpecification("Electronics"));
   
   var products = await _repository.FindAsync(spec);
   ```

2. **Reusable Business Predicates**
   ```csharp
   public class ActiveProductsSpecification : BaseSpecification<Product>
   {
       public ActiveProductsSpecification()
       {
           Where(p => p.IsActive && p.StockQuantity > 0);
       }
   }
   
   // Reuse in multiple contexts
   var activeSpec = new ActiveProductsSpecification();
   var count = await _repository.CountAsync(activeSpec);
   var paginated = await _repository.GetPaginatedAsync(activeSpec, page: 1, size: 10);
   ```

3. **Complex Query Composition**
   ```csharp
   var spec = new FluentSpecificationBuilder<Product>()
       .Where(p => p.IsActive)
       .Where(b => b
           .Group(g => g
               .And(p => p.Category == "Electronics")
               .Or(p => p.Category == "Computers"))
           .And(p => p.Price >= minPrice && p.Price <= maxPrice))
       .Build();
   
   var results = await _repository.FindAsync(spec);
   ```

4. **In-Memory Validation (Non-Throwing)**
   ```csharp
   public class Order
   {
       public bool CanBeCancelled()
       {
           var spec = new OrderCanBeCancelledSpecification();
           return spec.IsSatisfiedBy(this);  // Returns bool, doesn't throw
       }
   }
   
   public class OrderCanBeCancelledSpecification : BaseSpecification<Order>
   {
       public OrderCanBeCancelledSpecification()
       {
           Where(o => o.Status == OrderStatus.Pending)
               .And(o => o.CreatedAt > DateTime.UtcNow.AddDays(-7));
       }
   }
   ```

---

## Code: Complete Examples

### Example 1: IBusinessRule for Invariant Enforcement

```csharp
// Domain Entity
public class BankAccount : Entity<Guid>
{
    private decimal _balance;
    private bool _isClosed;
    
    public BankAccount(Guid id, decimal initialBalance)
        : base(id)
    {
        // Business rule: Initial balance cannot be negative
        CheckRule(new AccountBalanceCannotBeNegativeRule(initialBalance));
        
        _balance = initialBalance;
        _isClosed = false;
    }
    
    public void Withdraw(decimal amount)
    {
        // Business rule: Cannot withdraw from closed account
        CheckRule(new AccountMustBeOpenRule(_isClosed));
        
        // Business rule: Cannot withdraw more than balance
        CheckRule(new WithdrawalAmountCannotExceedBalanceRule(_balance, amount));
        
        _balance -= amount;
        AddDomainEvent(new FundsWithdrawnEvent(Id, amount));
    }
}

// Business Rules
using AkbarAmd.SharedKernel.Domain.BusinessRules;

public class AccountBalanceCannotBeNegativeRule : BaseBusinessRule
{
    private readonly decimal _balance;
    
    public AccountBalanceCannotBeNegativeRule(decimal balance)
    {
        _balance = balance;
    }
    
    public override bool IsSatisfied() => _balance >= 0;
    public override string Message => "Account balance cannot be negative.";
}

public class AccountMustBeOpenRule : BaseBusinessRule
{
    private readonly bool _isClosed;
    
    public AccountMustBeOpenRule(bool isClosed)
    {
        _isClosed = isClosed;
    }
    
    public override bool IsSatisfied() => !_isClosed;
    public override string Message => "Cannot perform operation on a closed account.";
}

public class WithdrawalAmountCannotExceedBalanceRule : BaseBusinessRule
{
    private readonly decimal _balance;
    private readonly decimal _amount;
    
    public WithdrawalAmountCannotExceedBalanceRule(decimal balance, decimal amount)
    {
        _balance = balance;
        _amount = amount;
    }
    
    public override bool IsSatisfied() => _amount <= _balance;
    public override string Message => $"Withdrawal amount {_amount} cannot exceed balance {_balance}.";
}
```

### Example 2: ISpecification<T> for Query Building

```csharp
// Specifications
public class ActiveBankAccountsSpecification : BaseSpecification<BankAccount>
{
    public ActiveBankAccountsSpecification()
    {
        Where(a => !a.IsClosed && a.Balance > 0);
    }
}

public class BankAccountsWithLowBalanceSpecification : BaseSpecification<BankAccount>
{
    private readonly decimal _threshold;
    
    public BankAccountsWithLowBalanceSpecification(decimal threshold)
    {
        _threshold = threshold;
        Where(a => a.Balance < threshold);
    }
}

// Repository Usage
public class BankAccountService
{
    private readonly IRepository<BankAccount> _repository;
    
    public BankAccountService(IRepository<BankAccount> repository)
    {
        _repository = repository;
    }
    
    public async Task<IEnumerable<BankAccount>> GetActiveAccountsAsync()
    {
        var spec = new ActiveBankAccountsSpecification();
        return await _repository.FindAsync(spec);
    }
    
    public async Task<int> CountLowBalanceAccountsAsync(decimal threshold)
    {
        var spec = new BankAccountsWithLowBalanceSpecification(threshold);
        return await _repository.CountAsync(spec);
    }
    
    public async Task<PaginatedResult<BankAccount>> GetActiveAccountsPaginatedAsync(
        int page, int size)
    {
        var spec = new ActiveBankAccountsSpecification();
        return await _repository.GetPaginatedAsync(spec, page, size);
    }
}
```

### Example 3: Hybrid Approach (Both Patterns)

```csharp
public class Order : Entity<Guid>
{
    private OrderStatus _status;
    private decimal _totalAmount;
    private DateTime _createdAt;
    
    // IBusinessRule: Invariant enforcement
    public Order(Guid id, decimal totalAmount)
        : base(id)
    {
        CheckRule(new OrderTotalMustBePositiveRule(totalAmount));
        
        _totalAmount = totalAmount;
        _status = OrderStatus.Pending;
        _createdAt = DateTime.UtcNow;
    }
    
    // IBusinessRule: State transition validation
    public void Cancel()
    {
        CheckRule(new OrderCanBeCancelledRule(_status));
        _status = OrderStatus.Cancelled;
    }
    
    // ISpecification: In-memory validation (non-throwing)
    public bool CanBeCancelled()
    {
        var spec = new OrderCanBeCancelledSpecification();
        return spec.IsSatisfiedBy(this);
    }
}

// IBusinessRule implementation
using AkbarAmd.SharedKernel.Domain.BusinessRules;

public class OrderCanBeCancelledRule : BaseBusinessRule
{
    private readonly OrderStatus _status;
    
    public OrderCanBeCancelledRule(OrderStatus status)
    {
        _status = status;
    }
    
    public override bool IsSatisfied() => _status == OrderStatus.Pending;
    public override string Message => "Only pending orders can be cancelled.";
}

// ISpecification implementation (for queries)
public class OrderCanBeCancelledSpecification : BaseSpecification<Order>
{
    public OrderCanBeCancelledSpecification()
    {
        Where(o => o.Status == OrderStatus.Pending)
            .And(o => o.CreatedAt > DateTime.UtcNow.AddDays(-7));
    }
}

// Usage in Application Layer
public class OrderService
{
    private readonly IRepository<Order> _repository;
    
    public async Task<IEnumerable<Order>> GetCancellableOrdersAsync()
    {
        var spec = new OrderCanBeCancelledSpecification();
        return await _repository.FindAsync(spec);
    }
}
```

---

## Validation & Best Practices

### SOLID Compliance

**IBusinessRule:**
- ✅ **SRP**: Each rule encapsulates a single business constraint
- ✅ **OCP**: New rules can be added without modifying entities (via `BaseBusinessRule`)
- ✅ **DIP**: Entities depend on `IBusinessRule` abstraction
- ✅ **Namespace**: `AkbarAmd.SharedKernel.Domain.Contracts.BusinessRules`
- ✅ **Base Class**: `BaseBusinessRule` in `AkbarAmd.SharedKernel.Domain.BusinessRules`
- ✅ **Namespace**: `AkbarAmd.SharedKernel.Domain.Contracts.BusinessRules`
- ✅ **Base Class**: `BaseBusinessRule` in `AkbarAmd.SharedKernel.Domain.BusinessRules`

**ISpecification<T>:**
- ✅ **SRP**: Each specification encapsulates a single query concern
- ✅ **OCP**: Specifications are composable and extensible
- ✅ **DIP**: Repositories depend on `ISpecification<T>` abstraction

### Testing Strategy

**IBusinessRule Tests:**
```csharp
using AkbarAmd.SharedKernel.Domain.BusinessRules;

[Fact]
public void OrderTotalMustBePositiveRule_WhenNegative_IsNotSatisfied()
{
    // Arrange
    var rule = new OrderTotalMustBePositiveRule(-100m);
    
    // Act
    var result = rule.IsSatisfied();
    
    // Assert
    Assert.False(result);
    Assert.Equal("Order total amount must be greater than zero.", rule.Message);
    Assert.Contains("OrderTotalMustBePositiveRule", rule.ToString());
}
```

**ISpecification<T> Tests:**
```csharp
[Fact]
public void ActiveProductsSpecification_WhenProductIsActive_IsSatisfied()
{
    // Arrange
    var spec = new ActiveProductsSpecification();
    var product = new Product { IsActive = true, StockQuantity = 10 };
    
    // Act
    var result = spec.IsSatisfiedBy(product);
    
    // Assert
    Assert.True(result);
}
```

### Decision Matrix

| Scenario | Use IBusinessRule | Use ISpecification<T> |
|----------|------------------|----------------------|
| Entity construction validation | ✅ | ❌ |
| State transition validation | ✅ | ❌ |
| Value object validation | ✅ | ❌ |
| Repository query filtering | ❌ | ✅ |
| Collection filtering | ❌ | ✅ |
| Reusable query predicates | ❌ | ✅ |
| In-memory non-throwing checks | ❌ | ✅ |
| Composable criteria (AND/OR/NOT) | ❌ | ✅ |
| EF Core expression translation | ❌ | ✅ |

---

## Notes & Trade-offs

### IBusinessRule Advantages
- **Fail-fast**: Immediate exception on violation
- **Clear error messages**: Built-in `Message` property
- **Simple contract**: Easy to implement and test
- **Invariant enforcement**: Perfect for domain constraints

### IBusinessRule Limitations
- **Not composable**: Cannot combine rules with AND/OR/NOT
- **In-memory only**: Cannot be translated to database queries
- **Exception-based**: Always throws, no graceful handling

### ISpecification<T> Advantages
- **Highly composable**: AND, OR, NOT operations
- **Query-ready**: Translates to EF Core expressions
- **Reusable**: Can be used across multiple contexts
- **Non-throwing**: Returns boolean for flexible handling

### ISpecification<T> Limitations
- **No built-in error messages**: Must handle validation messages separately
- **More complex**: Requires understanding of expression trees
- **Not for invariants**: Should not replace IBusinessRule for critical constraints

### Anti-Patterns to Avoid

1. **Using ISpecification<T> for Invariants**
   ```csharp
   // ❌ BAD: Using specification for invariant enforcement
   public Order(decimal totalAmount)
   {
       var spec = new OrderTotalMustBePositiveSpecification();
       if (!spec.IsSatisfiedBy(new { TotalAmount = totalAmount }))
           throw new Exception("Invalid order");
   }
   
   // ✅ GOOD: Using business rule
   public Order(decimal totalAmount)
   {
       CheckRule(new OrderTotalMustBePositiveRule(totalAmount));
   }
   ```

2. **Using IBusinessRule for Queries**
   ```csharp
   // ❌ BAD: Trying to use business rule for querying
   var rule = new ActiveProductsRule();
   var products = _repository.FindAsync(rule);  // Won't work!
   
   // ✅ GOOD: Using specification
   var spec = new ActiveProductsSpecification();
   var products = await _repository.FindAsync(spec);
   ```

3. **Mixing Concerns**
   ```csharp
   // ❌ BAD: Business rule that queries database
   public class OrderCanBeCancelledRule : IBusinessRule
   {
       private readonly IRepository<Order> _repository;  // Infrastructure dependency!
       
       public bool IsSatisfied()
       {
           // This violates DIP and makes rule untestable
           var existingOrders = _repository.FindAsync(...);
           // ...
       }
   }
   ```

---

## Summary

**IBusinessRule** and **ISpecification<T>** are complementary patterns that serve different purposes:

- **IBusinessRule**: Enforce domain invariants with fail-fast exceptions
- **ISpecification<T>**: Express query criteria and reusable predicates

Use **IBusinessRule** when you need to **prevent invalid state** (construction, state transitions).  
Use **ISpecification<T>** when you need to **filter and query** collections.

Both patterns follow SOLID principles and support clean architecture by keeping business logic in the domain layer, separated from infrastructure concerns.

## Namespace Reference

### Business Rules
- **Interface**: `AkbarAmd.SharedKernel.Domain.Contracts.BusinessRules.IBusinessRule`
- **Base Class**: `AkbarAmd.SharedKernel.Domain.BusinessRules.BaseBusinessRule`
- **Exception**: `AkbarAmd.SharedKernel.Domain.Exceptions.DomainBusinessRuleValidationException`

### Specifications
- **Interface**: `AkbarAmd.SharedKernel.Domain.Contracts.Specifications.ISpecification<T>`
- **Base Class**: `AkbarAmd.SharedKernel.Domain.Contracts.Specifications.BaseSpecification<T>`
- **Builder**: `AkbarAmd.SharedKernel.Domain.Specifications.FluentSpecificationBuilder<T>`

### Usage in Entities
Both `Entity` and `ValueObject` base classes provide the `CheckRule(IBusinessRule rule)` method for validating business rules:

```csharp
using AkbarAmd.SharedKernel.Domain;
using AkbarAmd.SharedKernel.Domain.BusinessRules;

public class Order : Entity<Guid>
{
    public void Cancel()
    {
        CheckRule(new OrderCanBeCancelledRule(Status));
        // ... cancellation logic
    }
}
```

