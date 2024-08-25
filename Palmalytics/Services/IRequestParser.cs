using Microsoft.AspNetCore.Http;
using Palmalytics.Model;

namespace Palmalytics.Services
{
    public interface IRequestParser
    {
        RequestData Parse(HttpRequest request);

        string ParseLanguage(string acceptLanguageHeader);
    }
}