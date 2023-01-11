function MainSettings(props) {
  return (
    <Page>
      <Section
        title={<Text bold align="center">WebSocket Settings</Text>}>
        <TextInput
          settingsKey="passcode"
          label="Your Passcode"
        />
        <TextInput
          settingsKey="servercode"
          label="Server Code"
        />
      </Section>
    </Page>
  );
}
registerSettingsPage(MainSettings);