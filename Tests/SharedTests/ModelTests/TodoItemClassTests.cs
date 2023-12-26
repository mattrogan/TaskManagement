using TaskManagement.Shared.Models;

namespace TaskManagement.Tests.SharedTests.ModelTests
{
    [TestClass]
    public class TodoItemClassTests
    {
        [TestMethod]
        public void TodoItem_Constructor_ShouldInstantiateWhenParameterless()
        {
            var model = new TodoItem();
            Assert.IsNotNull(model);
        }

        [TestMethod]
        public void TodoItem_Constructor_ShouldPopulateTheModel()
        {
            var dateNow = DateTime.Now;
            var model = new TodoItem("Foo", "Bar", dateNow);

            Assert.IsNotNull(model);

            Assert.AreEqual("Foo", model.Title);
            Assert.AreEqual("Bar", model.Description);
            Assert.AreEqual(dateNow, model.DueDate);
        }

        [TestMethod]
        public void TodoItem_Equals_ShouldReturnTrueWhenObjectsEqual()
        {
            var dateNow = DateTime.Now;
            var task1 = new TodoItem("Foo", "Bar", dateNow);
            var task2 = new TodoItem("Foo", "Bar", dateNow);

            Assert.AreEqual(task1, task2);
        }

        [TestMethod]
        public void TodoItem_Equals_ShouldReturnFalseWhenObjectsNotEqual()
        {
            var dateNow = DateTime.Now;
            var task1 = new TodoItem("1", "One", dateNow);
            var task2 = new TodoItem("2", "Two", dateNow.AddDays(3));

            Assert.AreNotEqual(task1, task2);
        }
    }
}
