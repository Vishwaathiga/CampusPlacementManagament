using CampusPlacementManagement.Data;
using CampusPlacementManagement.Models;
using CampusPlacementManagement.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagement.Repositories.Implementations
{
    public class StudentRepository : IStudentRepository
    {
        private readonly ApplicationDbContext _context;

        public StudentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Student?> GetByIdAsync(int id)
        {
            return await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.StudentId == id);
        }

        public async Task<Student?> GetByUserIdAsync(string userId)
        {
            return await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }

        public async Task<List<Student>> GetAllAsync(int? collegeId)
        {
            var query = _context.Students
                .Include(s => s.User)
                .AsQueryable();

            if (collegeId.HasValue)
            {
                query = query.Where(s => s.CollegeId == collegeId.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<List<Student>> GetPendingAsync(int collegeId)
        {
            return await _context.Students
                .Include(s => s.User)
                .Where(s => s.CollegeId == collegeId &&
                            s.Status == Enums.StudentStatus.Pending)
                .ToListAsync();
        }

        public async Task<bool> RegisterNumberExistsAsync(
            int collegeId, string registerNumber)
        {
            return await _context.Students.AnyAsync(s =>
                s.CollegeId == collegeId &&
                s.RegisterNumber == registerNumber);
        }

        public async Task AddAsync(Student student)
        {
            await _context.Students.AddAsync(student);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}