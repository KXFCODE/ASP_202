﻿using System;
namespace ASP_202.Data.Entity
{
	public class Topic
	{
        public Guid Id { get; set; }

        public Guid ThemeId { get; set; }

        public String Title { get; set; } = null!;

        public String Descriprion { get; set; } = null!;

        public Guid AuthorId { get; set; }

        public DateTime CreatedDt { get; set; }
    }
}

