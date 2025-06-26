using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Exceptions
{
    public class PortfolioNotFoundException : Exception
    {
        public PortfolioNotFoundException(int portfolioId)
            : base($"Portfolio with ID {portfolioId} was not found") { }
    }

    public class PortfolioValidationException : Exception
    {
        public PortfolioValidationException(string message) : base(message) { }
    }

    public class UnauthorizedPortfolioAccessException : Exception
    {
        public UnauthorizedPortfolioAccessException(int userId, int portfolioId)
            : base($"User {userId} is not authorized to access portfolio {portfolioId}") { }
    }
}
