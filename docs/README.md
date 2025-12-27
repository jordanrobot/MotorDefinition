# Documentation

This directory contains the documentation for the MotorDefinition library, built with Jekyll and deployed to GitHub Pages.

## Viewing the Documentation

The documentation is automatically deployed to GitHub Pages at:
https://jordanrobot.github.io/MotorDefinition/

## Local Development

To build and preview the documentation locally:

### Prerequisites

- Ruby 2.7 or higher
- Bundler gem

### Setup

```bash
cd docs
bundle install
```

### Build and Serve

```bash
bundle exec jekyll serve
```

The site will be available at `http://localhost:4000/MotorDefinition/`

### Build Only

```bash
bundle exec jekyll build
```

The built site will be in the `_site` directory.

## Automatic Deployment

The documentation is automatically built and deployed via GitHub Actions when:
- Changes are pushed to the `main` branch
- A pull request is merged into the `main` branch

See `.github/workflows/deploy-pages.yml` for the deployment configuration.

## Structure

- `index.md` - Home page
- `QuickStart.md` - Quick start guide
- `UserGuide.md` - Detailed user guide
- `TermsAndDefinitions.md` - Terminology reference
- `api/` - API documentation
- `adr/` - Architecture Decision Records
- `_config.yml` - Jekyll configuration
- `Gemfile` - Ruby dependencies for local development
