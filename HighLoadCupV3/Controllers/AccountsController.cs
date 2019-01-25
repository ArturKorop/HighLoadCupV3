//using System;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Threading.Tasks;
//using HighLoadCupWithMongo.Model;
//using HighLoadCupWithMongo.Model.Dto;
//using HighLoadCupWithMongo.Model.Exceptions;
//using Microsoft.AspNetCore.Mvc;
//using Newtonsoft.Json;

//namespace HighLoadCupWithMongo.Controllers
//{
//    [Route("[controller]")]
//    [ApiController]
//    public class AccountsController : ControllerBase
//    {
//        private  static readonly JsonSerializer _serializer = new JsonSerializer();
//        private const int TooMuchTimeNotifier = 50;

//        private const string AppJsonText = "application/json";
//        private const string EmptyValue = "{}";
//        private static readonly BadRequestResult _bad = new BadRequestResult();
//        private static readonly StatusCodeResult _wrong = new StatusCodeResult(405);

//        //[HttpGet("filter")]
//        //public async Task<ActionResult> Filter()
//        //{
//        //    //var sw = new Stopwatch();
//        //    //sw.Start();

//        //    var filter = Holder.Instance.Filter;
//        //    var queries = Request.Query.ToDictionary(x => x.Key, x => x.Value.First());
//        //    var data = await filter.FilterByAsync(queries);
//        //    //if (sw.ElapsedMilliseconds >= TooMuchTimeNotifier)
//        //    //{
//        //    //    Console.WriteLine($"{DateTime.Now.ToLongTimeString()} Query [{Request.QueryString.Value}] in {sw.ElapsedMilliseconds} ms");
//        //    //}

//        //    if (data == null)
//        //    {
//        //        return _bad;
//        //    }

//        //    return Content(data, "application/json");
//        //}

//        [HttpGet("filter")]
//        public ActionResult Filter()
//        {
//            var sw = new Stopwatch();
//            sw.Start();

//            var filter = Holder.Instance.Filter;
//            var queries = Request.Query.ToDictionary(x => x.Key, x => x.Value.First());
//            var data = filter.FilterBy(queries);
//            if (sw.ElapsedMilliseconds >= TooMuchTimeNotifier)
//            {
//                Console.WriteLine($"{DateTime.Now.ToLongTimeString()} Query [{Request.QueryString.Value}] in {sw.ElapsedMilliseconds} ms");
//            }

//            if (data == null)
//            {
//                return _bad;
//            }

//            return Content(data, "application/json");
//        }

//        //[HttpGet("group")]
//        //public async Task<ActionResult> Group()
//        //{
//        //    //return _wrong;
//        //    //var sw = new Stopwatch();
//        //    //sw.Start();

//        //    var filter = Holder.Instance.Group;
//        //    var dict = Request.Query.ToDictionary(x => x.Key, x => x.Value.First());
//        //    var parser = Holder.Instance.GroupQueryParser;
//        //    if (!parser.TryParse(dict, out var groupQuery))
//        //    {
//        //        return _bad;
//        //    }

//        //    var data = await filter.GroupByAsync(groupQuery);
//        //    //if (sw.ElapsedMilliseconds >= TooMuchTimeNotifier)
//        //    //{
//        //    //    Console.WriteLine(
//        //    //        $"{DateTime.Now.ToLongTimeString()}Query [{Request.QueryString.Value}] in {sw.ElapsedMilliseconds} ms");
//        //    //}

//        //    if (data == null)
//        //    {
//        //        return _bad;
//        //    }

//        //    return Content(data, "application/json");
//        //}

//        [HttpGet("group")]
//        public ActionResult Group()
//        {
//            //return _wrong;
//            //var sw = new Stopwatch();
//            //sw.Start();

//            var filter = Holder.Instance.Group;
//            var dict = Request.Query.ToDictionary(x => x.Key, x => x.Value.First());
//            var parser = Holder.Instance.GroupQueryParser;
//            if (!parser.TryParse(dict, out var groupQuery))
//            {
//                return _bad;
//            }

//            var data = filter.GroupBy(groupQuery);
//            //if (sw.ElapsedMilliseconds >= TooMuchTimeNotifier)
//            //{
//            //    Console.WriteLine(
//            //        $"{DateTime.Now.ToLongTimeString()}Query [{Request.QueryString.Value}] in {sw.ElapsedMilliseconds} ms");
//            //}

//            if (data == null)
//            {
//                return _bad;
//            }

//            return Content(data, "application/json");
//        }

//        [HttpGet("{id}/recommend")]
//        public ActionResult Recommend(string id)
//        {
//            //return _wrong;
//            //var sw = new Stopwatch();
//            //sw.Start();

//            if (!int.TryParse(id, out var idValue))
//            {
//                return _bad;
//            }

//            if (idValue < 0 || idValue >= Holder.Instance.InMemory.Accounts.Length || Holder.Instance.InMemory.Accounts[idValue] == null)
//            {
//                return StatusCode(404);
//            }

//            var dict = Request.Query.ToDictionary(x => x.Key, x => x.Value.First());

//            string key = null;
//            string value = null;
//            if (dict.ContainsKey(Names.City))
//            {
//                key = Names.City;
//                value = dict[Names.City];
//                dict.Remove(Names.City);
//            }
//            else if (dict.ContainsKey(Names.Country))
//            {
//                key = Names.Country;
//                value = dict[Names.Country];
//                dict.Remove(Names.Country);
//            }

//            if (!string.IsNullOrEmpty(key) && string.IsNullOrEmpty(value))
//            {
//                return _bad;
//            }

//            if (!dict.ContainsKey(Names.Limit) || !int.TryParse(dict[Names.Limit], out var limit) || limit < 1)
//            {
//                return _bad;
//            }

//            dict.Remove(Names.Limit);
//            dict.Remove("query_id");
//            if (dict.Count > 0)
//            {
//                return BadRequest();
//            }

//            var data = Holder.Instance.Recommend.GetRecommendations(idValue, key, value, limit);
//            //if (sw.ElapsedMilliseconds >= TooMuchTimeNotifier)
//            //{
//            //    Console.WriteLine(
//            //        $"{DateTime.Now.ToLongTimeString()}Query [{Request.QueryString.Value}] in {sw.ElapsedMilliseconds} ms");
//            //}
            
//            return Content(data, "application/json");
//        }

//        [HttpGet("{id}/suggest")]
//        public ActionResult Suggest(string id)
//        {
//            //return _wrong;
//            if (!int.TryParse(id, out var idValue))
//            {
//                return _bad;
//            }

//            if (idValue < 0 || idValue >= Holder.Instance.InMemory.Accounts.Length || Holder.Instance.InMemory.Accounts[idValue] == null)
//            {
//                return StatusCode(404);
//            }

//            var dict = Request.Query.ToDictionary(x => x.Key, x => x.Value.First());

//            string key = null;
//            string value = null;
//            if (dict.ContainsKey(Names.City))
//            {
//                key = Names.City;
//                value = dict[Names.City];
//                dict.Remove(Names.City);
//            }
//            else if (dict.ContainsKey(Names.Country))
//            {
//                key = Names.Country;
//                value = dict[Names.Country];
//                dict.Remove(Names.Country);
//            }

//            if (!string.IsNullOrEmpty(key) && string.IsNullOrEmpty(value))
//            {
//                return _bad;
//            }

//            if (!dict.ContainsKey(Names.Limit) || !int.TryParse(dict[Names.Limit], out var limit) || limit < 1)
//            {
//                return _bad;
//            }

//            dict.Remove(Names.Limit);
//            dict.Remove("query_id");
//            if (dict.Count > 0)
//            {
//                return BadRequest();
//            }

//            var data = Holder.Instance.Suggest.GetSuggestions(idValue, limit, key, value);

//            return Content(data, "application/json");
//        }

//        [HttpGet]
//        [Route("{*url}")]
//        public ActionResult AllOtherGetRequests()
//        {
//            return NotFound();
//        }

//        [HttpPost]
//        [Route("{*url}")]
//        public ActionResult AllOtherPostRequests()
//        {
//            Holder.Instance.InMemory.NotifyAboutPost(0);
//            return NotFound();
//        }

//        [HttpPost("new")]
//        public ActionResult New()
//        {
//            var likes = 0;
//            AccountDto dto;
//            try
//            {
//                using (var reader = new JsonTextReader(new StreamReader(Request.Body)))
//                {
//                    dto = _serializer.Deserialize<AccountDto>(reader);
//                }
//            }
//            catch
//            {
//                Holder.Instance.InMemory.NotifyAboutPost(0);
//                return BadRequest();
//            }

//            try
//            {
//                Holder.Instance.Updater.AddAndValidateNewAccount(dto);
//                if (dto.Likes != null)
//                {
//                    Holder.Instance.InMemory.LikesStorage.AddToBuffer(dto.Id, dto.Likes);
//                    likes = dto.Likes.Length;
//                }


//                //TODO: move to const
//                return StatusCode(201, EmptyValue);
//            }
//            catch(InvalidUpdateException ex)
//            {
//                return BadRequest();
//            }
//            finally
//            {
//                Holder.Instance.InMemory.NotifyAboutPost(likes);
//            }
//        }

//        [HttpPost("{id}")]
//        public ActionResult Update(string id)
//        {
//            if (!int.TryParse(id, out var idValue))
//            {
//                Holder.Instance.InMemory.NotifyAboutPost(0);
//                return NotFound();
//            }

//            AccountUpdatDto dto;
//            try
//            {
//                using (var reader = new JsonTextReader(new StreamReader(Request.Body)))
//                {
//                    dto = _serializer.Deserialize<AccountUpdatDto>(reader);
//                }
//            }
//            catch
//            {
//                Holder.Instance.InMemory.NotifyAboutPost(0);
//                return BadRequest();
//            }

//            var likes = 0;
//            try
//            {
//                Holder.Instance.Updater.UpdateExistedAccount(idValue, dto);
//                if (dto.Likes != null)
//                {
//                    likes = dto.Likes.Length;
//                    for (int i = 0; i < dto.Likes.Length; i++) 
//                    {
//                        Holder.Instance.InMemory.LikesStorage.UpdateBuffer(dto.Likes[i], idValue);
//                    }
//                }

//                //TODO: move to const
//                return StatusCode(202, EmptyValue);
//            }
//            catch(AccountNotFoundException ex)
//            {
//                return NotFound();
//            }
//            catch (InvalidUpdateException ex)
//            {
//                return BadRequest();
//            }
//            finally
//            {
//                Holder.Instance.InMemory.NotifyAboutPost(likes);
//            }
//        }

//        [HttpPost("likes")]
//        public ActionResult UpdateLikes()
//        {
//            LikesUpdateDto dto;
//            try
//            {
//                using (var reader = new JsonTextReader(new StreamReader(Request.Body)))
//                {
//                    dto = _serializer.Deserialize<LikesUpdateDto>(reader);
//                }
//            }
//            catch
//            {
//                Holder.Instance.InMemory.NotifyAboutPost(0);
//                return BadRequest();
//            }

//            var ids = Holder.Instance.InMemory.IdSet;
//            if (dto.Likes.Any(x => !ids.Contains(x.Likee) || !ids.Contains(x.Liker)))
//            {
//                Holder.Instance.InMemory.NotifyAboutPost(0);
//                return BadRequest();
//            }

//            foreach (var likePair in dto.Likes)
//            {
//                lock (Holder.Instance.InMemory.LikesStorage)
//                {
//                    Holder.Instance.InMemory.LikesStorage.UpdateBuffer(likePair);
//                }
//            }

//            Holder.Instance.InMemory.NotifyAboutPost(dto.Likes.Length);
//            return StatusCode(202, EmptyValue);
//        }
//    }
//}
