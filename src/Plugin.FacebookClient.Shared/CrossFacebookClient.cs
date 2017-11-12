using Plugin.FacebookClient.Abstractions;
using System;

namespace Plugin.FacebookClient
{
  /// <summary>
  /// Cross platform FacebookClient implemenations
  /// </summary>
  public class CrossFacebookClient
  {
    static Lazy<IFacebookClient> Implementation = new Lazy<IFacebookClient>(() => CreateFacebookClient(), System.Threading.LazyThreadSafetyMode.PublicationOnly);

    /// <summary>
    /// Current settings to use
    /// </summary>
    public static IFacebookClient Current
    {
      get
      {
        var ret = Implementation.Value;
        if (ret == null)
        {
          throw NotImplementedInReferenceAssembly();
        }
        return ret;
      }
    }

    static IFacebookClient CreateFacebookClient()
    {
#if NETSTANDARD1_0
        return null;
#else
        return new FacebookClientManager();
#endif
        }

        internal static Exception NotImplementedInReferenceAssembly()
    {
      return new NotImplementedException("This functionality is not implemented in the portable version of this assembly.  You should reference the NuGet package from your main application project in order to reference the platform-specific implementation.");
    }
  }
}
