using System;
namespace ASP_202.Services.Validation
{
	public interface IValidationService
	{
		bool Validate(String sourse, params ValidationTerms[] terms);
	}
}

