using AutoMapper;
using SavaloApp.Application.Abstracts.Repositories.TermsAndConditions;
using SavaloApp.Application.Abstracts.Services;
using SavaloApp.Application.Dtos.TermsAndCondition;
using SavaloApp.Application.GlobalException;
using SavaloApp.Domain.Entities;

namespace SavaloApp.Persistance.Concretes.Services;

public class TermsAndConditionService : ITermsAndConditionService
{
    private readonly ITermsAndConditionReadRepository _readRepository;
    private readonly ITermsAndConditionWriteRepository _writeRepository;
    private readonly IMapper _mapper;

    public TermsAndConditionService(
        ITermsAndConditionReadRepository readRepository,
        ITermsAndConditionWriteRepository writeRepository,
        IMapper mapper)
    {
        _readRepository = readRepository;
        _writeRepository = writeRepository;
        _mapper = mapper;
    }

    public async Task<TermsAndConditionDto> GetAsync()
    {
        var entity = await _readRepository.GetAsync(x => !x.IsDeleted);


        return _mapper.Map<TermsAndConditionDto>(entity);
    }


    public async Task CreateAsync(CreateTermsAndConditionDto dto)
    {
        var exists = await _readRepository.GetAsync(x => !x.IsDeleted);

        if (exists != null)
            throw new GlobalAppException("TERMS_AND_CONDITION_ALREADY_EXISTS");

        var entity = _mapper.Map<TermsAndCondition>(dto);

        await _writeRepository.AddAsync(entity);
        await _writeRepository.CommitAsync();
    }

    public async Task UpdateAsync(UpdateTermsAndConditionDto dto)
    {
        var entity = await _readRepository.GetAsync(x => x.Id.ToString() == dto.Id && !x.IsDeleted);

        if (entity == null)
            throw new GlobalAppException("TERMS_AND_CONDITION_NOT_FOUND");


        entity.Description = dto.Description;

       await _writeRepository.UpdateAsync(entity);
        await _writeRepository.CommitAsync();
    }

    public async Task DeleteAsync(string id)
    {
        var entity = await _readRepository.GetAsync(x => x.Id.ToString() == id && !x.IsDeleted);

        if (entity == null)
            throw new GlobalAppException("TERMS_AND_CONDITION_NOT_FOUND");

        entity.IsDeleted = true;

       await _writeRepository.UpdateAsync(entity);
        await _writeRepository.CommitAsync();
    }
}