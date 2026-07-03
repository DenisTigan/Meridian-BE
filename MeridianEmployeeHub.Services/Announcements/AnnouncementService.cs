using AutoMapper;
using MeridianEmployeeHub.Data.Entities;
using MeridianEmployeeHub.Data.Repositories.Interfaces;
using MeridianEmployeeHub.Services.Announcements.DTOs;
using MeridianEmployeeHub.Services.Exceptions;

namespace MeridianEmployeeHub.Services.Announcements
{
    public class AnnouncementService : IAnnouncementService
    {
        private readonly IAnnouncementRepository _repository;
        private readonly IMapper _mapper;

        public AnnouncementService(IAnnouncementRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        // ── GET toate (filtrat + paginat) ─────────────────────────────────────
        public async Task<PagedAnnouncementResponse> GetAllAsync(
            AnnouncementCategory? category,
            bool isPrivileged,
            int currentUserId,
            int page,
            int pageSize)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var (items, totalCount) = await _repository.GetFilteredAsync(
                category, isPrivileged, currentUserId, page, pageSize);

            return new PagedAnnouncementResponse
            {
                Items = _mapper.Map<IEnumerable<AnnouncementDto>>(items),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        // ── GET individual ────────────────────────────────────────────────────
        // Respectă aceeași regulă de vizibilitate: dacă e nepublicat,
        // doar autorul, HR sau Admin pot vedea anunțul.
        public async Task<AnnouncementDto?> GetByIdAsync(int id, bool isPrivileged, int currentUserId)
        {
            var announcement = await _repository.GetByIdAsync(id);

            if (announcement is null)
                return null;

            // Vizibilitate: angajat obișnuit nu vede anunțurile nepublicate ale altcuiva
            if (!announcement.IsPublished && !isPrivileged && announcement.AuthorId != currentUserId)
                throw new ForbiddenException(
                    "This announcement is not yet published and you are not its author.");

            return _mapper.Map<AnnouncementDto>(announcement);
        }

        // ── POST creare ───────────────────────────────────────────────────────
        public async Task<AnnouncementDto> CreateAsync(CreateAnnouncementRequest request, int authorId)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
                throw new ArgumentException("Title is required.");

            if (string.IsNullOrWhiteSpace(request.Content))
                throw new ArgumentException("Content is required.");

            var announcement = new Announcement
            {
                Title = request.Title.Trim(),
                Content = request.Content.Trim(),
                AuthorId = authorId,
                Category = request.Category,
                IsPublished = request.IsPublished,
                PublishedAt = request.PublishedAt
            };

            await _repository.AddAsync(announcement);
            await _repository.SaveChangesAsync();

            // Re-fetch cu Include(Author) pentru FullName în DTO
            var created = await _repository.GetByIdAsync(announcement.Id)
                ?? throw new InvalidOperationException("Failed to retrieve the created announcement.");

            return _mapper.Map<AnnouncementDto>(created);
        }

        // ── PUT actualizare ───────────────────────────────────────────────────
        // Ownership check: autorul sau Admin poate edita — identic cu UpdateEmployeeAsync
        public async Task<AnnouncementDto> UpdateAsync(
            int id,
            UpdateAnnouncementRequest request,
            int currentUserId,
            bool isAdmin)
        {
            var announcement = await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Announcement with id {id} not found.");

            // Verificare ownership — NU e o policy generică; logica e în service
            if (announcement.AuthorId != currentUserId && !isAdmin)
                throw new ForbiddenException(
                    "Only the author or an Admin can edit this announcement.");

            if (request.Title is not null)
                announcement.Title = request.Title.Trim();

            if (request.Content is not null)
                announcement.Content = request.Content.Trim();

            if (request.Category.HasValue)
                announcement.Category = request.Category.Value;

            if (request.IsPublished.HasValue)
                announcement.IsPublished = request.IsPublished.Value;

            // PublishedAt poate fi setat la null explicit pentru a anula programarea
            if (request.PublishedAt.HasValue)
                announcement.PublishedAt = request.PublishedAt.Value;

            await _repository.UpdateAsync(announcement);
            await _repository.SaveChangesAsync();

            return _mapper.Map<AnnouncementDto>(announcement);
        }

        // ── DELETE ────────────────────────────────────────────────────────────
        // Apelat exclusiv de Admin (policy AdminOnly pe controller)
        public async Task DeleteAsync(int id)
        {
            var announcement = await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Announcement with id {id} not found.");

            await _repository.DeleteAsync(announcement);
            await _repository.SaveChangesAsync();
        }
    }
}
