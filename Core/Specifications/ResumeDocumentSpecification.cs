using Core.Constants;
using Core.Entities;

namespace Core.Specifications;

public sealed class ResumeDocumentSpecification() : BaseSpecification<Document>(document => document.Type == DocumentType.Resume)
{
}
