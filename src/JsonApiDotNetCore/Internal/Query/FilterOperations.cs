// ReSharper disable InconsistentNaming
namespace JsonApiDotNetCore.Internal.Query
{
    public enum FilterOperations
    {
        eq = 0,
        lt = 1,
        gt = 2,
        le = 3,
        ge = 4,
        like = 5,
        ne = 6,
        @in = 7, // prefix with @ to use keyword
        nin = 8,
        isnull = 9,
        isnotnull = 10,
        all = 11,
        exclude = 12,
        sw = 13,//StartWith,
        ew = 14,//EndWith
    }
}
