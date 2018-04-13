using System;
using System.IO;
using WorkingTools.Extensions;
using WorkingTools.Extensions.Json;
using WorkingTools.FileSystem;

namespace SL.Configuration
{
    public static class ConfigLoader
    {
        public static TCfg Load<TCfg>(string filePath = "Configuration.json", bool? thorwException = null, bool? autoCreate = null, bool? autoSave = null, DateTimeOffset? overwriteObsolete = null)
        {
            if (thorwException == null) thorwException = false;
            if (autoCreate == null) autoCreate = true;
            if (autoSave == null) autoSave = false;

            try
            {
                if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentOutOfRangeException(nameof(filePath));

                if (overwriteObsolete != null)
                {
                    var lastWriteTime = FileWt.GetLastWriteTime(filePath);
                    if (overwriteObsolete > lastWriteTime) FileWt.Delete(filePath);
                }


                if (autoCreate.Value && !FileWt.Exists(filePath))
                {
                    var cfg = (TCfg)typeof(TCfg).Create();
                    if (autoSave.Value) Save(cfg, filePath, thorwException);
                    return cfg;
                }
                
                var sConf = FileWt.ReadAllText(filePath);
                if (!string.IsNullOrWhiteSpace(sConf))
                {
                    return sConf.FromJson<TCfg>();
                }
            }
            catch (Exception ex)
            {
                if (thorwException.Value) throw;
            }

            return default(TCfg);
        }

        public static bool Save<TCfg>(TCfg config, string cfgFilePath = "Configuration.json", bool? thorwException = null)
        {
            if (thorwException == null) thorwException = false;

            try
            {
                if (string.IsNullOrWhiteSpace(cfgFilePath)) throw new ArgumentOutOfRangeException(nameof(cfgFilePath));

                var sconf = config.ToJson(indented: true);
                FileWt.WriteAllText(cfgFilePath, sconf);

                return true;
            }
            catch (Exception)
            {
                if (thorwException.Value) throw;

                return false;
            }
        }
    }
}
