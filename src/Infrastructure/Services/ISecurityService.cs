using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services;
public interface ISecurityService
{
    public Task<string> GenerateTokenAsync();

    public Task<string> HashPasswordAsync(string password);
}
