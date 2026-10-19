# CSF.Screenplay.Playwright

**CSF.Screenplay.Playwright** is an [Extension] to CSF.Screenplay which allows [Actors] to control web browsers using [Microsoft Playwright].

It provides the ability `BrowseTheWeb` (a browser context and page per actor, over one shared browser), `UseABaseUri`, targets which wrap Playwright's lazy, auto-waiting locators (CSS, test id, role, text, label, placeholder, XPath), actions, questions and tasks for the common interactions, screenshots and traces as report assets — automatically when a performable fails — and `AddPlaywright()` for dependency injection, configured through `PlaywrightOptions`.

Learn more at the [Playwright extension documentation page].

[Extension]: https://csf-dev.github.io/CSF.Screenplay/glossary/Extension.html
[Actors]: https://csf-dev.github.io/CSF.Screenplay/api/CSF.Screenplay.Actor.html
[Microsoft Playwright]: https://playwright.dev/dotnet/
[Playwright extension documentation page]: https://csf-dev.github.io/CSF.Screenplay/docs/extensions/playwright/index.html
