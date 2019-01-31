using System;
using System.Linq;
using HighLoadCupV3.Model.Dto;
using HighLoadCupV3.Model.Exceptions;
using HighLoadCupV3.Model.InMemory;

namespace HighLoadCupV3.Model
{
    public class RepositoryUpdater
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        private static readonly int NowTs = Holder.Instance.CurrentTimeStamp;
        private readonly InMemoryRepository _inMemory;

        public RepositoryUpdater(InMemoryRepository repo)
        {
            _inMemory = repo;
        }

        public bool AddAndValidateNewAccount(AccountDto dto)
        {
            if (_inMemory.IsExistedAccountId(dto.Id))
            {
                return false;
            }

            if (_inMemory.Emails.Contains(dto.Email))
            {
                return false;   
            }

            lock (_inMemory)
            {
                AddNewAccount(dto, true);
            }

            return true;
        }

        public void AddLikesToNewAccounts(AccountDto dto)
        {
            if (dto.Likes == null)
            {
                return;
            }

            var id = dto.Id;
             _inMemory.Accounts[id].AddLikesFromToNewAccount(dto.Likes.Select(x=>x.Id).ToArray());
             foreach (var like in dto.Likes)
             {
                 _inMemory.Accounts[like.Id].AddLikeTo(id, like.TimeStamp);
             }
        }

        public void AddNewAccount(AccountDto dto, bool afterPost)
        {
            var id = dto.Id;
            var acc = new AccountData {Id = id};

            //FName
            acc.FNameIndex = _inMemory.FNameData.Add(dto.FName ?? string.Empty, id, afterPost);

            //SName
            acc.SNameIndex = _inMemory.SNameData.Add(dto.SName ?? string.Empty, id, afterPost);

            //Code and phone
            int code = 0;
            if (!string.IsNullOrEmpty(dto.Phone))
            {
                code = GetPhoneCode(dto.Phone);
                acc.Phone = dto.Phone;
            }

            acc.CodeIndex = _inMemory.CodeData.Add(code, id, afterPost);

            // Domain and Email
            int atIndex = dto.Email.IndexOf('@');
            acc.DomainIndex = _inMemory.DomainData.Add(dto.Email.Substring(atIndex + 1), id, afterPost);
            acc.Email = dto.Email;
            _inMemory.Emails.Add(dto.Email);

            //Sex
            acc.Sex = _inMemory.SexData.GetIndex(dto.Sex);
            _inMemory.SexData.Add(acc.Sex, id, afterPost);

            //Birth
            acc.Birth = dto.Birth;
            var birthYear = GetYearFromTs(dto.Birth);
            acc.BirthYearIndex = _inMemory.BirthYearData.Add(birthYear, id, afterPost);

            // Country
            acc.CountryIndex = _inMemory.CountryData.Add(dto.Country ?? string.Empty, id, afterPost);

            //City
            acc.CityIndex = _inMemory.CityData.Add(dto.City ?? string.Empty, id, afterPost);

            // Join
            var joinedYear = GetYearFromTs(dto.Joined);
            acc.JoinedYearIndex = _inMemory.JoinedYearData.Add(joinedYear, id, afterPost);

            // Status
            acc.Status = _inMemory.StatusData.GetIndex(dto.Status);
            _inMemory.StatusData.Add(acc.Status, id, afterPost);

            // Premium
            byte premium = 0;
            if (dto.Premium != null)
            {
                acc.PremiumStart = dto.Premium.Start;
                acc.PremiumFinish = dto.Premium.Finish;
                if (dto.Premium.Start < NowTs && dto.Premium.Finish > NowTs)
                {
                    premium = 1;
                }
            }

            acc.PremiumIndex = premium;  
            _inMemory.PremiumData.Add(premium, id, acc.PremiumStart > 0, afterPost);

            // Interests
            if (dto.Interests != null)
            {
                acc.Interests = _inMemory.InterestsData.Add(dto.Interests, id, acc.PremiumIndex, acc.Status, acc.Sex, afterPost).ToArray();
                Array.Sort(acc.Interests);
            }

            // Main
            _inMemory.Accounts[id] = acc;
            _inMemory.MaxAccountId = Math.Max(_inMemory.MaxAccountId, id);
        }

        public byte UpdateExistedAccount(int id, AccountUpdatDto dto)
        {
            if (!_inMemory.IsExistedAccountId(id))
            {
                return 1;
                //throw new AccountNotFoundException($"Account with id {id} not exists");
            }

            if (dto.Email != null && (!IsValidEmail(dto.Email) || _inMemory.Emails.Contains(dto.Email)))
            {
                return 2;
                //throw new InvalidUpdateException($"Email {dto.Email} already existed");
            }

            if (dto.Sex != null && !_inMemory.SexData.ContainsValue(dto.Sex))
            {
                return 2;
                //throw new InvalidUpdateException($"Sex {dto.Sex} invalid");
            }

            if (dto.Status != null && !_inMemory.StatusData.ContainsValue(dto.Status))
            {
                return 2;
                //throw new InvalidUpdateException($"Sex {dto.Status} invalid");
            }

            lock (_inMemory)
            {
                UpdateExistedAccountImpl(id, dto);
            }

            return 0;
        }

        private void UpdateExistedAccountImpl(int id, AccountUpdatDto dto)
        {
            var acc = _inMemory.Accounts[id];
            var newPremium = acc.PremiumIndex;
            var newStatus = acc.Status;
            var newSex = acc.Sex;
            var prevPremium = acc.PremiumIndex;
            var prevStatus = acc.Status;
            var prevSex = acc.Sex;

            // FName
            if (dto.FName != null)
            {
                acc.FNameIndex = _inMemory.FNameData.UpdateOrAdd(dto.FName, id, acc.FNameIndex);
            }

            // SName
            if (dto.SName != null)
            {
                acc.SNameIndex = _inMemory.SNameData.UpdateOrAdd(dto.SName, id, acc.SNameIndex);
            }

            // Phone and Code
            if (!string.IsNullOrEmpty(dto.Phone))
            {
                var code = GetPhoneCode(dto.Phone);
                acc.Phone = dto.Phone;
                acc.CodeIndex = _inMemory.CodeData.UpdateOrAdd(code, id, acc.CodeIndex);
            }

            // Domain and Email
            if (dto.Email != null)
            {
                _inMemory.Emails.Remove(dto.Email);
                acc.Email = dto.Email;
                _inMemory.Emails.Add(dto.Email);

                int atIndex = dto.Email.IndexOf('@');
                var domain = dto.Email.Substring(atIndex + 1);
                 acc.DomainIndex = _inMemory.DomainData.UpdateOrAdd(domain, id, acc.DomainIndex);
            }

            // Sex
            if (dto.Sex != null)
            {
                var sexIndex = _inMemory.SexData.GetIndex(dto.Sex);
                acc.Sex = sexIndex;
                _inMemory.SexData.Update(sexIndex, id, acc.Sex);
                newSex = acc.Sex;
            }

            // Birth
            if (dto.Birth.HasValue)
            {
                acc.Birth = dto.Birth.Value;
                var birthYear = GetYearFromTs(dto.Birth.Value);
                acc.BirthYearIndex = _inMemory.BirthYearData.UpdateOrAdd(birthYear, id, acc.BirthYearIndex);
            }


            // Country
            if (dto.Country != null)
            {
                acc.CountryIndex = _inMemory.CountryData.UpdateOrAdd(dto.Country, id, acc.CountryIndex);
            }

            // City
            if (dto.City != null)
            {
                acc.CityIndex = _inMemory.CityData.UpdateOrAdd(dto.City, id, acc.CityIndex);
            }

            // Joined
            if (dto.Joined.HasValue)
            {
                var joinedYear = GetYearFromTs(dto.Joined.Value);
                acc.JoinedYearIndex = _inMemory.JoinedYearData.UpdateOrAdd(joinedYear, id, acc.JoinedYearIndex);
            }

            // Status
            if (dto.Status != null)
            {
                var statusIndex = _inMemory.StatusData.GetIndex(dto.Status);
                _inMemory.StatusData.Update(statusIndex, id, acc.Status);
                acc.Status = statusIndex;
                newStatus = statusIndex;
            }

            // Premium
            if (dto.Premium != null)
            {
                var previouslyExpired = acc.PremiumStart > 0;
                acc.PremiumStart = dto.Premium.Start;
                acc.PremiumFinish = dto.Premium.Finish;

                byte premium = 0;
                if (dto.Premium.Start < NowTs && dto.Premium.Finish > NowTs)
                {
                    premium = 1;
                }

                _inMemory.PremiumData.Update(premium, id, acc.PremiumIndex, previouslyExpired, acc.PremiumStart > 0);
                acc.PremiumIndex = premium;
                newPremium = premium;
            }

            // Interests
            if (dto.Interests != null)
            {
                acc.Interests = _inMemory.InterestsData.UpdateOrAdd(dto.Interests, id,
                        acc.Interests ?? Enumerable.Empty<byte>(),
                        prevPremium, prevStatus, prevSex,
                        newPremium, newStatus, newSex)
                    .ToArray();
                Array.Sort(acc.Interests);
            }
            else if ((prevSex != newSex || prevStatus != newStatus || prevPremium != newPremium) && acc.Interests != null)
            {
                _inMemory.InterestsData.UpdateRecommendationsData(id, acc.Interests,
                    prevPremium, prevStatus, prevSex,
                    newPremium, newStatus, newSex);
            }

            // Likes
            if (dto.Likes != null)
            {
                acc.AddLikesFromToNewAccount(dto.Likes.Select(x=>x.Id).ToArray());
                foreach (var like in dto.Likes)
                {
                    _inMemory.Accounts[like.Id].AddLikeTo(id, like.TimeStamp);
                }
            }
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private static int GetPhoneCode(string phone)
        {
            var index = phone.IndexOf('(');
            if(index == -1)
            {
                return 1;
            }

            return int.Parse(phone.Substring(index + 1, 3));
        }

        private static int GetYearFromTs(int ts)
        {
            var dt = Epoch.AddSeconds(ts);

            return dt.Year;
        }
    }
}
