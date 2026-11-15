using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using MobileLoginOtp.Domain.Entities;
using MobileLoginOtp.Domain.Interfaces;
using MobileLoginOtp.Infrastructure.Data;
using System.Linq;
using System.Threading.Tasks;
using MobileLoginOtp.Application.Interfaces;

namespace MobileLoginOtp.Infrastructure.Repositories
{
    public class OtpRepository : IOtpRepository
    {
        private readonly AppDbContext _context;

        public OtpRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddOtpRequestAsync(OtpRequest otpRequest)
        {
            await _context.OtpRequests.AddAsync(otpRequest);
        }

        public async Task<OtpRequest?> GetLatestUnusedOtpAsync(string phoneNumber)
        {
            return await _context.OtpRequests
                .Where(x => x.PhoneNumber == phoneNumber && !x.IsUsed)
                .OrderByDescending(x => x.CreatedAtUtc)
                .FirstOrDefaultAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}

