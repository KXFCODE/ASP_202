using ASP_202.Data;
using ASP_202.Data.Entity;
using ASP_202.Models.User;
using ASP_202.Services.Hash;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using ASP_202.Services.Kdf;
using Microsoft.Extensions.Primitives;
using System.Security.Claims;
using ASP_202.Services.Validation;
using ASP_202.Services.Email;
using ASP_202.Models;

namespace ASP_202.Controllers
{
    // [Route("User")]
    public class UserController : Controller
    {
        private readonly IHashService _hashService;
        private readonly ILogger<UserController> _logger;
        private readonly DataContext _dataContext;
        private readonly IRandomService _randomService;
        private readonly IKdfService _kdfService;
        private readonly IValidationService _validationService;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;




        public UserController(IHashService hashService, ILogger<UserController> logger, DataContext dataContext, IRandomService randomService, IKdfService kdfService, IValidationService validationService, IConfiguration configuration, IEmailService emailService)
        {
            _hashService = hashService;
            _logger = logger;
            _dataContext = dataContext;
            _randomService = randomService;
            _kdfService = kdfService;
            _validationService = validationService;
            _configuration = configuration;
            _emailService = emailService;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Registration()
        {
            return View();
        }
        public IActionResult RegisterUser(UserRegistrationModel userRegistrationModel)
        {
            // Перший етап оброблення даних - валідація
            // Є декілька підходів: а) до першої помилки, б) повна перевірка
            // За повної перевірки кожне з полів може мати своє повідомлення - 
            // потрібна додаткова модель UserValidationModel

            UserValidationModel validationResult = new();
            bool isModelValid = true;
            byte minPasswordLength = 3;

            #region Login Validation
            if (!_validationService.Validate(userRegistrationModel.Login, ValidationTerms.Login))
            {
                validationResult.LoginMessage = "Логин не соответствует шаблону";
                isModelValid = false;
            }
            if (_dataContext.Users.Any(u => u.Login.ToLower() == userRegistrationModel.Login.ToLower()))
            {
                validationResult.LoginMessage =
                    $"Логин '{userRegistrationModel.Login}' уже существует";
                isModelValid = false;
            }
            #endregion
            #region Password / Repeat Validation
            if (!_validationService.Validate(userRegistrationModel.Password, ValidationTerms.NotEmpty))
            {
                validationResult.PasswordMessage = "Пароль не може бути порожним";
                isModelValid = false;
            }
            else if (!_validationService.Validate(userRegistrationModel.Email, ValidationTerms.Password))
            {
                validationResult.PasswordMessage =
                $"Пароль закороткий, щонайменше 3 символи";
                isModelValid = false;
            }
            else if (!userRegistrationModel.Password.Equals(userRegistrationModel.RepeatPassword))
            {
                validationResult.RepeatPasswordMessage = "Паролі не збігаються";
                isModelValid = false;



            }
            #endregion
            #region Email Validation
            if (!_validationService.Validate(userRegistrationModel.Email, ValidationTerms.NotEmpty))
            {
                validationResult.EmailMessage = "Email не може бути порожним";
                isModelValid = false;
            }
            else if (!_validationService.Validate(userRegistrationModel.Email, ValidationTerms.Email))
            {
                 
               validationResult.EmailMessage = "Email не відповідає шаблону";
               isModelValid = false;
                
            }
            #endregion
            #region RealName Validation
            if (String.IsNullOrEmpty(userRegistrationModel.RealName))
            {
                validationResult.RealNameMessage = "Реальне ім'я не може бути порожним";
                isModelValid = false;
            }
            else
            {
                String nameRegex = @"^.+$";
                if (!Regex.IsMatch(userRegistrationModel.RealName, nameRegex))
                {
                    validationResult.RealNameMessage = "Реальне ім'я не відповідає шаблону";
                    isModelValid = false;
                }
            }
            #endregion
            #region IsAgree Validation
            if (userRegistrationModel.IsAgree == false)
            {
                validationResult.IsAgreeMessage = "Реєстрація дозволяється тільки з дотриманням правил сайту";
                isModelValid = false;
            }
            #endregion
            #region Avatar Uploading
            String avatarFilename = null!;
            if (userRegistrationModel.Avatar is not null)
            {
                // завантажуємо файл, якщо він є. Відсутність файлу - припустима
                // схема: перевіямо файл що він є картинкою, переносимо у /avatars
                // TODO: Перевірити розмір файлу (більше 1кБ), видати повідомлення якщо це не так
                // TODO: Перевірити тип (Avatar.ContentType) - має бути "image/***"
                // TODO: Перевірити на наявність такого файлу у папці "wwwroot/avatars/"

                // Відокремлюємо розширення файлу
                String ext = Path.GetExtension(userRegistrationModel.Avatar.FileName);
                // хешуємо ім'я файлу
                String hash = (_hashService.Hash(
                    userRegistrationModel.Avatar.FileName + Guid.NewGuid()))[..16];
                // формуємо нове ім'я
                avatarFilename = hash + ext;
                string path = "wwwroot/avatars/" + avatarFilename;
                /* Д.З. Реалізувати додаткову перевірку на те, що файл із згенерованим
                 * ім'ям вже є у папці "wwwroot/avatars/". Зробити перевірку циклічною
                 * на випадок повторного збігу.
                 */
                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    userRegistrationModel.Avatar.CopyTo(fileStream);
                }
                ViewData["avatarFilename"] = avatarFilename;

            }
            #endregion

            if (isModelValid)
            {
                String salt = _randomService.RandomString(8);
                User user = new()
                {
                    Id = Guid.NewGuid(),
                    Login = userRegistrationModel.Login,
                    PasswordSalt = salt,
                    PasswordHash = _kdfService.GetDerivedKey(userRegistrationModel.Password, salt),
                    Avatar = avatarFilename,
                    Email = userRegistrationModel.Email,
                    RealName = userRegistrationModel.RealName,
                    RegisterDt = DateTime.Now,
                    LastEnterDt = null,
                    EmailCode = _randomService.ConfirmCode(6),
                };

                _dataContext.Users.Add(user);

                var emailConfirmToken = _GenerateEmailToken(user);

                _dataContext.SaveChanges();

                _SendConfirmEmail(user, emailConfirmToken);

                // показуємо сторінку з підтвердженням реєстрації
                return View(userRegistrationModel);
            }
            else
            {
                // повертаємо на форму реєстрації
                ViewData["validationResult"] = validationResult;
                // спосіб перейти на представлення, що не збігається з назвою методу
                return View("Registration");
            }
        }
        private bool _SendConfirmEmail(Data.Entity.User user,
            Data.Entity.EmailConfirmToken emailConfirmToken)
        {

            // Отправляем код подтверждения
            try
            {
                return _emailService.Send("confirm_email", new Models.Email.ConfirmEmailModel()
                {
                    Email = user.Email,
                    EmailCode = user.EmailCode!,
                    RealName = user.RealName,
                    ConfirmUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}/User/ConfirmToken?token={emailConfirmToken.Id}"

                });
            }
            catch (Exception ex)
            {
                _logger.LogError("_emailService error '{ex}'", ex.Message);
                return false;
            }
        }

        private EmailConfirmToken _GenerateEmailToken(Data.Entity.User user)
        {
            Data.Entity.EmailConfirmToken emailConfirmToken = new()
            {
                Id = Guid.NewGuid(),
                UserEmail = user.Email,
                UserId = user.Id,
                Moment = DateTime.Now,
                Used = 0
            };
            _dataContext.EmailConfirmTokens.Add(emailConfirmToken);

            return emailConfirmToken;
        }

        [HttpPost]
        public String AuthUser()
        {
            StringValues loginValues = Request.Form["user-login"];
            if (loginValues.Count == 0)
            {
                return "No login";
            }
            String? login = loginValues[0] ?? "";


            StringValues passwordValues = Request.Form["user-password"];
            if (passwordValues.Count == 0)
            {
                return "No password";
            }
            String? password = passwordValues[0] ?? "";

            User? user = _dataContext.Users.Where(u => u.Login == login).FirstOrDefault();
            if (user is not null)
            {
                if (user.PasswordHash == _kdfService.GetDerivedKey(password!, user.PasswordSalt))
                {
                    HttpContext.Session.SetString("authUserId", user.Id.ToString());

                    return $"OK";
                }
            }

            return $"Автентифікацію відхилено";

        }

        public RedirectToActionResult Logout()
        {
            HttpContext.Session.Remove("authUserId");
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public JsonResult ConfirmEmail([FromBody] String code)
        {
            StatusDataModel model = new();

            if (String.IsNullOrEmpty(code))
            {
                model.Status = "400";
                model.Data = "Missing required param: code";
            }
            else if (HttpContext.User.Identity?.IsAuthenticated != true)
            {
                model.Status = "401";
                model.Data = "Unathenticated";
            }
            else
            {
                User? user = null;
                try
                {
                    user = _dataContext.Users.Find(Guid.Parse(
                    HttpContext.User.Claims
                    .First(claim => claim.Type == ClaimTypes.Sid).Value
                    ));
                }
                catch { }
                if (user is null)
                {
                    model.Status = "403";
                    model.Data = "Unathorized";
                }
                else if (user.EmailCode is null)
                {
                    model.Status = "208";
                    model.Data = "Already confirmed";
                }
                else if (user.EmailCode != code)
                {
                    model.Status = "406";
                    model.Data = "Code Not Accepted";
                }
                else
                {
                    user.EmailCode = null;
                    _dataContext.SaveChanges();
                    model.Status = "200";
                    model.Data = "OK";
                }
            }

            return Json(model);
        }

        public ViewResult ConfirmToken([FromQuery] String token)
        {
            Guid tokenId;
            try {

                tokenId = Guid.Parse(token);
                var emailConfirmToken = _dataContext.EmailConfirmTokens.Find(tokenId)
                    ?? throw new Exception();
                var user = _dataContext.Users.Find(emailConfirmToken.UserId)
                    ?? throw new Exception();
                if (user.Email != emailConfirmToken.UserEmail)
                    throw new Exception();
                emailConfirmToken.Used++;
                user.EmailCode = null;
                _dataContext.SaveChanges();
                ViewData["tokenResult"] = "Пошта успішно підтверджена";
            }
            catch
            {
                ViewData["tokenResult"] = "Неправильний токен, не змінюйте посилання з листа";
                return View();
            }
            return View(); }

        [HttpPatch]

        public String ResendEmailCode()
        {
            if (HttpContext.User.Identity?.IsAuthenticated != true)
            {
                return "Unathenticated";
            }
            User? user = null;
            try
            {
                user = _dataContext.Users.Find(Guid.Parse(
                HttpContext.User.Claims
                .First(claim => claim.Type == ClaimTypes.Sid).Value
                ));
            }
            catch { }
            if (user is null)
            {
                return "Unathorized";
            }

            var emailConfirmToken = _GenerateEmailToken(user);

            user.EmailCode = _randomService.ConfirmCode(6);

            _dataContext.SaveChanges();

            if(_SendConfirmEmail(user, emailConfirmToken)) return "OK";

            return "Error sending Email";
        }

        public IActionResult Profile([FromRoute] String id)
        {
            //_logger.LogInformation(id);
            Data.Entity.User? user = _dataContext.Users.FirstOrDefault(u => u.Login == id);
            if (user is not null) // Пользователь с запрошеным Login
            {
                Models.User.ProfileModel model = new(user);
                if (String.IsNullOrEmpty(model.Avatar))
                {
                    model.Avatar = "no-avatar.png";
                }
                // Проверяем авторизован ли пользователь
                // HttpContext.User - заложено в AuthMiddleware

                if (HttpContext.User.Identity is not null
                    && HttpContext.User.Identity.IsAuthenticated)
                {

                    String userLogin =
                        HttpContext.User.Claims
                            .First(claim => claim.Type == ClaimTypes.NameIdentifier)
                            .Value;

                    if (model.Login == userLogin)
                    {
                        model.IsPersonal = true;
                    }
                }
                return View(model);
            }
            else
            {
                return NotFound();
            }

        }

        [HttpPut]
        public JsonResult Update([FromBody] UserUpdateModel model)
        {
            if(model is null)
            {
                return Json(new { status = "Error", data = "No or invalid Data" });

            }
            if(HttpContext.User.Identity?.IsAuthenticated != true)
            {
                return Json(new { status = "Error", data = "Unauthenticated" });

            }
            User? user = null;
            try
            {
                user = _dataContext.Users.Find( Guid.Parse(
                HttpContext.User.Claims
                .First(claim => claim.Type == ClaimTypes.Sid).Value
                ));
            }
            catch { }
            if(user is null)
            {
                return Json(new { status = "Error", data = "Unauthenticated" });

            }
            switch (model.Field)
            {
                case "realname":
                    if (!_validationService.Validate(model.Value, ValidationTerms.RealName))
                    {
                        return Json(new
                        {
                            status = "Error",
                            data = $"Validation failed for field '{model.Field}', value = '{model.Value}'"
                        });
                    }
                    user.RealName = model.Value;
                    _dataContext.SaveChanges();
                    return Json(new { status = "OK", data = $"Name changed to `{user.RealName}`" });

                default:

                    return Json(new { status = "Error", data = $"Unknown field `{model.Field}`" });
            }
        }
    }
    
}
