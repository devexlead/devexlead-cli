> ⚠️ **Warning:** This feature is **experimental** and may change in future releases.  
> Feedback is welcome! Please [create a GitHub issue](https://github.com/devexlead/devexlead-cli/issues/new) to share your thoughts or report any issues.

- Install the developer experience CLI
`dotnet tool install --global dxc --version 0.0.19-g255ded9190`
- Configure the required parameters:
  - `dxc vault update --key "AtlassianBaseUrl" --value {value}`
  - `dxc vault update --key "AtlassianUser" --value {value}`
  - `dxc vault update --key "AtlassianKey" --value {value}`
- Run `dxc jira create`
- Follow the prompts
![WindowsTerminal_RH2cEWI5Ln](https://github.com/user-attachments/assets/04ef4073-e2df-47e3-a064-15c8f00f56af)
