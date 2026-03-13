using AutoMapper;
using SavaloApp.Application.Abstracts.Repositories.Icons;
using SavaloApp.Application.Abstracts.Services;
using SavaloApp.Application.Dtos.Icon;
using SavaloApp.Application.GlobalException;
using SavaloApp.Domain.Entities;

namespace SavaloApp.Persistance.Concretes.Services;

public class IconService:IIconService
{
    private const string IconFolder = "icons";

    private readonly IIconReadRepository _readRepo;
    private readonly IIconWriteRepository _writeRepo;
    private readonly IFileService _fileService;
    private readonly IMapper _mapper;

    public IconService(
        IIconReadRepository readRepo,
        IIconWriteRepository writeRepo,
        IFileService fileService,
        IMapper mapper)
    {
        _readRepo = readRepo;
        _writeRepo = writeRepo;
        _fileService = fileService;
        _mapper = mapper;
    }
    public async Task<IconDto> GetIconAsync(string id)
    {
        var gid = ParseGuidOrThrow(id);

        var entity = await _readRepo.GetAsync(x => x.Id == gid && !x.IsDeleted)
                     ?? throw new GlobalAppException("ICON_NOT_FOUND");

        return _mapper.Map<IconDto>(entity);
    }

    public async Task<List<IconDto>> GetAllIconsAsync()
    {
        var entities = await _readRepo.GetAllAsync(x => !x.IsDeleted);
        return _mapper.Map<List<IconDto>>(entities);
    }

    public async  Task<IconDto> CreateIconAsync(CreateIconDto dto)
    {
        var entity = _mapper.Map<Icon>(dto);

        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.IsDeleted = false;

        if (dto.Icon != null && dto.Icon.Length > 0)
            entity.IconName = await _fileService.UploadFile(dto.Icon,IconFolder);

        await _writeRepo.AddAsync(entity);
        await _writeRepo.CommitAsync();

        return _mapper.Map<IconDto>(entity);
    }

    public async Task DeleteIconAsync(string id)
    {
        var gid = ParseGuidOrThrow(id);

        var entity = await _readRepo.GetAsync(x => x.Id == gid && !x.IsDeleted)
                     ?? throw new GlobalAppException("ICON_NOT_FOUND");

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