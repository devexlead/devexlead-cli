> ⚠️ **Warning:** This feature is **experimental** and may change in future releases.  
> Feedback is welcome! Please [create a GitHub issue](https://github.com/devexlead/devexlead-cli/issues/new) to share your thoughts or report any issues.

## How to use it
- Install the Developer Experience CLI
`dotnet tool install --global dxc --version 0.0.19-g255ded9190`
- Configure the required parameters (these parameters are encrypted and stored locally):
  - `dxc vault update --key "AtlassianBaseUrl" --value {YourAtlassianBaseUrl}`
  - `dxc vault update --key "AtlassianUser" --value {YourAtlassianUsername}`
  - `dxc vault update --key "AtlassianKey" --value {YourAtlassianAPIKey}`
- Run `dxc jira create`
- Follow the prompts

## Demo
![WindowsTerminal_RH2cEWI5Ln](https://github.com/user-attachments/assets/04ef4073-e2df-47e3-a064-15c8f00f56af)
