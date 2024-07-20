using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace worker2.Services;

/// <summary>
/// Finds all local and remote regexes (any source, including regex101.com).
/// </summary>
public class Regex101Service : IRegex101Service
{
    private readonly IEmailSender emailer;

    public Regex101Service(IEmailSender emailSender)
    {
        emailer = emailSender;
    }

    public Task<List<Regex101Pattern>> GetAll()
    {
        throw new NotImplementedException();
    }

    public Task<List<Regex101Pattern>> Search(Regex101Pattern search)
    {
        throw new NotImplementedException();
    }

    public Task<Regex101Pattern> GetById(int id)
    {
        throw new NotImplementedException();
    }

    public Task<int> Create(params Regex101Pattern[] model)
    {
        throw new NotImplementedException();
    }

    public Task Update(int id, Regex101Pattern model)
    {
        throw new NotImplementedException();
    }

    public Task Delete(int id)
    {
        throw new NotImplementedException();
    }

    public Task<int> GetCount()
    {
        throw new NotImplementedException();
    }

    public Task<List<string>> FindTables()
    {
        throw new NotImplementedException();
    }
}