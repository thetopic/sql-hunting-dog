using System;
using System.Reflection;

namespace HuntingDog.Core {
    public class DogVersion {
        public DogVersion(Version version, string url) {
            Version = version;
            UrlToDownload = url;
        }

        public Version Version { get; }

        public override string ToString() {
            return string.Format("{0}.{1}", Version.Major, Version.Minor);
        }

        public string UrlToDownload { get; set; }

        static Version _currentVersion;
        public static Version Current {
            get {
                if (_currentVersion == null) {
                    var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
                    _currentVersion = new Version(currentVersion.Major, currentVersion.Minor);
                }

                return _currentVersion;
            }
        }


    }
}
