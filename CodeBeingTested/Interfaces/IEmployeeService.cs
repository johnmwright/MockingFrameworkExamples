using System.Collections.Generic;

namespace CodeBeingTested.Interfaces
{
    public interface IEmployeeService
    {
        IEnumerable<IEmployee> GetEmployeesInNewYorkOffice();
    }
}