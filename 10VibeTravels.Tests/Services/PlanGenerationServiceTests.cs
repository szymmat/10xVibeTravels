using _10xVibeTravels.Services;
using _10xVibeTravels.Interfaces;
using _10xVibeTravels.Data;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.EntityFrameworkCore;
using _10xVibeTravels.Requests;
using _10xVibeTravels.Models;
using _10xVibeTravels.Dtos;
using _10xVibeTravels.Exceptions;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using System.Net; // Added for HttpStatusCode

namespace _10VibeTravels.Tests.Services
{
    [TestFixture]
    public class PlanGenerationServiceTests
    {
        private Mock<IDbContextFactory<ApplicationDbContext>> _mockContextFactory;
        private Mock<ApplicationDbContext> _mockDbContext;
        private Mock<ILogger<PlanGenerationService>> _mockLogger;
        private Mock<IOpenRouterService> _mockOpenRouterService;
        private PlanGenerationService _planGenerationService;

        private Mock<DbSet<Note>> _mockNotesSet;
        private Mock<DbSet<UserProfile>> _mockUserProfilesSet;
        private Mock<DbSet<UserInterest>> _mockUserInterestsSet;
        private Mock<DbSet<Plan>> _mockPlansSet;

        // Backing lists for test data
        private List<Note> _testNotesData;
        private List<UserProfile> _testUserProfilesData;
        private List<UserInterest> _testUserInterestsData;
        private List<Plan> _testPlansData;

        [SetUp]
        public void Setup()
        {
            _mockContextFactory = new Mock<IDbContextFactory<ApplicationDbContext>>();
            _mockDbContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            _mockLogger = new Mock<ILogger<PlanGenerationService>>();
            _mockOpenRouterService = new Mock<IOpenRouterService>();

            _mockContextFactory.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_mockDbContext.Object);

            // Initialize data lists
            _testNotesData = new List<Note>();
            _testUserProfilesData = new List<UserProfile>();
            _testUserInterestsData = new List<UserInterest>();
            _testPlansData = new List<Plan>();

            // Setup Notes DbSet
            _mockNotesSet = new Mock<DbSet<Note>>();
            ConfigureDbSetMock(_mockNotesSet, _testNotesData);
            _mockDbContext.Setup(c => c.Notes).Returns(_mockNotesSet.Object);

            // Setup UserProfiles DbSet
            _mockUserProfilesSet = new Mock<DbSet<UserProfile>>();
            ConfigureDbSetMock(_mockUserProfilesSet, _testUserProfilesData);
            _mockDbContext.Setup(c => c.UserProfiles).Returns(_mockUserProfilesSet.Object);

            // Setup UserInterests DbSet
            _mockUserInterestsSet = new Mock<DbSet<UserInterest>>();
            ConfigureDbSetMock(_mockUserInterestsSet, _testUserInterestsData);
            _mockDbContext.Setup(c => c.UserInterests).Returns(_mockUserInterestsSet.Object);

            // Setup Plans DbSet
            _mockPlansSet = new Mock<DbSet<Plan>>();
            ConfigureDbSetMock(_mockPlansSet, _testPlansData);
            _mockDbContext.Setup(c => c.Plans).Returns(_mockPlansSet.Object);
            
            _mockDbContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                         .ReturnsAsync(1); 

            _planGenerationService = new PlanGenerationService(
                _mockContextFactory.Object,
                _mockLogger.Object,
                _mockOpenRouterService.Object);
        }

        private void ConfigureDbSetMock<T>(Mock<DbSet<T>> mockSet, List<T> backingListData) where T : class
        {
            var queryableData = backingListData.AsQueryable();

            mockSet.As<IAsyncEnumerable<T>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(() => new TestAsyncEnumerator<T>(backingListData.AsQueryable().GetEnumerator()));

            mockSet.As<IQueryable<T>>().Setup(m => m.Provider)
                .Returns(() => new TestAsyncQueryProvider<T>(backingListData.AsQueryable().Provider));
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression)
                .Returns(() => backingListData.AsQueryable().Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType)
                .Returns(() => backingListData.AsQueryable().ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator())
                .Returns(() => backingListData.AsQueryable().GetEnumerator());

            // For methods like Add, AddRangeAsync, Remove, etc.
            mockSet.Setup(m => m.Add(It.IsAny<T>())).Callback<T>(item => backingListData.Add(item));
            mockSet.Setup(m => m.AddRange(It.IsAny<IEnumerable<T>>())).Callback<IEnumerable<T>>(items => backingListData.AddRange(items));
            mockSet.Setup(m => m.Remove(It.IsAny<T>())).Callback<T>(item => backingListData.Remove(item));
            // AddRangeAsync is an extension method on DbSet, but it internally calls AddRange. 
            // So, mocking AddRange should be sufficient for it to be reflected in the backing list.
            // However, the service uses AddRangeAsync directly so we might need to mock it on the DbContext.Plans specifically if an issue arises.
        }

        [Test]
        public async Task GeneratePlanProposalsAsync_ValidRequest_ReturnsProposals()
        {
            // Arrange
            var userId = "testUser";
            var noteId = Guid.NewGuid();
            var request = new GeneratePlanProposalRequest
            {
                NoteId = noteId,
                StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5)),
                Budget = 500
            };

            var note = new Note { Id = noteId, UserId = userId, Content = "Test note content", Title = "Test Title" };
            var userProfile = new UserProfile
            {
                UserId = userId,
                User = new ApplicationUser { Id = userId },
                TravelStyle = new TravelStyle { Id = Guid.NewGuid(), Name = "Relaxed" },
                Intensity = new Intensity { Id = Guid.NewGuid(), Name = "Low" },
                Budget = 1000
            };
            var userInterests = new List<UserInterest>
            {
                new UserInterest { UserId = userId, InterestId = Guid.NewGuid(), Interest = new Interest { Id = Guid.NewGuid(), Name = "Sightseeing" } }
            };

            _testNotesData.Clear(); _testNotesData.Add(note);
            _testUserProfilesData.Clear(); _testUserProfilesData.Add(userProfile);
            _testUserInterestsData.Clear(); _testUserInterestsData.AddRange(userInterests);
            _testPlansData.Clear();

            var aiResponse = new TravelPlanAIResponse
            {
                Items = new List<_10xVibeTravels.Dtos.GeneratedPlanDto>
                {
                    new _10xVibeTravels.Dtos.GeneratedPlanDto { Title = "Plan 1", Content = "Content 1" },
                    new _10xVibeTravels.Dtos.GeneratedPlanDto { Title = "Plan 2", Content = "Content 2" },
                    new _10xVibeTravels.Dtos.GeneratedPlanDto { Title = "Plan 3", Content = "Content 3" }
                }
            };
            _mockOpenRouterService.Setup(s => s.SendChatAsync<TravelPlanAIResponse>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<_10xVibeTravels.Models.ModelParameters>(), It.IsAny<ResponseFormat>()
            )).ReturnsAsync(aiResponse);
            
            // For AddRangeAsync specifically on Plans, as it returns a Task and not void.
            _mockPlansSet.Setup(s => s.AddRangeAsync(It.IsAny<IEnumerable<Plan>>(), It.IsAny<CancellationToken>()))
                .Returns((IEnumerable<Plan> items, CancellationToken ct) => {
                    _testPlansData.AddRange(items);
                    return Task.CompletedTask;
                });
            _mockDbContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(3); 

            // Act
            var result = await _planGenerationService.GeneratePlanProposalsAsync(request, userId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);
            _mockOpenRouterService.Verify(s => s.SendChatAsync<TravelPlanAIResponse>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<_10xVibeTravels.Models.ModelParameters>(), It.IsAny<ResponseFormat>()), Times.Once);
            _mockPlansSet.Verify(db => db.AddRangeAsync(It.Is<List<Plan>>(plans => plans.Count == 3), It.IsAny<CancellationToken>()), Times.Once);
            _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once); 
        }
        
        [Test]
        public void GeneratePlanProposalsAsync_NoteNotFound_ThrowsNoteNotFoundException()
        {
            // Arrange
            var userId = "testUser";
            var request = new GeneratePlanProposalRequest { NoteId = Guid.NewGuid() };

            _testNotesData.Clear(); 
            _testUserProfilesData.Clear(); _testUserProfilesData.Add(new UserProfile { UserId = userId, User = new ApplicationUser { Id = userId }});
            _testUserInterestsData.Clear();

            // Act & Assert
            Assert.ThrowsAsync<NoteNotFoundException>(
                async () => await _planGenerationService.GeneratePlanProposalsAsync(request, userId));
        }

        [Test]
        public void GeneratePlanProposalsAsync_UserProfileNotFound_ThrowsApplicationException()
        {
            // Arrange
            var userId = "testUser";
            var noteId = Guid.NewGuid();
            var request = new GeneratePlanProposalRequest { NoteId = noteId };
            var note = new Note { Id = noteId, UserId = userId, Content = "Test", Title = "Test Title" };

            _testNotesData.Clear(); _testNotesData.Add(note);
            _testUserProfilesData.Clear();
            _testUserInterestsData.Clear();

            // Act & Assert
            var ex = Assert.ThrowsAsync<ApplicationException>(
                async () => await _planGenerationService.GeneratePlanProposalsAsync(request, userId));
            Assert.That(ex.Message, Is.EqualTo($"UserProfile not found for user '{userId}'."));
        }

        [Test]
        public void GeneratePlanProposalsAsync_NoteForbidden_ThrowsNoteForbiddenException()
        {
            // Arrange
            var ownerUserId = "ownerUser";
            var attackerUserId = "attackerUser";
            var noteId = Guid.NewGuid();
            var request = new GeneratePlanProposalRequest { NoteId = noteId };
            var note = new Note { Id = noteId, UserId = ownerUserId, Title="T", Content = "C" }; 

            _testNotesData.Clear(); _testNotesData.Add(note);
            var attackerProfile = new UserProfile { UserId = attackerUserId, User = new ApplicationUser { Id = attackerUserId } };
            _testUserProfilesData.Clear(); _testUserProfilesData.Add(attackerProfile);
            _testUserInterestsData.Clear();

            // Act & Assert
            Assert.ThrowsAsync<NoteForbiddenException>(
                async () => await _planGenerationService.GeneratePlanProposalsAsync(request, attackerUserId));
        }

        [Test]
        public void GeneratePlanProposalsAsync_InvalidDateRange_ThrowsInvalidDateRangeException()
        {
            // Arrange
            var userId = "testUser";
            var noteId = Guid.NewGuid();
            var request = new GeneratePlanProposalRequest
            {
                NoteId = noteId,
                StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5)),
                EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)) 
            };
            var note = new Note { Id = noteId, UserId = userId, Title="T", Content = "C" };
            var userProfile = new UserProfile { UserId = userId, User = new ApplicationUser { Id = userId } };

            _testNotesData.Clear(); _testNotesData.Add(note);
            _testUserProfilesData.Clear(); _testUserProfilesData.Add(userProfile);
            _testUserInterestsData.Clear();

            // Act & Assert
            Assert.ThrowsAsync<InvalidDateRangeException>(
                async () => await _planGenerationService.GeneratePlanProposalsAsync(request, userId));
        }

        [Test]
        public void GeneratePlanProposalsAsync_BudgetNotAvailable_ThrowsBudgetNotAvailableException()
        {
            // Arrange
            var userId = "testUser";
            var noteId = Guid.NewGuid();
            var request = new GeneratePlanProposalRequest
            {
                NoteId = noteId,
                StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5)),
                Budget = null 
            };
            var note = new Note { Id = noteId, UserId = userId, Title="T", Content="C" };
            var userProfile = new UserProfile { UserId = userId, Budget = null, User = new ApplicationUser{ Id = userId} }; 

            _testNotesData.Clear(); _testNotesData.Add(note);
            _testUserProfilesData.Clear(); _testUserProfilesData.Add(userProfile);
            _testUserInterestsData.Clear();

            // Act & Assert
            Assert.ThrowsAsync<BudgetNotAvailableException>(
                async () => await _planGenerationService.GeneratePlanProposalsAsync(request, userId));
        }

        [Test]
        public void GeneratePlanProposalsAsync_AIServiceReturnsNull_ThrowsAiServiceException()
        {
            // Arrange
            var userId = "testUser";
            var noteId = Guid.NewGuid();
            var request = new GeneratePlanProposalRequest
            {
                NoteId = noteId,
                StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5)),
                Budget = 500
            };
            var note = new Note { Id = noteId, UserId = userId, Content = "Test content", Title="T" };
            var userProfile = new UserProfile { UserId = userId, Budget = 1000, User = new ApplicationUser { Id = userId } };

            _testNotesData.Clear(); _testNotesData.Add(note);
            _testUserProfilesData.Clear(); _testUserProfilesData.Add(userProfile);
            _testUserInterestsData.Clear();

            _mockOpenRouterService.Setup(s => s.SendChatAsync<TravelPlanAIResponse>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<_10xVibeTravels.Models.ModelParameters>(), It.IsAny<ResponseFormat>()
            )).ReturnsAsync((TravelPlanAIResponse)null!); 

            // Act & Assert
            var ex = Assert.ThrowsAsync<AiServiceException>(
                async () => await _planGenerationService.GeneratePlanProposalsAsync(request, userId));
            Assert.That(ex.Message, Is.EqualTo("Error communicating with the AI service: Failed to generate plans due to an AI service error."));
        }

        [Test]
        public void GeneratePlanProposalsAsync_AIServiceReturnsNotThreePlans_ThrowsAiServiceException()
        {
            // Arrange
            var userId = "testUser";
            var noteId = Guid.NewGuid();
            var request = new GeneratePlanProposalRequest
            {
                NoteId = noteId,
                StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5)),
                Budget = 500
            };
            var note = new Note { Id = noteId, UserId = userId, Content = "Test content", Title="T" };
            var userProfile = new UserProfile { UserId = userId, Budget = 1000, User = new ApplicationUser { Id = userId } };

            _testNotesData.Clear(); _testNotesData.Add(note);
            _testUserProfilesData.Clear(); _testUserProfilesData.Add(userProfile);
            _testUserInterestsData.Clear();

            var aiResponse = new TravelPlanAIResponse
            {
                Items = new List<_10xVibeTravels.Dtos.GeneratedPlanDto> { new _10xVibeTravels.Dtos.GeneratedPlanDto { Title = "P1", Content = "C1"} } 
            };
            _mockOpenRouterService.Setup(s => s.SendChatAsync<TravelPlanAIResponse>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<_10xVibeTravels.Models.ModelParameters>(), It.IsAny<ResponseFormat>()
            )).ReturnsAsync(aiResponse);

            // Act & Assert
            var ex = Assert.ThrowsAsync<AiServiceException>(
                async () => await _planGenerationService.GeneratePlanProposalsAsync(request, userId));
            Assert.That(ex.Message, Is.EqualTo("Error communicating with the AI service: Failed to generate plans due to an AI service error."));
        }

        [Test]
        public void GeneratePlanProposalsAsync_AIServiceThrowsOpenRouterException_ThrowsAiServiceException()
        {
            // Arrange
            var userId = "testUser";
            var noteId = Guid.NewGuid();
            var request = new GeneratePlanProposalRequest
            {
                NoteId = noteId,
                StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5)),
                Budget = 500
            };
            var note = new Note { Id = noteId, UserId = userId, Content = "Test content", Title="T" };
            var userProfile = new UserProfile { UserId = userId, Budget = 1000, User = new ApplicationUser { Id = userId } };

            _testNotesData.Clear(); _testNotesData.Add(note);
            _testUserProfilesData.Clear(); _testUserProfilesData.Add(userProfile);
            _testUserInterestsData.Clear();

            _mockOpenRouterService.Setup(s => s.SendChatAsync<TravelPlanAIResponse>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<_10xVibeTravels.Models.ModelParameters>(), It.IsAny<ResponseFormat>()
            )).ThrowsAsync(new OpenRouterException("AI service error", HttpStatusCode.InternalServerError));

            // Act & Assert
            var ex = Assert.ThrowsAsync<AiServiceException>(
                async () => await _planGenerationService.GeneratePlanProposalsAsync(request, userId));
            Assert.That(ex.Message, Is.EqualTo("Error communicating with the AI service: Failed to generate plans due to an AI service error: AI service error"));
            Assert.IsInstanceOf<OpenRouterException>(ex.InnerException);
        }
        
        [Test]
        public void GeneratePlanProposalsAsync_AIServiceThrowsGenericException_ThrowsAiServiceException()
        {
            // Arrange
            var userId = "testUser";
            var noteId = Guid.NewGuid();
            var request = new GeneratePlanProposalRequest
            {
                NoteId = noteId,
                StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5)),
                Budget = 500
            };
            var note = new Note { Id = noteId, UserId = userId, Content = "Test content", Title="T" };
            var userProfile = new UserProfile { UserId = userId, Budget = 1000, User = new ApplicationUser { Id = userId } };

            _testNotesData.Clear(); _testNotesData.Add(note);
            _testUserProfilesData.Clear(); _testUserProfilesData.Add(userProfile);
            _testUserInterestsData.Clear();

            _mockOpenRouterService.Setup(s => s.SendChatAsync<TravelPlanAIResponse>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<_10xVibeTravels.Models.ModelParameters>(), It.IsAny<ResponseFormat>()
            )).ThrowsAsync(new Exception("Generic AI error"));

            // Act & Assert
            var ex = Assert.ThrowsAsync<AiServiceException>(
                async () => await _planGenerationService.GeneratePlanProposalsAsync(request, userId));
            Assert.That(ex.Message, Is.EqualTo("Error communicating with the AI service: Failed to generate plans due to an AI service error."));
             Assert.IsInstanceOf<Exception>(ex.InnerException);
            Assert.That(ex.InnerException.Message, Is.EqualTo("Generic AI error"));
        }

        [Test]
        public void GeneratePlanProposalsAsync_DbUpdateExceptionOnSave_ThrowsApplicationException()
        {
            // Arrange
            var userId = "testUser";
            var noteId = Guid.NewGuid();
            var request = new GeneratePlanProposalRequest
            {
                NoteId = noteId,
                StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5)),
                Budget = 500
            };
            var note = new Note { Id = noteId, UserId = userId, Content = "Test content", Title="T" };
            var userProfile = new UserProfile { UserId = userId, Budget = 1000, User = new ApplicationUser { Id = userId } };

            _testNotesData.Clear(); _testNotesData.Add(note);
            _testUserProfilesData.Clear(); _testUserProfilesData.Add(userProfile);
            _testUserInterestsData.Clear();
            _testPlansData.Clear();

            var aiResponse = new TravelPlanAIResponse
            {
                Items = new List<_10xVibeTravels.Dtos.GeneratedPlanDto>
                {
                    new _10xVibeTravels.Dtos.GeneratedPlanDto { Title = "P1", Content = "C1" },
                    new _10xVibeTravels.Dtos.GeneratedPlanDto { Title = "P2", Content = "C2" },
                    new _10xVibeTravels.Dtos.GeneratedPlanDto { Title = "P3", Content = "C3" }
                }
            };
            _mockOpenRouterService.Setup(s => s.SendChatAsync<TravelPlanAIResponse>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<_10xVibeTravels.Models.ModelParameters>(), It.IsAny<ResponseFormat>()
            )).ReturnsAsync(aiResponse);

            _mockPlansSet.Setup(s => s.AddRangeAsync(It.IsAny<IEnumerable<Plan>>(), It.IsAny<CancellationToken>()))
                .Returns((IEnumerable<Plan> items, CancellationToken ct) => {
                    _testPlansData.AddRange(items);
                    return Task.CompletedTask;
                });
            _mockDbContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new DbUpdateException("DB error", new Exception("Inner DB error")));

            // Act & Assert
            var ex = Assert.ThrowsAsync<ApplicationException>(
                async () => await _planGenerationService.GeneratePlanProposalsAsync(request, userId));
            Assert.That(ex.Message, Is.EqualTo("An error occurred while saving the plan proposals."));
            Assert.IsInstanceOf<DbUpdateException>(ex.InnerException);
        }

        // Tests for AcceptPlanProposalAsync
        [Test]
        public async Task AcceptPlanProposalAsync_ValidPlan_UpdatesStatusAndModifiedAt()
        {
            // Arrange
            var userId = "testUser";
            var planId = Guid.NewGuid();
            var originalModifiedAt = DateTime.UtcNow.AddMinutes(-5);
            var plan = new Plan { Id = planId, UserId = userId, Status = PlanStatus.Generated.ToString(), ModifiedAt = originalModifiedAt, Title = "T", Content="C" };
            
            _testPlansData.Clear(); _testPlansData.Add(plan);
            // Ensure SaveChangesAsync is verifiable for this test if needed, or reset mocks.
            // For this specific test, we expect SaveChangesAsync to be called once.
            _mockDbContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1).Verifiable();

            // Act
            await _planGenerationService.AcceptPlanProposalAsync(userId, planId);

            // Assert
            Assert.AreEqual(PlanStatus.Accepted.ToString(), plan.Status);
            Assert.IsTrue(plan.ModifiedAt > originalModifiedAt);
            _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void AcceptPlanProposalAsync_PlanNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var userId = "testUser";
            var planId = Guid.NewGuid();

            _testPlansData.Clear();

            // Act & Assert
            var ex = Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _planGenerationService.AcceptPlanProposalAsync(userId, planId));
            Assert.That(ex.Message, Is.EqualTo($"Plan with ID {planId} not found for user {userId}."));
        }

        [Test]
        public void AcceptPlanProposalAsync_PlanNotGeneratedStatus_ThrowsInvalidOperationException()
        {
            // Arrange
            var userId = "testUser";
            var planId = Guid.NewGuid();
            var plan = new Plan { Id = planId, UserId = userId, Status = PlanStatus.Accepted.ToString(), Title="T", Content="c" };

            _testPlansData.Clear(); _testPlansData.Add(plan);

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _planGenerationService.AcceptPlanProposalAsync(userId, planId));
            Assert.That(ex.Message, Is.EqualTo("Only plans with status 'Generated' can be accepted."));
        }

        // Tests for RejectPlanProposalAsync
        [Test]
        public async Task RejectPlanProposalAsync_ValidPlan_UpdatesStatusAndModifiedAt()
        {
            // Arrange
            var userId = "testUser";
            var planId = Guid.NewGuid();
            var originalModifiedAt = DateTime.UtcNow.AddMinutes(-5);
            var plan = new Plan { Id = planId, UserId = userId, Status = PlanStatus.Generated.ToString(), ModifiedAt = originalModifiedAt, Title="T", Content="c" };

            _testPlansData.Clear(); _testPlansData.Add(plan);
            _mockDbContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1).Verifiable();

            // Act
            await _planGenerationService.RejectPlanProposalAsync(userId, planId);

            // Assert
            Assert.AreEqual(PlanStatus.Rejected.ToString(), plan.Status);
            Assert.IsTrue(plan.ModifiedAt > originalModifiedAt);
            _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void RejectPlanProposalAsync_PlanNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var userId = "testUser";
            var planId = Guid.NewGuid();

            _testPlansData.Clear();

            // Act & Assert
            var ex = Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _planGenerationService.RejectPlanProposalAsync(userId, planId));
            Assert.That(ex.Message, Is.EqualTo($"Plan with ID {planId} not found for user {userId}."));
        }

        [Test]
        public void RejectPlanProposalAsync_PlanNotGeneratedStatus_ThrowsInvalidOperationException()
        {
            // Arrange
            var userId = "testUser";
            var planId = Guid.NewGuid();
            var plan = new Plan { Id = planId, UserId = userId, Status = PlanStatus.Rejected.ToString(), Title="T", Content="c" };

            _testPlansData.Clear(); _testPlansData.Add(plan);

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _planGenerationService.RejectPlanProposalAsync(userId, planId));
            Assert.That(ex.Message, Is.EqualTo("Only plans with status 'Generated' can be rejected."));
        }
    }
}

// Helper classes for mocking EF Core async operations (TestAsyncQueryProvider, TestAsyncEnumerator)
// These would typically be in a shared test utility file.
// Source: https://learn.microsoft.com/en-us/ef/ef6/testing/mocking?redirectedfrom=MSDN
// Adapted for EF Core

internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    internal TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner;
    }

    public IQueryable CreateQuery(Expression expression)
    {
        return new TestAsyncEnumerable<TEntity>(expression);
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new TestAsyncEnumerable<TElement>(expression);
    }

    public object Execute(Expression expression)
    {
        return _inner.Execute(expression);
    }

    public TResult Execute<TResult>(Expression expression)
    {
        return _inner.Execute<TResult>(expression);
    }

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
    {
        var task = Task.FromResult(Execute(expression));
        
        // For ToListAsync, FirstOrDefaultAsync, etc.
        if (typeof(TResult).IsGenericType && typeof(TResult).GetGenericTypeDefinition() == typeof(Task<>)) {
            var resultType = typeof(TResult).GetGenericArguments()[0];
            var executionResult = Execute(expression);

            if (executionResult is IAsyncEnumerable<TEntity> asyncEnumerableResult)
            {
                //This part is tricky, as ToListAsync would need to enumerate. Returning a Task of List for simplicity here.
                //This might not cover all cases perfectly but is common for mocking.
                if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    var listMethod = typeof(Enumerable).GetMethod(nameof(Enumerable.ToList)).MakeGenericMethod(resultType.GetGenericArguments()[0]);
                    executionResult = listMethod.Invoke(null, new object[] { asyncEnumerableResult });
                }
            }
             return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))
                               .MakeGenericMethod(resultType)
                               .Invoke(null, new[] { executionResult });
        }
        
        // For CountAsync, AnyAsync etc.
        return (TResult)(object)task; // This might need more specific handling for non-generic Task results like int for CountAsync
    }
}

internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable)
        : base(enumerable)
    { }

    public TestAsyncEnumerable(Expression expression)
        : base(expression)
    { }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
}

internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner;
    }

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> MoveNextAsync()
    {
        return new ValueTask<bool>(_inner.MoveNext());
    }

    public T Current => _inner.Current;
} 