Contributing
============

A few simple rules for the road.

1. Before opening a PR with a significant change, open an issue so we can discuss design issues. Significant = behavior or API change
2. New functionality must have tests, and they must pass.
3. Play nice. This is a community fork, i.e. not a Microsoft-offical project. In other words, I'm doing this in my free time.

## Contributing to the documentation

If you are contributing to the documentation, ensure that your Pull Request does not contain updates to any files in the `/docs` folder and only updates to files in the `/docfx_project` folder. The `/docs` folder is where **DocFX** generates the documentation, and due to the verbose nature of those files, it makes reviewing Pull Requests a difficult process. Also, the files generated in that folder will contain links to your forked repo which is not correct.