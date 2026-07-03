using AutoMapper;
using MeridianEmployeeHub.Data.Entities;
using MeridianEmployeeHub.Data.Repositories.Interfaces;
using MeridianEmployeeHub.Services.QuickLinks.DTOs;

namespace MeridianEmployeeHub.Services.QuickLinks
{
    public class QuickLinkService : IQuickLinkService
    {
        private readonly IQuickLinkRepository _repository;
        private readonly IMapper _mapper;

        public QuickLinkService(IQuickLinkRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        // ── GET toate active (listă plată, sortate Category → OrderIndex) ─────
        // Frontend-ul grupează vizual pe baza câmpului Category.
        public async Task<IEnumerable<QuickLinkDto>> GetAllActiveAsync()
        {
            var links = await _repository.GetAllActiveAsync();
            return _mapper.Map<IEnumerable<QuickLinkDto>>(links);
        }

        public async Task<QuickLinkDto?> GetByIdAsync(int id)
        {
            var link = await _repository.GetByIdAsync(id);
            return link is null ? null : _mapper.Map<QuickLinkDto>(link);
        }

        // ── POST creare ───────────────────────────────────────────────────────
        public async Task<QuickLinkDto> CreateAsync(CreateQuickLinkRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Name is required.");

            if (string.IsNullOrWhiteSpace(request.Url))
                throw new ArgumentException("Url is required.");

            var link = new QuickLink
            {
                Name = request.Name.Trim(),
                Url = request.Url.Trim(),
                IconName = request.IconName.Trim(),
                Category = request.Category.Trim(),
                OrderIndex = request.OrderIndex,
                IsActive = request.IsActive
            };

            await _repository.AddAsync(link);
            await _repository.SaveChangesAsync();

            return _mapper.Map<QuickLinkDto>(link);
        }

        // ── PUT actualizare ───────────────────────────────────────────────────
        public async Task<QuickLinkDto> UpdateAsync(int id, UpdateQuickLinkRequest request)
        {
            var link = await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"QuickLink with id {id} not found.");

            if (request.Name is not null)
                link.Name = request.Name.Trim();

            if (request.Url is not null)
                link.Url = request.Url.Trim();

            if (request.IconName is not null)
                link.IconName = request.IconName.Trim();

            if (request.Category is not null)
                link.Category = request.Category.Trim();

            if (request.OrderIndex.HasValue)
                link.OrderIndex = request.OrderIndex.Value;

            if (request.IsActive.HasValue)
                link.IsActive = request.IsActive.Value;

            await _repository.UpdateAsync(link);
            await _repository.SaveChangesAsync();

            return _mapper.Map<QuickLinkDto>(link);
        }

        // ── DELETE ────────────────────────────────────────────────────────────
        public async Task DeleteAsync(int id)
        {
            var link = await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"QuickLink with id {id} not found.");

            await _repository.DeleteAsync(link);
            await _repository.SaveChangesAsync();
        }

        // ── PATCH /reorder — actualizare batch ────────────────────────────────
        // Strategia adoptată: ID-uri necunoscute sunt IGNORATE (nu aruncă excepție).
        // Motivare: operația de reorder vine dintr-un drag-and-drop pe frontend care
        // reflectă starea curentă — ID-uri lipsă (șterse între timp) nu trebuie să
        // blocheze salvarea ordinii celorlalte.
        // Toate entitățile sunt încărcate într-un singur query (WHERE Id IN ...),
        // actualizate în memorie, apoi un singur SaveChangesAsync().
        public async Task ReorderAsync(ReorderQuickLinksRequest request)
        {
            if (request.OrderedIds is null || request.OrderedIds.Count == 0)
                throw new ArgumentException("OrderedIds cannot be empty.");

            // Un singur query către baza de date — WHERE Id IN (...)
            var links = (await _repository.GetByIdsAsync(request.OrderedIds))
                .ToDictionary(q => q.Id);

            // Actualizare în memorie — ordinea din array determină OrderIndex.
            // EF Core urmărește entitățile deja încărcate prin GetByIdsAsync,
            // deci simpla modificare a proprietății este suficientă;
            // SaveChangesAsync de la final va detecta automat modificările.
            for (int i = 0; i < request.OrderedIds.Count; i++)
            {
                var id = request.OrderedIds[i];
                if (links.TryGetValue(id, out var link))
                {
                    link.OrderIndex = (byte)Math.Min(i, byte.MaxValue);
                    // Fără apel explicit UpdateAsync — EF Core change tracking detectează modificarea
                }
                // ID necunoscut → ignorat silențios
            }

            // Un singur SaveChangesAsync pentru TOATE modificările
            await _repository.SaveChangesAsync();
        }
    }
}
