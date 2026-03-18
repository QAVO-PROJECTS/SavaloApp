using AutoMapper;
using SavaloApp.Application.Abstracts.Repositories.CurrencyAccounts;
using SavaloApp.Application.Abstracts.Services;
using SavaloApp.Application.Dtos.CurrencyAccount;
using SavaloApp.Application.GlobalException;
using SavaloApp.Domain.Entities;

namespace SavaloApp.Persistance.Concretes.Services;

public class CurrencyAccountService : ICurrencyAccountService
{
    private const string CurrencyFolder = "currency-accounts";

    private readonly ICurrencyAccountReadRepository _readRepo;
    private readonly ICurrencyAccountWriteRepository _writeRepo;
    private readonly IFileService _fileService;
    private readonly IMapper _mapper;

    public CurrencyAccountService(
        ICurrencyAccountReadRepository readRepo,
        ICurrencyAccountWriteRepository writeRepo,
        IFileService fileService,
        IMapper mapper)
    {
        _readRepo = readRepo;
        _writeRepo = writeRepo;
        _fileService = fileService;
        _mapper = mapper;
    }

    public async Task<List<CurrencyAccountDto>> GetAllCurrenciesAsync(string userId)
    {
        var entities = await _readRepo.GetAllAsync(x => !x.IsDeleted && x.UserId == userId);
        return _mapper.Map<List<CurrencyAccountDto>>(entities);
    }

    public async Task<CurrencyAccountDto> GetCurrencyAsync(string id, string userId)
    {
        var gid = ParseGuidOrThrow(id);

        var entity = await _readRepo.GetAsync(x =>
            x.Id == gid &&
            !x.IsDeleted &&
            x.UserId == userId)
            ?? throw new GlobalAppException("CURRENCY_NOT_FOUND");

        return _mapper.Map<CurrencyAccountDto>(entity);
    }

    public async Task<CurrencyAccountDto> CreateCurrencyAsync(CreateCurrencyAccountDto dto, string userId)
    {
        var entity = _mapper.Map<CurrencyAccount>(dto);

        entity.Id = Guid.NewGuid();
        entity.UserId = userId;
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.IsDeleted = false;

        if (dto.Icon != null && dto.Icon.Length > 0)
            entity.Icon = await _fileService.UploadFile(dto.Icon, CurrencyFolder);

        await _writeRepo.AddAsync(entity);
        await _writeRepo.CommitAsync();

        return _mapper.Map<CurrencyAccountDto>(entity);
    }

    public async Task UpdateCurrencyAsync(UpdateCurrencyAccountDto dto, string userId)
    {
        var gid = ParseGuidOrThrow(dto.Id);

        var entity = await _readRepo.GetAsync(x =>
            x.Id == gid &&
            !x.IsDeleted &&
            x.UserId == userId)
            ?? throw new GlobalAppException("CURRENCY_NOT_FOUND");

        if (!string.IsNullOrWhiteSpace(dto.Name))
            entity.Name = dto.Name;

        if (dto.Icon != null && dto.Icon.Length > 0)
            entity.Icon = await _fileService.UploadFile(dto.Icon, CurrencyFolder);

        entity.UpdatedAt = DateTime.UtcNow;

        await _writeRepo.UpdateAsync(entity);
        await _writeRepo.CommitAsync();
    }

    public async Task DeleteCurrencyAsync(string id, string userId)
    {
        var gid = ParseGuidOrThrow(id);

        var entity = await _readRepo.GetAsync(x =>
            x.Id == gid &&
            !x.IsDeleted &&
            x.UserId == userId)
            ?? throw new GlobalAppException("CURRENCY_NOT_FOUND");

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        await _writeRepo.UpdateAsync(entity);
        await _writeRepo.CommitAsync();
    }

    private static Guid ParseGuidOrThrow(string id)
    {
        if (!Guid.TryParse(id, out var gid))
            throw new GlobalAppException("INVALID_ID");

        return gid;
    }
}