using NLog;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace VehicleControl_Viewer.Common
{
    public class RedisCacheManager
    {
        string REDIS_KEY_WORK_REDIS_USING_COUNT = "REDIS_USING_COUNT";
        string productID = string.Empty;
        const string REDIS_SERVER_CONFIGURATION = "redis.ohxc.mirle.com.tw:6379";


        private static Lazy<ConfigurationOptions> configOptions
               = new Lazy<ConfigurationOptions>(() =>
               {
                   var configOptions = new ConfigurationOptions();
                   configOptions.EndPoints.Add(REDIS_SERVER_CONFIGURATION);
                   configOptions.ClientName = "OHxCRedisConnection";
                   configOptions.ConnectTimeout = 100000;
                   configOptions.SyncTimeout = 100000;
                   configOptions.AbortOnConnectFail = false;
                   return configOptions;
               });

        private static Lazy<ConnectionMultiplexer> conn
                    = new Lazy<ConnectionMultiplexer>(
                        () => ConnectionMultiplexer.Connect(configOptions.Value));

        Logger logger = LogManager.GetCurrentClassLogger();
        public RedisCacheManager(string product_id)
        {
            productID = product_id;
            REDIS_KEY_WORK_REDIS_USING_COUNT = $"{productID}_{REDIS_KEY_WORK_REDIS_USING_COUNT}";
        }


        object _lock = new object();
        bool redisConnectionValid = true;
        private ConnectionMultiplexer GetConnection()
        {
            return conn.Value;

        }

        public void SubscriptionEvent(string subscription_key, Action<RedisChannel, RedisValue> action)
        {
            ISubscriber sub = GetConnection().GetSubscriber();
            subscription_key = $"{productID}_{subscription_key}";
            sub.Subscribe(subscription_key, action);
        }
        public void UnsubscribeEvent(string subscription_key, Action<RedisChannel, RedisValue> action)
        {

            ISubscriber sub = GetConnection().GetSubscriber();
            subscription_key = $"{productID}_{subscription_key}";
            sub.Unsubscribe(subscription_key, action);
        }

        public void PublishEvent(string key, string value)
        {
            ISubscriber sub = GetConnection().GetSubscriber();
            key = $"{productID}_{key}";
            sub.Publish(key, value);
        }


        private IDatabase Database(int? db = null)
        {
            try
            {
                int db_index = -1;
                //#if DEBUG
                //                db_index = 1;
                //#endif
                return !redisConnectionValid ? null : GetConnection().GetDatabase(db ?? db_index);
            }
            catch (Exception ex)
            {
                redisConnectionValid = false;
                logger.Error(String.Format("Unable to create Redis connection: {0}", ex.Message));
                return null;
            }
        }


        public bool KeyExists(string key)
        {
            IDatabase db = Database();
            if (db == null) return false;
            key = $"{productID}_{key}";
            UsingCount();
            return db.KeyExists(key);
        }
        public bool KeyDelete(string key)
        {
            IDatabase db = Database();
            if (db == null) return false;
            key = $"{productID}_{key}";
            UsingCount();
            return db.KeyDelete(key);
        }

        public IEnumerable<RedisValue> SetScan(string key)
        {
            IDatabase db = Database();
            if (db == null) return null;
            key = $"{productID}_{key}";
            UsingCount();
            return db.SetScan(key);
        }


        public bool stringSetAsync(string key, RedisValue set_object, TimeSpan? timeOut = null, When when = When.Always)
        {
            IDatabase db = Database();
            if (db == null) return false;
            key = $"{productID}_{key}";
            db.StringSetAsync(key, set_object, timeOut, when);
            UsingCount();
            return true;
        }

        public bool ListRightPushAsync(string key, RedisValue set_object, TimeSpan? timeOut = null, When when = When.Always)
        {
            IDatabase db = Database();
            if (db == null) return false;
            key = $"{productID}_{key}";
            db.ListRightPushAsync(key, set_object);
            UsingCount();
            return true;
        }

        public bool HashSet(string key, HashEntry[] hashEntry)
        {
            IDatabase db = Database();
            if (db == null) return false;
            key = $"{productID}_{key}";
            db.HashSet(key, hashEntry);
            UsingCount();
            return true;
        }
        public bool HashSet(string key, RedisValue hashField, RedisValue value, When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            IDatabase db = Database();
            if (db == null) return false;
            key = $"{productID}_{key}";
            db.HashSet(key, hashField, value, when, flags);
            UsingCount();
            return true;
        }


        public bool StringIncrementAsync(string key, long value = 1, CommandFlags flags = CommandFlags.None)
        {
            IDatabase db = Database();
            if (db == null) return false;
            key = $"{productID}_{key}";
            db.StringIncrementAsync(key, value, flags);
            return true;
        }

        public RedisValue StringGet(string key)
        {
            IDatabase db = Database();
            if (db == null) return string.Empty;
            UsingCount();
            key = $"{productID}_{key}";
            var value = db.StringGet(key);
            if (!value.HasValue)
            {
                logger.Warn($"redis key not exist:{key}");
                return string.Empty;
            }
            return value;
        }

        public RedisValue[] StringGet(RedisKey[] keys)
        {
            IDatabase db = Database();
            if (db == null) return null;
            UsingCount();

            var value = db.StringGet(keys);
            if (value.Length == 0)
            {
                return null;
            }
            return value;
        }

        public RedisValue ListGetByIndexAsync(string key, long index)
        {
            IDatabase db = Database();
            if (db == null) return string.Empty;
            key = $"{productID}_{key}";
            UsingCount();
            var value = db.ListGetByIndexAsync(key, index);
            if (!value.Result.HasValue)
            {
                logger.Warn($"redis key not exist:{key}");
                return string.Empty;
            }

            return value.Result;
        }


        public IEnumerable<RedisKey> KeysFromServer(string pattern)
        {
            return GetConnection().GetServer(configOptions.Value.EndPoints.First()).Keys(pattern: pattern);
        }

        public long ListLength(string list_key)
        {
            IDatabase db = Database();
            if (db == null) return 0;
            list_key = $"{productID}_{list_key}";
            UsingCount();
            return db.ListLength(list_key);
        }


        public void UsingCount()
        {
            IDatabase db = Database();
            if (db == null) return;
            db.StringIncrementAsync(REDIS_KEY_WORK_REDIS_USING_COUNT);
        }

        public void StringIncrementAsync(string key)
        {
            IDatabase db = Database();
            if (db == null) return;
            key = $"{productID}_{key}";
            db.StringIncrementAsync(key);
        }
        public void StringDecrementAsync(string key)
        {
            IDatabase db = Database();
            if (db == null) return;
            key = $"{productID}_{key}";
            db.StringDecrementAsync(key);
        }



        public bool HashExists(RedisKey key, RedisValue hashField)
        {
            IDatabase db = Database();
            if (db == null) return false;
            key = $"{productID}_{key}";
            db.HashExists(key, hashField);
            UsingCount();
            return true;
        }

        public Task<RedisValue[]> HashValuesAsync(string key)
        {
            IDatabase db = Database();
            if (db == null) return null;
            key = $"{productID}_{key}";
            var value = db.HashValuesAsync(key);
            UsingCount();
            return value;
        }



    }
}
