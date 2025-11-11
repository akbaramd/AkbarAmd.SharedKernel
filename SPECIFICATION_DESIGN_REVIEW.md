# بررسی کامل پیاده‌سازی Specification Pattern

## 1. درک مسئله (Problem Understanding)

پیاده‌سازی الگوی Specification برای پشتیبانی از:
- فیلتر کردن (Criteria)
- مرتب‌سازی (Sorting)
- Pagination
- Include کردن navigation properties
- ترکیب چندین criteria با AND

**Scope:**
- بررسی انطباق با SOLID
- بررسی استفاده صحیح از Design Patterns
- شناسایی کلاس‌های اضافی
- بررسی نام‌گذاری و ساختار

---

## 2. یافته‌ها (Findings)

### 🔴 مشکل 1: نقض Separation of Concerns - Namespace Violations

**مشکل:**
- `IPaginatedSortableSpecification<T>` در `Contracts.Repositories` قرار دارد
- `SortDirection` enum در `Contracts.Repositories` قرار دارد
- `PaginatedSortableSpecification<T>` در `Domain.Specifications` قرار دارد

**چرا مشکل است:**
- Specification contracts باید در `Contracts.Specifications` باشند، نه `Contracts.Repositories`
- Repository contracts برای repository interfaces هستند، نه specification interfaces
- این باعث coupling غیرضروری و confusion در ساختار می‌شود

**تأثیر:**
- نقض SRP: Repository namespace مسئولیت specification را دارد
- نقض Clean Architecture: لایه‌ها به درستی جدا نشده‌اند

---

### 🔴 مشکل 2: تکرار کد (DRY Violation) - ExpressionCombiner

**مشکل:**
- `ExpressionCombiner` کلاس static است که منطق ترکیب expression را دارد
- `BaseSpecification` هم یک `AndAlso` private method دارد که همان کار را می‌کند
- هر دو `ParameterReplacer` داخلی دارند

**کد تکراری:**
```csharp
// در BaseSpecification
private static Expression<Func<T, bool>> AndAlso(...)
{
    var param = left.Parameters[0];
    var rewrittenRightBody = ParameterReplacer.Replace(...);
    // ...
}

// در ExpressionCombiner
public static Expression<Func<T, bool>> AndAlso(...)
{
    var param = left.Parameters[0];
    var replacer = new ParameterReplacer(...);
    // ...
}
```

**چرا مشکل است:**
- نقض DRY: کد تکراری نگهداری را سخت می‌کند
- اگر باگی در یکی باشد، باید در هر دو جا اصلاح شود
- `ExpressionCombiner` public است اما استفاده نمی‌شود (dead code)

**تأثیر:**
- Maintenance burden
- Potential bugs
- Code smell

---

### 🔴 مشکل 3: نقض Encapsulation - ISpecification Interface

**مشکل:**
```csharp
public interface ISpecification<T>
{
    List<Expression<Func<T, object>>> Includes { get; }
    List<string> IncludeStrings { get; }
}
```

**چرا مشکل است:**
- Interface یک `List<>` mutable را expose می‌کند
- هر کسی که interface را implement کند، باید mutable collection را return کند
- این باعث می‌شود client بتواند collection را modify کند

**راه حل:**
```csharp
IReadOnlyList<Expression<Func<T, object>>> Includes { get; }
IReadOnlyList<string> IncludeStrings { get; }
```

**تأثیر:**
- نقض Encapsulation
- امکان تغییر ناخواسته state
- نقض Immutability principle

---

### 🔴 مشکل 4: طراحی ناسازگار Sorting - دو رویکرد متفاوت

**مشکل:**
- `ISpecification` از `OrderBy` و `OrderByDescending` استفاده می‌کند
- `IPaginatedSortableSpecification` از `SortBy` و `Direction` استفاده می‌کند
- این دو رویکرد متفاوت هستند و confusion ایجاد می‌کنند

**کد:**
```csharp
// در ISpecification
Expression<Func<T, object>>? OrderBy { get; }
Expression<Func<T, object>>? OrderByDescending { get; }

// در IPaginatedSortableSpecification
Expression<Func<T, object>>? SortBy { get; }
SortDirection Direction { get; }
```

**چرا مشکل است:**
- دو API مختلف برای یک کار (sorting)
- `Evaluator` باید هر دو را handle کند (complexity)
- اگر specification هم `OrderBy` و هم `SortBy` داشته باشد، کدام اولویت دارد؟

**تأثیر:**
- نقض Consistency principle
- افزایش Complexity
- Confusion برای developers

---

### 🔴 مشکل 5: NonPagingNoIncludeWrapperSpecification ناقص

**مشکل:**
```csharp
public sealed class NonPagingNoIncludeWrapperSpecification<T> : ISpecification<T>
{
    // فقط ISpecification را implement می‌کند
    // IPaginatedSortableSpecification را handle نمی‌کند
}
```

**چرا مشکل است:**
- اگر specification wrapped شده `IPaginatedSortableSpecification` باشد، `SortBy` و `Direction` از دست می‌روند
- Decorator pattern ناقص implement شده
- فقط `ISpecification` را wrap می‌کند، نه تمام interfaces

**تأثیر:**
- از دست رفتن اطلاعات sorting
- Decorator pattern ناقص
- نقض LSP: نمی‌تواند جایگزین کامل specification اصلی باشد

---

### 🔴 مشکل 6: نقض SRP - BaseSpecification مسئولیت‌های زیاد

**مشکل:**
`BaseSpecification` چندین مسئولیت دارد:
1. مدیریت criteria (جمع‌آوری و ترکیب)
2. مدیریت includes
3. مدیریت sorting
4. مدیریت paging
5. ترکیب expression trees (AndAlso logic)
6. Parameter replacement

**چرا مشکل است:**
- یک کلاس با 6 مسئولیت مختلف
- اگر منطق ترکیب expression تغییر کند، باید `BaseSpecification` را تغییر دهیم
- اگر منطق paging تغییر کند، باید `BaseSpecification` را تغییر دهیم

**تأثیر:**
- نقض SRP
- Hard to test
- Hard to maintain
- Hard to extend

---

### 🔴 مشکل 7: نقض OCP - سخت برای Extend

**مشکل:**
- برای اضافه کردن منطق جدید (مثلاً OR logic برای criteria)، باید `BaseSpecification` را modify کنیم
- برای تغییر نحوه ترکیب criteria، باید base class را تغییر دهیم

**چرا مشکل است:**
- باید base class را برای extension تغییر دهیم
- نقض Open/Closed Principle

**راه حل:**
- استفاده از Strategy pattern برای criteria combination
- استفاده از Builder pattern برای specification construction

---

### 🔴 مشکل 8: نام‌گذاری ضعیف

**مشکل:**
- `NonPagingNoIncludeWrapperSpecification` نام بسیار طولانی است
- `ExpressionCombiner` نام generic است و مشخص نیست برای چه استفاده می‌شود

**پیشنهاد:**
- `NonPagingNoIncludeWrapperSpecification` → `CountOptimizedSpecification` یا `CriteriaOnlySpecification`
- `ExpressionCombiner` → حذف شود (redundant)

---

### 🔴 مشکل 9: Missing Abstractions

**مشکل:**
- هیچ interface برای `ISortSpecification` وجود ندارد
- اگر بخواهیم sorting را مستقل از pagination استفاده کنیم، نمی‌توانیم

**راه حل:**
```csharp
public interface ISortSpecification<T>
{
    Expression<Func<T, object>>? SortBy { get; }
    SortDirection Direction { get; }
}
```

---

### 🔴 مشکل 10: PaginatedSortableSpecification namespace اشتباه

**مشکل:**
- `PaginatedSortableSpecification` در `Domain.Specifications` است
- باید در `Contracts.Specifications` باشد (مثل `PaginatedSpecification`)

**چرا:**
- Consistency با سایر base classes
- Contracts باید در Contracts namespace باشند

---

## 3. پیشنهاد معماری بهبود یافته (Proposed Architecture)

### 3.1 ساختار Namespace

```
Contracts.Specifications/
  ├── ISpecification<T>
  ├── ISortSpecification<T>          [NEW]
  ├── IPaginatedSpecification<T>
  ├── IPaginatedSortableSpecification<T>  [MOVED from Repositories]
  ├── BaseSpecification<T>
  ├── PaginatedSpecification<T>
  ├── PaginatedSortableSpecification<T>   [MOVED from Domain.Specifications]
  ├── CountOptimizedSpecification<T>      [RENAMED from NonPagingNoIncludeWrapperSpecification]
  └── SortDirection enum                   [MOVED from Repositories]
```

### 3.2 Separation of Concerns

**1. Expression Combination Logic:**
- یک `IExpressionCombiner<T>` interface
- یک `ExpressionTreeCombiner<T>` implementation
- `BaseSpecification` از combiner استفاده می‌کند (Dependency Injection)

**2. Specification Building:**
- `ISpecificationBuilder<T>` interface
- `SpecificationBuilder<T>` implementation (Builder Pattern)

**3. Sorting:**
- `ISortSpecification<T>` interface (مستقل از pagination)
- `IPaginatedSortableSpecification<T>` extends both

### 3.3 Design Patterns

**1. Strategy Pattern برای Criteria Combination:**
```csharp
public interface ICriteriaCombinationStrategy<T>
{
    Expression<Func<T, bool>> Combine(
        Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right);
}
```

**2. Decorator Pattern بهبود یافته:**
```csharp
public class CountOptimizedSpecification<T> : ISpecification<T>
{
    // باید تمام interfaces را wrap کند
    // اگر wrapped spec ISortSpecification است، آن را هم expose کند
}
```

**3. Builder Pattern:**
```csharp
public class SpecificationBuilder<T>
{
    public SpecificationBuilder<T> WithCriteria(...);
    public SpecificationBuilder<T> WithSorting(...);
    public SpecificationBuilder<T> WithPaging(...);
    public ISpecification<T> Build();
}
```

---

## 4. کد بهبود یافته (Improved Code)

### 4.1 ISpecification با Encapsulation صحیح

```csharp
namespace MCA.SharedKernel.Domain.Contracts.Specifications;

public interface ISpecification<T>
{
    Expression<Func<T, bool>>? Criteria { get; }
    IReadOnlyList<Expression<Func<T, object>>> Includes { get; }
    IReadOnlyList<string> IncludeStrings { get; }
    Expression<Func<T, object>>? OrderBy { get; }
    Expression<Func<T, object>>? OrderByDescending { get; }
    int Take { get; }
    int Skip { get; }
    bool IsPagingEnabled { get; }
}
```

### 4.2 ISortSpecification جدید

```csharp
namespace MCA.SharedKernel.Domain.Contracts.Specifications;

public enum SortDirection
{
    Ascending = 0,
    Descending = 1
}

public interface ISortSpecification<T>
{
    Expression<Func<T, object>>? SortBy { get; }
    SortDirection Direction { get; }
}
```

### 4.3 IPaginatedSortableSpecification در جای درست

```csharp
namespace MCA.SharedKernel.Domain.Contracts.Specifications;

public interface IPaginatedSortableSpecification<T> 
    : IPaginatedSpecification<T>, ISortSpecification<T>
{
    // فقط ترکیب دو interface است
}
```

### 4.4 Expression Combiner به عنوان Service

```csharp
namespace MCA.SharedKernel.Domain.Contracts.Specifications;

public interface IExpressionCombiner<T>
{
    Expression<Func<T, bool>> AndAlso(
        Expression<Func<T, bool>>? left,
        Expression<Func<T, bool>>? right);
    
    Expression<Func<T, bool>> OrElse(
        Expression<Func<T, bool>>? left,
        Expression<Func<T, bool>>? right);
    
    Expression<Func<T, bool>> Not(Expression<Func<T, bool>> expr);
}
```

### 4.5 BaseSpecification با Dependency Injection

```csharp
namespace MCA.SharedKernel.Domain.Contracts.Specifications;

public abstract class BaseSpecification<T> : ISpecification<T>
{
    private readonly IExpressionCombiner<T> _combiner;
    private readonly List<Expression<Func<T, bool>>> _criteria = new();

    protected BaseSpecification(IExpressionCombiner<T>? combiner = null)
    {
        _combiner = combiner ?? new DefaultExpressionCombiner<T>();
    }

    public Expression<Func<T, bool>>? Criteria
        => _criteria.Count switch
        {
            0 => null,
            1 => _criteria[0],
            _ => _criteria.Aggregate(_combiner.AndAlso)
        };

    public IReadOnlyList<Expression<Func<T, object>>> Includes { get; } 
        = new List<Expression<Func<T, object>>>().AsReadOnly();
    
    // ... rest of implementation
}
```

---

## 5. اعتبارسنجی و تست (Validation & Tests)

### 5.1 Unit Tests مورد نیاز

**1. BaseSpecification Tests:**
- تست ترکیب چند criteria با AND
- تست null safety
- تست encapsulation (Includes نباید mutable باشد)

**2. ExpressionCombiner Tests:**
- تست AndAlso با null inputs
- تست OrElse
- تست Not
- تست parameter replacement

**3. CountOptimizedSpecification Tests:**
- تست که paging حذف می‌شود
- تست که includes حذف می‌شوند
- تست که criteria حفظ می‌شود
- تست که sorting حفظ می‌شود (اگر ISortSpecification باشد)

**4. PaginatedSortableSpecification Tests:**
- تست constructor validation
- تست که paging اعمال می‌شود
- تست که sorting اعمال می‌شود

### 5.2 Integration Tests

- تست با EfCoreSpecificationEvaluator
- تست end-to-end با repository

---

## 6. خلاصه مشکلات (Summary)

### مشکلات Critical (باید فوراً اصلاح شوند):

1. ✅ **Namespace violations** - IPaginatedSortableSpecification و SortDirection در جای اشتباه
2. ✅ **ExpressionCombiner redundancy** - کد تکراری
3. ✅ **ISpecification encapsulation** - mutable collections
4. ✅ **Inconsistent sorting design** - دو رویکرد متفاوت
5. ✅ **NonPagingNoIncludeWrapperSpecification ناقص** - SortBy را handle نمی‌کند

### مشکلات Important (باید اصلاح شوند):

6. ✅ **BaseSpecification SRP violation** - مسئولیت‌های زیاد
7. ✅ **OCP violation** - سخت برای extend
8. ✅ **Missing ISortSpecification** - abstraction مفقود

### مشکلات Minor (بهبود code quality):

9. ✅ **نام‌گذاری ضعیف** - نام‌های طولانی
10. ✅ **PaginatedSortableSpecification namespace** - باید در Contracts باشد

---

## 7. اولویت‌بندی اصلاحات

### Phase 1: Critical Fixes
1. جابجایی `IPaginatedSortableSpecification` و `SortDirection` به `Contracts.Specifications`
2. حذف `ExpressionCombiner` و استفاده از منطق `BaseSpecification`
3. تغییر `ISpecification` به `IReadOnlyList`
4. یکپارچه‌سازی sorting design

### Phase 2: Important Improvements
5. اضافه کردن `ISortSpecification` interface
6. بهبود `CountOptimizedSpecification` برای handle کردن تمام interfaces
7. Refactor `BaseSpecification` با Strategy pattern

### Phase 3: Code Quality
8. Rename `NonPagingNoIncludeWrapperSpecification`
9. جابجایی `PaginatedSortableSpecification` به `Contracts.Specifications`

---

## 8. Trade-offs

### مزایای پیشنهاد:
- ✅ انطباق کامل با SOLID
- ✅ Separation of Concerns بهتر
- ✅ Testability بیشتر
- ✅ Extensibility بیشتر
- ✅ Consistency در design

### معایب:
- ⚠️ Breaking changes برای کد موجود
- ⚠️ نیاز به refactoring در repository implementations
- ⚠️ کمی complexity بیشتر (اما justified)

---

## نتیجه‌گیری

پیاده‌سازی فعلی **عملکردی** است اما **چندین نقض SOLID** دارد و نیاز به **refactoring** دارد. مشکلات اصلی:
1. Namespace violations
2. Code duplication
3. Encapsulation issues
4. Inconsistent design
5. Missing abstractions

با اعمال تغییرات پیشنهادی، کد **production-ready** و **maintainable** خواهد شد.

