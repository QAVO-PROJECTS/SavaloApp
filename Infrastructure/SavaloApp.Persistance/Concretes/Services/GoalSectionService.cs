using AutoMapper;
using SavaloApp.Application.Abstracts.Repositories.GoalSections;
using SavaloApp.Application.Abstracts.Services;
using SavaloApp.Application.Dtos.GoalSection;
using SavaloApp.Application.GlobalException;
using SavaloApp.Domain.Entities;

namespace SavaloApp.Persistance.Concretes.Services;

public class GoalSectionService:IGoalSectionService
{
        private const string GoalSectionFolder = "goal-sections";

    private readonly IGoalSectionReadRepository _readRepo;
    private readonly IGoalSectionWriteRepository _writeRepo;
    private readonly IFileService _fileService;
    private readonly IMapper _mapper;

    public GoalSectionService(
        IGoalSectionReadRepository readRepo,
        IGoalSectionWriteRepository writeRepo,
        IFileService fileService,
        IMapper mapper)
    {
        _readRepo = readRepo;
        _writeRepo = writeRepo;
        _fileService = fileService;
        _mapper = mapper;
    }

    public async Task<GoalSectionDto> GetGoalSectionAsync(string id)
    {
        var gid = ParseGuidOrThrow(id);

        var entity = await _readRepo.GetAsync(x => x.Id == gid && !x.IsDeleted)
                     ?? throw new GlobalAppException("GOAL_SECTION_NOT_FOUND");

        return _mapper.Map<GoalSectionDto>(entity);
    }

    public async Task<List<GoalSectionDto>> GetAllGoalSectionsAsync()
    {
        var entities = await _readRepo.GetAllAsync(x => !x.IsDeleted);
        return _mapper.Map<List<GoalSectionDto>>(entities);
    }

    public async Task<GoalSectionDto> CreateGoalSectionAsync(CreateGoalSectionDto dto)
    {
        var entity = _mapper.Map<GoalSection>(dto);

        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.IsDeleted = false;

        if (dto.Icon != null && dto.Icon.Length > 0)
            entity.Icon = await _fileService.UploadFile(dto.Icon, GoalSectionFolder);

        await _writeRepo.AddAsync(entity);
        await _writeRepo.CommitAsync();

        return _mapper.Map<GoalSectionDto>(entity);
    }

    public async Task<GoalSectionDto> UpdateGoalSectionAsync(UpdateGoalSectionDto dto)
    {
        var gid = ParseGuidOrThrow(dto.Id);

        var entity = await _readRepo.GetAsync(x => x.Id == gid && !x.IsDeleted)
                     ?? throw new GlobalAppException("GOAL_SECTION_NOT_FOUND");

        if (!string.IsNullOrWhiteSpace(dto.Name))
            entity.Name = dto.Name;

        if (dto.Icon != null && dto.Icon.Length > 0)
            entity.Icon = await _fileService.UploadFile(dto.Icon, GoalSectionFolder);

        entity.UpdatedAt = DateTime.UtcNow;

        await _writeRepo.UpdateAsync(entity);
        await _writeRepo.CommitAsync();

        return _mapper.Map<GoalSectionDto>(entity);
    }

    public async Task DeleteGoalSectionAsync(string id)
    {
        var gid = ParseGuidOrThrow(id);

        var entity = await _readRepo.GetAsync(x => x.Id == gid && !x.IsDeleted)
                     ?? throw new GlobalAppException("GOAL_SECTION_NOT_FOUND");

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