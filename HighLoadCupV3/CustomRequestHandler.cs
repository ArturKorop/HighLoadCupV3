using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HighLoadCupV3.Model;
using HighLoadCupV3.Model.Dto;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace HighLoadCupV3
{
    public class CustomRequestHandler
    {
        private static readonly JsonSerializer Serializer = new JsonSerializer();
        private const int TooMuchTimeNotifier = 5;

        private const string AppJsonText = "application/json";
        private const string EmptyValue = "{}";

        private readonly ResponseData _bad = new ResponseData(400, null);
        private readonly ResponseData _notFound = new ResponseData(404, null);

        private const string AccountsNew = "/accounts/new/";
        private const string AccountsLikes = "/accounts/likes/";
        private const string AccountsFilter = "/accounts/filter/";
        private const string AccountsGroup = "/accounts/group/";
        private const string AccountsRecommend = "/recommend/";
        private const string AccountsSuggest = "/suggest/";
        private const string Accounts = "/accounts/";

        static CustomRequestHandler()
        {
            Serializer.NullValueHandling = NullValueHandling.Ignore;
        }

        public async Task Handle(HttpContext context)
        {
            var request = context.Request;
            var response = context.Response;

            var data = GetResponseData(request);
            response.StatusCode = data.StatusCode;
            if (data.Content != null)
            {
                response.ContentType = AppJsonText;

                using (var ms = new MemoryStream())
                {
                    using (var sw = new StreamWriter(ms, Encoding.Default))
                    {
                        Serializer.Serialize(sw, data.Content);
                        sw.Flush();

                        response.ContentLength = ms.Length;

                        ms.WriteTo(response.Body);
                        ms.Flush();
                    }
                }
            }
        }

        private ResponseData GetResponseData(HttpRequest request)
        {
            var path = request.Path.Value;
            ResponseData data = null;
            if (request.Method == "POST")
            {
                switch (path)
                {
                    case AccountsNew:
                        data = New(request);
                        break;
                    case AccountsLikes:
                        data = UpdateLikes(request);
                        break;
                    default:
                    {
                        if (path.Length > 10)
                        {
                            var updateIdSubstring = path.Substring(10, path.Length - 11);
                            data = Update(updateIdSubstring, request);
                            break;
                        }
                        else
                        {
                            data = AllOtherPostRequests();
                            break;
                        }
                    }
                }

                Holder.Instance.InMemory.NotifyAboutPost();
            }
            else
            {
                switch (path)
                {
                    case AccountsFilter:
                        data = Filter(request);
                        break;
                    case AccountsGroup:
                        data = Group(request);
                        break;
                    default:
                    {
                        if (path.StartsWith(Accounts))
                        {
                            if (path.EndsWith(AccountsRecommend))
                            {
                                var from = Accounts.Length;
                                var to = path.Length - from - AccountsRecommend.Length;
                                if (to > 0)
                                {
                                    data = Recommend(path.Substring(from, to), request);
                                    break;
                                }
                            }
                            else if (path.EndsWith(AccountsSuggest))
                            {
                                var from = Accounts.Length;
                                var to = path.Length - from - AccountsSuggest.Length;
                                if (to > 0)
                                {
                                    data = Suggest(path.Substring(from, to), request);
                                    break;
                                }
                            }
                        }

                        data = AllOtherGetRequests();
                        break;
                    }
                }

                Holder.Instance.InMemory.NotifyAboutGet();
            }

            return data;
        }

        public ResponseData Filter(HttpRequest request)
        {
            var filter = Holder.Instance.Filter;
            var queries = request.Query.ToDictionary(x => x.Key, x => x.Value.First());
            var data = filter.FilterBy(queries);

            if (data == null)
            {
                return _bad;
            }

            return new ResponseData(200, data);
        }

        public ResponseData Group(HttpRequest request)
        {
            var filter = Holder.Instance.Group;
            var dict = request.Query.ToDictionary(x => x.Key, x => x.Value.First());
            var parser = Holder.Instance.GroupQueryParser;
            if (!parser.TryParse(dict, out var groupQuery))
            {
                return _bad;
            }

            var data = filter.GroupBy(groupQuery);

            if (data == null)
            {
                return _bad;
            }

            return new ResponseData(200, data);
        }

        public ResponseData Recommend(string id, HttpRequest request)
        {
            if (!int.TryParse(id, out var idValue))
            {
                return _bad;
            }

            if (idValue < 0 || idValue >= Holder.Instance.InMemory.Accounts.Length || Holder.Instance.InMemory.Accounts[idValue] == null)
            {
                return _notFound;
            }

            var dict = request.Query.ToDictionary(x => x.Key, x => x.Value.First());

            string key = null;
            string value = null;
            if (dict.ContainsKey(Names.City))
            {
                key = Names.City;
                value = dict[Names.City];
                dict.Remove(Names.City);
            }
            else if (dict.ContainsKey(Names.Country))
            {
                key = Names.Country;
                value = dict[Names.Country];
                dict.Remove(Names.Country);
            }

            if (!string.IsNullOrEmpty(key) && string.IsNullOrEmpty(value))
            {
                return _bad;
            }

            if (!dict.ContainsKey(Names.Limit) || !int.TryParse(dict[Names.Limit], out var limit) || limit < 1)
            {
                return _bad;
            }

            dict.Remove(Names.Limit);
            dict.Remove("query_id");
            if (dict.Count > 0)
            {
                return _bad;
            }

            var data = Holder.Instance.Recommend.GetRecommendations(idValue, key, value, limit);

            return new ResponseData(200, data);
        }

        public ResponseData Suggest(string id, HttpRequest request)
        {
            if (!int.TryParse(id, out var idValue))
            {
                return _bad;
            }

            if (idValue < 0 || idValue >= Holder.Instance.InMemory.Accounts.Length || Holder.Instance.InMemory.Accounts[idValue] == null)
            {
                return _notFound;
            }

            var dict = request.Query.ToDictionary(x => x.Key, x => x.Value.First());

            string key = null;
            string value = null;
            if (dict.ContainsKey(Names.City))
            {
                key = Names.City;
                value = dict[Names.City];
                dict.Remove(Names.City);
            }
            else if (dict.ContainsKey(Names.Country))
            {
                key = Names.Country;
                value = dict[Names.Country];
                dict.Remove(Names.Country);
            }

            if (!string.IsNullOrEmpty(key) && string.IsNullOrEmpty(value))
            {
                return _bad;
            }

            if (!dict.ContainsKey(Names.Limit) || !int.TryParse(dict[Names.Limit], out var limit) || limit < 1)
            {
                return _bad;
            }

            dict.Remove(Names.Limit);
            dict.Remove("query_id");
            if (dict.Count > 0)
            {
                return _bad;
            }

            var data = Holder.Instance.Suggest.GetSuggestions(idValue, limit, key, value);

            return new ResponseData(200, data);
        }

        public ResponseData AllOtherGetRequests()
        {
            return _notFound;
        }

        public ResponseData AllOtherPostRequests()
        {
            return _notFound;
        }

        public ResponseData New(HttpRequest request)
        {
            AccountDto dto;
            try
            {
                using (var reader = new JsonTextReader(new StreamReader(request.Body)))
                {
                    dto = Serializer.Deserialize<AccountDto>(reader);
                }
            }
            catch
            {
                return _bad;
            }

            var isSuccessfulUpdate = Holder.Instance.Updater.AddAndValidateNewAccount(dto);
            if (!isSuccessfulUpdate)
            {
                return _bad;
            }

            if (dto.Likes != null)
            {
                var accounts = Holder.Instance.InMemory.Accounts;
                accounts[dto.Id].AddLikesFromToNewAccount(dto.Likes.Select(x => x.Id).ToArray());
                var buffer = Holder.Instance.InMemory.LikesBuffer;
                foreach (var like in dto.Likes)
                {
                    buffer.AddLikes(dto.Id, like.Id, like.TimeStamp);
                }
            }

            return new ResponseData(201, EmptyValue);
        }

        public ResponseData Update(string id, HttpRequest request)
        {
            if (!int.TryParse(id, out var idValue))
            {
                return _notFound;
            }

            AccountUpdatDto dto;
            try
            {
                using (var reader = new JsonTextReader(new StreamReader(request.Body)))
                {
                    dto = Serializer.Deserialize<AccountUpdatDto>(reader);
                }
            }
            catch
            {
                return _bad;
            }

            var updateCode = Holder.Instance.Updater.UpdateExistedAccount(idValue, dto);
            if (updateCode == 0)
            {
                if (dto.Likes != null)
                {
                    var buffer = Holder.Instance.InMemory.LikesBuffer;
                    foreach (var dtoLike in dto.Likes)
                    {
                        buffer.AddLikes(idValue, dtoLike.Id, dtoLike.TimeStamp);
                    }
                }

                return new ResponseData(202, EmptyValue);
            }
            if (updateCode == 1)
            {
                return _notFound;
            }
            else
            {
                return _bad;
            }
        }

        public ResponseData UpdateLikes(HttpRequest request)
        {
            var deserializer = new LikesUpdateDeserializer(Holder.Instance.InMemory);
            var likes = deserializer.Deserialize(request.Body);

            if(likes == null)
            {
                return _bad;
            }

            var buffer = Holder.Instance.InMemory.LikesBuffer;
            foreach (var likePair in likes)
            {
                buffer.AddLikes(likePair.Liker, likePair.Likee, likePair.TimeStamp);
            }

            return new ResponseData(202, EmptyValue);
        }
    }
}

