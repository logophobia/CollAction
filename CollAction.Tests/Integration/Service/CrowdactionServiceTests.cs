﻿using CollAction.Data;
using CollAction.GraphQl.Mutations.Input;
using CollAction.Models;
using CollAction.Services.Crowdactions;
using CollAction.Services.Crowdactions.Models;
using CollAction.Services.Email;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CollAction.Tests.Integration.Service
{
    [Trait("Category", "Integration")]
    public sealed class CrowdactionServiceTests : IntegrationTestBase
    {
        private readonly ICrowdactionService crowdactionService;
        private readonly ApplicationDbContext context;
        private readonly SignInManager<ApplicationUser> signInManager;

        public CrowdactionServiceTests() : base(false)
        {
            crowdactionService = Scope.ServiceProvider.GetRequiredService<ICrowdactionService>();
            context = Scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            signInManager = Scope.ServiceProvider.GetRequiredService<SignInManager<ApplicationUser>>();
        }

        [Fact]
        public async Task TestCrowdactionCreate()
        {
            var user = await context.Users.FirstAsync();
            var claimsPrincipal = await signInManager.CreateUserPrincipalAsync(user);
            var r = new Random();

            var newCrowdaction =
                new NewCrowdaction()
                {
                    Name = "test" + Guid.NewGuid(),
                    Categories = new List<Category>() { Category.Other, Category.Health },
                    Description = Guid.NewGuid().ToString(),
                    DescriptionVideoLink = "https://www.youtube.com/embed/xY0XTysJUDY",
                    End = DateTime.Now.AddDays(30),
                    Start = DateTime.Now.AddDays(10),
                    Goal = Guid.NewGuid().ToString(),
                    CreatorComments = Guid.NewGuid().ToString(),
                    Proposal = Guid.NewGuid().ToString(),
                    Target = 40,
                    Tags = new string[3] { $"a{r.Next(1000)}", $"b{r.Next(1000)}", $"c{r.Next(1000)}" }
                };
            CrowdactionResult crowdactionResult = await crowdactionService.CreateCrowdaction(newCrowdaction, claimsPrincipal, CancellationToken.None);
            int? crowdactionId = crowdactionResult.Crowdaction?.Id;
            Assert.NotNull(crowdactionId);
            Crowdaction retrievedCrowdaction = await context.Crowdactions.Include(c => c.Tags).ThenInclude(t => t.Tag).FirstOrDefaultAsync(c => c.Id == crowdactionId);
            Assert.NotNull(retrievedCrowdaction);

            Assert.True(crowdactionResult.Succeeded);
            Assert.False(crowdactionResult.Errors.Any());
            Assert.Equal(crowdactionResult.Crowdaction?.Name, retrievedCrowdaction.Name);
            Assert.True(Enumerable.SequenceEqual(crowdactionResult.Crowdaction?.Tags.Select(t => t.Tag.Name).OrderBy(t => t), retrievedCrowdaction.Tags.Select(t => t.Tag.Name).OrderBy(t => t)));
        }

        [Fact]
        public async Task TestCrowdactionUpdate()
        {
            var user = await context.Users.FirstAsync();
            var newCrowdaction = new NewCrowdactionInternal("test" + Guid.NewGuid(), 100, "test", "test", "test", null, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), null, null, null, null, new[] { Category.Community }, Array.Empty<string>(), CrowdactionDisplayPriority.Bottom, CrowdactionStatus.Running, 0, user.Id);
            Crowdaction crowdaction = await crowdactionService.CreateCrowdactionInternal(newCrowdaction, CancellationToken.None);
            Assert.NotNull(crowdaction);

            Crowdaction currentCrowdaction = await context.Crowdactions.Include(c => c.Owner).FirstAsync(c => c.OwnerId != null);
            var owner = await signInManager.CreateUserPrincipalAsync(currentCrowdaction.Owner ?? throw new InvalidOperationException("Owner is null"));
            var r = new Random();
            var updatedCrowdaction =
                new UpdatedCrowdaction()
                {
                    Name = Guid.NewGuid().ToString(),
                    BannerImageFileId = currentCrowdaction.BannerImageFileId,
                    Categories = new[] { Category.Community, Category.Environment },
                    CreatorComments = currentCrowdaction.CreatorComments,
                    Description = currentCrowdaction.Description,
                    OwnerId = currentCrowdaction.OwnerId,
                    DescriptionVideoLink = "https://www.youtube-nocookie.com/embed/xY0XTysJUDY",
                    DescriptiveImageFileId = currentCrowdaction.DescriptiveImageFileId,
                    DisplayPriority = CrowdactionDisplayPriority.Top,
                    End = DateTime.Now.AddDays(30),
                    Start = DateTime.Now.AddDays(10),
                    Goal = Guid.NewGuid().ToString(),
                    Tags = new string[3] { $"a{r.Next(1000)}", $"b{r.Next(1000)}", $"c{r.Next(1000)}" },
                    Id = currentCrowdaction.Id,
                    NumberCrowdactionEmailsSent = 3,
                    Proposal = currentCrowdaction.Proposal,
                    Status = CrowdactionStatus.Running,
                    Target = 33
                };
            var newCrowdactionResult = await crowdactionService.UpdateCrowdaction(updatedCrowdaction, CancellationToken.None);
            Assert.True(newCrowdactionResult.Succeeded);
            int? newCrowdactionId = newCrowdactionResult.Crowdaction?.Id;
            Assert.NotNull(newCrowdactionId);
            Crowdaction retrievedCrowdaction = await context.Crowdactions.Include(c => c.Tags).ThenInclude(t => t.Tag).FirstOrDefaultAsync(c => c.Id == newCrowdactionId);

            Assert.NotNull(retrievedCrowdaction);
            Assert.Equal(updatedCrowdaction.Name, retrievedCrowdaction.Name);
            Assert.True(Enumerable.SequenceEqual(updatedCrowdaction.Tags.OrderBy(t => t), retrievedCrowdaction.Tags.Select(t => t.Tag.Name).OrderBy(t => t)));

            await crowdactionService.DeleteCrowdaction(newCrowdactionId ?? -1, CancellationToken.None);
            retrievedCrowdaction = await context.Crowdactions.Include(c => c.Tags).ThenInclude(t => t.Tag).FirstOrDefaultAsync(c => c.Id == newCrowdactionId);
            Assert.Equal(CrowdactionStatus.Deleted, retrievedCrowdaction.Status);
        }

        [Fact]
        public async Task TestCommentCreate()
        {
            var newCrowdaction = new NewCrowdactionInternal("test" + Guid.NewGuid(), 100, "test", "test", "test", null, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), null, null, null, null, new[] { Category.Community }, Array.Empty<string>(), CrowdactionDisplayPriority.Bottom, CrowdactionStatus.Running, 0, null);
            Crowdaction crowdaction = await crowdactionService.CreateCrowdactionInternal(newCrowdaction, CancellationToken.None);
            Assert.NotNull(crowdaction);

            ApplicationUser user = await context.Users.FirstAsync();
            ClaimsPrincipal userPrincipal = await signInManager.CreateUserPrincipalAsync(user);
            CrowdactionComment firstComment = await crowdactionService.CreateComment("test test", crowdaction.Id, userPrincipal, CancellationToken.None);
            Assert.NotNull(firstComment);

            await Assert.ThrowsAnyAsync<Exception>(() => crowdactionService.CreateComment("test test <script />", crowdaction.Id, userPrincipal, CancellationToken.None));
            await Assert.ThrowsAnyAsync<Exception>(() => crowdactionService.CreateComment("test test <a href=\"javascript:alert('hello')\" />", crowdaction.Id, userPrincipal, CancellationToken.None));

            await crowdactionService.DeleteComment(firstComment.Id, CancellationToken.None);
            CrowdactionComment retrievedComment = await context.CrowdactionComments.FirstOrDefaultAsync(c => c.Id == firstComment.Id);
            Assert.Null(retrievedComment);

            CrowdactionComment sanitizedComment = await crowdactionService.CreateComment("test test <p><a href=\"www.google.com\" /></p>", crowdaction.Id, userPrincipal, CancellationToken.None);
            Assert.Equal("test test <p><a href=\"https://www.google.com\" rel=\"nofollow ugc\"></a></p>", sanitizedComment.Comment);
            await crowdactionService.DeleteComment(sanitizedComment.Id, CancellationToken.None);
        }

        [Fact]
        public async Task TestCrowdactionCommitAnonymous()
        { 
            // Setup
            var user = await context.Users.FirstAsync();
            Crowdaction crowdaction = new Crowdaction($"test-{Guid.NewGuid()}", CrowdactionStatus.Running, user.Id, 10, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1), "t", "t", "t", null, null);
            context.Crowdactions.Add(crowdaction);
            await context.SaveChangesAsync();

            // Test
            string testEmail = GetTestEmail();
            var result = await crowdactionService.CommitToCrowdactionAnonymous(testEmail, crowdaction.Id, CancellationToken.None);
            Assert.Equal(AddParticipantScenario.AnonymousCreatedAndAdded, result.Scenario);
            result = await crowdactionService.CommitToCrowdactionAnonymous(testEmail, crowdaction.Id, CancellationToken.None);
            Assert.Equal(AddParticipantScenario.AnonymousNotRegisteredPresentAndAlreadyParticipating, result.Scenario);
        }

        [Fact]
        public async Task TestCrowdactionCommitLoggedIn()
        {
            // Setup
            var user = await context.Users.FirstAsync();
            var userClaim = await signInManager.CreateUserPrincipalAsync(user);
            var crowdaction = new Crowdaction($"test-{Guid.NewGuid()}", CrowdactionStatus.Running, user.Id, 10, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1), "t", "t", "t", null, null);
            context.Crowdactions.Add(crowdaction);
            await context.SaveChangesAsync();

            // Test
            var result = await crowdactionService.CommitToCrowdactionLoggedIn(userClaim, crowdaction.Id, CancellationToken.None);
            Assert.Equal(AddParticipantScenario.LoggedInAndAdded, result.Scenario);
            result = await crowdactionService.CommitToCrowdactionLoggedIn(userClaim, crowdaction.Id, CancellationToken.None);
            Assert.Equal(AddParticipantScenario.LoggedInAndNotAdded, result.Scenario);
        }

        [Fact]
        public async Task TestCrowdactionEmail()
        {
            var user = await context.Users.FirstAsync();
            var claimsUser = await signInManager.CreateUserPrincipalAsync(user);
            var newCrowdaction =
                new Crowdaction(
                    name: $"test{Guid.NewGuid()}",
                    categories: new List<CrowdactionCategory>() { new CrowdactionCategory(Category.Environment), new CrowdactionCategory(Category.Community) },
                    tags: new List<CrowdactionTag>(),
                    description: Guid.NewGuid().ToString(),
                    descriptionVideoLink: Guid.NewGuid().ToString(),
                    start: DateTime.Now.AddDays(-10),
                    end: DateTime.Now.AddDays(30),
                    goal: Guid.NewGuid().ToString(),
                    creatorComments: Guid.NewGuid().ToString(),
                    proposal: Guid.NewGuid().ToString(),
                    target: 40,
                    status: CrowdactionStatus.Running,
                    displayPriority: CrowdactionDisplayPriority.Medium,
                    ownerId: user.Id);
            context.Crowdactions.Add(newCrowdaction);
            await context.SaveChangesAsync();

            Assert.Equal(0, newCrowdaction.NumberCrowdactionEmailsSent);
            Assert.True(crowdactionService.CanSendCrowdactionEmail(newCrowdaction));
            await crowdactionService.SendCrowdactionEmail(newCrowdaction.Id, "test", "test", claimsUser, CancellationToken.None);
            Assert.Equal(1, newCrowdaction.NumberCrowdactionEmailsSent);
            Assert.True(crowdactionService.CanSendCrowdactionEmail(newCrowdaction));
            for (int i = 0; i < 3; i++)
            {
                await crowdactionService.SendCrowdactionEmail(newCrowdaction.Id, "test", "test", claimsUser, CancellationToken.None);
            }

            Assert.Equal(4, newCrowdaction.NumberCrowdactionEmailsSent);
            Assert.False(crowdactionService.CanSendCrowdactionEmail(newCrowdaction));
        }

        [Fact]
        public async Task TestCrowdactionSearch()
        {
            Random r = new Random();
            Category searchCategory = (Category)r.Next(7);
            for (int i = 0; i < r.Next(10, 30); i++)
            {
                var newCrowdaction = new NewCrowdactionInternal("test" + Guid.NewGuid(), 100, "test", "test", "test", null, DateTime.UtcNow.AddDays(r.Next(-20, 20)), DateTime.UtcNow.AddDays(r.Next(21, 50)), null, null, null, null, new[] { searchCategory }, Array.Empty<string>(), CrowdactionDisplayPriority.Bottom, (CrowdactionStatus)r.Next(3), 0, null);
                Crowdaction crowdaction = await crowdactionService.CreateCrowdactionInternal(newCrowdaction, CancellationToken.None);
            }

            Assert.True(await crowdactionService.SearchCrowdactions(null, null).AnyAsync());

            Assert.True(await crowdactionService.SearchCrowdactions(searchCategory, null).Include(c => c.Categories).AllAsync(c => c.Categories.Any(pc => pc.Category == searchCategory)));
            Assert.True(await crowdactionService.SearchCrowdactions(null, SearchCrowdactionStatus.Closed).AllAsync(c => c.End < DateTime.UtcNow));
            Assert.True(await crowdactionService.SearchCrowdactions(searchCategory, SearchCrowdactionStatus.Closed).AllAsync(c => c.End < DateTime.UtcNow));
            Assert.True(await crowdactionService.SearchCrowdactions(null, SearchCrowdactionStatus.ComingSoon).AllAsync(c => c.Start > DateTime.UtcNow && c.Status == CrowdactionStatus.Running));
            Assert.True(await crowdactionService.SearchCrowdactions(searchCategory, SearchCrowdactionStatus.ComingSoon).AllAsync(c => c.Start > DateTime.UtcNow && c.Status == CrowdactionStatus.Running));
            Assert.True(await crowdactionService.SearchCrowdactions(null, SearchCrowdactionStatus.Open).AllAsync(c => c.Start <= DateTime.UtcNow && c.End >= DateTime.UtcNow && c.Status == CrowdactionStatus.Running));
            Assert.True(await crowdactionService.SearchCrowdactions(searchCategory, SearchCrowdactionStatus.Open).AllAsync(c => c.Start <= DateTime.UtcNow && c.End >= DateTime.UtcNow && c.Status == CrowdactionStatus.Running));
        }

        protected override void ConfigureReplacementServicesProvider(IServiceCollection collection)
        {
            collection.AddTransient(s => new Mock<IEmailSender>().Object);
        }

        private static string GetTestEmail()
            => $"collaction-test-email-{Guid.NewGuid()}@collaction.org";
    }
}
