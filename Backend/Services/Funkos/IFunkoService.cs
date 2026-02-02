using Backend.Errors;
using Backend.Models.Dto;
using Backend.Models.Dto.Funkos;
using CSharpFunctionalExtensions;

namespace Backend.Services.Funkos;

public interface IFunkoService
{
    public Task<Result<FunkoResponseDto, FunkoError>> GetByIdAsync(long id);
    public Task<Result<PageResponse<FunkoResponseDto>, FunkoError>> GetAllAsync(FilterDto filter);
    public Task<Result<FunkoResponseDto, FunkoError>> SaveAsync(FunkoRequestDto dto);
    public Task<Result<FunkoResponseDto, FunkoError>> UpdateAsync(long id, FunkoRequestDto dto);
    public Task<Result<FunkoResponseDto, FunkoError>> DeleteAsync(long id);
    public Task<Result<FunkoResponseDto, FunkoError>> PatchAsync(long id, FunkoPatchRequestDto dto);
}