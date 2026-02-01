namespace AkbarAmd.SharedKernel.Application.Models.ServiceResult;


public readonly struct Unit
{
    public static readonly Unit Value = default;
    public override string ToString() => "()";
}