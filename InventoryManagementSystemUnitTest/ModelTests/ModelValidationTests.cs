using Inventory_Management_System.Models;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace InventoryManagementSystemUnitTest.ModelTests
{
    public class ModelValidationTests
    {
        [Theory]
        [InlineData("", 5, false)]
        [InlineData("Monitor", -2, false)]
        [InlineData("Mouse", 10, true)]
        public void Item_Model_Should_Respect_Validation(string name, int qty, bool expected)
        {
            var model = new Item { Name = name, Quantity = qty };
            var context = new ValidationContext(model);
            var results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(model, context, results, true);

            Assert.Equal(expected, isValid);
        }
    }
}
