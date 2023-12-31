﻿using System;
namespace ASP_202.Data.Entity
{
	public class User
	{
		public Guid Id              { get; set; }

        public String Login         { get; set; } = null!;

        public String RealName      { get; set; } = null!;

        public String Email         { get; set; } = null!;

        public String? EmailCode    { get; set; } = null!;

        public String PasswordHash  { get; set; } = null!;

        public String PasswordSalt  { get; set; } = null!;

        public String? Avatar       { get; set; } = null!;

        public DateTime RegisterDt  { get; set; }

        public DateTime? LastEnterDt { get; set; }

        // Добавлено 2023-04-19, работа над Profile
        public Boolean IsEmailPublic { get; set; } = false; // отображать ли email в профиле для других пользователей
        // Добавление полей в БД с двнными возможно или если это NULLABLE поле,
        // или если его значение default
        public Boolean IsRealNamePublic { get; set; } = false;

        public Boolean IsDatesPublic { get; set; } = false;

    }
}

