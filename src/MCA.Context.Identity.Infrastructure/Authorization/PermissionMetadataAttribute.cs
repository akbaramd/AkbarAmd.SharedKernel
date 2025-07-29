namespace CleanArchitecture.Infrastructure.Authorization;

/// <summary>
///  فرا‌متادیتا برای نمایـش در UI و ساخت درخت دسته‌بندی.
///  هر فیلد <see cref="Permission"/> باید دقیقاً یکی از این اتریبیوت‌ها داشته باشد.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public sealed class PermissionMetadataAttribute(
    string categoryPathKey,          // ex: "cat.finance/cat.invoice"
    string titleKey,                 // ex: "perm.invoice.read"
    string? icon = null,
    int order = 0) : Attribute
{
    public string CategoryPathKey { get; } = categoryPathKey;
    public string TitleKey        { get; } = titleKey;
    public string? Icon           { get; } = icon;
    public int Order              { get; } = order;
}


