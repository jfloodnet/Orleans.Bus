using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sample
{
    public interface IApiWorker
    {
        Task<int> Search(string subject);
    }

    static class ApiWorker
    {
        public static IApiWorker Create(string api)
        {
            if (api == "facebook")
                return new FacebookApiWorker();

            if (api == "twitter")
                return new TwitterApiWorker();

            throw new InvalidOperationException("Unknown api: " + api);
        }
    }

    class FacebookApiWorker : IApiWorker
    {
        readonly Random random = new Random();
        
        public Task<int> Search(string subject)
        {
            return Task.FromResult(random.Next(0, 100));
        }
    }

    class TwitterApiWorker : IApiWorker
    {
        readonly Random random = new Random();

        public Task<int> Search(string subject)
        {
            return Task.FromResult(random.Next(0, 100));
        }
    }

    class DemoWorker : IApiWorker
    {
        readonly Random random = new Random();
        readonly string api;
        long requests;

        public DemoWorker(string api)
        {
            this.api = api;
        }

        public Task<int> Search(string subject)
        {
            requests++;

            if (requests % 10 == 0)
                throw new ApiUnavailableException(api);

            return Task.FromResult(random.Next(0, 100));
        }
    }
}
