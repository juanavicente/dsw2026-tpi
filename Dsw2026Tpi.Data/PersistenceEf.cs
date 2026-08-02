using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Text.Json;
using System.Reflection;
using System.Data;

namespace Dsw2026Tpi.Data;

public class PersistenceEf : IPersistence
{
    private readonly Dsw2026TpiDbContext _context;

    public PersistenceEf(Dsw2026TpiDbContext context)
    {
        _context = context;
    }

    public async Task<T> Add<T>(T entity) where T : EntityBase
    {
        await _context.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task AddRange<T>(IEnumerable<T> entities) where T : EntityBase
    {
        await _context.AddRangeAsync(entities);
        await _context.SaveChangesAsync();
    }

    public async Task<T> Delete<T>(T entity) where T : EntityBase
    {
        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;

        _context.Update(entity);
        await _context.SaveChangesAsync();

        return entity;
    }

    public async Task<T?> First<T>(Expression<Func<T, bool>> predicate, params string[] include) where T : EntityBase
    {
        return await Include(_context.Set<T>(), include)
            .Where(e => !e.IsDeleted)
            .FirstOrDefaultAsync(predicate);
    }

    public async Task<IEnumerable<T>?> GetAll<T>(params string[] include) where T : EntityBase
    {
        return await Include(_context.Set<T>(), include)
            .Where(e => !e.IsDeleted)
            .ToListAsync();
    }

    public async Task<T?> GetById<T>(Guid id, params string[] include) where T : EntityBase
    {
        return await Include(_context.Set<T>(), include)
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
    }

    public async Task<IEnumerable<T>?> GetFiltered<T>(Expression<Func<T, bool>> predicate, params string[] include) where T : EntityBase
    {
        return await Include(_context.Set<T>(), include)
            .Where(e => !e.IsDeleted)
            .Where(predicate)
            .ToListAsync();
    }

    public async Task<T> Update<T>(T entity) where T : EntityBase
    {
        _context.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<Pagination<T>> Paginate<T, TKey>(
    int pageSize,
    int pageIndex,
    Expression<Func<T, bool>> predicate,
    Expression<Func<T, TKey>> sortOrder,
    params string[] includes) where T : EntityBase
    {
        if (pageSize <= 0)
            throw new ArgumentException(
                "El tamaño de página debe ser mayor a cero.",
                nameof(pageSize));

        if (pageIndex < 0)
            throw new ArgumentException(
                "El índice de página no puede ser negativo.",
                nameof(pageIndex));

        var filtered = Include(_context.Set<T>(), includes)
            .Where(e => !e.IsDeleted)
            .Where(predicate)
            .OrderBy(sortOrder);

        var total = await filtered.CountAsync();

        var data = await filtered
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new Pagination<T>(
            pageSize,
            pageIndex,
            total,
            data);
    }

    private static IQueryable<T> Include<T>(
        IQueryable<T> query,
        string[] includes) where T : EntityBase
    {
        var includedQuery = query;

        foreach (var include in includes)
        {
            includedQuery = includedQuery.Include(include);
        }

        return includedQuery;
    }

    public async Task<List<DateOnly>> GetHolidays()
    {
        var path = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Sources",
            "holidays.json");

        if (!File.Exists(path))
            return new List<DateOnly>();

        var json = await File.ReadAllTextAsync(path);

        return JsonSerializer.Deserialize<List<DateOnly>>(json)
               ?? new List<DateOnly>();
    }

    public async Task ExecuteInTransaction(
    Func<Task> action,
    IsolationLevel isolationLevel = IsolationLevel.Serializable)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync(isolationLevel);

        try
        {
            await action();

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}