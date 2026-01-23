using Contracting.API.Controllers.Shared;
using Contracting.Application.Features.Business.EngineerSiteReport.Command.CreateEngineerSiteSurveyQuestion;
using Contracting.Application.Features.Business.EngineerSiteReport.Command.DeleteEngineerSiteSurveyQuestion;
using Contracting.Application.Features.Business.EngineerSiteReport.Command.UpdateEngineerSiteSurveyQuestion;
using Contracting.Application.Features.Business.EngineerSiteReport.Query.GetEngineerSiteSurveyQuestions;
using Contracting.Shared.BusinessDtos.EngineerSiteReportDto;
using Contracting.Shared.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Contracting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EngineerSiteSurveyQuestionController : APIBaseController
    {
        private readonly IMediator _mediator;

        public EngineerSiteSurveyQuestionController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetActiveQuestions([FromQuery] BaseFilterDto filter)
        {
            var query = new GetEngineerSiteSurveyQuestionsQuery(filter);
            var result = await _mediator.Send(query);

            return result.Match(
                questions => Ok(questions),
                errors => Problem(errors)
            );
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEngineerSiteSurveyQuestionTemplateDto dto)
        {
            var command = new CreateEngineerSiteSurveyQuestionCommand(dto);
            var result = await _mediator.Send(command);

            return result.Match(
                question => Ok(question),
                errors => Problem(errors)
            );
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateEngineerSiteSurveyQuestionTemplateDto dto)
        {
            var command = new UpdateEngineerSiteSurveyQuestionCommand(dto);
            var result = await _mediator.Send(command);

            return result.Match(
                question => Ok(question),
                errors => Problem(errors)
            );
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var command = new DeleteEngineerSiteSurveyQuestionCommand(id);
            var result = await _mediator.Send(command);

            return result.Match(
                response => Ok(response),
                errors => Problem(errors)
            );
        }
    }
}
