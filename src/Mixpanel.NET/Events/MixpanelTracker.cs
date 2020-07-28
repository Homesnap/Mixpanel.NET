using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Linq;
using System.Threading.Tasks;

namespace Mixpanel.NET.Events
{
    public class MixpanelTracker : MixpanelClientBase, IEventTracker
    {
        private readonly TrackerOptions _options;

        /// <summary>
        /// Creates a new Mixpanel tracker for a given API token
        /// </summary>
        /// <param name="token">The API token for MixPanel</param>
        /// <param name="http">An implementation of IMixpanelHttp, <see cref="MixpanelHttp"/>
        /// Determines if class names and properties will be serialized to JSON literally.
        /// If false (the default) spaces will be inserted between camel-cased words for improved 
        /// readability on the reporting side.
        /// </param>
        /// <param name="options">Optional: Specific options for the API <see cref="TrackerOptions"/></param>
        public MixpanelTracker(string token, IMixpanelHttp http = null, TrackerOptions options = null)
            : base(token, http)
        {
            _options = options ?? new TrackerOptions();
        }

        public bool Track(string @event, IDictionary<string, object> properties)
        {
            var propertyBag = properties.FormatProperties();
            // Standardize token and time values for Mixpanel
            propertyBag["token"] = token;

            if (_options.SetEventTime && !properties.Keys.Any(x => x.ToLower() == "time"))
                propertyBag["time"] = DateTime.UtcNow.FormatDate();

            var data = new JavaScriptSerializer().Serialize(new Dictionary<string, object>
            {
                { "event", @event },
                { "properties", propertyBag }
            });

            var values = "data=" + data.Base64Encode();

            if (_options.Test) values += "&test=1";

            var contents = _options.UseGet
              ? http.Get(Resources.MixpanelTrackUrl, values)
              : http.Post(Resources.MixpanelTrackUrl, values);

            return contents == "1";
        }

        public async Task<bool> TrackAsync(string @event, IDictionary<string, object> properties)
        {
            var propertyBag = properties.FormatProperties();
            // Standardize token and time values for Mixpanel
            propertyBag["token"] = token;

            if (_options.SetEventTime && !properties.Keys.Any(x => x.ToLower() == "time"))
                propertyBag["time"] = DateTime.UtcNow.FormatDate();

            var data = new JavaScriptSerializer().Serialize(new Dictionary<string, object>
            {
                { "event", @event },
                { "properties", propertyBag }
            });

            var values = "data=" + data.Base64Encode();

            if (_options.Test) values += "&test=1";

            var contents = _options.UseGet
                ? await http.GetAsync(Resources.MixpanelTrackUrl, values).ConfigureAwait(false)
                : await http.PostAsync(Resources.MixpanelTrackUrl, values).ConfigureAwait(false);

            return contents == "1";
        }

        public bool Track(MixpanelEvent @event)
        {
            return Track(@event.Event, @event.Properties);
        }

        public async Task<bool> TrackAsync(MixpanelEvent @event)
        {
            return await TrackAsync(@event.Event, @event.Properties).ConfigureAwait(false);
        }

        public bool Track<T>(T @event)
        {
            return Track(@event.ToMixpanelEvent(_options.LiteralSerialization));
        }
        
        public async Task<bool> TrackAsync<T>(T @event)
        {
            return await TrackAsync(@event.ToMixpanelEvent(_options.LiteralSerialization)).ConfigureAwait(false);
        }

    }
}