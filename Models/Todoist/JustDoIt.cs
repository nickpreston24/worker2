using CodeMechanic.Types;

namespace worker2;

public class JustDoIt
{
    public bool Enabled => Env.Enabled("RUN_SCRAPER_JUSTDOIT");
    public EnvVar[] Env { get; set; } = Array.Empty<EnvVar>();
}

public static class EnvVarExts
{
    public static bool Enabled(
        this IEnumerable<EnvVar> settings
        , string name
    ) => !settings.IsNullOrEmpty()
         && settings.Any(setting =>
             setting.EnvironmentVarName.Equals(
                 name,
                 StringComparison
                     .InvariantCultureIgnoreCase));
}

public class EnvVar
{
    public string EnvironmentVarName { get; set; } = string.Empty;
    public bool HasValue => Value.NotEmpty();
    public string Value => Environment.GetEnvironmentVariable(EnvironmentVarName) ?? string.Empty;
}