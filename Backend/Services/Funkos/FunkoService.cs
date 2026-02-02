using Backend.Errors;
using Backend.Mappers;
using Backend.Models;
using Backend.Models.Dto;
using Backend.Repositories.Categorias;
using CSharpFunctionalExtensions;

using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using Backend.Models.Dto.Funkos;

namespace Backend.Services.Funkos;

public class FunkoService(
    IFunkoRepository funkoRepository,
    ICategoryRepository categoryRepository, 
    IMemoryCache cache,
    ILogger<FunkoService> log,
    IConfiguration configuration) : IFunkoService
{
    private const string CachePrefix = "funko:";
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };
    
    public async Task<Result<FunkoResponseDto, FunkoError>> GetByIdAsync(long id)
    {
        log.LogDebug("Buscando funko con ID: {Id}", id);
        
        var cacheKey = $"{CachePrefix}:{id}";
        if (cache.TryGetValue(cacheKey, out Funko? cachedFunko))
        {
            log.LogInformation("Funko recuperado de la caché: {Id}", id);
            return Result.Success<FunkoResponseDto, FunkoError>(cachedFunko!.ToResponse());
        }
        
        var funko = await funkoRepository.GetByIdAsync(id);
        
        if (funko != null)
        {
            cache.Set(cacheKey, funko, _cacheDuration);
            return Result.Success<FunkoResponseDto, FunkoError>(funko.ToResponse());
        }

        log.LogWarning("No se encontró el funko con ID: {Id}", id);
        return Result.Failure<FunkoResponseDto, FunkoError>(new NotFoundError($"No se encontro funko con ID: {id}"));
    }

    public async Task<Result<PageResponse<FunkoResponseDto>, FunkoError>> GetAllAsync(FilterDto filter)
    {
        log.LogInformation($"Buscando funkos | Pagina: {filter.Page}, Size: {filter.Size}");

        var (funkos, totalCount) = await funkoRepository.GetAllAsync(filter);
        var response = funkos.Select(it => it.ToResponse()).ToList();

        var page = new PageResponse<FunkoResponseDto>
        {
            Items = response,
            TotalCount = totalCount,
            Page = filter.Page,
            Size = filter.Size
        };
        
        return Result.Success<PageResponse<FunkoResponseDto>, FunkoError>(page);
    }


    public async Task<Result<FunkoResponseDto, FunkoError>> SaveAsync(FunkoRequestDto dto)
    {
        log.LogInformation("Guardando nuevo funko: {Nombre}", dto.Nombre);
        
        if (!await IsValid(dto)) 
            return Result.Failure<FunkoResponseDto, FunkoError>(new ValidationError("La categoria no es valida"));
        
        var c = await categoryRepository.GetByNameAsync(dto.Categoria);
        var funko = dto.ToModel();
        funko.Categoria = c!;
        funko.CategoriaId = c!.Id;
        
        var result = await funkoRepository.SaveAsync(funko);
        log.LogDebug("Funko guardado correctamente");
        return Result.Success<FunkoResponseDto, FunkoError>(result.ToResponse());
    }

    public async Task<Result<FunkoResponseDto, FunkoError>> UpdateAsync(long id, FunkoRequestDto dto)
    {
        log.LogInformation("Actualizando funko con ID: {Id}", id);
        
        if (!await IsValid(dto))
            return Result.Failure<FunkoResponseDto, FunkoError>(new ValidationError("La categoria no es valida"));

        var toSave = dto.ToModel();
        var c = await categoryRepository.GetByNameAsync(dto.Categoria);
        toSave.Id = id;
        toSave.Categoria = c!;
        toSave.CategoriaId = c!.Id;
        
        var found = await funkoRepository.UpdateAsync(id, toSave);
        if (found == null) 
            return Result.Failure<FunkoResponseDto, FunkoError>(new NotFoundError($"No se encontro funko con id: {id}"));
        log.LogDebug("Funko actualizado correctamente");
        
        cache.Remove(CachePrefix + id);
        return Result.Success<FunkoResponseDto, FunkoError>(toSave.ToResponse());
    }
    
    public async Task<Result<FunkoResponseDto, FunkoError>> PatchAsync(long id, FunkoPatchRequestDto dto)
    {
        var foundFunko = await funkoRepository.GetByIdAsync(id);
        
        if (foundFunko == null) return Result.Failure<FunkoResponseDto, FunkoError>(new NotFoundError($"Funko {id} no encontrado"));
        
        if (dto.Nombre != null) foundFunko.Nombre = dto.Nombre;
        
        if (dto.Precio != null) foundFunko.Precio = (double)dto.Precio;
        
        if (dto.Image != null) foundFunko.Image =  dto.Image;

        if (dto.Categoria != null)
        {
            var foundCategory = await categoryRepository.GetByNameAsync(dto.Categoria);
            if (foundCategory == null)
            {
                return Result.Failure<FunkoResponseDto, FunkoError>(new ConflictError($"La categoría: {dto.Categoria} no existe."));
            }
            // Asignarmos el CategoryId obtenido de la búsqueda
            // Para establecer la relación de FK correctamente
            foundFunko.Categoria = foundCategory;
            foundFunko.CategoriaId = foundCategory.Id;
        }

        await funkoRepository.UpdateAsync(id, foundFunko);
        
        log.LogDebug("Funko actualizado correctamente (Patch)");
        cache.Remove(CachePrefix + id);
        return foundFunko.ToResponse();
    }
    
    public async Task<Result<FunkoResponseDto, FunkoError>> DeleteAsync(long id)
    {
        log.LogWarning("Eliminando funko con ID: {Id}", id);
        var deleted = await funkoRepository.DeleteAsync(id);
        
        if (deleted != null)
        {
            cache.Remove(CachePrefix + id);
            log.LogDebug("Funko eliminado correctamente");
            return Result.Success<FunkoResponseDto, FunkoError>(deleted.ToResponse());
        }
        return Result.Failure<FunkoResponseDto, FunkoError>(new NotFoundError($"No se encontro funko con ID: {id}"));
    }

    private async Task<bool> IsValid(FunkoRequestDto f)
    {
        log.LogDebug("Validando categoría para funko: {Categoria}", f.Categoria);
        var category = await categoryRepository.GetByNameAsync(f.Categoria);
        if (category == null)
        {
            log.LogError($"La categoria no es valida. categoria encontrada: {category}");
            return false;
        }
        log.LogDebug($"Categoria validada correctamente. Categoria: {category}");
        return true;
    }
    

}