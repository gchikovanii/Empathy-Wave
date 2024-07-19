using System.Globalization;
using System.Text.RegularExpressions;

namespace EmphatyWave.ApiService.Infrastructure.Middlewares
{
    public class CultureConfigurationMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;
        
        public async Task Invoke(HttpContext context)
        {
            var cultureName = "en-US";
            var queryCulture = context.Request.Headers["Accept-Language"].ToString();
            if (!string.IsNullOrWhiteSpace(queryCulture))
                cultureName = queryCulture.Split(',')[0];
            try
            {
                var culture = new CultureInfo(cultureName);
                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = culture;
            }
            catch (CultureNotFoundException)
            {
                var fallBackCulture = new CultureInfo("en-US");
                CultureInfo.CurrentCulture = fallBackCulture;
                CultureInfo.CurrentUICulture = fallBackCulture;
            }
            await _next.Invoke(context).ConfigureAwait(false);
        }
    }
}
