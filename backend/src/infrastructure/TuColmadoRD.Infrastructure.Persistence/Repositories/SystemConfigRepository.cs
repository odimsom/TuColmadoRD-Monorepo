using Microsoft.EntityFrameworkCore;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Security;
using TuColmadoRD.Core.Domain.Entities.System;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;
using TuColmadoRD.Infrastructure.Persistence.Contexts;

namespace TuColmadoRD.Infrastructure.Persistence.Repositories;

public class SystemConfigRepository : ISystemConfigRepository
{
    private const string LktKey = "security.last_known_time";
    private readonly TuColmadoDbContext _dbContext;

    public SystemConfigRepository(TuColmadoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string?> GetLastKnownTimeAsync()
    {
        var config = await _dbContext.SystemConfigs
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Key == LktKey);

        return config?.Value;
    }

    public async Task UpdateLastKnownTimeAsync(string newTime)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var config = await _dbContext.SystemConfigs.FirstOrDefaultAsync(c => c.Key == LktKey);
            if (config == null)
            {
                config = new SystemConfig(LktKey, newTime);
                _dbContext.SystemConfigs.Add(config);
            }
            else
            {
                config.UpdateValue(newTime);
            }

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<OperationResult<string?, DomainError>> GetAsync(string key)
    {
        try
        {
            var config = await _dbContext.SystemConfigs.AsNoTracking().FirstOrDefaultAsync(c => c.Key == key);
            return OperationResult<string?, DomainError>.Good(config?.Value);
        }
        catch (Exception ex)
        {
            return OperationResult<string?, DomainError>.Bad(new TuColmadoRD.Core.Domain.Errors.SyncError("DatabaseQueryFailed", ex.Message));
        }
    }

    public async Task<OperationResult<TuColmadoRD.Core.Domain.Base.Result.Unit, DomainError>> SetAsync(string key, string value)
    {
        try
        {
            var config = await _dbContext.SystemConfigs.FirstOrDefaultAsync(c => c.Key == key);
            if (config == null)
                _dbContext.SystemConfigs.Add(new SystemConfig(key, value));
            else
                config.UpdateValue(value);
            
            await _dbContext.SaveChangesAsync();
            return OperationResult<TuColmadoRD.Core.Domain.Base.Result.Unit, DomainError>.Good(new TuColmadoRD.Core.Domain.Base.Result.Unit());
        }
        catch (Exception ex)
        {
            return OperationResult<TuColmadoRD.Core.Domain.Base.Result.Unit, DomainError>.Bad(new TuColmadoRD.Core.Domain.Errors.SyncError("DatabaseCommitFailed", ex.Message));
        }
    }
}
