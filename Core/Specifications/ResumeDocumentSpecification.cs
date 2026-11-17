using Core.Constants;
using Core.Entities;

namespace Core.Specifications;

public sealed class ResumeDocumentSpecification() 
: BaseSpecification<AgoraFile>(document => document.Title.Contains(Category.Resume.ToString()))
{
}
