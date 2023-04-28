using System.Globalization;

namespace FytSoa.Exam;

public static class SiteUtils
{
    public static string Sale(int first, int two)
    {
        if (first==0 || two==0)
        {
            return "100%";
        }
        return first>two ? (1-Convert.ToDouble(two)/Convert.ToDouble(first)).ToString("p") : 
            (1-Convert.ToDouble(first)/Convert.ToDouble(two)).ToString("p");
    }
}