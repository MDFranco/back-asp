using Xunit;
using Moq;
using WebAppiN5now.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebAppiN5now.Model;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using WebAppiN5now.Data;

namespace PermissionsApi.Tests.Controllers
{
    public class PermissionsControllerTests
    {
        private readonly PermissionsController _controller;
        private readonly Mock<DbSet<Permission>> _mockSet;
        private readonly Mock<ApplicationDbContext> _mockContext;

        public PermissionsControllerTests()
        {
            _mockSet = new Mock<DbSet<Permission>>();
            _mockContext = new Mock<ApplicationDbContext>();

            _controller = new PermissionsController(_mockContext.Object, null, null);
        }

        [Fact]
        public async Task GetPermissions_ReturnsOkResult_WithAListOfPermissions()
        {
            // Arrange
            var permissions = new List<Permission> { new Permission { Id = 1, NombreEmpleado = "Juan", ApellidoEmpleado = "Perez", TipoPermiso = 1, FechaPermiso = DateTime.Now } };
            _mockSet.Setup(m => m.ToListAsync()).ReturnsAsync(permissions);
            _mockContext.Setup(c => c.Permissions).Returns(_mockSet.Object);

            // Act
            var result = await _controller.GetPermissions();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnPermissions = Assert.IsType<List<Permission>>(okResult.Value);
            Assert.Equal(1, returnPermissions.Count);
        }
    }
}
