using Moq;
using MS.Core.Constants;
using MS.DataAccess.Context;
using MS.DataAccess.Setup;
using MS.DataAccess.UOW;
using Xunit;

namespace MS.DataAccess.Tests.UOW
{
    public class UnitOfWorkTests
    {
        private readonly UnitOfWork<IDataContext> _unitOfWork;
        private readonly Mock<IDataContext> _dataContextMock;
        private readonly Mock<IDataContext> _readOnlyDataContextMock;

        public UnitOfWorkTests()
        {
            _dataContextMock = new Mock<IDataContext>();
            _readOnlyDataContextMock = new Mock<IDataContext>();
            _unitOfWork = new UnitOfWork<IDataContext>(_dataContextMock.Object, _readOnlyDataContextMock.Object);
        }

        [Fact]
        [Trait(TestConstants.TEST_CATEGORY_LABEL, TestConstants.TEST_CATEGORY_UNIT)]
        public void WhenRepositoryRequested_GetRepository_ShouldReturnTheSameRepositoryOnSequentialCall()
        {
            var repo = _unitOfWork.GetRepository<Entity<IDataContext>>();
            var repoRequestedAgain = _unitOfWork.GetRepository<Entity<IDataContext>>();

            Assert.Equal(repo, repoRequestedAgain);
        }

        [Fact]
        [Trait(TestConstants.TEST_CATEGORY_LABEL, TestConstants.TEST_CATEGORY_UNIT)]
        public void WhenReadRepositoryRequested_GetReadOnlyRepository_ShouldReturnTheSameRepositoryOnSequentialCall()
        {
            var repo = _unitOfWork.GetReadOnlyRepository<Entity<IDataContext>>();
            var repoRequestedAgain = _unitOfWork.GetReadOnlyRepository<Entity<IDataContext>>();

            Assert.Equal(repo, repoRequestedAgain);
        }
    }
}