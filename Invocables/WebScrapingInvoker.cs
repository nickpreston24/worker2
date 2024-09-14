using Coravel.Invocable;
using CodeMechanic.Bash;

namespace worker2;

public class WebScrapingInvoker : IInvocable
{
    public async Task Invoke()
    {
        string url = "curl https://github.com/nickpreston24/gig_it";
        // string url = "curl https://ammoseek.com/ammo/224-valkyrie";
        var html = await url.Bash(verbose: false, s => Console.WriteLine(s));
        FSExtensions.CreateFileHere("ammoseek_test.html", html);
    }
}