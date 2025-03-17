

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![Contributors](https://img.shields.io/github/contributors/devexlead/devexlead-cli)
[![Star this repo](https://img.shields.io/github/stars/devexlead/devexlead-cli?style=social)](https://github.com/devexlead/devexlead-cli/stargazers)

> [!NOTE]
> Suggestions, Ideas and Contributions are welcome.
> If you find this project helpful, please give it a star 🌟

# Installation

`dotnet tool install --global dxc --version 0.0.53-ga74c99d943`

# Goals

- Enhance the developer experience for local development and testing.
- Accelerate the onboarding process for setting up a new laptop, reducing the time and effort required.
- Streamline the configuration of the development environment, minimizing cognitive load.

# Modules

- `vault`: Encrypt and save secrets locally
- `command`: Create shorcuts for commonly used commands
- `setup`: Install software for a new laptop
- `transfer`: Transfer configuration between machines
- `tool`: Developer tools
- `git`: Git operations against a pre-defined set of repositories
- `ssl`: Create and install Self Signed Certificates
- `jira`: https://github.com/devexlead/devexlead-cli/blob/main/docs/jira.md

Links to be added (in the meantime you can check `dxc --help`)

# Intellisense

The tool relies on PowerShell [PSReadLine](https://learn.microsoft.com/en-us/powershell/module/psreadline/) to provide intellisense capabilities
![WindowsTerminal_XzhdeUatYN](https://github.com/user-attachments/assets/288fbce1-6df6-4c90-a48f-436f2604f6d7)
