using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TechNetworkControlApi.DTO;

public class JwtDto
{
    public string? AccessToken { get; set; }
    public string RefreshToken { get; set; }
}

public class JwtHeaderDto
{
    public string? access_token { get; set; }
    public string refresh_token { get; set; }
}

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class FromCookieAttribute : Attribute, IBindingSourceMetadata, IModelNameProvider
{
    public BindingSource BindingSource => CookieBindingSource.Instance;

    public string Name { get; set; }
}

public static class CookieBindingSource
{
    public static readonly BindingSource Instance = new BindingSource(
        "Cookie",
        "Cookie",
        isGreedy: false,
        isFromRequest: true);
}

public static class HeadersCookieBindingSource
{
    public static readonly BindingSource Instance = new BindingSource(
        "Headers",
        "Headers",
        isGreedy: false,
        isFromRequest: true
    );
}

public class CookieValueProviderFactory : IValueProviderFactory
{
    public Task CreateValueProviderAsync(ValueProviderFactoryContext context)
    {
        var cookies = context.ActionContext.HttpContext.Request.Cookies;
        var headers = context.ActionContext.HttpContext.Request.Headers;

        context.ValueProviders.Add(new CookieValueProvider(CookieBindingSource.Instance, cookies));
        context.ValueProviders.Add(new HeadersCookieValueProvider(CookieBindingSource.Instance, headers));
        
        return Task.CompletedTask;
    }
}

public class CookieValueProvider : BindingSourceValueProvider
{
    private IRequestCookieCollection Cookies { get; }
    
    public CookieValueProvider(BindingSource bindingSource, IRequestCookieCollection cookies) : base(bindingSource)
    {
        Cookies = cookies;
    }

    public override bool ContainsPrefix(string prefix)
    {
        return Cookies.ContainsKey(prefix);
    }

    public override ValueProviderResult GetValue(string key)
    {
        return Cookies.TryGetValue(key, out var value) ? new ValueProviderResult(value) : ValueProviderResult.None;
    }
}

public class HeadersCookieValueProvider : BindingSourceValueProvider
{
    private IHeaderDictionary Headers { get; }

    public HeadersCookieValueProvider(BindingSource bindingSource, IHeaderDictionary headers) : base(bindingSource)
    {
        Headers = headers;
    }

    public override bool ContainsPrefix(string prefix)
    {
        return Headers.ContainsKey(prefix);
    }

    public override ValueProviderResult GetValue(string key)
    {
        return Headers.TryGetValue(key, out var value) ? new ValueProviderResult(value) : ValueProviderResult.None;
    }
}