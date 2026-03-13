using AutoMapper;
using SavaloApp.Application.Abstracts.Repositories.CategorySections;
using SavaloApp.Application.Abstracts.Services;
using SavaloApp.Application.Dtos.CategorySection;
using SavaloApp.Application.GlobalException;
using SavaloApp.Domain.Entities;

namespace SavaloApp.Persistance.Concretes.Services;

public class CategorySectionService : ICategorySectionService
{
    private const string CategorySectionFolder = "category-sections";

    private readonly ICategorySectionReadRepository _readRepo;
    private readonly ICategorySectionWriteRepository _writeRepo;
    private readonly IFileService _fileService;
    private readonly IMapper _mapper;

    public CategorySectionService(
        ICategorySectionReadRepository readRepo,
        ICategorySectionWriteRepository writeRepo,
        IFileService fileService,
        IMapper mapper)
    {
        _readRepo = readRepo;
        _writeRepo = writeRepo;
        _fileService = fileService;
        _mapper = mapper;
    }

    public async Task<CategorySectionDto> GetCategorySectionAsync(string id)
    {
        var gid = ParseGuidOrThrow(id);

        var entity = await _readRepo.GetAsync(x => x.Id == gid && !x.IsDeleted)
                     ?? throw new GlobalAppException("CATEGORY_SECTION_NOT_FOUND");

        return _mapper.Map<CategorySectionDto>(entity);
    }

    public async Task<List<CategorySectionDto>> GetAllCategorySectionsAsync()
    {
        var entities = await _readRepo.GetAllAsync(x => !x.IsDeleted);
        return _mapper.Map<List<CategorySectionDto>>(entities);
    }

    public async Task<CategorySectionDto> CreateCategorySectionAsync(CreateCategorySectionDto dto)
    {
        var entity = _mapper.Map<CategorySection>(dto);

        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.IsDeleted = false;

        if (dto.Icon != null && dto.Icon.Length > 0)
            entity.Icon = await _fileService.UploadFile(dto.Icon, CategorySectionFolder);

        await _writeRepo.AddAsync(entity);
        await _writeRepo.CommitAsync();

        return _mapper.Map<CategorySectionDto>(entity);
    }

    public async Task<CategorySectionDto> UpdateCategorySectionAsync(UpdateCategorySectionDto dto)
    {
        var gid = ParseGuidOrThrow(dto.Id);

        var entity = await _readRepo.GetAsync(x => x.Id == gid && !x.IsDeleted)
                     ?? throw new GlobalAppException("CATEGORY_SECTION_NOT_FOUND");

        if (!string.IsNullOrWhiteSpace(dto.Name))
            entity.Name = dto.Name;

        if (dto.Icon != null && dto.Icon.Length > 0)
            entity.Icon = await _fileService.UploadFile(dto.Icon, CategorySectionFolder);

        entity.UpdatedAt = DateTime.UtcNow;

        await _writeRepo.UpdateAsync(entity);
        await _writeRepo.CommitAsync();

        return _mapper.Map<CategorySectionDto>(entity);
    }

    public async Task DeleteCategorySectionAsync(string id)
    {
        var gid = ParseGuidOrThrow(id);

        var entity = await _readRepo.GetAsync(x => x.Id == gid && !x.IsDeleted)
                     ?? throw new GlobalAppException("CATEGORY_SECTION_NOT_FOUND");

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