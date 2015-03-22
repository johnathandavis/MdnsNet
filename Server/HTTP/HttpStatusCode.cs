using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchClassLib.HTTP
{
    public enum HttpStatusCode
    {
        _200OK,
        _301TempRedirect,
        _302PermRedirect,
        _400BadRequest,
        _403Forbidden,
        _404NotFound,
        _500ServerError,
        _501NotImplemented,
        _503ServiceUnavailable,
    }
}
