using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFCProj.Helpers
{
    public static class ValidationHelper
    {
        public static (bool isValid, string[] errors) Validate<T>(T input) where T : class
        {
            var context = new ValidationContext(input);
            var results = new List<ValidationResult>();

            if (Validator.TryValidateObject(input, context, results, true))
            {
                return (true, Array.Empty<string>());
            }
            else
            {
                return (false, results.Select(t => t.ErrorMessage).ToArray());
            }
        }
    }
}
