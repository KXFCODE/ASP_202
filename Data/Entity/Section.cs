using System;
namespace ASP_202.Data.Entity
{
	public class Section
	{
		public Guid Id { get; set; }
		public String Title { get; set; } = null!;
		public String Descriprion { get; set; } = null!;
		public Guid AuthorId { get; set; }
		public DateTime CreatedDt { get; set; }

		public User Author { get; set; } = null!;
		public List<Rate> RateList { get; set; } = null!;

	}
}

