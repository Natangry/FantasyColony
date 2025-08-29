public enum JobType
{
    None = 0,
    Builder = 1,
}

public static class JobTypeExt
{
    public static string Name(this JobType t) => t.ToString();
}
