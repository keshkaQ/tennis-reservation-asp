using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace TennisReservation.Infrastructure.Postgres.Repositories
{
    public abstract class RepositoryBase<TEntity,TId> where TEntity : class
    {
        protected readonly TennisReservationDbContext _dbContext;
        protected readonly DbSet<TEntity> _dbSet;
        protected readonly ILogger _logger;
        private readonly string _entityName;

        protected RepositoryBase(TennisReservationDbContext dbContext, ILogger logger)
        {
            _dbContext = dbContext;
            _dbSet = dbContext.Set<TEntity>();
            _logger = logger;
            _entityName = typeof(TEntity).Name;
        }

        // Выполняет асинхронную операцию БД с транзакцией, логированием и обработкой ошибок.
        // Используется для операций, которые не требуют возврата значения (Update, Delete)
        protected async Task<Result> ExecuteAsync(Func<IDbContextTransaction,Task> operation,
            string successMessage,string errorMessage, CancellationToken cancellationToken = default)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                await operation(transaction);
                await transaction.CommitAsync(cancellationToken);
                _logger.LogInformation(successMessage);
                return Result.Success();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, errorMessage);
                return Result.Failure(errorMessage);
            }
        }

        // Выполняет асинхронную операцию БД с транзакцией, логированием и обработкой ошибок.
        // Используется для операций, которые возвращают значение (Create, Get)
        protected async Task<Result<T>> ExecuteAsync<T>(Func<IDbContextTransaction, Task<T>> operation,string successMessage,string errorMessage,
            CancellationToken cancellationToken = default)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var result = await operation(transaction);
                await transaction.CommitAsync(cancellationToken);
                _logger.LogInformation(successMessage);
                return Result.Success(result);
            }
            catch (OperationCanceledException)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogWarning("Операция отменена: {EntityName}", _entityName);
                return Result.Failure<T>("Операция была отменена");
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Ошибка БД: {ErrorMessage}", ex.InnerException?.Message);
                return Result.Failure<T>(errorMessage);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, errorMessage);
                return Result.Failure<T>(errorMessage);
            }
        }

        // Выполняет операцию БД БЕЗ транзакции (для простых Get запросов)
        protected async Task<Result<T>> ExecuteQueryAsync<T>(
            Func<Task<T>> operation,
            string errorMessage,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await operation();
                return Result.Success(result);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Запрос отменен: {EntityName}", _entityName);
                return Result.Failure<T>("Запрос был отменен");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, errorMessage);
                return Result.Failure<T>(errorMessage);
            }
        }

        protected async Task<bool> ExecuteCheckAsync(
           Func<Task<bool>> operation,
           string errorMessage,
           CancellationToken cancellationToken = default)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, errorMessage);
                return false;
            }
        }
    }
}
