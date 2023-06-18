using ASP_202.Data;
using ASP_202.Data.Entity;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class RatesController : ControllerBase
{
    private readonly DataContext _dataContext;

    public RatesController(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    [HttpGet]
    public object Get()
    {
        return new { result = "Пришёл запрос методом GET" };
    }

    [HttpPost]
    public object Post([FromBody] RequestData data)
    {
        String result = null!;
        if (data == null
            || data.ItemId == null
            || data.Value == null
            || data.UserId == null)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            result = $"Недостатня кількість параметрів: value={data?.Value} user={data?.UserId} item={data?.ItemId}";
        }
        else
        {
            Guid itemId, userId;
            int value;
            try
            {
                itemId = Guid.Parse(data.ItemId);
                userId = Guid.Parse(data.UserId);
                value = Convert.ToInt32(data.Value);
                Rate? rate = _dataContext.Rates
                    .FirstOrDefault(r => r.ItemId == itemId && r.UserId == userId);

                if (rate == null)
                {
                    _dataContext.Rates.Add(new()
                    {
                        ItemId = itemId,
                        UserId = userId,
                        Rating = value
                    });
                    _dataContext.SaveChanges();
                    HttpContext.Response.StatusCode = StatusCodes.Status201Created;
                    result = $"Дані внесені";
                }
                else if (rate.Rating != value)
                {
                    rate.Rating = value;
                    _dataContext.SaveChanges();
                    
                    HttpContext.Response.StatusCode = StatusCodes.Status202Accepted;
                    result = $"Данные обновлены";
                }
                else
                {
                    HttpContext.Response.StatusCode = StatusCodes.Status406NotAcceptable;
                    result = $"Дані вже наявні user={data?.UserId} item={data?.ItemId}";
                }
            }
            catch
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                result = $"Параметри не пройшли валідацію: value={data?.Value} user={data?.UserId} item={data?.ItemId}";
            }
        }
        return new { result };
        
    }

    [HttpPut]
    public object Put([FromBody] RequestData data)
    {
        return new { result = $"Пришёл запрос методом PUT с value={data.Value}" };
    }

    [HttpDelete]
    public object Delete([FromBody] RequestData data)
    {
        
        String result = null!;
        if (data == null
            || data.ItemId == null
            || data.Value == null
            || data.UserId == null)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            result = $"Недостатня кількість параметрів: value={data?.Value} user={data?.UserId} item={data?.ItemId}";
        }
        else
        {
            Guid itemId, userId;
            int value;
            try
            {
                itemId = Guid.Parse(data.ItemId);
                userId = Guid.Parse(data.UserId);
                value = Convert.ToInt32(data.Value);
                    
                Rate? rate = _dataContext.Rates
                    .FirstOrDefault(r => r.ItemId == itemId && r.UserId == userId);                if(_dataContext.Rates.Any(r => r.ItemId == itemId && r.UserId == userId))
                if(rate is null)    
                {
                    HttpContext.Response.StatusCode = StatusCodes.Status406NotAcceptable;
                    result = $"Данные отсутствуют в БД (нельзя удалить) user={data?.UserId} item={data?.ItemId}";
                }
                else
                {
                    _dataContext.Rates.Remove(rate);
                    _dataContext.SaveChanges();
                    
                    HttpContext.Response.StatusCode = StatusCodes.Status202Accepted;
                    result = $"Данные удалены";
                }
            }
            catch
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                result = $"Параметри не пройшли валідацію: value={data?.Value} user={data?.UserId} item={data?.ItemId}";
            }
            result = $"Надійшов запит методом POST з value={data.Value} user={data.UserId} item={data.ItemId}";
        }
        return new { result };
    }

    public object Default()
    {
        String str;
        using (var stream = new StreamReader(HttpContext.Request.Body))
        {
            str = stream.ReadToEndAsync().Result;
        }

        return new
        {
            result = $"Пришёл запрос методом {HttpContext.Request.Method} {str}"
        };
    }

    public class RequestData
    {
        public String? ItemId { get; set; }
        public String? UserId { get; set; }
        public String? Value { get; set; }
    }
}
    
