using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Guild
{
    public static List<Guild> _guilds = new(10);

    [HideInInspector] public int _id;
    [HideInInspector] public string _name;

    [HideInInspector] public Sprite _icon;
    [HideInInspector] public List<Country> _countries = new(15);
    [HideInInspector] public List<Offer> _offers = new(10);
    [HideInInspector] public List<Offer> _completedOffers = new(10);

    [HideInInspector] public int[] _date = new int[3];
    [HideInInspector] public string _ideology = "Any";

    [HideInInspector] public GuildType _type;
    [HideInInspector] public Relations _relations;
    [HideInInspector] public Storage _storage;

    public Guild(string name, Sprite image, CountrySettings owner, GuildType type, Relations relations, string ideology)
    {
        DateManager dateManager = ReferencesManager.Instance.dateManager;
        CountryManager countryManager = ReferencesManager.Instance.countryManager;

        _id = _guilds.Count;
        _name = name;
        _icon = image;
        _ideology = ideology;

        _countries.Add(new Country
        {
            country = owner,
            role = Role.Owner,
            date = dateManager.currentDate
        });

        _date = dateManager.currentDate;
        _storage = new Storage();

        _type = type;
        _relations = relations;
        owner.guilds.Add(this);

        foreach (CountrySettings country in countryManager.countries)
        {
            if (country.vassalOf == owner)
            {
                _countries.Add(new Country
                {
                    country = country,
                    role = Role.Puppet,
                    date = dateManager.currentDate
                });

                country.guilds.Add(this);
            }
        }
    }

    ~Guild()
    {
        Delete();
    }

    public static void Create(string name, Sprite image, CountrySettings owner, GuildType type, Relations relations, string ideology)
    {
        _guilds.Add(new Guild(name, image, owner, type, relations, ideology));
    }

    public void Delete()
    {
        foreach (Country country in _countries)
        {
            country.country.guilds.Remove(this);
        }

        _guilds.Remove(this);
    }

    public static Guild GetGuild(string name)
    {
        return _guilds.FirstOrDefault(guild => guild._name == name);
    }

    public static CountrySettings GetGuildOwner(int id)
    {
        Guild guild = GetGuild(id);

        foreach (Country country in guild._countries)
        {
            if (country.role == Role.Owner) return country.country;
        }

        return null;
    }

    public static Guild GetGuild(int id)
    {
        return _guilds.Find(guild => guild._id == id);
    }

    public List<CountrySettings> GetCountries()
    {
        return _countries.Select(country => country.country).ToList();
    }

    public Country GetCountry(CountrySettings country)
    {
        return _countries.FirstOrDefault(_country => _country.country == country);
    }

    public void Kick(CountrySettings country)
    {
        CountryManager countryManager = ReferencesManager.Instance.countryManager;

        for (int i = 0; i < _countries.Count; i++)
        {
            Country _country = _countries[i];

            if (_country.country == country)
            {
                if (_countries.Count <= 1)
                {
                    Delete();
                    return;
                }

                if (_country.role == Role.Owner)
                {
                    int owners = _countries.Count(c => c.role == Role.Owner) - 1;
                    if (owners < 1)
                    {
                        Country biggestCountry = _countries.OrderByDescending(item => item.country.myRegions).First();
                        biggestCountry.role = Role.Owner;
                    }
                }

                if (_country.role == Role.Puppet)
                {
                    if (_countries.Any(cou => cou.country == _country.country.vassalOf))
                    {
                        return;
                    }
                }

                foreach (CountrySettings cou2 in countryManager.countries)
                {
                    if (country.vassalOf == cou2)
                    {
                        Kick(cou2);
                    }
                }

                _countries.Remove(_country);
                _country.country.guilds.Remove(this);
            }
        }
    }

    public static void Join(Guild guild, CountrySettings country)
    {
        if (guild.GetCountry(country) == null)
        {
            guild._countries.Add(new Country
            {
                country = country,
                role = Role.Default,
                date = ReferencesManager.Instance.dateManager.currentDate
            });

            country.guilds.Add(guild);
            guild.SyncRelations();
        }
    }

    public int CountSize()
    {
        return _countries.Sum(country => country.country.myRegions.Count);
    }

    public static string GetText(Action action, string first, string second)
    {
        switch (action)
        {
            case Action.Kick:
                return $"{first} предлагает изгнать государство {second}";
            case Action.Invite:
                return $"{first} предлагает пригласить государство {second}";
            case Action.Join:
                return $"{first} желает вступить в альянс";
            case Action.AskGold:
                return $"{first} просит золото из казны в количестве {second}";
            case Action.AskFood:
                return $"{first} просит провизию из казны в количестве {second}";
            case Action.AskRecruits:
                return $"{first} просит рекрутов из казны в количестве {second}";
            case Action.AskFuel:
                return $"{first} просит топливо из казны в количестве {second}";
            case Action.Attack:
                return $"{first} предлагает объявить войну государству {second}";
            case Action.Peace:
                return $"{first} предлагает объявить мир государству {second}";
            case Action.Promote:
                return first == second ? $"{first} желает получить повышение" : $"{first} просит повысить роль государства {second}";
            case Action.Demote:
                return first == second ? $"{first} хочет понижение, зачем?" : $"{first} просит понизить роль государства {second}";
            default:
                return "эх, баг какой-то случился, бро не паникуй";
        }
    }

    public bool Contains(CountrySettings country)
    {
        return _countries.Any(cou => cou.country == country);
    }

    public bool Contains(Country country)
    {
        return _countries.Contains(country);
    }

    public void SyncRelations()
    {
        DiplomatyUI diplomatyUI = ReferencesManager.Instance.diplomatyUI;

        foreach (Country country1 in _countries)
        {
            foreach (Country country2 in _countries)
            {
                if (country1.country != country2.country)
                {
                    var relation = diplomatyUI.FindCountriesRelation(country1.country, country2.country);
                    relation.union = _relations.union;
                    relation.trade = _relations.trade;
                    relation.pact = _relations.pact;
                    relation.right = _relations.right;
                }
            }
        }
    }

    [System.Serializable]
    public struct Relations
    {
        public bool union;
        public bool trade;
        public bool pact;
        public bool right;
    }

    [System.Serializable]
    public class Offer
    {
        public int[] date;
        public object arg; // заменено на object для общей типизации

        public Guild guild;
        public CountrySettings starter;
        public List<Country> agree;
        public List<Country> disagree;

        public bool completed = false;

        public Action action;

        public Offer(Guild guild, CountrySettings starter, object arg, Action action)
        {
            DateManager dateManager = ReferencesManager.Instance.dateManager;

            this.guild = guild;
            this.starter = starter;
            this.arg = arg;
            this.action = action;
            this.completed = false;

            date = dateManager.currentDate;
            agree = new List<Country>(15);
            disagree = new List<Country>(15);
        }

        public bool Check()
        {
            return agree.Count >= (guild._countries.Count - disagree.Count);
        }

        public bool Execute()
        {
            DateManager dateManager = ReferencesManager.Instance.dateManager;

            if (Check())
            {
                switch (action)
                {
                    case Action.Kick:
                        if (arg is Country countryArg)
                        {
                            guild.Kick(countryArg.country);
                        }
                        break;
                    case Action.Invite:
                        if (arg is CountrySettings receiver)
                        {
                            var inviteMessageSettings = new DiplomatyUI.MessageSettings(
                                GetGuildOwner(guild._id), receiver, guild._id, "GuildInvite", true);

                            ReferencesManager.Instance.diplomatyUI.SpawnGuildMessage(inviteMessageSettings);

                            if (receiver.ideology == GetGuildOwner(guild._id).ideology &&
                                ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(receiver, GetGuildOwner(guild._id)).relationship >= 10)
                            {
                                Join(guild, receiver);
                            }
                        }
                        break;
                    case Action.Join:
                        if (arg is CountrySettings newMember)
                        {
                            Join(guild, newMember);
                        }
                        break;
                    case Action.Attack:
                        if (arg is CountrySettings target)
                        {
                            guild.SyncRelations();
                            ReferencesManager.Instance.diplomatyUI.AISendOffer("Объявить войну", GetGuildOwner(guild._id), target, false);
                        }
                        break;
                    case Action.Peace:
                        if (arg is CountrySettings peaceTarget)
                        {
                            ReferencesManager.Instance.diplomatyUI.AISendOffer("Заключить мир", GetGuildOwner(guild._id), peaceTarget, false);
                        }
                        break;
                    case Action.AskGold:
                    case Action.AskFood:
                    case Action.AskRecruits:
                    case Action.AskFuel:
                        if (arg is int requestedAmount)
                        {
                            switch (action)
                            {
                                case Action.AskGold:
                                    if (guild._storage.gold >= requestedAmount)
                                    {
                                        guild._storage.gold -= requestedAmount;
                                        starter.money += requestedAmount;
                                    }
                                    break;
                                case Action.AskFood:
                                    if (guild._storage.food >= requestedAmount)
                                    {
                                        guild._storage.food -= requestedAmount;
                                        starter.food += requestedAmount;
                                    }
                                    break;
                                case Action.AskRecruits:
                                    if (guild._storage.recruits >= requestedAmount)
                                    {
                                        guild._storage.recruits -= requestedAmount;
                                        starter.recruits += requestedAmount;
                                    }
                                    break;
                                case Action.AskFuel:
                                    if (guild._storage.fuel >= requestedAmount)
                                    {
                                        guild._storage.fuel -= requestedAmount;
                                        starter.fuel += requestedAmount;
                                    }
                                    break;
                            }
                        }
                        UpdateUI();
                        break;
                    case Action.Promote:
                        if (arg is Country)
                        {
                            Country countryToPromote = (Country)arg;

                            countryToPromote.role = (Role)((short)countryToPromote.role - 1);

                            if (countryToPromote.country.isPlayer)
                            {
                                var promote_message = new DiplomatyUI.MessageSettings(
                                    GetGuildOwner(guild._id), countryToPromote.country, guild._id, "GuildPromote", false);

                                ReferencesManager.Instance.diplomatyUI.SpawnGuildMessage(promote_message);
                            }
                        }

                        break;
                    case Action.Demote:
                        if (arg is Country)
                        {
                            Country countryToDemote = (Country)arg;

                            countryToDemote.role = (Role)((short)countryToDemote.role + 1);

                            if (countryToDemote.country.isPlayer)
                            {
                                var promote_message = new DiplomatyUI.MessageSettings(
                                    GetGuildOwner(guild._id), countryToDemote.country, guild._id, "GuildDemote", false);

                                ReferencesManager.Instance.diplomatyUI.SpawnGuildMessage(promote_message);
                            }
                        }

                        break;
                }

                this.completed = true;
                guild._completedOffers.Add(this);
                guild._offers.Remove(this);

                return true;
            }

            return false;
        }

        public bool Voted(CountrySettings country)
        {
            foreach (Country cou in agree)
            {
                if (cou.country == country)
                {
                    return true;
                }
            }

            foreach (Country cou in disagree)
            {
                if (cou.country == country)
                {
                    return true;
                }
            }

            return false;
        }

        private void UpdateUI()
        {
            CountryManager countryManager = ReferencesManager.Instance.countryManager;

            if (starter.isPlayer)
            {
                countryManager.UpdateValuesUI();
                countryManager.UpdateIncomeValuesUI();
            }
        }
    }

    [System.Serializable]
    public struct Storage
    {
        public int gold;
        public int food;
        public int recruits;
        public int fuel;
    }

    [System.Serializable]
    public class Country
    {
        public int[] date;

        public CountrySettings country;

        public Role role;
    }

    [System.Serializable]
    public enum Action : short
    {
        Kick = 0, // выгнать
        Invite = 1, // пригласить
        Join = 2, // вступить
        AskGold = 3, // попросить золото
        AskFood = 4, // попросить еду
        AskRecruits = 5, // попросить рекрутов
        AskFuel = 6, // попросить топливо
        Attack = 7, // напасть
        Peace = 8, // помириться
        Promote = 9, // повысить
        Demote = 10 // понизить
    }

    [System.Serializable]
    public enum Role : short
    {
        Owner = 0,       // овнер имеет фулл права, может быть несколько
        Vice_Owner = 1,  // вице овнер не может: удалять альянс, выгонять участников без голосования, повышать выше помощника
        Elder = 2,       // помощник не может: приглашать без голосования, использовать казну
        Default = 3,     // обычный участник
        Puppet = 4,      // марионетка любой страны, много чо не может
    }

    [System.Serializable]
    public enum GuildType : short
    {
        Alliance = 0, // торговый
        Organization = 1 // военный
    }

    [System.Serializable]
    public struct FlagSprite
    {
        public string guild_name;
        public Sprite sprite;
    }
}
