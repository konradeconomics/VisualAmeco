using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<AmecoEntitySaver> _logger;

    public AmecoEntitySaver(
        IChapterRepository chapterRepo,
        ISubchapterRepository subchapterRepo,
        IVariableRepository variableRepo,
        ICountryRepository countryRepo,
        IValueRepository valueRepo,
        IUnitOfWork unitOfWork,
        ILogger<AmecoEntitySaver> logger)
    {
        _chapterRepo = chapterRepo ?? throw new ArgumentNullException(nameof(chapterRepo));
        _subchapterRepo = subchapterRepo ?? throw new ArgumentNullException(nameof(subchapterRepo));
        _variableRepo = variableRepo ?? throw new ArgumentNullException(nameof(variableRepo));
        _countryRepo = countryRepo ?? throw new ArgumentNullException(nameof(countryRepo));
        _valueRepo = valueRepo ?? throw new ArgumentNullException(nameof(valueRepo));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SaveAsync(MappedAmecoRow row)
    {

        Chapter? chapter = await _chapterRepo.GetByNameAsync(row.ChapterName);
        if (chapter == null)
        {
            chapter = new Chapter { Name = row.ChapterName };
            await _chapterRepo.AddAsync(chapter);
        }

        Subchapter? subchapter = await _subchapterRepo.GetByNameAsync(row.SubchapterName, chapter.Id);
        if (subchapter == null)
        {
            subchapter = new Subchapter
            {
                Name = row.SubchapterName,
                Chapter = chapter
            };
            await _subchapterRepo.AddAsync(subchapter);
        }

        Variable? variable = await _variableRepo.GetByCodeAsync(row.VariableCode);
        if (variable == null)
        {
            variable = new Variable
            {
                Code = row.VariableCode,
                Name = row.VariableName,
                UnitCode = row.UnitCode,
                UnitDescription = row.UnitDescription,
                SubChapter = subchapter 
            };
            await _variableRepo.AddAsync(variable);
        }

        Country? country = await _countryRepo.GetByCodeAsync(row.CountryCode);
        if (country == null)
        {
            country = new Country
            {
                Code = row.CountryCode,
                Name = row.CountryName
            };
            await _countryRepo.AddAsync(country);
        }


        foreach (var valueDto in row.Values)
        {
            var newValue = new Value
            {
                Variable = variable,
                Country = country, 
                Year = valueDto.Year,
                Amount = valueDto.Amount,
                Month = null,
                IsMonthly = false
            };
            await _valueRepo.AddAsync(newValue);
        }

        try
        {
            await _unitOfWork.SaveChangesAsync();
            _logger.LogTrace("Saved {ChangeCount} changes for VariableCode {VariableCode} / CountryCode {CountryCode}", variable.Code, country.Code);
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "DbUpdateException saving changes for VariableCode {VariableCode} / CountryCode {CountryCode}. See inner exception.", variable?.Code ?? row.VariableCode, country?.Code ?? row.CountryCode);
            throw;
        }
    }
}