using LinkUpPro.Core.Application.Interfaces.Services;
using LinkUpPro.Core.Application.Interfaces.Repositories;
using LinkUpPro.Core.Domain.Exceptions;
using LinkUpPro.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace LinkUpPro.Infrastructure.Persistence.UnitOfWork
{
    public class EFUnitOfWork : IUnitOfWork
    {
        private readonly LinkUpProDbContext _context;

        public EFUnitOfWork(LinkUpProDbContext context)
        {
            _context = context;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new ConcurrencyException("El juego fue modificado por el otro jugador. por favor, intente nuevamente.", ex);
            }
        }
    }
}
