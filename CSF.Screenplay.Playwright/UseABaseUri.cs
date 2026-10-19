using System;

namespace CSF.Screenplay.Playwright
{
    /// <summary>
    /// Screenplay ability which allows an <see cref="Actor"/> to use a base Uri, 'completing' relative Uris at runtime.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This ability enables the use of a technique whereby the environment/location of a web application (which is being
    /// tested or manipulated by the Playwright Extension for Screenplay) may be set and changed at runtime, instead of hard-coded —
    /// a developer's machine, a testing environment, a staging environment.
    /// Write instances of <see cref="NamedUri"/> with relative Uris, such as <c>new NamedUri("user/shoppingCart", "the user's shopping cart")</c>,
    /// grant the actor this ability, and <see cref="Tasks.NavigateToUrl"/> (via <see cref="PerformableBuilder.NavigateTo(NamedUri)"/>)
    /// <see cref="NamedUri.RebaseTo(Uri)">rebases</see> the named Uri to the base Uri before opening it.
    /// </para>
    /// </remarks>
    /// <seealso cref="Actions.OpenUrl"/>
    /// <seealso cref="Tasks.NavigateToUrl"/>
    /// <seealso cref="NamedUri"/>
    public class UseABaseUri : ICanReport
    {
        /// <inheritdoc/>
        public ReportFragment GetReportFragment(Actor actor, IFormatsReportFragment formatter)
            => formatter.Format("{Actor} is able to use the base URI {BaseUri}", actor.Name, BaseUri);

        /// <summary>
        /// Gets the base URI.
        /// </summary>
        public Uri BaseUri { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="UseABaseUri"/>.
        /// </summary>
        /// <param name="baseUri">The base URI; an absolute URI</param>
        /// <exception cref="ArgumentNullException">If <paramref name="baseUri"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If <paramref name="baseUri"/> is not absolute.</exception>
        public UseABaseUri(Uri baseUri)
        {
            if (baseUri is null) throw new ArgumentNullException(nameof(baseUri));
            if (!baseUri.IsAbsoluteUri) throw new ArgumentException("The base URI must be absolute.", nameof(baseUri));
            BaseUri = baseUri;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="UseABaseUri"/>.
        /// </summary>
        /// <param name="baseUri">The base URI; an absolute URI</param>
        public UseABaseUri(string baseUri) : this(new Uri(baseUri, UriKind.Absolute)) { }
    }

    /// <summary>
    /// A URI with a human-readable name, for reporting.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The URI may be relative, in which case it is expected to be rebased to an absolute base URI at runtime by an actor
    /// with the <see cref="UseABaseUri"/> ability; see <see cref="RebaseTo(Uri)"/>.
    /// </para>
    /// </remarks>
    public sealed class NamedUri : IHasName
    {
        /// <inheritdoc/>
        public string Name { get; }

        /// <summary>
        /// Gets the URI.
        /// </summary>
        public Uri Uri { get; }

        /// <summary>
        /// Gets a copy of the current instance in which the URI is rebased to the specified base URI, if the URI is relative.
        /// An absolute URI is returned unchanged.
        /// </summary>
        /// <param name="baseUri">An absolute base URI</param>
        /// <returns>A named URI whose URI is absolute</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="baseUri"/> is <see langword="null" />.</exception>
        public NamedUri RebaseTo(Uri baseUri)
        {
            if (baseUri is null) throw new ArgumentNullException(nameof(baseUri));
            return Uri.IsAbsoluteUri ? this : new NamedUri(new Uri(baseUri, Uri), Name);
        }

        /// <inheritdoc/>
        public override string ToString() => Name;

        /// <summary>
        /// Initializes a new instance of <see cref="NamedUri"/>.
        /// </summary>
        /// <param name="uri">The URI; relative or absolute</param>
        /// <param name="name">An optional human-readable name; the URI itself when omitted</param>
        /// <exception cref="ArgumentNullException">If <paramref name="uri"/> is <see langword="null" />.</exception>
        public NamedUri(Uri uri, string name = null)
        {
            Uri = uri ?? throw new ArgumentNullException(nameof(uri));
            Name = name ?? uri.ToString();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="NamedUri"/>.
        /// </summary>
        /// <param name="uri">The URI; relative or absolute</param>
        /// <param name="name">An optional human-readable name; the URI itself when omitted</param>
        public NamedUri(string uri, string name = null) : this(new Uri(uri, UriKind.RelativeOrAbsolute), name) { }

        /// <summary>Converts a URI to an unnamed <see cref="NamedUri"/>.</summary>
        public static implicit operator NamedUri(Uri uri) => uri is null ? null : new NamedUri(uri);

        /// <summary>Converts a URI string to an unnamed <see cref="NamedUri"/>.</summary>
        public static implicit operator NamedUri(string uri) => uri is null ? null : new NamedUri(uri);
    }
}
