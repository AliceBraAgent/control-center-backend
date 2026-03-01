using ControlCenter.Api.Models.Dtos;

namespace ControlCenter.Api.Services;

public interface ISearchService
{
    Task<SearchResultsDto> SearchAsync(string query);
}
