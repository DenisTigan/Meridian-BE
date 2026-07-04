using MeridianEmployeeHub.Data.Entities;
using MeridianEmployeeHub.Data.Repositories.Interfaces;
using MeridianEmployeeHub.Services.Exceptions;
using MeridianEmployeeHub.Services.LeaveRequests.DTOs;

namespace MeridianEmployeeHub.Services.LeaveRequests
{
    public class LeaveRequestService : ILeaveRequestService
    {
        private readonly ILeaveRequestRepository _requestRepository;
        private readonly ILeaveBalanceRepository _balanceRepository;
        private readonly IEmployeeRepository _employeeRepository;

        public LeaveRequestService(
            ILeaveRequestRepository requestRepository,
            ILeaveBalanceRepository balanceRepository,
            IEmployeeRepository employeeRepository)
        {
            _requestRepository = requestRepository;
            _balanceRepository = balanceRepository;
            _employeeRepository = employeeRepository;
        }

        public async Task<IEnumerable<LeaveRequestDto>> GetRequestsAsync(int currentUserId, bool isHROrAdmin, bool isManager)
        {
            IEnumerable<LeaveRequest> requests;

            if (isHROrAdmin)
            {
                requests = await _requestRepository.GetRequestsAsync(null, null);
            }
            else if (isManager)
            {
                requests = await _requestRepository.GetRequestsAsync(currentUserId, currentUserId);
            }
            else
            {
                requests = await _requestRepository.GetRequestsAsync(currentUserId, null);
            }

            return requests.Select(MapToDto);
        }

        public async Task<LeaveRequestDto?> GetRequestByIdAsync(int id, int currentUserId, bool isHROrAdmin)
        {
            var request = await _requestRepository.GetByIdAsync(id);
            if (request == null) return null;

            // Ownership check: self, manager of the self, or HR/Admin
            if (!isHROrAdmin && request.EmployeeId != currentUserId && request.Employee.ManagerId != currentUserId)
            {
                throw new ForbiddenException("You are not allowed to view this leave request.");
            }

            return MapToDto(request);
        }

        public async Task<LeaveRequestDto> CreateRequestAsync(int currentUserId, CreateLeaveRequest requestDto)
        {
            var totalDays = CalculateWorkingDays(requestDto.StartDate, requestDto.EndDate);
            if (totalDays <= 0)
            {
                throw new ArgumentException("The selected date range does not contain any working days.");
            }

            var request = new LeaveRequest
            {
                EmployeeId = currentUserId,
                LeaveType = requestDto.LeaveType,
                StartDate = requestDto.StartDate,
                EndDate = requestDto.EndDate,
                TotalDays = totalDays,
                Reason = requestDto.Reason,
                Status = LeaveRequestStatus.Pending
            };

            await _requestRepository.AddAsync(request);
            await _requestRepository.SaveChangesAsync();

            // Fetch with Includes for DTO
            var createdRequest = await _requestRepository.GetByIdAsync(request.Id);
            return MapToDto(createdRequest!);
        }

        public async Task<LeaveRequestDto> ReviewRequestAsync(int id, int reviewerId, ReviewLeaveRequest reviewDto)
        {
            var request = await _requestRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Leave request with id {id} not found.");

            if (request.Status != LeaveRequestStatus.Pending)
            {
                throw new ConflictException("This request has already been reviewed.");
            }

            if (reviewDto.Status != LeaveRequestStatus.Approved && reviewDto.Status != LeaveRequestStatus.Rejected)
            {
                throw new ArgumentException("Status can only be updated to Approved or Rejected.");
            }

            if (reviewDto.Status == LeaveRequestStatus.Approved)
            {
                short year = (short)request.StartDate.Year;
                var balance = await _balanceRepository.GetByEmployeeYearAndTypeAsync(request.EmployeeId, year, request.LeaveType);

                if (balance == null)
                {
                    // Auto-create default balance if missing to handle edge cases
                    balance = new LeaveBalance
                    {
                        EmployeeId = request.EmployeeId,
                        Year = year,
                        LeaveType = request.LeaveType,
                        AllottedDays = GetDefaultAllottedDays(request.LeaveType),
                        UsedDays = 0
                    };
                    await _balanceRepository.AddAsync(balance);
                    // No save changes yet, will save everything at the end
                }

                if (balance.AllottedDays - balance.UsedDays < request.TotalDays)
                {
                    throw new ConflictException("Insufficient leave balance for this request.");
                }

                // Balance is sufficient, deduct days
                balance.UsedDays += request.TotalDays;
                
                if (balance.Id != 0) // if not just created
                {
                    await _balanceRepository.UpdateAsync(balance);
                }
            }

            request.Status = reviewDto.Status;
            request.ReviewedById = reviewerId;
            request.ManagerComment = reviewDto.ManagerComment;

            await _requestRepository.UpdateAsync(request);
            await _requestRepository.SaveChangesAsync();

            return MapToDto(request);
        }

        private decimal CalculateWorkingDays(DateOnly startDate, DateOnly endDate)
        {
            if (startDate > endDate)
                throw new ArgumentException("Start date must be before or equal to end date.");

            int workingDays = 0;
            var currentDate = startDate;

            while (currentDate <= endDate)
            {
                if (currentDate.DayOfWeek != DayOfWeek.Saturday && currentDate.DayOfWeek != DayOfWeek.Sunday)
                {
                    workingDays++;
                }
                currentDate = currentDate.AddDays(1);
            }

            return workingDays;
        }

        private decimal GetDefaultAllottedDays(LeaveType leaveType)
        {
            return leaveType switch
            {
                LeaveType.Annual => 21,
                LeaveType.Sick => 10,
                LeaveType.Personal => 5,
                LeaveType.Maternity => 90,
                LeaveType.Paternity => 10,
                _ => 0
            };
        }

        private LeaveRequestDto MapToDto(LeaveRequest request)
        {
            return new LeaveRequestDto
            {
                Id = request.Id,
                EmployeeId = request.EmployeeId,
                EmployeeName = $"{request.Employee?.FirstName} {request.Employee?.LastName}".Trim(),
                LeaveType = request.LeaveType,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                TotalDays = request.TotalDays,
                Reason = request.Reason,
                Status = request.Status,
                ReviewedById = request.ReviewedById,
                ReviewedByName = request.ReviewedBy != null ? $"{request.ReviewedBy.FirstName} {request.ReviewedBy.LastName}".Trim() : null,
                ManagerComment = request.ManagerComment,
                CreatedAt = request.CreatedAt,
                UpdatedAt = request.UpdatedAt
            };
        }
    }
}
