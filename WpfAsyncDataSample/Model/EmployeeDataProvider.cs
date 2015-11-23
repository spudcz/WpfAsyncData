using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WpfAsyncDataSample.Model
{
    public class EmployeeDataProvider
    {
        public Task<DataResult<IList<Employee>>> LoadEmployeesAsync(CancellationToken token, IProgress<double> progress)
        {
            return Task.Run(() => LoadEmployees(token, progress), token);
        }

        private DataResult<IList<Employee>> LoadEmployees(CancellationToken token, IProgress<double> progress)
        {
            var employees = XDocument.Load(@"data\sampledata.xml")
                .Descendants("Employee")
                .Select(x => new Employee {
                    Number = int.Parse(x.Descendants("Number").First().Value),
                    Name = x.Descendants("Name").First().Value,
                    Surname = x.Descendants("Surname").First().Value,
                    Salary = double.Parse(x.Descendants("Salary").First().Value)
                })
                .ToList();

            var itemIndex = 0;
            var result = new DataResult<IList<Employee>> {
                Result = new List<Employee>()
            };

            try {
                foreach (var employee in employees) {
                    Thread.CurrentThread.Join(100);
                    result.Result.Add(employee);
                    token.ThrowIfCancellationRequested();
                    progress.Report((++itemIndex*100)/employees.Count);
                }
            }
            catch (OperationCanceledException ex) {
                result.Exception = ex;
            }

            return result;
        }
    }
}