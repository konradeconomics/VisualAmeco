using VisualAmeco.Application.DTOs;
using VisualAmeco.Application.Interfaces;
using VisualAmeco.Core.Entities;
using VisualAmeco.Core.Interfaces;


namespace VisualAmeco.Application.Services;

public class AmecoEntitySaver : IAmecoEntitySaver
{
    private readonly IChapterRepository _chapterRepo;
    private readonly ISubchapterRepository _subchapterRepo;
    private readonly IVariableRepository _variableRepo;
    private readonly ICountryRepository _countryRepo;
    private readonly IValueRepository _valueRepo;
    private readonly IUnitOfWork _unitOfWork;

    public AmecoEntitySaver(
        IChapterRepository chapterRepo,
        ISubchapterRepository subchapterRepo,
        IVariableRepository variableRepo,
        ICountryRepository countryRepo,
        IValueRepository valueRepo,
        IUnitOfWork unitOfWork)
    {
        _chapterRepo = chapterRepo;
        _subchapterRepo = subchapterRepo;
        _variableRepo = variableRepo;
        _countryRepo = countryRepo;
        _valueRepo = valueRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task SaveAsync(MappedAmecoRow row)
    {
        var chapter = await _chapterRepo.GetByNameAsync(row.ChapterName)
                      ?? new Chapter { Name = row.ChapterName };
        await _chapterRepo.AddAsync(chapter);

        var subchapter = await _subchapterRepo.GetByNameAsync(row.SubchapterName, chapter.Id)
                         ?? new Subchapter { Name = row.SubchapterName, ChapterId = chapter.Id };
        await _subchapterRepo.AddAsync(subchapter);

        var variable = await _variableRepo.GetByCodeAsync(row.VariableCode)
                       ?? new Variable
                       {
                           Code = row.VariableCode,
                           Name = row.VariableName,
                           Unit = row.Unit,
                           SubChapterId = subchapter.Id
                       };
        await _variableRepo.AddAsync(variable);

        var country = await _countryRepo.GetByCodeAsync(row.CountryCode)
                      ?? new Country
                      {
                          Code = row.CountryCode,
                          Name = row.CountryName
                      };
        await _countryRepo.AddAsync(country);

        foreach (var value in row.Values)
        {
            await _valueRepo.AddAsync(new Value
            {
                VariableId = variable.Id,
                CountryId = country.Id,
                Year = value.Year,
                Amount = value.Amount,
                Month = null,
                IsMonthly = false
            });
        }

        await _unitOfWork.SaveChangesAsync();
    }
}