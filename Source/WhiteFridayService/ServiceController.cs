using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using MySql.Data.MySqlClient;

namespace WhiteFriday.Service
{
    public sealed class ServiceController : IServiceController
    {
        private readonly Dictionary<string, Type> _avaiableTargets;
        private readonly Dictionary<string, TargetProvider> _activeTargets;
        private readonly Dictionary<int, TargetProvider> _database2Target;

        private XmlNode _config;
        private bool _running;
        private MySqlConnection _connection;

        public XmlNode Configuration { get { return _config; } }
        public TimeSpan QueryInterval { get; private set; }
        public TimeSpan QueryDelay { get; private set; }
        public int MaxRetries { get; private set; }

        public ServiceController()
        {
            _avaiableTargets = new Dictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);
            _activeTargets = new Dictionary<string, TargetProvider>(StringComparer.InvariantCultureIgnoreCase);
            _database2Target = new Dictionary<int, TargetProvider>();
        }

        public void Run()
        {
            _running = true;

            Console.WriteLine("WhiteFriday daemon starting up");

            if (!LoadConfiguration())
                return;

            if (!OpenDatabase())
                return;

            LoadPlugins();
            AssignTargets();

            Console.WriteLine("WhiteFriday is ready!");

            foreach (TargetProvider provider in _activeTargets.Values)
                provider.Activate();

            while (_running)
                Thread.Yield();
        }

        public ProductDescriptor[] QueryProducts(int targetID)
        {
            List<ProductDescriptor> lst = new List<ProductDescriptor>();
            MySqlCommand cmd = new MySqlCommand("SELECT `product_id`, `url` FROM `products_targets` WHERE `target_id`=@tid", _connection);
            cmd.Parameters.AddWithValue("@tid", targetID);

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            DataSet ds = new DataSet();

            da.Fill(ds, "products_targets");

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                int pid = int.Parse(dr[0].ToString());
                string url = dr[1].ToString();

                lst.Add(new ProductDescriptor
                {
                    ProductID = pid,
                    ProductUrl = url,
                    Price = GetLastPriceData(pid, targetID)
                });
            }

            ds.Dispose();
            da.Dispose();
            cmd.Dispose();

            return lst.ToArray();
        }

        private PriceData GetLastPriceData(int productID, int targetID)
        {
            PriceData pd = new PriceData();

            MySqlCommand cmd = new MySqlCommand("SELECT `old_price`, `new_price`, `discount` FROM `price_history` WHERE `product_id`=@pid AND `target_id`=@tid ORDER BY `id` DESC LIMIT 1", _connection);
            cmd.Parameters.AddWithValue("@pid", productID);
            cmd.Parameters.AddWithValue("@tid", targetID);

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            DataSet ds = new DataSet();

            da.Fill(ds, "products_targets");

            ds.Dispose();
            da.Dispose();
            cmd.Dispose();

            if (ds.Tables[0].Rows.Count != 1)
                return null;

            pd.FromPrice = decimal.Parse(ds.Tables[0].Rows[0][0].ToString());
            pd.CurrentPrice = decimal.Parse(ds.Tables[0].Rows[0][1].ToString());
            pd.CurrentDiscount = decimal.Parse(ds.Tables[0].Rows[0][2].ToString());

            return pd;
        }

        public void UpdateProductPrice(int targetID, int productID, PriceData data)
        {
            MySqlCommand cmd = new MySqlCommand("INSERT INTO `price_history`(`product_id`, `target_id`, `date`, `old_price`, `new_price`, `discount`) VALUES (@pid, @tid, NOW(), @oprice, @nprice, @discount)", _connection);
            cmd.Parameters.AddWithValue("@pid", productID);
            cmd.Parameters.AddWithValue("@tid", targetID);
            cmd.Parameters.AddWithValue("@oprice", data.FromPrice);
            cmd.Parameters.AddWithValue("@nprice", data.CurrentPrice);
            cmd.Parameters.AddWithValue("@discount", data.CurrentDiscount);
            cmd.ExecuteNonQuery();
            cmd.Dispose();
        }

        public void Stop()
        {
            _running = false;
        }

        private bool OpenDatabase()
        {
            Console.WriteLine("Connecting to database...");

            try
            {
                _connection = new MySqlConnection(_config.SelectNodeValue("DatabaseConnection"));
                _connection.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error connecting to database: {0}", ex);
                return false;
            }

            return true;
        }

        private void AssignTargets()
        {
            int count = 0;

            foreach (XmlNode node in _config.SelectNodes("Targets/Target"))
            {
                Type type;

                if (node.Attributes["Name"] == null || node.Attributes["ID"] == null)
                {
                    Console.WriteLine("Ignoring target, missing required attributes.");
                    continue;
                }

                string name = node.Attributes["Name"].Value;
                int id;

                if (!int.TryParse(node.Attributes["ID"].Value, out id))
                {
                    Console.WriteLine("Ignoring target '{0}', invalid ID format.", name);
                    continue;
                }

                if (_activeTargets.ContainsKey(name))
                {
                    Console.WriteLine("Ignoring target '{0}', already active.", name);
                    continue;
                }

                if (!_avaiableTargets.TryGetValue(name, out type))
                {
                    Console.WriteLine("Target '{0}' not avaiable.", name);
                    continue;
                }

                TargetProvider provider;

                try
                {
                   provider = (TargetProvider)Activator.CreateInstance(type, this, node);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error activating target '{0}': {1}", name, ex);
                    continue;
                }

                _activeTargets[name] = provider;
                _database2Target[id] = provider;
                count++;
            }

            Console.WriteLine("Activated {0} targets", count);
        }

        private bool LoadConfiguration()
        {
            try
            {
                XmlDocument document = new XmlDocument();
                document.Load("Config/Config.xml");
                _config = document.SelectSingleNode("Config");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading configuration: {0}", ex);
                return false;
            }

            int temp;

            if (!int.TryParse(_config.SelectNodeValue("QueryInterval"), out temp))
                temp = 86400;

            QueryInterval = TimeSpan.FromSeconds(temp);

            if (!int.TryParse(_config.SelectNodeValue("QueryDelay"), out temp))
                temp = 1000;

            QueryDelay = TimeSpan.FromMilliseconds(temp);

            if (!int.TryParse(_config.SelectNodeValue("MaxRetries"), out temp))
                temp = 5;

            MaxRetries = temp;

            return true;
        }

        private void LoadPlugins()
        {
            int count = 0;
            string prefix = _config.SelectNodeValue("PluginsPrefix", "WhiteFriday.");

            foreach (string file in Directory.GetFiles(_config.SelectNodeValue("PluginsDirectory", "Plugins"), "*.dll"))
            {
                if (Path.GetFileName(file).StartsWith(prefix))
                {
                    Assembly asm;

                    try
                    {
                        asm = Assembly.LoadFile(Path.GetFullPath(file));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error loading plugins '{0}': {1}", file, ex);
                        continue;
                    }

                    if (LoadPlugins(asm))
                        count++;
                }
            }

            Console.WriteLine("Loaded {0} plugins", count);
        }

        private bool LoadPlugins(Assembly asm)
        {
            bool loaded = false;

            foreach (Type type in asm.ExportedTypes)
            {
                if (Attribute.IsDefined(type, typeof(TargetProviderAttribute)))
                {
                    TargetProviderAttribute attribute = type.GetCustomAttribute<TargetProviderAttribute>();
                    _avaiableTargets[attribute.Name] = type;
                    loaded = true;
                }
            }

            return loaded;
        }
    }
}
