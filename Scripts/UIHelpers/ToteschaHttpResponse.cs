using System;

#nullable enable
namespace ToteschaMinecraftLauncher;

public class ToteschaHttpResponse<T>
{
    public T? Data {get;set;}
    public string? Error {get;set;}

}
