using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Xml;
using Timer = System.Timers.Timer;

namespace WhiteFriday
{
    public abstract class TargetProvider
    {
        private readonly Timer _interval;
        private Thread _workThread;

        public IServiceController Controller { get; private set; }
        public XmlNode Configuration { get; private set; }

        public int DatabaseID { get; private set; }
        public TimeSpan QueryInterval { get; protected set; }
        public TimeSpan QueryDelay { get; protected set; }
        public int MaxRetries { get; protected set; }

        protected TargetProvider(IServiceController controller, XmlNode configuration)
        {
            Controller = controller;
            Configuration = configuration;

            LoadConfig();

            _interval = new Timer(QueryInterval.TotalMilliseconds);
            _interval.AutoReset = true;
            _interval.Elapsed += _interval_Elapsed;
        }

        public void Activate()
        {
            _interval.Start();
            _interval_Elapsed(null, null);
        }

        public void Deactivate()
        {
            _interval.Stop();
        }

        protected abstract PriceData Process(string data);

        private void Process()
        {
            ProductDescriptor[] products = Controller.QueryProducts(DatabaseID);

            if (products == null)
                return;

            foreach (ProductDescriptor product in products)
            {
                PriceData data = StartProcess(product.ProductUrl);

                if (data == null)
                    continue;

                if (product.Price == null || !product.Price.Equals(data))
                    Controller.UpdateProductPrice(DatabaseID, product.ProductID, data);
            }
        }

        private PriceData StartProcess(string url)
        {
            PriceData price = null;

            for (int i = 0; i < MaxRetries; i++)
            {
                string data;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.KeepAlive = false;
                request.Proxy = null;
                request.UserAgent = "Chrome 28.0.1468.0";

                HttpWebResponse response = null;

                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error requesting data, retrying: {0}", ex);
                }

                if (response == null)
                    continue;

                StreamReader stream = new StreamReader(response.GetResponseStream());
                data = stream.ReadToEnd();
                stream.Close();

                response.Dispose();

                try
                {
                    price = Process(data);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error processing data, retrying: {0}", ex);
                    continue;
                }

                break;
            }

            return price;
        }

        private void LoadConfig()
        {
            int temp;

            DatabaseID = int.Parse(Configuration.Attributes["ID"].Value);

            if (!int.TryParse(Configuration.SelectNodeValue("QueryInterval"), out temp))
                temp = (int)Controller.QueryInterval.TotalSeconds;

            QueryInterval = TimeSpan.FromSeconds(temp);

            if (!int.TryParse(Configuration.SelectNodeValue("QueryDelay"), out temp))
                temp = (int)Controller.QueryDelay.TotalSeconds;

            QueryDelay = TimeSpan.FromMilliseconds(temp);

            if (!int.TryParse(Configuration.SelectNodeValue("MaxRetries"), out temp))
                temp = Controller.MaxRetries;

            MaxRetries = temp;
        }

        private void _interval_Elapsed(object sender, ElapsedEventArgs e)
        {
            _workThread = new Thread(Process);
            _workThread.IsBackground = true;
            _workThread.Start();
        }
    }
}
