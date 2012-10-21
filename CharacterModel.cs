using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Diagnostics;

namespace WoWApiHelper
{
    [DataContract()]
    [DebuggerDisplay("{Name} - {Realm}")]
    public class CharacterModel : IExtensibleDataObject
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "realm")]
        public string Realm { get; set; }

        [DataMember(Name = "race")]
        public int Race { get; set; }

        [DataMember(Name = "class")]
        public int Class { get; set; }

        [DataMember(Name = "battlegroup")]
        public string Battlegroup { get; set; }

        [DataMember(Name = "level")]
        public int Level { get; set; }

        [DataMember(Name = "thumbnail")]
        public string Thumbnail { get; set; }

        [DataMember(Name = "items")]
        public IEnumerable<dynamic> Items { get; set; }

        public string ThumbnailURL
        {
            get
            {
                return string.Format("http://eu.battle.net/static-render/eu/{0}", Thumbnail);
            }
        }

        public string AvatarURL
        {
            get
            {
                return ThumbnailURL.Replace("avatar.jpg", "profilemain.jpg");
            }
        }

        public ExtensionDataObject ExtensionData { get; set; }
    }
}
