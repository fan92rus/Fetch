
# Boilerpipe.Net

Boilerpipe.Net is a port of the boilerpipe (https://code.google.com/p/boilerpipe/) Java library.

The Boilerpipe.Net library provides algorithms to detect and remove the surplus "clutter" (boilerplate, templates) around the main textual content of a web page.

The library already provides specific strategies for common tasks (for example: news article extraction) and may also be easily extended for individual problem settings.

Extracting content is very fast (milliseconds), just needs the input document (no global or site-level information required) and is usually quite accurate.

## Installation

To install [Boilerpipe.Net](https://www.nuget.org/packages/Boilerpipe.Net) from the [NuGet Gallery](http://www.nuget.org), run the following in the [Package Manager Console](http://docs.nuget.org/docs/start-here/using-the-package-manager-console)
```powershell
PM> Install-Package Boilerpipe.Net
```

## Usage

```c#
using Boilerpipe.Net.Extractors;
...
string html = ...
// NOTE: Use ArticleExtractor unless DefaultExtractor gives better results for you
string text = CommonExtractors.ArticleExtractor.GetText(html);

```

# License

Boilerpipe.Net is licensed under [LGPL V3](LICENSE).
