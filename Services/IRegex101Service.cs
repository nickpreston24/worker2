namespace worker2.Services;

public interface IRegex101Service
{
    Task<List<Regex101Pattern>> GetAll();
    Task<List<Regex101Pattern>> Search(Regex101Pattern search);
    Task<Regex101Pattern> GetById(int id);
    Task<int> Create(params Regex101Pattern[] model);
    Task Update(int id, Regex101Pattern model);
    Task Delete(int id);
    Task<int> GetCount();
    Task<List<string>> FindTables();
}