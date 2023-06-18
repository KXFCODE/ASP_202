using System;
namespace ASP_202.Models.Forum
{
	public class ForumSectionModel
	{
        public String Title { get; set; } = null!;
        public String Descriprion { get; set; } = null!;
        public String Logo { get; set; } = null!;
        public String CreatedDtString { get; set; } = null!;
        public String UrlIdString { get; set; } = null!;

        public String AuthorName { get; set; } = null!;
        public String AuthorAvatar { get; set; } = null!;

        public int LikesCount { get; set; }
        public int DislikesCount { get; set; }
        public int? GivenRate { get; set; }

        public int Sights { get; set; }

    }
}

