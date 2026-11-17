using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Constants;

public static class AppDefinition
{
    public const string RateLimitPolicy = "auth";
    /// <summary> 10 MB </summary>
    public const long MaxFileSize = 10 * 1024 * 1024; 
}