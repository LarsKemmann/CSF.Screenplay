using System;
using Microsoft.Playwright;

namespace CSF.Screenplay.Playwright.Elements
{
    /// <summary>
    /// Extension methods for <see cref="ITarget"/>.
    /// </summary>
    public static class TargetExtensions
    {
        /// <summary>
        /// Gets a target which matches the <paramref name="inner"/> target only within the elements matched by the <paramref name="outer"/> one.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The result builds a Playwright locator chain: <c>outer.Locator(inner)</c>. Both targets stay lazy.
        /// </para>
        /// </remarks>
        /// <param name="inner">The target to look for</param>
        /// <param name="outer">The target within which to look</param>
        /// <param name="name">An optional human-readable name; "<c>{inner} within {outer}</c>" when omitted</param>
        /// <returns>A target</returns>
        /// <exception cref="ArgumentNullException">If either target is <see langword="null" />.</exception>
        public static ITarget Within(this ITarget inner, ITarget outer, string name = null)
        {
            if (inner is null) throw new ArgumentNullException(nameof(inner));
            if (outer is null) throw new ArgumentNullException(nameof(outer));
            return new NestedTarget(inner, outer, name ?? $"{inner.Name} within {outer.Name}");
        }

        /// <summary>
        /// Gets a target which matches the n-th (zero-based) of the elements matched by the specified target.
        /// </summary>
        /// <param name="target">A target matching several elements</param>
        /// <param name="index">The zero-based index</param>
        /// <param name="name">An optional human-readable name</param>
        /// <returns>A target</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="target"/> is <see langword="null" />.</exception>
        public static ITarget Nth(this ITarget target, int index, string name = null)
        {
            if (target is null) throw new ArgumentNullException(nameof(target));
            return new DerivedTarget(target, locator => locator.Nth(index), name ?? $"the {Ordinal(index + 1)} of {target.Name}");
        }

        /// <summary>
        /// Gets a target which matches the first of the elements matched by the specified target.
        /// </summary>
        /// <param name="target">A target matching several elements</param>
        /// <param name="name">An optional human-readable name</param>
        /// <returns>A target</returns>
        public static ITarget First(this ITarget target, string name = null)
        {
            if (target is null) throw new ArgumentNullException(nameof(target));
            return new DerivedTarget(target, locator => locator.First, name ?? $"the first of {target.Name}");
        }

        /// <summary>
        /// Gets a target which matches the last of the elements matched by the specified target.
        /// </summary>
        /// <param name="target">A target matching several elements</param>
        /// <param name="name">An optional human-readable name</param>
        /// <returns>A target</returns>
        public static ITarget Last(this ITarget target, string name = null)
        {
            if (target is null) throw new ArgumentNullException(nameof(target));
            return new DerivedTarget(target, locator => locator.Last, name ?? $"the last of {target.Name}");
        }

        /// <summary>
        /// Gets a target which matches only those of the specified target's elements which contain the given text.
        /// </summary>
        /// <param name="target">A target matching several elements</param>
        /// <param name="text">The text the elements must contain</param>
        /// <param name="name">An optional human-readable name</param>
        /// <returns>A target</returns>
        public static ITarget ContainingText(this ITarget target, string text, string name = null)
        {
            if (target is null) throw new ArgumentNullException(nameof(target));
            return new DerivedTarget(target, locator => locator.Filter(new LocatorFilterOptions { HasText = text }), name ?? $"{target.Name} containing '{text}'");
        }

        static string Ordinal(int number)
        {
            if (number % 100 >= 11 && number % 100 <= 13) return $"{number}th";
            switch (number % 10)
            {
                case 1: return $"{number}st";
                case 2: return $"{number}nd";
                case 3: return $"{number}rd";
                default: return $"{number}th";
            }
        }

        sealed class NestedTarget : ITarget
        {
            readonly ITarget inner, outer;
            public string Name { get; }
            public ILocator GetLocator(IPage page) => outer.GetLocator(page).Locator(inner.GetLocator(page));
            public override string ToString() => Name;
            public NestedTarget(ITarget inner, ITarget outer, string name) { this.inner = inner; this.outer = outer; Name = name; }
        }

        sealed class DerivedTarget : ITarget
        {
            readonly ITarget source;
            readonly Func<ILocator, ILocator> derive;
            public string Name { get; }
            public ILocator GetLocator(IPage page) => derive(source.GetLocator(page));
            public override string ToString() => Name;
            public DerivedTarget(ITarget source, Func<ILocator, ILocator> derive, string name) { this.source = source; this.derive = derive; Name = name; }
        }
    }
}
