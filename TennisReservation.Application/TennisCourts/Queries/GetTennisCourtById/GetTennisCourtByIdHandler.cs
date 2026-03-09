using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TennisReservation.Application.Database;
using TennisReservation.Application.TennisCourts.Queries.GetTennisCourtById;
using TennisReservation.Contracts.TennisCourts.DTO;
using TennisReservation.Domain.Models;

public class GetTennisCourtByIdHandler
{
    private readonly IReadDbContext _readDbContext;
    private readonly ILogger<GetTennisCourtByIdHandler> _logger;

    public GetTennisCourtByIdHandler(IReadDbContext readDbContext, ILogger<GetTennisCourtByIdHandler> logger)
    {
        _readDbContext = readDbContext;
        _logger = logger;
    }

    public async Task<TennisCourtDto?> HandleAsync(GetTennisCourtByIdQuery query, CancellationToken cancellationToken)
    {
        try
        {
            return await _readDbContext.TennisCourtsRead
                .Where(tc => tc.Id == new TennisCourtId(query.Id))
                .Select(t => new TennisCourtDto(
                    t.Id.Value,
                    t.Name,
                    t.HourlyRate,
                    t.Description
                )).FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении корта {TennisCourtId}", query.Id);
            return null;
        }
    }
}