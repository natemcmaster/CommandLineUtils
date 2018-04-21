Contributing
============

A few simple rules for the road.

1. Before opening a PR with a significant change, open an issue so we can discuss design issues. Significant = behavior or API change
2. New functionality must have tests, and they must pass.
3. Play nice. This is a community fork, i.e. not a Microsoft-offical project. In other words, I'm doing this in my free time.

## Contributing to the documentation

If you are contributing to the documentation, you can build and preview the docs by executing `./docs/generate.ps1`. The `/docs` folder contains the source which **DocFX** uses generates the documentation.
The results of this are saved to the `gh-pages` branch of the repository.

Run `./docs/generate.ps1 -Serve` to build the docs and serve them on <http://localhost:8080>.
