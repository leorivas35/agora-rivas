using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Core.Constants;
using Core.Entities;
using Core.Interfaces;
using Core.Specifications;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace API.Tests;

public sealed class GenericRepositoryTests
{
    [Fact]
    public async Task AddAndSaveChangesAsync_PersistsEntity()
    {
        await using var context = CreateContext();
        var repository = new GenericRepository<Document>(context);
        Document document = CreateDocument(DocumentType.Resume, "Resume");

        repository.Add(document);
        int savedCount = await repository.SaveChangesAsync();

        Assert.Equal(1, savedCount);
        Assert.Equal(document, await repository.GetByIdAsync(document.Id));
        Assert.True(await repository.ExistsAsync(document.Id));
    }

    [Fact]
    public async Task UpdateAndRemove_SaveChangesPersistChanges()
    {
        await using var context = CreateContext();
        var repository = new GenericRepository<Document>(context);
        Document document = CreateDocument(DocumentType.Book, "Book");
        repository.Add(document);
        await repository.SaveChangesAsync();

        document.Title = "Updated book";
        repository.Update(document);
        await repository.SaveChangesAsync();

        Assert.Equal("Updated book", (await repository.GetByIdAsync(document.Id))?.Title);

        repository.Remove(document);
        await repository.SaveChangesAsync();

        Assert.Null(await repository.GetByIdAsync(document.Id));
        Assert.False(await repository.ExistsAsync(document.Id));
    }

    [Fact]
    public async Task GetEntityWithSpecAsync_ReturnsResumeMatchingSpecification()
    {
        await using var context = CreateContext();
        var repository = new GenericRepository<Document>(context);
        Document resume = CreateDocument(DocumentType.Resume, "Resume");
        repository.Add(resume);
        repository.Add(CreateDocument(DocumentType.Book, "Book"));
        await repository.SaveChangesAsync();

        Document? result = await repository.GetEntityWithSpecAsync(new ResumeDocumentSpecification());

        Assert.Equal(resume, result);
        Assert.Equal(1, await repository.CountAsync(new ResumeDocumentSpecification()));
    }

    [Fact]
    public async Task GetEntityWithSpecAsync_AppliesProjection()
    {
        await using var context = CreateContext();
        var repository = new GenericRepository<Document>(context);
        Document document = CreateDocument(DocumentType.Resume, "Resume");
        repository.Add(document);
        await repository.SaveChangesAsync();

        string? title = await repository.GetEntityWithSpecAsync(new DocumentTitleSpecification());

        Assert.Equal("Resume", title);
    }

    private static AgoraRivasContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AgoraRivasContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AgoraRivasContext(options);
    }

    private static Document CreateDocument(DocumentType type, string title)
    {
        return new Document
        {
            FileName = $"{title}.pdf",
            Title = title,
            Author = "Test author",
            ContentType = "application/pdf",
            SizeInBytes = 1,
            StoragePath = "/test/document.pdf",
            Type = type
        };
    }

    private sealed class DocumentTitleSpecification : ISpecification<Document, string>
    {
        public Expression<Func<Document, bool>>? Criteria => null;
        public Expression<Func<Document, object>>? OrderBy => null;
        public Expression<Func<Document, object>>? OrderByDescending => null;
        public List<Expression<Func<Document, object>>> Includes { get; } = [];
        public List<string> ThenIncludes { get; } = [];
        public bool IsDistinct => false;
        public int Skip => 0;
        public int Take => 0;
        public bool IsPagingEnabled => false;
        public Expression<Func<Document, string>> Select => document => document.Title;

        public IQueryable<Document> ApplyCriteria(IQueryable<Document> query)
        {
            return query;
        }
    }
}
